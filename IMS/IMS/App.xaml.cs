using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using IMS.Persistence;
using IMS.Model;
using IMS.ViewModel;
using Microsoft.Win32;

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
        private SettingsWindow _settings;

        public App()
        {
            Startup += App_Startup;
        }

        public event EventHandler LoadSimulation;
        public event EventHandler CreateSimulation;
        public event EventHandler SaveSimulation;
        public event EventHandler SaveDiary;
        public event EventHandler ExitSimulation;

        private void App_Startup(object sender, StartupEventArgs e)
        {
            _dataAccess = new IMSDataAccess();

            _model = new IMSModel(_dataAccess);

            _viewModel = new MainViewModel(_model);

            _viewModel.LoadSimulation += new EventHandler(ViewModel_LoadSimulation);
            _viewModel.SaveSimulation += new EventHandler(ViewModel_SaveSimulation);
            _viewModel.CreateSimulation += new EventHandler(ViewModel_CreateSimulation);
            _viewModel.SaveDiary += new EventHandler(ViewModel_SaveDiary);
            _viewModel.ExitSimulation += new EventHandler(ViewModel_ExitSimulation);

            _view = new MainWindow
            {
                DataContext = _viewModel
            };
            _view.Show();

            _model.NewSimulation();
        }

        private async void ViewModel_LoadSimulation(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Title = "Load simulation";
                openFileDialog.Filter = "IMS load (*.ims) | *.ims";
                if (openFileDialog.ShowDialog() == true)
                {
                    try
                    {
                        MainViewModel vm = new MainViewModel(_model);
                        await _model.LoadSimulationAsync(openFileDialog.FileName);
                    }
                    catch (IMSDataException)
                    {
                        MessageBox.Show("Error occurred during load", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch
            {
                MessageBox.Show("Cannot load game", "IMS", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ViewModel_SaveSimulation(object sender, EventArgs e)
        {
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Title = "Save game";
                saveFileDialog.Filter = "IMS savegame (*.ims) | *.ims";
                if (saveFileDialog.ShowDialog() == true)
                {
                    try
                    {
                        await _model.SaveSimulationAsync(saveFileDialog.FileName);
                    }
                    catch (IMSDataException)
                    {
                        MessageBox.Show("Error occurred during save", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch
            {
                MessageBox.Show("Cannot save game", "IMS", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private void ViewModel_CreateSimulation(object sender, EventArgs e)
        {
            _settings = new SettingsWindow
            {
                DataContext = _viewModel
            };
            _settings.ShowDialog();
        }

        private void ViewModel_SaveDiary(object sender, EventArgs e)
        {
            //
        }

        private void ViewModel_ExitSimulation(object sender, EventArgs e)
        {
            Shutdown();
        }
    }
}
