using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMS.Persistence.Entities;
using IMS.Model;
using IMS.ViewModel.Fields;
using System.Collections.ObjectModel;

namespace IMS.ViewModel
{
    public class SettingsViewModel : ViewModelBase
    {

        #region Private Fields

        private IMSModel _model;
        private Dictionary<EntityType, String> _entityToColor;
        private int _sizeX;
        private int _sizeY;
        private string _fieldColor;
        private string _whichEntity;
        //private bool _robotRadioButtonIsChecked;

        #endregion

        #region Properties
        public DelegateCommand CreateSimulationCommand { get; private set; }
        public DelegateCommand ResetSimulationCommand { get; private set; }
        public DelegateCommand ViewField { get; private set; }
        public DelegateCommand ModifyField { get; private set; }
        public ObservableCollection<TableField> Fields { get; private set; }
        public DelegateCommand SetSizeCommand { get; private set; }
        public DelegateCommand ChangeColorCommand { get; private set; }

        public Int32 SizeX {
            get { return _sizeX; }
            set
            {
                if (_model.SizeX != value)
                {
                    _sizeX = value;
                    OnPropertyChanged(nameof(SizeX));
                }
            }
        }
        public Int32 SizeY {
            get { return _sizeY; }
            set
            {
                if (_model.SizeY != value)
                {
                    _sizeY = value;
                    OnPropertyChanged(nameof(SizeY));
                }
            }
        }

        public String FieldColor
        {
            get { return _fieldColor; }
            set
            {
                if (_fieldColor == "White") //ha a modelből az adott field empty
                {
                    _fieldColor = value;
                }
            }
        }

        /*public bool RobotBtnChecked
        {
            get { return _robotRadioButtonIsChecked; }
            set
            {
                _robotRadioButtonIsChecked = true;
                _fieldColor = "Gold";
                OnPropertyChanged(nameof(FieldColor));
            }
        }*/

        #endregion

        #region Events
        public event EventHandler CreateSimulation;
        public event EventHandler ResetSimulation;
        public event EventHandler SetSimulationSize;
        public event EventHandler ColorChanged;
        #endregion


        #region constructor
        public SettingsViewModel(IMSModel model)
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

            SetSizeCommand = new DelegateCommand(x => OnGenerateEmptyTable());
            CreateSimulationCommand = new DelegateCommand(param => OnCreateSimulation());
            ResetSimulationCommand = new DelegateCommand(param => OnResetSimulation());
            ChangeColorCommand = new DelegateCommand(param => OnColorChanged());


            _fieldColor = "White";

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
        public void GenerateTable()
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
                    });
                }
            }
            OnPropertyChanged(nameof(SizeX));
            OnPropertyChanged(nameof(SizeY));
        }

        /// <summary>
        /// syncs the model's table with a pre-existing
        /// viewmodel table (Fields)
        /// </summary>
        public void SetupTable()
        {
            foreach (TableField field in Fields)
            {
                field.Color = EntityToColor(_model[field.X, field.Y].Type);
                field.Direction = _model[field.X, field.Y].Direction.ToString();
            }
        }

        public void ChangeFieldColor()
        {
            TableField field = Fields[0];
            switch (FieldColor)
            {
                case "Gold":
                    // _model.setField(field.X, field.Y, "Robot");
                    break;
            }
            GenerateTable();
            SetupTable();
        }

        public void AddNewField(int ind)
        {
            int i = ind / SizeX;
            int j = ind % SizeX;
            Fields.Add(new TableField
            {
                X = i,
                Y = j,
                Color = EntityToColor(stringToEntity(_whichEntity)),
                Direction = Direction.NONE.ToString(),
                Number = i * SizeX + j,
                ViewField = new DelegateCommand(param => ChangeFieldInfo(Convert.ToInt32(param)))
            });
        }

        private EntityType stringToEntity(string ent)
        {
            switch (ent)
            {
                case "Robot":
                    return EntityType.Robot;

                case "Pod":
                    return EntityType.Pod;

                case "Dock":
                    return EntityType.Dock;

                case "Target":
                    return EntityType.Destination;

                default:
                    return EntityType.Empty;
            }
        }

        public void ChangeFieldInfo(int ind)
        {

        }

        private void Model_SimulationCreated(Object sender, EventArgs e)
        {
            GenerateTable();
            SetupTable();
        }
        #endregion

        #region Event methods

        private void OnCreateSimulation()
        {
            
            if (CreateSimulation != null)
                CreateSimulation(this, EventArgs.Empty);
        }

        private void OnResetSimulation()
        {

            if (ResetSimulation != null)
                ResetSimulation(this, EventArgs.Empty);
        }

        private void OnGenerateEmptyTable() 
        {
            if (SetSimulationSize != null)
                SetSimulationSize(this, EventArgs.Empty);
        }

        private void OnColorChanged()
        {
            if (ColorChanged != null)
                ColorChanged(this, EventArgs.Empty);
        }

        #endregion

    }
}
