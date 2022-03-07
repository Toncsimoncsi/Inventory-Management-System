using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using IMS.Persistence.Entities;
using IMS.Model;
using IMS.ViewModel.Fields;

namespace IMS.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        #region Private Fields

        private IMSModel _model;
        private Dictionary<EntityType, String> _entityToColor;

        #endregion


        #region Properties

        public DelegateCommand LoadSimulationCommand { get; private set; }
        public DelegateCommand CreateSimulationCommand { get; private set; }
        public DelegateCommand SaveSimulationCommand { get; private set; }
        public DelegateCommand SaveDiaryCommand { get; private set; }
        public DelegateCommand ExitCommand { get; private set; }
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
        public event EventHandler CreateSimulation;
        public event EventHandler SaveSimulation;
        public event EventHandler SaveDiary;
        public event EventHandler ExitSimulation;

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
        }

        #endregion


        #region Methods

        private String EntityToColor(EntityType type)
        {
            return _entityToColor[type];
        }

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
                        Direction = Direction.NONE.ToString()
                    });
                }
            }
        }


        private void OnLoadSimulation()
        {
            if (LoadSimulation != null)
                LoadSimulation(this, EventArgs.Empty);
        }

        private void OnCreateSimulation()
        {
            if (CreateSimulation != null)
                CreateSimulation(this, EventArgs.Empty);
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
