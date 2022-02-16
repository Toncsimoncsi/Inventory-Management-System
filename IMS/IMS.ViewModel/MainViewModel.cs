using System;
using IMS.Model;

namespace IMS.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private IMSModel _model;

        public MainViewModel(IMSModel model)
        {
            _model = model;
        }
    }
}
