using System;
using System.Diagnostics;
using System.Threading.Tasks;
using IMS.Persistence;
using IMS.Persistence.Entities;
using IMS.Model.Simulation;
using System.Collections.Generic;
using System.Linq;

namespace IMS.Model
{
    public class IMSModel : IIMSModel
    {
        #region Private fields
        // A raktár négyzethálóba van szervezve.Vannak benne akkumulátoros robotok(Ri), célállomások(Si),
        //       dokkolónak nevezett töltőállomások(Di), és pod-nak nevezett állványok(P). A robotok
        //akkumulátorának van egy maximális töltöttségi állapota, ami egy egész szám.Az állványokon termékek
        //vannak, amit a fenti ábrán az állványra írt számok jelölnek.Egy állványon minden termékszámból
        //legfeljebb egy szerepelhet. A termékszámok a célállomás számok közül kerülnek ki. A robotok feladata,
        //hogy a termékeket az azonos számú célállomásokhoz vigyék (1-es termék az S1 célállomáshoz, 2-es az
        //S2-höz, stb.)


        private Entity[,] _gameTable; // simtábla
        private IMSData _IMSData;
        private IMSDataAccess _dataAccess; // adatelérés

        private ConflictBasedSearch cbs;

        private Dictionary<Robot, List<Pos>> routes;
        private Dictionary<Robot, List<Direction>> rotations;
        private Boolean SimulationFinished;


        private int _steps;
        private int _allEnergy;
        private int _speed;
        private int _time;
        private int _counter;


        #endregion

        #region Public properties

        public Entity this[Int32 x, Int32 y]
        {
            get
            {
                if (x < 0 || x > _gameTable.GetLength(0))
                    throw new ArgumentException("Bad column index.", "x");
                if (y < 0 || y > _gameTable.GetLength(1))
                    throw new ArgumentException("Bad row index.", "y");

                return _gameTable[x, y];
            }
        }

        public Int32 SizeX { get { return _gameTable.GetLength(0); } set { } }

        public Int32 SizeY { get { return _gameTable.GetLength(1); } set { } }

        public IMSData IMSData { get { return _IMSData; } }
        public int Steps { get { return _steps; } }
        public int AllEnergy { get { return _allEnergy; } }
        public int Speed { get { return _speed; } }
        public int Time { get { return _time; } }

        //public int Steps { get; }
        //public int AllEnergy {get;}
        #endregion

        #region Events
        /// <summary>
        ///  Simulation kezdetének eseménye.
        /// </summary>
        public event EventHandler<EventArgs> SimulationCreated; //either loaded or created

        public event EventHandler<EventArgs> SimulationStarted;
        public event EventHandler<EventArgs> SimulationOver;
        public event EventHandler<EventArgs> TableCreated; //created empty table on create mode
        public event EventHandler<EventArgs> TableChanged;
        /// <summary>
        /// Simulation végének eseménye.
        /// </summary>
        public event EventHandler SpeedChanged;

        /// <summary>
        /// Lépések szám változásának eseménye.
        /// </summary>
        public event EventHandler StepsChanged;
        /// <summary>
        /// Energia Fogyasztás eseménye.
        /// </summary>
        public event EventHandler EnergyChanged;
        /// <summary>
        /// Idő eltelésének eseménye.
        /// </summary>
        public event EventHandler TimePassed;



        /// <summary>
        /// Simulation megnyerésének eseménye.
        /// </summary>
        public event EventHandler<DiaryEventArgs> SimulationWon;


        #endregion

        #region Constructors

        public IMSModel() : this(null) { }

        public IMSModel(IMSDataAccess dataAccess)
        {
            _gameTable = new Entity[12, 12];
            _IMSData = new IMSData(_gameTable.GetLength(0), _gameTable.GetLength(1));

            _dataAccess = dataAccess;
            _steps = 0;
            _allEnergy = 0;
            _counter = 0;
            _speed = 1;
            _time = 0;
            SimulationFinished = false;
            //astar = new();
            //NewSimulation();
        }

        #endregion

        #region Public methods

        public void NewSimulation()
        {
            /*
            for (Int32 i = 0; i < _gameTable.GetLength(0); i++)
            {
                for (Int32 j = 0; j < _gameTable.GetLength(1); j++)
                {
                    _gameTable[i, j] = new Empty(i, j);
                }
            }
            */

            _gameTable = _createEmptyTable(_gameTable.GetLength(0), _gameTable.GetLength(1));
            OnSimulationCreated();
        }

        //public

        public void GenerateEmtyTableForSettingsWindow(int x, int y)
        {
            _gameTable = new Entity[x, y];
            for (Int32 i = 0; i < _gameTable.GetLength(0); i++)
            {
                for (Int32 j = 0; j < _gameTable.GetLength(1); j++)
                {
                    _gameTable[i, j] = new Empty(i, j);
                }
            }
            OnTableCreated();
        }

        public void setSpeed(int n)
        {
            _speed += n;
            OnSpeedChanged(_speed);
        }

        public async Task LoadSimulationAsync(String path)
        {
            if (_dataAccess == null)
                return;


            //IMSData values = await _dataAccess.LoadSimulationAsync(path);
            _IMSData = await _dataAccess.LoadSimulationAsync(path);
            _gameTable = _extractTableFromIMSData();


            /*
            if (values.Length != _gameTable.Length)
                throw new IMSDataException("Error occurred during game loading.");

            for (Int32 i = 0; i < _gameTable.GetLength(0); i++)
                for (Int32 j = 0; j < _gameTable.GetLength(1); j++)
                {
                    _gameTable[i, j] = values[i * _gameTable.GetLength(0) + j];

                    OnFieldChanged(i, j, _gameTable[i, j]);
                }
            */

            OnSimulationCreated();

            //NewSimulation();

        }

        public async Task SaveSimulationAsync(String path)
        {
            if (_dataAccess == null)
                return;

            await _dataAccess.SaveSimulationAsync(path, _IMSData);
        }

        #endregion

        #region Private methods

        private Entity[,] _createEmptyTable(Int32 sizeX, Int32 sizeY)
        {
            Entity[,] table = new Entity[sizeX, sizeY];

            for (Int32 i = 0; i < table.GetLength(0); i++)
            {
                for (Int32 j = 0; j < table.GetLength(1); j++)
                {
                    table[i, j] = new Empty(i, j);
                }
            }

            return table;
        }

        private Entity[,] _extractTableFromIMSData()
        {
            //Entity[,] table = new Entity[_IMSData.SizeX, _IMSData.SizeY];

            Entity[,] table = _createEmptyTable(_IMSData.SizeX, _IMSData.SizeY);

            foreach (Robot robot in _IMSData.EntityData.RobotData)
            {
                table[robot.Pos.X, robot.Pos.Y] = robot;
            }
            foreach (Pod pod in _IMSData.EntityData.PodData)
            {
                table[pod.Pos.X, pod.Pos.Y] = pod;
            }
            foreach (RobotUnderPod robotUnderPod in _IMSData.EntityData.RobotUnderPodData)
            {
                table[robotUnderPod.Pos.X, robotUnderPod.Pos.Y] = robotUnderPod;
            }
            foreach (Destination destination in _IMSData.EntityData.DestinationData)
            {
                table[destination.Pos.X, destination.Pos.Y] = destination;
            }
            foreach (Dock dock in _IMSData.EntityData.DockData)
            {
                table[dock.Pos.X, dock.Pos.Y] = dock;
            }

            return table;
        }

        public void AdvanceTime()
        {

            if (SimulationFinished)
            {
                int max = -1;
                foreach (KeyValuePair<Robot, List<Pos>> entry in routes)
                {
                    //find longest path
                    if (entry.Value.Count() > max)
                    {
                        max = entry.Value.Count();
                    }
                }
                if (_counter <= max)
                {
                    //rotate
                    foreach (var item in rotations)
                    {
                        if (item.Value.Count() > _counter)
                        {
                            item.Key.Rotate(item.Value[_counter]);
                        }

                    }
                    //move
                    foreach (var item in routes)
                    {
                        if (item.Value.Count() > _counter)
                        {
                            item.Key.Move(item.Value[_counter]);
                        }

                    }
                    _steps++;
                    OnTableChanged();
                    _counter++;

                }
            }
            _time++;
            OnTimePassed(_time);

        }

        public void Simulation()
        {
            //random table
            //example

            Robot Robot1 = new Robot(0, 0, Direction.UP, 1000, 1000, 1);
            Robot Robot2 = new Robot(0, 10, Direction.UP, 1000, 1000, 1);
            //Robot Robot3 = new Robot(3, 3, Direction.UP, 1000, 1000, 1);
            //Robot Robot4 = new Robot(9, 9, Direction.UP, 1000, 1000, 1);
            _IMSData.EntityData.RobotData.Add(Robot1);
            _IMSData.EntityData.RobotData.Add(Robot2);
            //_IMSData.EntityData.RobotData.Add(Robot3);
            //_IMSData.EntityData.RobotData.Add(Robot4);
            Dictionary<Int32, Int32> asd = new Dictionary<Int32, Int32>() { { 2, 1 } };
            Dictionary<Int32, Int32> asd2 = new Dictionary<Int32, Int32>() { { 1, 1 } };
            //Dictionary<Int32, Int32> asd3 = new Dictionary<Int32, Int32>() { { 3, 1 } };
            //Dictionary<Int32, Int32> asd4 = new Dictionary<Int32, Int32>() { { 4, 1 } };
            Pod pod1 = new Pod(0, 9, asd);
            Pod pod2 = new Pod(0, 1, asd2);
            //Pod pod3 = new Pod(9, 1, asd3);
            //Pod pod4 = new Pod(1, 4, asd4);
            _IMSData.EntityData.PodData.Add(pod1);
            _IMSData.EntityData.PodData.Add(pod2);
            //_IMSData.EntityData.PodData.Add(pod3);
            //_IMSData.EntityData.PodData.Add(pod4);
            Destination dest1 = new Destination(11, 0, 1);
            Destination dest2 = new Destination(3, 11, 2);
            //Destination dest3 = new Destination(6, 4, 4);
            //Destination dest4 = new Destination(5, 3, 1);
            _IMSData.EntityData.DestinationData.Add(dest1);
            _IMSData.EntityData.DestinationData.Add(dest2);
            //_IMSData.EntityData.DestinationData.Add(dest3);
            //_IMSData.EntityData.DestinationData.Add(dest4);
            Dock dock1 = new Dock(4, 3);
            Dock dock2 = new Dock(6, 10);
            _IMSData.EntityData.DockData.Add(dock1);
            _IMSData.EntityData.DockData.Add(dock2);

            routes = new Dictionary<Robot, List<Pos>>();
            _counter = 0;
            OnTableChanged();
            OnSimulationStarted();
            cbs = new ConflictBasedSearch(IMSData);
            routes = cbs.CheckConflicts();
            rotations = cbs.Rotations;
            SimulationFinished = true;
        }

        #endregion

        #region Event triggers

        /// <summary>
        /// Játék kezdetének eseménykiváltása.
        /// </summary>
        private void OnSimulationStarted()
        {
            if (SimulationStarted != null)
                SimulationStarted(this, EventArgs.Empty);
        }

        private void OnSimulationCreated()
        {
            if (SimulationCreated != null)
                SimulationCreated(this, EventArgs.Empty);
        }
        private void OnTableCreated()
        {
            if (TableCreated != null)
                TableCreated(this, EventArgs.Empty);
        }

        /// <summary>
        /// Játék végének eseménykiváltása.
        /// </summary>
        private void OnSimulationOver()
        {
            if (SimulationOver != null)
                SimulationOver(this, EventArgs.Empty);
        }
        /// <summary>
        /// Sebesség változásának eseménykiváltása.
        /// </summary>
        /// <param name="speed">A sebesség.</param>
        private void OnStepsChanged()
        {
            if (SimulationWon != null)
                StepsChanged(this, EventArgs.Empty);
        }
        /// <summary>
        /// Sebesség változásának eseménykiváltása.
        /// </summary>
        /// <param name="speed">A sebesség.</param>
        private void OnAllEnergyChanged()
        {
            if (SimulationWon != null)
                EnergyChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// Sebesség változásának eseménykiváltása.
        /// </summary>
        /// <param name="speed">A sebesség.</param>
        private void OnSpeedChanged(Int32 speed)
        {
            if (SimulationWon != null)
                SpeedChanged(this, new SpeedChangedEventArgs(speed));
        }
        /// <summary>
        /// Idő múlásának eseménykiváltása.
        /// </summary>
        /// <param name="time">Az idő.</param>
        private void OnTimePassed(Int32 time)
        {
            if (SimulationWon != null)
                TimePassed(this, new TimePassedEventArgs(time));
        }
        /// <summary>
        /// Mezőváltozás eseménykiváltása.
        /// </summary>
        /// <param name="x">Oszlop index.</param>
        /// <param name="y">Sor index.</param>
        /// <param name="Entity">Játékos.</param>

        private void OnTableChanged()
        {
            _gameTable = _extractTableFromIMSData();
            if (TableChanged != null)
                TableChanged(this, EventArgs.Empty);
        }
        #endregion

    }

}
