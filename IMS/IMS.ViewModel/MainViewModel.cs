using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using IMS.Persistence.Entities;
using IMS.Model;
using IMS.ViewModel.Fields;
using System.Diagnostics;

namespace IMS.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        #region Private Fields

        private IMSModel _model;
        private Dictionary<EntityType, String> _entityToColor;

        private ViewModelBase _currentView;
        private string _entityInfo;
        private int _speedText;

        #endregion

        #region Properties
        public ViewModelBase CurrentView
        {
            get { return _currentView; }
            set
            {
                if (value != _currentView)
                {
                    _currentView = value;
                    OnPropertyChanged(nameof(CurrentView));
                }
            }
        }

        public string EntityInfo
        {
            get { return _entityInfo; }
            set
            {
                _entityInfo = value;
            }
        }

        public int STEPS
        {
            get { return _model.Steps; }
            private set { }
        }
        public int ALLENERGY
        {
            get { return _model.AllEnergy; }
            private set { }
        }

        public int SpeedText
        {
            get { return _model.Speed; }
            set
            {
                _speedText = value;
                OnPropertyChanged();
            }
        }
        public DelegateCommand LoadSimulationCommand { get; private set; }
        public DelegateCommand OpenSettingsCommand { get; private set; }
        public DelegateCommand SaveSimulationCommand { get; private set; }
        public DelegateCommand SaveDiaryCommand { get; private set; }
        public DelegateCommand StartStopCommand { get; private set; }
        public DelegateCommand SlowerCommand { get; private set; }
        public DelegateCommand FasterCommand { get; private set; }
        public DelegateCommand StopCommand { get; private set; }
        public DelegateCommand ViewField { get; private set; }
        public DelegateCommand ModifyField { get; private set; }
        public ObservableCollection<TableField> Fields { get; private set; }
        public Int32 SizeX { get { return _model.SizeX; } }
        public Int32 SizeY { get { return _model.SizeY; } }

        #endregion


        #region Events

        public event EventHandler LoadSimulation;
        //public event EventHandler CreateSimulation;
        public event EventHandler OpenSettings;
        public event EventHandler SaveSimulation;
        public event EventHandler SaveDiary;
        public event EventHandler StartStop;
        public event EventHandler ClickedOnTable;
        public event EventHandler SpeedUp;
        public event EventHandler SpeedDown;

        #endregion


        #region Constructors

        public MainViewModel(IMSModel model)
        {
            _entityToColor = new Dictionary<EntityType, String>();
            _entityToColor.Add(EntityType.Destination, "Green");
            _entityToColor.Add(EntityType.Robot, "Yellow");
            _entityToColor.Add(EntityType.Dock, "Blue");
            _entityToColor.Add(EntityType.Empty, "White");
            _entityToColor.Add(EntityType.Pod, "Gray");
            _entityToColor.Add(EntityType.RobotUnderPod, "Purple");

            Fields = new ObservableCollection<TableField>();

            _model = model;
            _model.SimulationCreated += new EventHandler<EventArgs>(Model_SimulationCreated);

            LoadSimulationCommand = new DelegateCommand(param => OnLoadSimulation());

            //CreateSimulationCommand = new DelegateCommand(param => OnCreateSimulation());
            SaveSimulationCommand = new DelegateCommand(param => OnSaveSimulation());
            SaveDiaryCommand = new DelegateCommand(param => OnSaveDiary());
            StartStopCommand = new DelegateCommand(param => OnStartStopSimulation());
            OpenSettingsCommand = new DelegateCommand(param => OnOpenSettings());
            FasterCommand = new DelegateCommand(param => OnSpeedUp());
            SlowerCommand = new DelegateCommand(param => OnSpeedDown());
            //ViewField = new DelegateCommand(param => OnFieldClicked());

            _speedText = _model.Speed;

        }

        #endregion


        #region Methods

        private String EntityToColor(EntityType type)
        {
            return _entityToColor[type];
        }

        /// <summary>
        /// creates an empty viewmodel table (Fields)
        /// </summary>
        private void GenerateTable()
        {
            Fields.Clear();
            for (Int32 i = 0; i < SizeX; ++i)
            {
                for (Int32 j = 0; j < SizeY; ++j)
                {
                    Fields.Add(new TableField
                    {
                        X = i,
                        Y = j,
                        Color = EntityToColor(EntityType.Empty),
                        Direction = Direction.NONE.ToString(),
                        Number = i * SizeX + j,
                        ViewField = new DelegateCommand(param => ViewFieldInfo(Convert.ToInt32(param)))
                    });
                }
            }
            OnPropertyChanged(nameof(SizeX));
            OnPropertyChanged(nameof(SizeY));
            //OnPropertyChanged(nameof(Fields));
        }

        /// <summary>
        /// syncs the model's table with a pre-existing
        /// viewmodel table (Fields)
        /// </summary>
        private void SetupTable()
        {
            foreach (TableField field in Fields)
            {
                field.Color = EntityToColor(_model[field.X, field.Y].Type);
                if (field.Color != "White")
                {
                    Debug.WriteLine("non-white field found, should be displayed");
                }
                field.Direction = _model[field.X, field.Y].Direction.ToString();
            }
            //OnPropertyChanged(nameof(Fields));
        }

        private void ViewFieldInfo(int ind)
        {
            TableField field = Fields[ind];

            switch (field.Color)
            {
                case "Yellow":
                    _entityInfo = "Robot info\n";
                    _entityInfo += "Battery: " + "\n";
                    _entityInfo += "Direction: " + field.Direction + "\n";
                    _entityInfo += "Consumed energy: " + "0" + "\n";
                    break;
                case "Green":
                    _entityInfo = "Destination\n";
                    break;
                case "Blue":
                    _entityInfo = "Dock\n";
                    break;
                case "Gray":
                    _entityInfo = "Pod info\n";
                    _entityInfo += "Pod content: " + "\n";
                    break;
                case "Purple":
                    _entityInfo = "Robot under pod info\n";
                    _entityInfo += "Robot battery: " + "\n";
                    _entityInfo += "Robot direction: " + field.Direction + "\n";
                    _entityInfo += "Consumed energy: " + "0" + "\n";
                    _entityInfo += "Pod content: " + "\n";
                    break;
                case "White":
                    _entityInfo = "Empty!\n";
                    break;
            }
            _entityInfo += "Row: " + field.X.ToString() + "\nColumn: " + field.Y.ToString();

            if (ClickedOnTable != null)
                ClickedOnTable(this, EventArgs.Empty);

            //az entitydatabl kiszedni az infokat?
        }

        private void Model_SimulationCreated(Object sender, EventArgs e)
        {
            //Debug.WriteLine("Model_SimulationCreated called in viewmodel");
            GenerateTable();
            SetupTable();
        }

        public void CreateSimulationFromSettingsWindow()
        {
            GenerateTable();
            SetupTable();
        }
        #endregion

        #region Event methods

        private void OnOpenSettings()
        {
            if (OpenSettings != null)
                OpenSettings(this, EventArgs.Empty);
        }

        private void OnFieldClicked()
        {
            if (ClickedOnTable != null)
                ClickedOnTable(this, EventArgs.Empty);
        }

        private void OnLoadSimulation()
        {
            if (LoadSimulation != null)
                LoadSimulation(this, EventArgs.Empty);
        }
        private void OnSaveSimulation()
        {
            if (SaveSimulation != null)
                SaveSimulation(this, EventArgs.Empty);
        }

        private void OnSaveDiary()
        {
            if (SaveDiary != null)
                SaveDiary(this, EventArgs.Empty);
        }

        private void OnSpeedUp()
        {
            if (SpeedUp != null)
                SpeedUp(this, EventArgs.Empty);
        }

        private void OnSpeedDown()
        {
            if (SpeedDown != null)
                SpeedDown(this, EventArgs.Empty);
        }


        private void OnStartStopSimulation()
        {
            if (StartStop != null)
                StartStop(this, EventArgs.Empty);
        }

        #endregion
    }

}
