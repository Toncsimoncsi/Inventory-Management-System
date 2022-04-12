using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMS.Persistence.Entities;
using IMS.Model;
using IMS.ViewModel.Fields;
using System.Collections.ObjectModel;
using System.Diagnostics;

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
        private EntityType _selectedType;
        //private bool _robotRadioButtonIsChecked;

        #endregion

        #region Properties
        public DelegateCommand CreateSimulationCommand { get; private set; }
        public DelegateCommand ResetSimulationCommand { get; private set; }
        public DelegateCommand ViewField { get; private set; }
        //public DelegateCommand ModifyField { get; private set; }
        //public DelegateCommand SelectFieldCommand { get; private set; }
        public ObservableCollection<TableField> Fields { get; private set; }
        public DelegateCommand SetSizeCommand { get; private set; }
        public DelegateCommand ChangeColorCommand { get; private set; }
        public DelegateCommand SelectDockCommand { get; private set; }
        public DelegateCommand SelectPodCommand { get; private set; }
        public DelegateCommand SelectDestinationCommand { get; private set; }
        public DelegateCommand SelectRobotCommand { get; private set; }

        public EntityType SelectedType
        {
            get { return _selectedType; }
        }

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
                if( _fieldColor == "White") //ha a modelből az adott field empty
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
        public event EventHandler StartSimulation;
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
            //_model.TableCreated_SVM += new EventHandler<EventArgs>(Model_TableCreated);
            _model.FieldChanged_SVM += new EventHandler<FieldChangedEventArgs>(Model_FieldChanged_SVM);

            SetSizeCommand = new DelegateCommand(x => OnGenerateEmptyTable());
            CreateSimulationCommand = new DelegateCommand(param => OnCreateSimulation());
            ResetSimulationCommand = new DelegateCommand(param => OnResetSimulation());
            ChangeColorCommand = new DelegateCommand(param => OnColorChanged());
            //SelectFieldCommand = new DelegateCommand(param => _changeSelection((String)param));
            SelectRobotCommand = new DelegateCommand(x => _changeSelection(EntityType.Robot));
            SelectPodCommand = new DelegateCommand(x => _changeSelection(EntityType.Pod));
            SelectDestinationCommand = new DelegateCommand(x => _changeSelection(EntityType.Destination));
            SelectDockCommand = new DelegateCommand(x => _changeSelection(EntityType.Dock));

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
            Debug.WriteLine("generate new table, SizeX: " + SizeX.ToString() + ", SizeY: " + SizeY.ToString());
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
                        Number = j * SizeX + i,
                        //PutField = new DelegateCommand(param => SelectEntityField((EntityType)Enum.Parse(typeof(EntityType), param.ToString())))
                        PutFieldCommand = new DelegateCommand(param => PutEntity(Convert.ToInt32(param)))
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

        /*
        private void SelectEntityField(EntityType type)
        {
            _selectedType = type;
        }
        */

        private void PutEntity(Int32 ind)
        {
            //Debug.WriteLine("PutEntity called");
            TableField field = Fields[ind];
            _model.ChangeField(field.X,field.Y,_selectedType);
        }

        private void Model_SimulationCreated(Object sender, EventArgs e)
        {
            GenerateTable();
            SetupTable();
        }

        private void Model_FieldChanged_SVM(Object sender, FieldChangedEventArgs e)
        {
            Debug.WriteLine("Model_FieldChanged_SVM called, "+e.X.ToString()+", "+e.Y.ToString());
            //TODO: get values from _tempTable
            //Fields[e.Y * _model.SizeX + _model.SizeY].Entity = _model[e.X, e.Y];
            //Fields[e.Y * _model.SizeX + _model.SizeY].Type = _model[e.X, e.Y].Type;
            //Fields[e.Y * _model.SizeX + _model.SizeY].Color = EntityToColor(_model[e.X, e.Y].Type);

            Fields[e.Y * _model.TempSizeX + e.X].Entity = _model.GetTemp(e.X, e.Y);
            Fields[e.Y * _model.TempSizeX + e.X].Type = _model.GetTemp(e.X, e.Y).Type;
            Fields[e.Y * _model.TempSizeX + e.X].Color = EntityToColor(_model.GetTemp(e.X, e.Y).Type);
            //Debug.WriteLine(Fields[e.Y * _model.SizeX + _model.SizeY].Entity.Pos.X.ToString());
            //Debug.WriteLine(Fields[e.Y * _model.SizeX + _model.SizeY].Entity.Pos.Y.ToString());
            //Debug.WriteLine(Fields[e.Y * _model.SizeX + _model.SizeY].X.ToString());
            //Debug.WriteLine(Fields[e.Y * _model.SizeX + _model.SizeY].Y.ToString());
        }

        /*
        private void _changeSelection(String typeStr)
        {
            Debug.WriteLine("changed selection");
            _selectedType = (EntityType)Enum.Parse(typeof(EntityType),typeStr);
        }
        */

        private void _changeSelection(EntityType type)
        {
            //Debug.WriteLine("changed selection");
            //_selectedType = (EntityType)Enum.Parse(typeof(EntityType), typeStr);
            _selectedType = type;
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
            Debug.WriteLine("OnGenerateEmptyTable called");
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
