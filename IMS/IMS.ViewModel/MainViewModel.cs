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

        public int STEPS { 
            get { return _model.Steps; }
            private set { }
        }
        public int ALLENERGY {
            get { return _model.AllEnergy; }
            private set { }
        }
        public DelegateCommand LoadSimulationCommand { get; private set; }
        public DelegateCommand OpenSettingsCommand { get; private set; }
        public DelegateCommand SaveSimulationCommand { get; private set; }
        public DelegateCommand SaveDiaryCommand { get; private set; }
        public DelegateCommand ExitCommand { get; private set; }
        public DelegateCommand SlowerCommand { get; private set; }
        public DelegateCommand FasterCommand { get; private set; }
        public DelegateCommand StopCommand { get; private set; }
        //public DelegateCommand ViewField { get; private set; }
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
        public event EventHandler ExitSimulation;
        public event EventHandler ClickedOnTable;

        #endregion


        #region Constructors

        public MainViewModel(IMSModel model)
        {
            _entityToColor = new Dictionary<EntityType, String>();
            _entityToColor.Add(EntityType.Destination,"Green");
            _entityToColor.Add(EntityType.Robot,"Yellow");
            _entityToColor.Add(EntityType.Dock,"Blue");
            _entityToColor.Add(EntityType.Empty,"White");
            _entityToColor.Add(EntityType.Pod,"Gray");
            _entityToColor.Add(EntityType.RobotUnderPod,"Purple");

            Fields = new ObservableCollection<TableField>();

            _model = model;
            _model.SimulationCreated += new EventHandler<EventArgs>(Model_SimulationCreated);

            LoadSimulationCommand = new DelegateCommand(param => OnLoadSimulation());

            //CreateSimulationCommand = new DelegateCommand(param => OnCreateSimulation());
            SaveSimulationCommand = new DelegateCommand(param => OnSaveSimulation());
            SaveDiaryCommand = new DelegateCommand(param => OnSaveDiary());
            ExitCommand = new DelegateCommand(param => OnExitSimulation());
            OpenSettingsCommand = new DelegateCommand(param => OnOpenSettings());
            //ViewField = new DelegateCommand(param => OnFieldClicked());

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
                        Type = EntityType.Empty,
                        Entity = _model[i, j],
                        Number = i * SizeX + j,
                        ViewFieldCommand = new DelegateCommand(param => ViewFieldInfo(Convert.ToInt32(param)))
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
                field.Color = EntityToColor(_model[field.X,field.Y].Type);
                field.Direction = _model[field.X, field.Y].Direction.ToString();
                field.Type = _model[field.X, field.Y].Type;
                field.Entity = _model[field.X, field.Y];
            }
            //OnPropertyChanged(nameof(Fields));
        }

        private void ViewFieldInfo(int ind)
        {
            TableField field = Fields[ind];

            switch (field.Color)
            {
                case "Yellow":
                    Robot robot = (Robot)field.Entity;
                    _entityInfo = "Robot info\n";
                    _entityInfo += "Capacity: " + robot.Capacity.ToString() + "\n";
                    _entityInfo += "Battery: " + robot.EnergyLeft.ToString() + "\n";
                    _entityInfo += "Direction: " + field.Direction + "\n";
                    _entityInfo += "Consumed energy: " + robot.EnergyConsumption.ToString() + "\n";
                    _entityInfo += "ID of destination: " + robot.DestinationID.ToString() + "\n";
                    break;
                case "Green":
                    _entityInfo = "Destination\n";
                    _entityInfo += "ID: " + ((Destination)field.Entity).ID.ToString() + "\n";
                    break;
                case "Blue":
                    _entityInfo = "Dock\n";
                    break;
                case "Gray":
                    _entityInfo = "Pod info\n";
                    _entityInfo += "Pod content (productID : pieces):" + "\n";
                    Pod pod = (Pod)field.Entity;
                    foreach(Int32 product in pod.Products.Keys)
                    {
                        _entityInfo += product.ToString() + " : " + pod.Products[product].ToString() +"\n";
                    }
                    break;
                case "Purple":
                    RobotUnderPod robotUnderPod = (RobotUnderPod)field.Entity;
                    _entityInfo = "Robot under pod info\n";
                    _entityInfo += "Capacity: " + robotUnderPod.Capacity.ToString() + "\n";
                    _entityInfo += "Battery: " + robotUnderPod.EnergyLeft.ToString() + "\n";
                    _entityInfo += "Direction: " + field.Direction + "\n";
                    _entityInfo += "Consumed energy: " + robotUnderPod.EnergyConsumption.ToString() + "\n";
                    _entityInfo += "ID of destination: " + robotUnderPod.DestinationID.ToString() + "\n";
                    foreach (Int32 product in robotUnderPod.Products.Keys)
                    {
                        _entityInfo += product.ToString() + " : " + robotUnderPod.Products[product].ToString() + "\n";
                    }
                    break;
                case "White":
                    _entityInfo = "Empty!\n";
                    break;
            }
            _entityInfo += "Row: " + field.X.ToString() + "\nColumn: " + field.Y.ToString();

            if (ClickedOnTable != null)
                ClickedOnTable(this, EventArgs.Empty);

            //az entitydataból kiszedni az infokat?
        }

        private void Model_SimulationCreated(Object sender, EventArgs e)
        {
            //Debug.WriteLine("Model_SimulationCreated called in viewmodel");
            GenerateTable();
            SetupTable();
        }

        public void CreateSimulationFromSettingsWindow()
        {
            _model.CreateTableFromSettings();
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


        private void OnExitSimulation()
        {
            if (ExitSimulation != null)
                ExitSimulation(this, EventArgs.Empty);
        }

        #endregion
    }
}
