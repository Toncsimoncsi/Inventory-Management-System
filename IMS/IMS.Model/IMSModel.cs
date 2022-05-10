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
        private Entity[,] _tempTable;
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


        private int _selectionX1;
        private int _selectionY1;
        private int _selectionX2;
        private int _selectionY2;


        #endregion

        #region Public properties

        /// <summary>
        /// make the model indexable
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
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
        /// <summary>
        /// width of the sim table
        /// </summary>
        public Int32 SizeX { get { return _gameTable.GetLength(0); } set { } }
        /// <summary>
        /// height of the sim table
        /// </summary>
        public Int32 SizeY { get { return _gameTable.GetLength(1); } set { } }


        public IMSData IMSData { get { return _IMSData; } }

        /// <summary>
        /// width of the creator's table
        /// </summary>
        public Int32 TempSizeX { get { return _tempTable.GetLength(0); } set { } }
        /// <summary>
        /// height of the creator's table
        /// </summary>
        public Int32 TempSizeY { get { return _tempTable.GetLength(1); } set { } }

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

        public event EventHandler<EventArgs> TableCreated_SVM;
        public event EventHandler<RobotMovedEventArgs> FieldChanged_SVM;
        public event EventHandler<SelectionChangedEventArgs> SelectionChanged_SVM;


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


            _selectionX1 = -1;
            _selectionY1 = -1;
            _selectionX2 = -1;
            _selectionY2 = -1;


            //NewSimulation();
        }

        #endregion

        #region Public methods

        /// <summary>
        /// start a new simulation
        /// </summary>
        public void NewSimulation()
        {
            SimulationFinished = false;
            _gameTable = _createEmptyTable(_gameTable.GetLength(0), _gameTable.GetLength(1));
            OnSimulationCreated();
        }

        /// <summary>
        /// attempt to relocate the selected block to a different position (block)
        /// the selected rectangle's points will be translated specified by (x1, y1) -> (x, y)
        /// </summary>
        /// <param name="x1">x of the first corner of the block</param>
        /// <param name="y1">y of the first corner of the block</param>
        /// <param name="x2">x of the second corner of the block</param>
        /// <param name="y2">y of the second corner of the block</param>
        /// <param name="x">x of the translation point</param>
        /// <param name="y">y of the translation point</param>
        public void RelocationAttempt(int x1, int y1, int x2, int y2, int x, int y)
        {
            int dx = x2 - x1;
            int dy = y2 - y1;

            if (x + dx < 0 || x + dx >= TempSizeX || y + dy < 0 || y + dy >= TempSizeY)
            {
                return;
            }

            if (dx < 0)
            {
                x1 = x2;
                x += dx;
                dx = -1 * dx;
            }
            if (dy < 0)
            {
                y1 = y2;
                y += dy;
                dy = -1 * dy;
            }
            if (x1 <= x + dx || x <= x1 + dx || y1 <= y + dy || y <= y1 + dy)
            {
                for (int i = 0; i < dx + 1; ++i)
                {
                    for (int j = 0; j < dy + 1; ++j)
                    {
                        if(_tempTable[x1+i,y1+j].Type == EntityType.Pod && _tempTable[x+i,y+j].Type == EntityType.Empty)
                        {
                            _tempTable[x + i, y + j] = _tempTable[x1 + i, y1 + j];
                            _tempTable[x + i, y + j].Pos.X = x + i;
                            _tempTable[x + i, y + j].Pos.Y = y + j;
                            _tempTable[x1 + i, y1 + j] = new Empty(x1 + i, y1 + j);
                            OnFieldChanged_SVM(x1 + i, y1 + j, _tempTable[x1 + i, y1 + j]);
                            OnFieldChanged_SVM(x + i, y + j, _tempTable[x + i, y + j]);
                        }
                    }
                }



            }
        }

        /// <summary>
        /// add a product to all of the pods and destinations inside the rectangle specified by the parameters
        /// if an entity is a destination, overwrite its ID
        /// if it's a pod, add to its product list
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="productID"></param>
        public void AddProduct(int x1, int y1, int x2, int y2, int productID)
        {
            OnSelectionChanged_SVM(x1, y1, false);
            int yInc = y1 <= y2 ? 1 : -1;
            int xInc = x1 <= x2 ? 1 : -1;
            for (int y = y1; yInc == 1 ? y <= y2 : y >= y2; y += yInc)
            {
                for (int x = x1; xInc == 1 ? x <= x2 : x >= x2; x += xInc)
                {
                    if (_tempTable[x, y].Type == EntityType.Destination)
                    {
                        ((Destination)_tempTable[x, y]).ID = productID;
                        OnFieldChanged_SVM(x, y, _tempTable[x, y]);
                    }
                    if (_tempTable[x, y].Type == EntityType.Pod)
                    {
                        ((Pod)_tempTable[x, y]).Products.Add(productID, 1);
                        OnFieldChanged_SVM(x, y, _tempTable[x, y]);
                    }
                }
            }
        }

        public void AddProductSelection(int x, int y)
        {
            OnSelectionChanged_SVM(x, y, true);
        }

        /// <summary>
        /// first step in selecting the AddProduct rectangle
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void Selection(int x, int y)
        {
            if (_selectionX1 == -1 && _selectionY1 == -1)
            {
                _selectionX1 = x;
                _selectionY1 = y;
                OnSelectionChanged_SVM(x, y, true);
            }
            else
            {
                if (_selectionX1 > x)
                {
                    _selectionX2 = _selectionX1;
                    _selectionX1 = x;
                }
                else
                {
                    _selectionX2 = x;
                }
                if (_selectionY1 > y)
                {
                    _selectionY2 = _selectionY1;
                    _selectionY1 = y;
                }
                else
                {
                    _selectionY2 = y;
                }
                //change multiple fields
                for (int _y = _selectionY1; _y <= _selectionY2; ++_y)
                {
                    for (int _x = _selectionX1; _x <= _selectionX2; ++_x)
                    {
                        OnSelectionChanged_SVM(_x, _y, true);
                    }
                }
            }
            //if only one of the xs or ys is -1, there's an error
        }

        public void EndSelection()
        {
            for (int y = _selectionY1; y <= _selectionY2; ++y)
            {
                for (int x = _selectionX1; x <= _selectionX2; ++x)
                {
                    OnSelectionChanged_SVM(x, y, false);
                }
            }
            _selectionX1 = -1;
            _selectionY1 = -1;
            _selectionX2 = -1;
            _selectionY2 = -1;
        }

        /// <summary>
        /// new, empty table for the creator
        /// </summary>
        /// <param name="x">width</param>
        /// <param name="y">height</param>
        public void GenerateEmtyTableForSettingsWindow(int x, int y)
        {
            _tempTable = new Entity[x, y];
            for (Int32 i = 0; i < _tempTable.GetLength(0); i++)
            {
                for (Int32 j = 0; j < _tempTable.GetLength(1); j++)
                {
                    _tempTable[i, j] = new Empty(i, j);
                }
            }
            OnTableCreated_SVM();
        }

        /// <summary>
        /// get the entity from the creator's table
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public Entity GetTemp(Int32 x, Int32 y)
        {
            return _tempTable[x, y];
        }
        /// <summary>
        /// makes _tempTable the official table (_gameTable) and creates a new IMSData
        /// </summary>
        public void CreateTableFromSettings()
        {
            SimulationFinished = false;
            _IMSData = new IMSData(_tempTable);
            _gameTable = _tempTable;
        }

        public void setSpeed(int n)
        {
            _speed += n;
            OnSpeedChanged(_speed);
        }

        /// <summary>
        /// place an entity down in the creator, put it inside the temporary table
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="type">type of the entity to be placed down</param>
        public void ChangeField(int x, int y, EntityType type)
        {
            switch (type)
            {
                case EntityType.Dock:
                    _tempTable[x,y] = new Dock(x, y);
                    break;
                case EntityType.Destination:
                    _tempTable[x, y] = new Destination(x, y);
                    break;
                case EntityType.Pod:
                    _tempTable[x, y] = new Pod(x, y);
                    break;
                case EntityType.Robot:
                    _tempTable[x, y] = new Robot(x, y);
                    break;
                case EntityType.RobotUnderPod:
                    _tempTable[x, y] = new RobotUnderPod(x, y);
                    break;
            }

            OnFieldChanged_SVM(x, y, _tempTable[x, y]);
        }

        /// <summary>
        /// the original function with extra inputs for the robot capacity and destination ID
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="type"></param>
        /// <param name="capacity"></param>
        /// <param name="id"></param>
        public void ChangeField(int x, int y, EntityType type, int capacity, int id)
        {
            if (type == EntityType.Robot && capacity > 0)
            {
                _tempTable[x, y] = new Robot(x, y, capacity, id);
                OnFieldChanged_SVM(x, y, _tempTable[x, y]);
            }
            if(type == EntityType.Destination)
            {
                _tempTable[x, y] = new Destination(x, y, id);
                OnFieldChanged_SVM(x, y, _tempTable[x, y]);
            }
        }

        /// <summary>
        /// rotate the robot clockwise
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void RotateRobot(int x, int y)
        {
            if (_tempTable[x, y].Type == EntityType.Robot && _tempTable[x, y].Direction != Direction.NONE)
            {
                Direction newDirection = _tempTable[x, y].Direction;
                switch (_tempTable[x, y].Direction)
                {
                    case Direction.UP:
                        newDirection = Direction.RIGHT;
                        break;
                    case Direction.RIGHT:
                        newDirection = Direction.DOWN;
                        break;
                    case Direction.DOWN:
                        newDirection = Direction.LEFT;
                        break;
                    case Direction.LEFT:
                        newDirection = Direction.UP;
                        break;
                }
                _tempTable[x, y] = new Robot((Robot)_tempTable[x, y], newDirection);

                OnFieldChanged_SVM(x, y, _tempTable[x, y]);
            }
        }

        /// <summary>
        /// load a previously saved simulation
        /// </summary>
        /// <param name="path">path to the saved simulation (ims)</param>
        /// <returns></returns>
        public async Task LoadSimulationAsync(String path)
        {
            if (_dataAccess == null)
                return;


            _IMSData = await _dataAccess.LoadSimulationAsync(path);
            _gameTable = _extractTableFromIMSData();

            OnSimulationCreated();

            //NewSimulation();

        }

        /// <summary>
        /// save the current state of the simulation
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public async Task SaveSimulationAsync(String path)
        {
            if (_dataAccess == null)
                return;

            await _dataAccess.SaveSimulationAsync(path, _IMSData);
        }

        /// <summary>
        /// save the diary (statistics, total usage etc)
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public async Task SaveDiaryAsync(String path)
        {
            if (_dataAccess == null)
                return;

            _IMSData.StepCount = _steps;

            await _dataAccess.SaveDiaryAsync(path, _IMSData);
        }

        #endregion

        #region Private methods

        /// <summary>
        /// create a table filled with Empty entities
        /// </summary>
        /// <param name="sizeX">width of the new table</param>
        /// <param name="sizeY">height of the new table</param>
        /// <returns></returns>
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

        /// <summary>
        /// 
        /// </summary>
        private void setTotalEnergy()
        {
            _allEnergy = _dataAccess.getTotalEnergy(_IMSData);
            OnAllEnergyChanged();
        }

        /// <summary>
        /// makes a table based on an already existing IMSData
        /// </summary>
        /// <returns></returns>
        private Entity[,] _extractTableFromIMSData()
        {
            Entity[,] table = _createEmptyTable(_IMSData.SizeX, _IMSData.SizeY);

            Debug.WriteLine("created new table, x: "+ _IMSData.SizeX.ToString()+", y: "+ _IMSData.SizeY.ToString());

            foreach (Robot robot in _IMSData.EntityData.RobotData)
            {
                //Debug.WriteLine("putting down robot, x: "+robot.Pos.X.ToString()+", y: "+robot.Pos.Y.ToString());
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

        /// <summary>
        /// calculate the state of the simulation
        /// </summary>
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
                    OnStepsChanged();
                    setTotalEnergy();
                    OnTableChanged();
                    _counter++;

                }
            }
            _time++;
            OnTimePassed(_time);
        }
        /// <summary>
        /// the main function which starts the simulation
        /// </summary>
        public void Simulation()
        {
            //random table
            //example

            //Robot Robot1 = new Robot(0, 0, Direction.UP, 1000, 1000, 1);
            //Robot Robot2 = new Robot(0, 10, Direction.UP, 1000, 1000, 1);
            //Robot Robot3 = new Robot(3, 3, Direction.UP, 1000, 1000, 1);
            //Robot Robot4 = new Robot(9, 9, Direction.UP, 1000, 1000, 1);
            //_IMSData.EntityData.RobotData.Add(Robot1);
            //_IMSData.EntityData.RobotData.Add(Robot2);
            //_IMSData.EntityData.RobotData.Add(Robot3);
            //_IMSData.EntityData.RobotData.Add(Robot4);
            //Dictionary<Int32, Int32> asd = new Dictionary<Int32, Int32>() { { 2, 1 } };
            //Dictionary<Int32, Int32> asd2 = new Dictionary<Int32, Int32>() { { 1, 1 } };
            //Dictionary<Int32, Int32> asd3 = new Dictionary<Int32, Int32>() { { 3, 1 } };
            //Dictionary<Int32, Int32> asd4 = new Dictionary<Int32, Int32>() { { 4, 1 } };
            //Pod pod1 = new Pod(0, 9, asd);
            //Pod pod2 = new Pod(0, 1, asd2);
            //Pod pod3 = new Pod(9, 1, asd3);
            //Pod pod4 = new Pod(1, 4, asd4);
            //_IMSData.EntityData.PodData.Add(pod1);
            //_IMSData.EntityData.PodData.Add(pod2);
            //_IMSData.EntityData.PodData.Add(pod3);
            //_IMSData.EntityData.PodData.Add(pod4);
            //Destination dest1 = new Destination(11, 0, 1);
            //Destination dest2 = new Destination(3, 11, 2);
            //Destination dest3 = new Destination(6, 4, 4);
            //Destination dest4 = new Destination(5, 3, 1);
            //_IMSData.EntityData.DestinationData.Add(dest1);
            //_IMSData.EntityData.DestinationData.Add(dest2);
            //_IMSData.EntityData.DestinationData.Add(dest3);
            //_IMSData.EntityData.DestinationData.Add(dest4);
            //Dock dock1 = new Dock(4, 3);
            //Dock dock2 = new Dock(6, 10);
            //_IMSData.EntityData.DockData.Add(dock1);
            //_IMSData.EntityData.DockData.Add(dock2);
            if (SimulationFinished)
            {
                return;
            }

            routes = new Dictionary<Robot, List<Pos>>();
            _counter = 0;
            OnTableChanged();
            OnSimulationStarted();
            cbs = new ConflictBasedSearch(IMSData);
            routes = cbs.CheckConflicts();
            rotations = cbs.Rotations;
            if (routes!= null)
            {
                SimulationFinished = true;
            }

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
        /// Szimuláció végének eseménykiváltása.
        /// </summary>
        private void OnSimulationOver()
        {
            if (SimulationOver != null)
                SimulationOver(this, EventArgs.Empty);
        }
        /// <summary>
        /// Lépések számának változásának eseménykiváltása.
        /// </summary>
        private void OnStepsChanged()
        {
            if (StepsChanged != null)
                StepsChanged(this, EventArgs.Empty);
        }
        /// <summary>
        /// Energiafogyasztás változásának eseménykiváltása.
        /// </summary>
        private void OnAllEnergyChanged()
        {
            if (EnergyChanged != null)
                EnergyChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// Sebesség változásának eseménykiváltása.
        /// </summary>
        /// <param name="speed">A sebesség.</param>
        private void OnSpeedChanged(Int32 speed)
        {
            if (SpeedChanged != null)
                SpeedChanged(this, new SpeedChangedEventArgs(speed));
        }
        /// <summary>
        /// Idő múlásának eseménykiváltása.
        /// </summary>
        /// <param name="time">Az idő.</param>
        private void OnTimePassed(Int32 time)
        {
            if (TimePassed != null)
                TimePassed(this, new TimePassedEventArgs(time));
        }

        /// <summary>
        /// A tábla megváltozásának eseménykiváltása
        /// </summary>
        private void OnTableChanged()
        {
            _gameTable = _extractTableFromIMSData();
            if (TableChanged != null)
                TableChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// Mezőváltozás eseménykiváltása a szerkesztő felületen.
        /// </summary>
        /// <param name="x">Oszlop index.</param>
        /// <param name="y">Sor index.</param>
        /// <param name="Entity">Játékos.</param>
        private void OnFieldChanged_SVM(Int32 x, Int32 y, Entity Entity)
        {
            if (FieldChanged_SVM != null)
                FieldChanged_SVM(this, new RobotMovedEventArgs(x, y, Entity));
        }

        private void OnSelectionChanged_SVM(Int32 x, Int32 y, Boolean isSelected)
        {
            if (SelectionChanged_SVM != null)
                SelectionChanged_SVM(this, new SelectionChangedEventArgs(x, y, isSelected));
        }

        private void OnTableCreated_SVM()
        {
            if (TableCreated_SVM != null)
                TableCreated_SVM(this, EventArgs.Empty);
        }

        #endregion

    }

}
