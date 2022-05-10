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
        private Dictionary<EntityType, String> _entityToImg;

        private ViewModelBase _currentView;
        private string _entityInfo;
        private int _speedText;
        private int _timerText;

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
            set
            {
                STEPS = value;
                OnPropertyChanged();
            }
        }
        public int ALLENERGY
        {
            get { return _model.AllEnergy; }
            private set
            {
                ALLENERGY = value;
                OnPropertyChanged();
            }
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


        public int TimerText
        {
            get { return _model.Time; }
            set
            {
                _timerText = value;
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

            _entityToImg = new Dictionary<EntityType, String>();
            _entityToImg.Add(EntityType.Robot, "/img/robot.png");
            _entityToImg.Add(EntityType.Pod, "/img/full-pod.png");
            _entityToImg.Add(EntityType.RobotUnderPod, "/img/robot-under-pod.png");
            _entityToImg.Add(EntityType.Destination, "/img/dest.png");
            _entityToImg.Add(EntityType.Dock, "/img/dock.png");
            _entityToImg.Add(EntityType.Empty, "/img/empty.png");

            Fields = new ObservableCollection<TableField>();

            _model = model;
            _model.SimulationCreated += new EventHandler<EventArgs>(Model_SimulationCreated);
            _model.TableChanged += new EventHandler<EventArgs>(Model_TableChanged);
            _model.StepsChanged += new EventHandler(Model_StepsChanged);
            _model.EnergyChanged += new EventHandler(Model_AllEnergyChanged);


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
            _timerText = _model.Time;

        }

        #endregion


        #region Methods

        private String EntityToColor(EntityType type)
        {
            return _entityToColor[type];
        }

        private String EntityToImg(EntityType type)
        {
            string res = _entityToImg[type];
            return res;
        }

        private string RobotImage(String dir)
        {
            //LEFT, UP, RIGHT, DOWN, NONE 
            switch (dir)
            {
                case "DOWN":
                    return "/img/robot-left.png";
                case "UP":
                    return "/img/robot-right.png";
                case "RIGHT":
                    return "/img/robot-down.png";
                default:
                    return "/img/robot.png";
            }
        }

        private string RobotUnderPodImage(String dir)
        {
            //LEFT, UP, RIGHT, DOWN, NONE 
            switch (dir)
            {
                case "DOWN":
                    return "/img/robot-under-pod-left.png";
                case "UP":
                    return "/img/robot-under-pod-right.png";
                case "RIGHT":
                    return "/img/robot-under-pod-down.png";
                default:
                    return "/img/robot-under-pod.png";
            }
        }

        private string FullOrEmptyPodImg(bool isEmpty)
        {
            if (isEmpty)
            {
                return "/img/empty-pod.png";
            }
            else
            {
                return "/img/full-pod.png";
            }
        }
        /// <summary>
        /// creates an empty viewmodel table (Fields)
        /// </summary>
        private void GenerateTable()
        {
            Fields.Clear();
            for (Int32 i = 0; i < SizeY; ++i)
            {
                for (Int32 j = 0; j < SizeX; ++j)
                {
                    Fields.Add(new TableField
                    {
                        X = j,
                        Y = i,
                        Color = EntityToColor(EntityType.Empty),
                        BgImage = EntityToImg(EntityType.Empty),
                        Direction = Direction.NONE.ToString(),
                        Type = EntityType.Empty,
                        Entity = _model[j, i],
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

                if(field.Type == EntityType.Robot)
                {
                    if (_model.isRobotUnderPod(field.X, field.Y))
                    {
                        field.BgImage = RobotUnderPodImage(field.Direction);
                    }
                    else
                    {
                        field.BgImage = RobotImage(field.Direction);
                    }
                }
                else {
                    field.BgImage = EntityToImg(_model[field.X, field.Y].Type);
                }

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
        }

        private void Model_SimulationCreated(Object sender, EventArgs e)
        {
            GenerateTable();
            SetupTable();
        }

        private void Model_TableChanged(Object sender, EventArgs e)
        {
            GenerateTable();
            SetupTable();
        }

        private void Model_StepsChanged(Object sender, EventArgs e)
        {
            OnPropertyChanged("STEPS");
        }
        private void Model_AllEnergyChanged(Object sender, EventArgs e)
        {
            OnPropertyChanged("ALLENERGY");
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
