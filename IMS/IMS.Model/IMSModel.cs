using System;
using System.Diagnostics;
using System.Threading.Tasks;
using IMS.Persistence;
using IMS.Persistence.Entities;
using IMS.Model.Simulation;

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
        private PathFinder _pathFinder;

        private int _steps;
        private int _allEnergy;
        private int _speed;


        #endregion

        #region Public properties

        /// <summary>
        /// Lépésszám lekérdezése.
        /// </summary>
        //public Int32 StepNumber { get { return _stepNumber; } }

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

        public Int32 Speed { get; set; }
        public int Steps { get { return _steps; } }
        public int AllEnergy { get { return _allEnergy; } }
        public int Speed { get { return _speed; } }

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
        public event EventHandler<RobotMovedEventArgs> FieldChanged;

        /// <summary>
        /// Simulation végének eseménye.
        /// </summary>
        public event EventHandler SpeedChanged;


        /// <summary>
        /// Simulation végének eseménye.
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
            _pathFinder = new PathFinder(_IMSData);
            _steps = 0;
            _allEnergy = 0;
<<<<<<< HEAD
            _speed = 5;
=======
            Speed = 1;
>>>>>>> 1cd091a0c33e378e207be20d0c88309fe9181ed0

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


        public void Simulation()
        {
            OnSimulationStarted();

            _pathFinder.moveRobotsStartToPod();
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
        private void OnFieldChanged(Int32 x, Int32 y, Entity Entity)
        {
            if (FieldChanged != null)
                FieldChanged(this, new RobotMovedEventArgs(x, y, Entity));
        }

        #endregion

    }

}
