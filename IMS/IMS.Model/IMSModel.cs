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


        #endregion

        #region Constructors

        public IMSModel() : this(null) { }

        public IMSModel(IMSDataAccess dataAccess)
        {
            _gameTable = new Entity[12, 12];
            _dataAccess = dataAccess;

            _steps = 0;
            _allEnergy = 0;

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

            Debug.WriteLine("ChangeField called");

            OnFieldChanged_SVM(x, y, _tempTable[x, y]);
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

        private void OnTableCreated_SVM()
        {
            if (TableCreated_SVM != null)
                TableCreated_SVM(this, EventArgs.Empty);
        }

        #endregion

    }
}
