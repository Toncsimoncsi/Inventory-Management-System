using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using inventory_management;
using inventory_management.Model;
using inventory_management.ViewModel;
using inventory_management.View;

namespace inventorymanagement
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private GameModel _model;
        private MainViewModel _viewModel;
        private MainWindow _view;

        public App()
        {
            Startup += App_Startup;
        }

        private void App_Startup(object sender, StartupEventArgs e)
        {
            _model = new GameModel();

            _viewModel = new MainViewModel(_model);

            _view = new MainWindow
            {
                DataContext = _viewModel
            };
            _view.Show();
        }
    }
}
