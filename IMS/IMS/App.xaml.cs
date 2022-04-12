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
using System.Windows.Threading;
using System.Diagnostics;


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
        private SettingsViewModel _settingsViewModel;
        private MainWindow _view;
        private SettingsWindow _settings;
        private DispatcherTimer _timer;

        public App()
        {
            Startup += App_Startup;
        }

        private void App_Startup(object sender, StartupEventArgs e)
        {
            _dataAccess = new IMSDataAccess();

            _model = new IMSModel(_dataAccess);

            _viewModel = new MainViewModel(_model);

            _viewModel.LoadSimulation += new EventHandler(ViewModel_LoadSimulation);
            _viewModel.SaveSimulation += new EventHandler(ViewModel_SaveSimulation);
            //_viewModel.CreateSimulation += new EventHandler(ViewModel_CreateSimulation);

            _viewModel.SaveDiary += new EventHandler(ViewModel_SaveDiary);
            _viewModel.StartStop += new EventHandler(ViewModel_StartStopSimulation);
            _viewModel.OpenSettings += new EventHandler(ViewModel_OpenSettings);
            _viewModel.ClickedOnTable += new EventHandler(ViewModel_EntityInfo);
            _viewModel.SpeedDown += new EventHandler(ViewModel_SpeedDown);
            _viewModel.SpeedUp += new EventHandler(ViewModel_SpeedUp);



            _settingsViewModel = new SettingsViewModel(_model);

            _settingsViewModel.CreateSimulation += new EventHandler(SVM_CreateSimulation);
            _settingsViewModel.ResetSimulation += new EventHandler(SVM_ResetSimulation);
            _settingsViewModel.SetSimulationSize += new EventHandler(SVM_SetSimultaionSize);
            _settingsViewModel.ColorChanged += new EventHandler(SVM_ChangeColor);


            _view = new MainWindow
            {
                DataContext = _viewModel
            };
            _view.Show();

            // időzítő létrehozása
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += new EventHandler(Timer_Tick);

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
            /*_settings = new SettingsWindow
            {
                DataContext = _viewModel
            };
            _settings.ShowDialog();*/
        }

        private void ViewModel_OpenSettings(object sender, EventArgs e)
        {
            _viewModel.CurrentView = _settingsViewModel;
            _settings = new SettingsWindow
            {
                DataContext = _settingsViewModel
            };
            _settings.ShowDialog();
        }

        private void ViewModel_EntityInfo(object sender, EventArgs e)
        {
            MessageBox.Show(_viewModel.EntityInfo, "IMS", MessageBoxButton.OK, MessageBoxImage.Information);
            //_model[0,0].GetType().ToString()
        }



        private void SVM_CreateSimulation(object sender, EventArgs e)
        {
            if (_settingsViewModel.SizeX != 0 && _settingsViewModel.SizeY != 0)
            {
                _viewModel.CurrentView = _viewModel;
                _viewModel.CreateSimulationFromSettingsWindow();
                _settings.Close();
            }
            else
            {
                MessageBox.Show("Cannot start simulation without a table", "IMS", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SVM_ResetSimulation(object sender, EventArgs e)
        {
            _settingsViewModel.SizeX = 0;
            _settingsViewModel.SizeY = 0;
            _model.GenerateEmtyTableForSettingsWindow(_settingsViewModel.SizeX, _settingsViewModel.SizeY);
            _settingsViewModel.GenerateTable();
            _settingsViewModel.SetupTable();
        }

        private void SVM_SetSimultaionSize(object sender, EventArgs e)
        {
            _model.GenerateEmtyTableForSettingsWindow(_settingsViewModel.SizeX, _settingsViewModel.SizeY);
            _settingsViewModel.GenerateTable();
            _settingsViewModel.SetupTable();
        }

        private void SVM_ChangeColor(object sender, EventArgs e)
        {
            //modellben megváltoztat az adott mező a FieldColorra a viewmodelből
            //_settingsViewModel.ChangeFieldColor();
        }

        private void ViewModel_SaveDiary(object sender, EventArgs e)
        {
            //
        }

        private void ViewModel_StartStopSimulation(object sender, EventArgs e)
        {
            if(_view.StartStopBtn.Content.ToString() == "▶️")
            {
                //_model start simulation
                _model.Simulation();
                _view.StartStopBtn.Content = "⏸";
                _timer.Start();
            }
            else
            {
                //_model stop simulation
                _view.StartStopBtn.Content = "▶️";
                _timer.Stop();
            }
        }

        private void ViewModel_SpeedDown(object sender, EventArgs e)
        {
            if (_model.Speed > 1)
            {
                _model.setSpeed(-1);
                _timer.Interval = TimeSpan.FromSeconds(1) / _model.Speed;
            }
            _viewModel.SpeedText = _model.Speed;
        }

        private void ViewModel_SpeedUp(object sender, EventArgs e)
        {
            if (_model.Speed <= 5)
            {
                _model.setSpeed(1);
                _timer.Interval = TimeSpan.FromSeconds(1) / _model.Speed;
            }
            _viewModel.SpeedText = _model.Speed;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            _model.AdvanceTime();
            _viewModel.TimerText = _model.Time;
        }

    }

}

