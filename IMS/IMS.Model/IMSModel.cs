using System;
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
        private EntityData _entityData;
        private IMSDataAccess _dataAccess; // adatelérés

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

        #endregion

        #region Events

        public event EventHandler<EventArgs> SimulationStarted;
        public event EventHandler<EventArgs> SimulationOver;
        public event EventHandler<EventArgs> SimulationCreated; //either loaded or created
        public event EventHandler<EventArgs> TableCreated; //created empty table on create mode
        public event EventHandler<FieldChangedEventArgs> FieldChanged;


        #endregion

        #region Constructors

        public IMSModel() : this(null) { }

        public IMSModel(IMSDataAccess dataAccess)
        {
            _gameTable = new Entity[12, 12];
            _dataAccess = dataAccess;

            //NewSimulation();
        }

        #endregion

        #region Public methods

        public void NewSimulation()
        {
            for (Int32 i = 0; i < _gameTable.GetLength(0); i++)
            {
                for (Int32 j = 0; j < _gameTable.GetLength(1); j++)
                {
                    _gameTable[i, j] = new Empty(i, j);
                }
            }
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


        public void Simulation(Int32 x, Int32 y)
        {

        }

        public async Task LoadSimulationAsync(String path)
        {
            if (_dataAccess == null)
                return;

            
            Entity[] values = await _dataAccess.LoadSimulationAsync(path);

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

            //OnSimulationCreated();

            NewSimulation();

        }

        public async Task SaveSimulationAsync(String path)
        {
            if (_dataAccess == null)
                return;

            Entity[] values = new Entity[_gameTable.Length];
            for (Int32 i = 0; i < _gameTable.GetLength(0); i++)
            {
                for (Int32 j = 0; j < _gameTable.GetLength(1); j++)
                {
                    values[i * _gameTable.GetLength(0) + j] = _gameTable[i, j];
                }
            }
            await _dataAccess.SaveSimulationAsync(path, values);
        }

        #endregion

        #region Private methods

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

        #endregion

    }
}
