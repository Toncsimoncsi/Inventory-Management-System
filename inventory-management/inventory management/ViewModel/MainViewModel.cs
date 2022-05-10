using System;
using System.Collections.Generic;
using System.Text;
using inventory_management.Model;

namespace inventory_management.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private GameModel _model;

        public MainViewModel(GameModel model)
        {
            _model = model;
        }
    }
}
