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
        private Boolean[,] blocked;
        private List<Dictionary<int, HashSet<Pos>>>[] obstacles;
        //private Dictionary<int, HashSet<Pos>>[] blocked;

        //private Pos[][] routes;
        //private Astar astar;
        //private AstarSpacetime AstarSpaceTime;


        private List<Direction>[] rotations;
        private List<Pos>[] routes;
        private Boolean SimulationFinished;


        private int _steps;
        private int _allEnergy;
        private int _speed;
        private int _time;
        private int _counter1;
        private int _counter2;


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
            _counter1 = 0;
            _counter2 = 0;
            List<Direction>[] rotations = new List<Direction>[IMSData.EntityData.RobotData.Count].Select(item => new List<Direction>()).ToArray();
            List<Dictionary<int, HashSet<Pos>>>[] obstacles = new List<Dictionary<int, HashSet<Pos>>>[IMSData.EntityData.RobotData.Count].Select(item => new List<Dictionary<int, HashSet<Pos>>>()).ToArray();

            _speed = 1;
            _time = 0;
            SimulationFinished = false;
            //astar = new();
            blocked = new Boolean[_gameTable.GetLength(0), _gameTable.GetLength(1)];
            updateBlock();
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
        //check if robot has enough energy to finish task
        private Dock closestDock(Robot robot)
        {
            Dock closestDock = _IMSData.EntityData.DockData[0];
            int shortestDistance = int.MaxValue;
            foreach (Dock dock in _IMSData.EntityData.DockData) // iterate over all docks which is closer
            {
                if (shortestDistance < robot.Pos.Distance(dock.Pos))
                {
                    shortestDistance = robot.Pos.Distance(dock.Pos);
                    closestDock = dock;
                }
            }
            return closestDock;
        }
        // update block array
        private void updateBlock()
        {
            for (int i = 0; i < blocked.GetLength(0); i++)
            {
                for (int j = 0; j < blocked.GetLength(1); j++)
                {
                    blocked[i, j] = true;
                }

            }
            //blocked = new Boolean[IMSData.SizeX][IMSData.SizeY];
            foreach (Robot robot in _IMSData.EntityData.RobotData)
            {
                //int x = robot.Pos.X;
                //int y = robot.Pos.Y;
                //blocked[robot.Pos.X][robot.Pos.Y;] = true;
                blocked[robot.Pos.X, robot.Pos.Y] = false;
            }
        }
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
                int max = routes.Max(x => x.Count);
                if (_counter2 < max)
                {
                    for (int i = 0; i < routes.Length; i++)
                    {
                        if (routes[i].Count < _counter2)
                        {
                            _IMSData.EntityData.RobotData[i].Move(routes[i][_counter2]);
                            _IMSData.EntityData.RobotData[i].Rotate(rotations[i][_counter2]);
                        }
                    }

                    _steps++;
                    OnTableChanged();
                    _counter2++;

                }
                else
                {
                    _counter2 = 0;
                }


            }
            _time++;
            OnTimePassed(_time);

        }


        //run the calculate paths on the table
        private List<Direction> convertTurn(Pos[] route1, Robot robot)
        {
            Direction direction = new Direction();
            List<Direction> directionList = new List<Direction>();
            directionList.Add(robot.Direction); //add robots initial direction
            for (int i = 0; i < route1.Length - 1; i++)
            {
                switch (route1[i + 1].X - route1[i].X + (route1[i + 1].Y - route1[i].Y) * 2) //x coordinate diff(-1 or 1) plus y  coordinate*2 diff(-2 or 2) and 
                {

                    case -1:// down  
                        direction = Direction.DOWN;
                        break;
                    case 1:// up
                        direction = Direction.UP;
                        break;
                    case -2:// left  
                        direction = Direction.LEFT;
                        break;
                    case 2:// right 
                        direction = Direction.RIGHT;
                        break;
                }
                directionList.Add(direction);
            }
            return directionList;
        }
        /*   public void Simulation2()
               {

                   //random table
                   //example
                   Robot Robot1 = new Robot(0, 0, Direction.UP, 1000, 1000, 1);
                   Robot Robot2 = new Robot(11, 11, Direction.UP, 1000, 1000, 1);
                   _IMSData.EntityData.RobotData.Add(Robot1);
                   _IMSData.EntityData.RobotData.Add(Robot2);
                   Destination dest1 = new Destination(11, 0, 1);
                   Destination dest2 = new Destination(11, 11, 1);
                   _IMSData.EntityData.DestinationData.Add(dest1);
                   _IMSData.EntityData.DestinationData.Add(dest2);
                   Dock dock1 = new Dock(6, 2);
                   Dock dock2 = new Dock(6, 9);
                   _IMSData.EntityData.DockData.Add(dock1);
                   _IMSData.EntityData.DockData.Add(dock2);
                   Dictionary<Int32, Int32> asd = new();
                   Pod pod1 = new Pod(6, 2, asd);
                   Pod pod2 = new Pod(9, 6, asd);
                   _IMSData.EntityData.PodData.Add(pod1);
                   _IMSData.EntityData.PodData.Add(pod2);
                   OnTableChanged();
                   OnSimulationStarted();
                   /////////////////
                   //need to not hard assign

                   //make routes and turns
                   for (int i = 0; i < 2; i++) //for every robot
                   {
                       updateBlock();
                       //how much energy required from start to pod
                       List<Pos> routeStartToPod = astar.FindPath(blocked, _IMSData.EntityData.RobotData[i].Pos, _IMSData.EntityData.PodData[i].Pos);
                       int startToPod = routeStartToPod.Count;
                       List<Direction> routeTurnStartToPod = convertTurn(routeStartToPod.ToArray(), _IMSData.EntityData.RobotData[i]);
                       int turnStartToPod = 0;
                       for (int j = 0; j < routeTurnStartToPod.Count - 1; j++)
                       {
                           if (routeTurnStartToPod[j + 1] == routeTurnStartToPod[j])
                           {
                               turnStartToPod++;
                           }
                       }
                       //how much energy required from pod to dest and back
                       List<Pos> routePodToDest = astar.FindPath(blocked, _IMSData.EntityData.PodData[i].Pos, _IMSData.EntityData.DestinationData[i].Pos);
                       int PodToDest = routePodToDest.Count;
                       List<Direction> routeTurnPodToDest = convertTurn(routePodToDest.ToArray(), _IMSData.EntityData.RobotData[i]);
                       int turnPodToDest = 0;
                       for (int j = 0; j < routeTurnPodToDest.Count - 1; j++)
                       {
                           if (routeTurnPodToDest[j + 1] == routeTurnPodToDest[j])
                           {
                               turnPodToDest++;
                           }
                       }
                       if (startToPod + turnStartToPod + PodToDest + (turnPodToDest * 2) < _IMSData.EntityData.RobotData[i].EnergyLeft) //enough charge to go to destination
                       {
                           //not enough
                           moveRobotToDock(_IMSData.EntityData.RobotData[i], closestDock(_IMSData.EntityData.RobotData[i]), i);// to dock
                           moveDockToPod(_IMSData.EntityData.RobotData[i], _IMSData.EntityData.PodData[i], i); //to pod
                           OnTableChanged();
                       }
                       //enough

                       /*for (int j = 0; j < routeStartToPod.Count; j++) //mindegyik robot egymas utan egyet lep
                       {
                          _IMSData.EntityData.RobotData[i].Move(routeStartToPod[j]);
                           _IMSData.EntityData.RobotData[i].Rotate(routeTurnStartToPod[j]);
                          OnTableChanged();
                       };*/
        //RobotUnderPod temp = new RobotUnderPod(_IMSData.EntityData.RobotData[i], _IMSData.EntityData.PodData[i]);
        //_IMSData.EntityData.RobotUnderPodData.Add(temp);
        //routes[i] = astar.FindPath(blocked, _IMSData.EntityData.RobotData[i].Pos, _IMSData.EntityData.PodData[i].Pos); //robot to pod energy enough otherwise error
        //rotations[i] = convertTurn(routes[i].ToArray(), _IMSData.EntityData.RobotData[i]);
        //at pod go to dest and back (hard assign in order)
        //need to implement not hard assign
        //moveRobotPodToDestThenPod(_IMSData.EntityData.RobotData[i], _IMSData.EntityData.DestinationData[i]);
        //}

        //}

        //move robot to pod
        public void Simulation()
        {
            //random table
            //example
            Robot Robot1 = new Robot(0, 0, Direction.UP, 1000, 1000, 1);
            Robot Robot2 = new Robot(0, 11, Direction.UP, 1000, 25, 1);
            _IMSData.EntityData.RobotData.Add(Robot1);
            _IMSData.EntityData.RobotData.Add(Robot2);
            Dictionary<Int32, Int32> asd = new();
            Dictionary<Int32, Int32> asd2 = new();
            Pod pod1 = new Pod(6, 2, asd);
            Pod pod2 = new Pod(6, 6, asd2);
            _IMSData.EntityData.PodData.Add(pod1);
            _IMSData.EntityData.PodData.Add(pod2);
            Destination dest1 = new Destination(11, 0, 1);
            Destination dest2 = new Destination(11, 11, 1);
            _IMSData.EntityData.DestinationData.Add(dest1);
            _IMSData.EntityData.DestinationData.Add(dest2);
            Dock dock1 = new Dock(4, 3);
            Dock dock2 = new Dock(6, 10);
            _IMSData.EntityData.DockData.Add(dock1);
            _IMSData.EntityData.DockData.Add(dock2);


            routes = new List<Pos>[2];
            rotations = new List<Direction>[2];
            routes[0] = new List<Pos>();
            routes[1] = new List<Pos>();
            rotations[0] = new List<Direction>();
            rotations[1] = new List<Direction>();
            _counter1 = 0;
            _counter2 = 0;
            OnTableChanged();
            OnSimulationStarted();
            Dictionary<Robot, Dictionary<int, HashSet<Pos>>> blocked = new Dictionary<Robot, Dictionary<int, HashSet<Pos>>>();
            Constraints cs = new Constraints(blocked);
            PathFinder pf = new PathFinder(IMSData,blocked);
            pf.FindPaths();
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
