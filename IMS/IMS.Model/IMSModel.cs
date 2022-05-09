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

        public Int32 TempSizeX { get { return _tempTable.GetLength(0); } set { } }
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


        public void RelocationAttempt(int x1, int y1, int x2, int y2, int x, int y)
        {
            //Debug.WriteLine("RelocationAttempt called");
            //Debug.WriteLine("x1: "+x1+", y1: "+y1+", x2: "+x2+", y2: "+y2+", x: "+x+", y: "+y);
            int dx = x2 - x1;
            int dy = y2 - y1;

            if (x + dx < 0 || x + dx >= TempSizeX || y + dy < 0 || y + dy >= TempSizeY)
            {
                //this means the other corner of the rectangle would be outside the map
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
            //Debug.WriteLine("x1: " + x1 + ", y1: " + y1 + ", x: " + x + ", y: " + y + ", dx: " + dx + ", dy: " + dy);
            //now: dx is the width, dy the height; x1, y1 the upper right corner of the 1st rectangle, x, y are the 2nd rect's pos

            //if there's an overlap between them, it will fail
            //TODO: maybe <= is needed. Check
            if (x1 <= x + dx || x <= x1 + dx || y1 <= y + dy || y <= y1 + dy)
            {
                //no overlap
                for (int i = 0; i < dx + 1; ++i)
                {
                    for (int j = 0; j < dy + 1; ++j)
                    {
                        //Debug.WriteLine(_tempTable[x1 + i, y1 + j].Type.ToString());
                        //Debug.WriteLine(i.ToString()+", "+j.ToString());
                        if(_tempTable[x1+i,y1+j].Type == EntityType.Pod && _tempTable[x+i,y+j].Type == EntityType.Empty)
                        {
                            //Debug.WriteLine("switching");
                            //swap only needs to happen in this case
                            _tempTable[x + i, y + j] = _tempTable[x1 + i, y1 + j];
                            _tempTable[x + i, y + j].Pos.X = x + i;
                            _tempTable[x + i, y + j].Pos.Y = y + j;
                            _tempTable[x1 + i, y1 + j] = new Empty(x1 + i, y1 + j);
                            OnFieldChanged_SVM(x1 + i, y1 + j, _tempTable[x1 + i, y1 + j]);
                            OnFieldChanged_SVM(x + i, y + j, _tempTable[x + i, y + j]);
                            //Debug.WriteLine("done switching");
                        }
                    }
                }



            }
            //else
            //{
                //overlap detected: abort
            //    return;
            //}

            //signal for the creation of a new table
        }

        public void AddProduct(int x1, int y1, int x2, int y2, int productID)
        {
            OnSelectionChanged_SVM(x1, y1, false);
            /*
            for (int y = y1; y <= y2; ++y)
            {
                for (int x = x1; x <= x2; ++x)
                {
                    if (_tempTable[x,y].Type == EntityType.Destination)
                    {
                        ((Destination)_tempTable[x, y]).ID = productID;
                        OnFieldChanged_SVM(x, y, _tempTable[x,y]);
                    }
                    if (_tempTable[x, y].Type == EntityType.Pod)
                    {
                        ((Pod)_tempTable[x, y]).Products.Add(productID,1);
                        OnFieldChanged_SVM(x, y, _tempTable[x,y]);
                    }
                }
            }
            */
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

        public void Selection(int x, int y)
        {
            if (_selectionX1 == -1 && _selectionY1 == -1)
            {
                _selectionX1 = x;
                _selectionY1 = y;
                //change one field
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
            //this doesn't test it if it's not used correctly
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

        /*public void ClearSettingsWindow()
        {
            _tempTable = new Entity[TempSizeX, TempSizeY];
            for (int i = 0; i < TempSizeX; ++i)
            {
                for (int j = 0; j < TempSizeY; ++j)
                {
                    OnFieldChanged_SVM(i, j, _tempTable[i, j]);
                }
            }
        }
        */

        public Entity GetTemp(Int32 x, Int32 y)
        {
            return _tempTable[x, y];
        }

        //makes _tempTable the official table (_gameTable) and creates a new IMSData
        public void CreateTableFromSettings()
        {
            _IMSData = new IMSData(_tempTable);
            //Debug.WriteLine("new IMSData created from _tempTable");
            _gameTable = _tempTable;
        }

        public void setSpeed(int n)
        {
            _speed += n;
            OnSpeedChanged(_speed);
        }


        //note: empty (blank) entity is placed there of the specified type
        public void ChangeField(int x, int y, EntityType type)
        {
            //Debug.WriteLine("ChangeField: "+x.ToString()+", "+y.ToString());
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

            //Debug.WriteLine("ChangeField called");

            OnFieldChanged_SVM(x, y, _tempTable[x, y]);
        }

        public void ChangeField(int x, int y, EntityType type, int capacity)
        {
            if (type == EntityType.Robot && capacity > 0)
            {
                _tempTable[x, y] = new Robot(x, y, capacity);
                OnFieldChanged_SVM(x, y, _tempTable[x, y]);
            }
            //if it's not a robot then do nothing
        }

        public void RotateRobot(int x, int y) // clockwise rotation
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
                //_tempTable[x, y] = new Robot(x, y, newDirection, ((Robot)_tempTable[x, y]).Capacity, );
                _tempTable[x, y] = new Robot((Robot)_tempTable[x, y], newDirection);

                OnFieldChanged_SVM(x, y, _tempTable[x, y]);
            }
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

        public async Task SaveDiaryAsync(String path)
        {
            if (_dataAccess == null)
                return;

            _IMSData.StepCount = _steps;

            await _dataAccess.SaveDiaryAsync(path, _IMSData);
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

        private void setTotalEnergy()
        {
            _allEnergy = _dataAccess.getTotalEnergy(_IMSData);
            OnAllEnergyChanged();
        }

        private Entity[,] _extractTableFromIMSData()
        {
            //Entity[,] table = new Entity[_IMSData.SizeX, _IMSData.SizeY];

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

        public void Simulation_old()
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
            if (StepsChanged != null)
                StepsChanged(this, EventArgs.Empty);
        }
        /// <summary>
        /// Sebesség változásának eseménykiváltása.
        /// </summary>
        /// <param name="speed">A sebesség.</param>
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
