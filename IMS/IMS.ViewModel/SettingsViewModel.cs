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
        private int _relocationStep;
        private int _x1;
        private int _x2;
        private int _y1;
        private int _y2;
        private string _fieldColor;
        private EntityType _selectedType;
        private int _selectedProduct;
        private int _selectionStep;
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
        public DelegateCommand RelocationCommand { get; private set; }
        public DelegateCommand AddProductCommand { get; private set; }

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

        public Int32 SelectedProduct
        {
            get; set;
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
            _relocationStep = 0;
            _selectionStep = 0;

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
            _model.SelectionChanged_SVM += new EventHandler<SelectionChangedEventArgs>(Model_SelectionChanged_SVM);

            SetSizeCommand = new DelegateCommand(x => OnGenerateEmptyTable());
            CreateSimulationCommand = new DelegateCommand(param => OnCreateSimulation());
            ResetSimulationCommand = new DelegateCommand(param => OnResetSimulation());
            ChangeColorCommand = new DelegateCommand(param => OnColorChanged());
            //SelectFieldCommand = new DelegateCommand(param => _changeSelection((String)param));
            SelectRobotCommand = new DelegateCommand(x => _changeSelection(EntityType.Robot));
            SelectPodCommand = new DelegateCommand(x => _changeSelection(EntityType.Pod));
            SelectDestinationCommand = new DelegateCommand(x => _changeSelection(EntityType.Destination));
            SelectDockCommand = new DelegateCommand(x => _changeSelection(EntityType.Dock));

            RelocationCommand = new DelegateCommand(x => _startRelocation());

            AddProductCommand = new DelegateCommand(x => _startSelection());

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
            //Debug.WriteLine("generate new table, SizeX: " + SizeX.ToString() + ", SizeY: " + SizeY.ToString());
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
                        BorderColor = "Gray",
                        Direction = Direction.NONE.ToString(),
                        Number = i * SizeX + j,
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
                field.BorderColor = "Gray";
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
            TableField field = Fields[ind];
            Debug.WriteLine("PutEntity called");
            if (_relocationStep == 0 && _selectionStep == 0)
            {
                Debug.WriteLine("putting down entity");
                _model.ChangeField(field.X, field.Y, _selectedType);
            }
            else if (_relocationStep == 1)
            {
                Debug.WriteLine("step 1 in relocation");
                _x1 = field.X;
                _y1 = field.Y;
                ++_relocationStep;
                _model.Selection(_x1, _y1);
            }
            else if (_relocationStep == 2)
            {
                Debug.WriteLine("step 2 in relocation");
                _x2 = field.X;
                _y2 = field.Y;
                ++_relocationStep;
                _model.Selection(_x2, _y2);
            }
            else if (_relocationStep == 3)
            {
                Debug.WriteLine("step 3 in relocation");
                //int dx = _x2 - _x1;
                //int dy = _y2 - _y1;

                int x = field.X;
                int y = field.Y;

                //if (x + dx < 0 || x + dx >= _model.SizeX || y + dy < 0 || y + dy >= _model.SizeY)
                //{
                //this means the other corner of the rectangle would be outside the map
                //also reset the whole process
                //    _relocationStep = 0;
                //    return;
                //}

                _model.RelocationAttempt(_x1, _y1, _x2, _y2, x, y);

                

                //reset:
                _relocationStep = 0;

                _model.EndSelection();
            }
            else if (_selectionStep == 1)
            {
                _x1 = field.X;
                _y1 = field.Y;
                ++_selectionStep;
            }
            else if (_selectionStep == 2)
            {
                _x2 = field.X;
                _y2 = field.Y;



                _model.AddProduct(_x1, _y1, _x2, _y2, SelectedProduct);

                _selectionStep = 0;
            }
        }

        private void Model_SimulationCreated(Object sender, EventArgs e)
        {
            GenerateTable();
            SetupTable();
        }

        private void Model_FieldChanged_SVM(Object sender, FieldChangedEventArgs e)
        {
            //Debug.WriteLine("Model_FieldChanged_SVM called, "+e.X.ToString()+", "+e.Y.ToString());
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

        private void Model_SelectionChanged_SVM(Object sender, SelectionChangedEventArgs e)
        {
            Fields[e.Y * _model.TempSizeX + e.X].BorderColor = e.IsSelected ? "Red" : "Gray";
        }

        private void _startRelocation()
        {
            //Debug.WriteLine("starting relocation");
            _relocationStep = 1;
        }

        private void _stopRelocation()
        {
            _relocationStep = 0;
        }

        private void _startSelection()
        {
            _selectionStep = 1;
        }

        private void _stopSelection()
        {
            _selectionStep = 0;
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
