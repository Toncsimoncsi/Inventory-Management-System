using System;
using System.Diagnostics;
using System.Threading.Tasks;
using IMS.Persistence;
using IMS.Persistence.Entities;

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

        private int _steps;
        private int _allEnergy;
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
        public Int32 TempSizeX { get { return _tempTable.GetLength(0); } set { } }
        public Int32 TempSizeY { get { return _tempTable.GetLength(1); } set { } }
        public int Steps { get { return _steps; } }
        public int AllEnergy { get { return _allEnergy; } }

        #endregion

        #region Events

        public event EventHandler<EventArgs> SimulationStarted;
        public event EventHandler<EventArgs> SimulationOver;
        public event EventHandler<EventArgs> SimulationCreated; //either loaded or created
        public event EventHandler<EventArgs> TableCreated; //created empty table on create mode
        public event EventHandler<FieldChangedEventArgs> FieldChanged;

        public event EventHandler<EventArgs> TableCreated_SVM;
        public event EventHandler<FieldChangedEventArgs> FieldChanged_SVM;
        public event EventHandler<SelectionChangedEventArgs> SelectionChanged_SVM;


        #endregion

        #region Constructors

        public IMSModel() : this(null) { }

        public IMSModel(IMSDataAccess dataAccess)
        {
            _gameTable = new Entity[12, 12];
            _dataAccess = dataAccess;

            _steps = 0;
            _allEnergy = 0;

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

            _gameTable = _createEmptyTable(_gameTable.GetLength(0),_gameTable.GetLength(1));
            OnSimulationCreated();
        }

        public void RelocationAttempt(int x1, int y1, int x2, int y2, int x, int y)
        {
            Debug.WriteLine("RelocationAttempt called");
            Debug.WriteLine("x1: "+x1+", y1: "+y1+", x2: "+x2+", y2: "+y2+", x: "+x+", y: "+y);
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

        public Entity GetTemp(Int32 x, Int32 y)
        {
            return _tempTable[x, y];
        }

        //makes _tempTable the official table (_gameTable) and creates a new IMSData
        public void CreateTableFromSettings()
        {
            _IMSData = new IMSData(_tempTable);
            _gameTable = _tempTable;
        }


        public void Simulation(Int32 x, Int32 y)
        {

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

            foreach (Robot robot in _IMSData.EntityData.RobotData){
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
        /// Mezőváltozás eseménykiváltása.
        /// </summary>
        /// <param name="x">Oszlop index.</param>
        /// <param name="y">Sor index.</param>
        /// <param name="Entity">Játékos.</param>
        private void OnFieldChanged(Int32 x, Int32 y, Entity Entity)
        {
            if (FieldChanged != null)
                FieldChanged(this, new FieldChangedEventArgs(x, y, Entity));
        }

        private void OnFieldChanged_SVM(Int32 x, Int32 y, Entity Entity)
        {
            if (FieldChanged_SVM != null)
                FieldChanged_SVM(this, new FieldChangedEventArgs(x, y, Entity));
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
