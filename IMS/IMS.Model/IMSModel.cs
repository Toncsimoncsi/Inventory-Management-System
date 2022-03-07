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

        //private IMSDataAccess _dataAccess;

        //public IMSModel(IMSDataAccess dataAccess)
        //{
        //    _dataAccess = dataAccess;
        //}

        private Entity _currentEntity; // aktuális játékos
        private Entity[,] _gameTable; // simtábla
        private Int32 _stepNumber; // lépésszám
        private EntityData _entityData;
        private IMSDataAccess _dataAccess; // adatelérés

        #endregion

        #region Public properties

        /// <summary>
        /// Lépésszám lekérdezése.
        /// </summary>
        public Int32 StepNumber { get { return _stepNumber; } }

        /// <summary>
        /// Mezőérték lekérdezése, vagy beállítása.
        /// </summary>
        /// <param name="x">Oszlop index.</param>
        /// <param name="y">Sor index.</param>
        /// <returns>A mező értéke.</returns>
        public Entity this[Int32 x, Int32 y]
        {
            get
            {
                if (x < 0 || x > _gameTable.GetLength(0)) // ellenőrizzük a tartományt
                    throw new ArgumentException("Bad column index.", "x");
                if (y < 0 || y > _gameTable.GetLength(1))
                    throw new ArgumentException("Bad row index.", "y");

                return _gameTable[x, y];
            }
        }

        public Int32 SizeX { get { return _gameTable.GetLength(0); } }

        public Int32 SizeY { get { return _gameTable.GetLength(1); } }

        #endregion

        #region Events

        /// <summary>
        ///  Játék kezdetének eseménye.
        /// </summary>
        public event EventHandler GameStarted;

        /// <summary>
        /// Játék végének eseménye.
        /// </summary>
        public event EventHandler GameOver;

        /// <summary>
        /// Játék megnyerésének eseménye.
        /// </summary>
        public event EventHandler<GameWonEventArgs> GameWon;

        /// <summary>
        /// Mezőváltozás eseménye.
        /// </summary>
        public event EventHandler<FieldChangedEventArgs> FieldChanged;


        #endregion

        #region Constructors

        /// <summary>
        /// Tic-Tac-Toe játék modell példányosítása.
        /// </summary>
        public IMSModel() : this(null) { }

        /// <summary>
        /// Tic-Tac-Toe játék modell példányosítása.
        /// </summary>
        /// <param name="dataAccess">Az adatelérés.</param>
        public IMSModel(IMSDataAccess dataAccess)
        {
            _gameTable = new Entity[12, 12]; // mátrix létrehozása
            _dataAccess = dataAccess;

            NewGame();
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Új játék indítása.
        /// </summary>
        public void NewGame()
        {
            for (Int32 i = 0; i < _gameTable.GetLength(0); i++) // végigmegyünk a mátrix elemein
                for (Int32 j = 0; j < _gameTable.GetLength(1); j++)
                {
                _gameTable[i, j] = new Empty(i,j) ; // a játékosok pozícióit töröljük
                }

            _stepNumber = 0;

            OnGameStarted();
        }
        /// <summary>
        /// Játék léptetése.
        /// </summary>
        /// <param name="x">Oszlop index.</param>
        /// <param name="y">Sor index.</param>
        public void Simulation(Int32 x, Int32 y)
        {
            //CheckGame();
        }

        /// <summary>
        /// Játék léptetése.
        /// </summary>
        /// <param name="x">Oszlop index.</param>
        /// <param name="y">Sor index.</param>
        public void StepGame(Int32 x, Int32 y)
        {
            if (x < 0 || x > _gameTable.GetLength(0))
                throw new ArgumentOutOfRangeException("x", "Bad column index.");
            if (y < 0 || y > _gameTable.GetLength(1))
                throw new ArgumentOutOfRangeException("y", "Bad row index.");
            if (_stepNumber >= 9) // ellenőrizzük a lépésszámot
                throw new InvalidOperationException("Game is over!");

            OnFieldChanged(x, y, _currentEntity); // jelezzük egy eseménykiváltással, hogy változott a mező

            _stepNumber++;
        }

        /// <summary>
        /// Játék betöltése.
        /// </summary>
        /// <param name="path">Elérési útvonal.</param>
        public async Task LoadGameAsync(String path)
        {
            if (_dataAccess == null)
                return;

            // végrehajtjuk a betöltést
            Entity[] values = await _dataAccess.LoadAsync(path);

            if (values.Length != _gameTable.Length)
                throw new IMSDataException("Error occurred during game loading.");

            // beállítjuk az értékeket
            for (Int32 i = 0; i < _gameTable.GetLength(0); i++)
                for (Int32 j = 0; j < _gameTable.GetLength(1); j++)
                {
                    _gameTable[i, j] = values[i * _gameTable.GetLength(0) + j];

                    OnFieldChanged(i, j, _gameTable[i, j]);
                }

            OnGameStarted();

            //CheckGame();
        }

        /// <summary>
        /// Játék mentése.
        /// </summary>
        /// <param name="path">Elérési útvonal.</param>
        public async Task SaveGameAsync(String path)
        {
            if (_dataAccess == null)
                return;

            // az értékeket kimásoljuk egy új tömbbe
            Entity[] values = new Entity[_gameTable.Length];
            for (Int32 i = 0; i < _gameTable.GetLength(0); i++)
                for (Int32 j = 0; j < _gameTable.GetLength(1); j++)
                {
                    values[i * _gameTable.GetLength(0) + j] = _gameTable[i, j];
                }

            // végrehajtjuk a mentést
            await _dataAccess.SaveAsync(path, values);
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Játék ellenőrzése.
        /// </summary>/
        /*
        private void CheckGame()
        {
            Entity won = Entity.NoEntity;

            for (int i = 0; i < 3; ++i) // ellenőrzések végrehajtása
            {
                if (_gameTable[i, 0] != 0 && _gameTable[i, 0] == _gameTable[i, 1] && _gameTable[i, 1] == _gameTable[i, 2])
                    won = _gameTable[i, 0];
            }
            for (int i = 0; i < 3; ++i)
            {
                if (_gameTable[0, i] != 0 && _gameTable[0, i] == _gameTable[1, i] && _gameTable[1, i] == _gameTable[2, i])
                    won = _gameTable[0, i];
            }
            if (_gameTable[0, 0] != 0 && _gameTable[0, 0] == _gameTable[1, 1] && _gameTable[1, 1] == _gameTable[2, 2])
                won = _gameTable[0, 0];
            if (_gameTable[0, 2] != 0 && _gameTable[0, 2] == _gameTable[1, 1] && _gameTable[1, 1] == _gameTable[2, 0])
                won = _gameTable[0, 2];

            if (won != Entity.NoEntity) // ha valaki győzött
            {
                OnGameWon(won); // esemény kiváltása
            }
            else if (_stepNumber == 9) // döntetlen játék
            {
                OnGameOver(); // esemény kiváltása
            }
        }
    */
        #endregion

        #region Event triggers

        /// <summary>
        /// Játék kezdetének eseménykiváltása.
        /// </summary>
        private void OnGameStarted()
        {
            if (GameStarted != null)
                GameStarted(this, EventArgs.Empty);
        }

        /// <summary>
        /// Játék végének eseménykiváltása.
        /// </summary>
        private void OnGameOver()
        {
            if (GameOver != null)
                GameOver(this, EventArgs.Empty);
        }

        /// <summary>
        /// Játék megnyerésének eseménykiváltása.
        /// </summary>
        /// <param name="Entity">A győztes játékos.</param>
        private void OnGameWon(Entity Entity)
        {
            if (GameWon != null)
                GameWon(this, new GameWonEventArgs(Entity));
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
