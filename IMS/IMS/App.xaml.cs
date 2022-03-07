﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using IMS.Persistence;
using IMS.Model;
using IMS.ViewModel;

namespace IMS
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private IMSDataAccess _dataAccess;
        private IMSModel _model;
        private MainViewModel _viewModel;
        private MainWindow _view;

        public App()
        {
            Startup += App_Startup;
        }

        private void App_Startup(object sender, StartupEventArgs e)
        {
            _dataAccess = new IMSDataAccess();

            _model = new IMSModel(_dataAccess);

            _viewModel = new MainViewModel(_model);

            _view = new MainWindow
            {
                DataContext = _viewModel
            };
            _view.Show();

            _model.NewSimulation();
        }
    }
}
