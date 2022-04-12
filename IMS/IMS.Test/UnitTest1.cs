using Microsoft.VisualStudio.TestTools.UnitTesting;
//using Moq;
using IMS.Model;
using IMS.Persistence;
using System;
using IMS.Persistence.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using IMS.Model.Simulation;

namespace IMS.Test
{
    [TestClass]
    public class UnitTest1
    {
        #region Fields
        private IMSModel _model;
        private IMSData _table;
        private IMSDataAccess _dataAccess;
        //private PathFinder _sim;
        //private Mock<IIMSDataAccess> _mock;
        #endregion

        #region Initialization
        [TestInitialize]
        public void Initialize()
        {
            //játéktábla még kell ide
            int totalEnergyConsumption = 0;
            int timeStep = 0;
            int sizeX = 12;
            int sizeY = 12;
            List<Pod> podData = new List<Pod>();
            podData.Add(new Pod(4, 3, new Dictionary<int, int>()));
            podData.Add(new Pod(4, 5, new Dictionary<int, int>()));
            podData.Add(new Pod(4, 6, new Dictionary<int, int>()));
            List<Destination> destinationData = new List<Destination>();
            destinationData.Add(new Destination(11, 5, 1));

            List<Dock> dockData = new List<Dock>();
            dockData.Add(new Dock(0, 11));

            List<Robot> robotData = new List<Robot>();
            Robot robot = new Robot(0, 0, 0, 5, 5, 1, 1);
            robotData.Add(robot);

            List<RobotUnderPod> robotUnderPodData = new List<RobotUnderPod>();
            Robot robot2 = new Robot(4, 4, 0, 5, 5, 1, 1);
            Pod pod2 = new Pod(4, 4, new Dictionary<int, int>());
            RobotUnderPod robotunder = new RobotUnderPod(4, 4, robot2, pod2);
            _table = new IMSData(podData, destinationData, dockData, robotData, robotUnderPodData, sizeX, sizeY, timeStep, totalEnergyConsumption);

            //_mock = new Mock<IIMSDataAccess>();
            //_mock.Setup(mock => mock.LoadSimulationAsync(It.IsAny<String>())).Returns(() => Task.FromResult(_table));

            //_model = new IMSModel(_mock.Object);
            _dataAccess = new IMSDataAccess();
            _model = new IMSModel(_dataAccess);
            //_model.LoadSimulationAsync(It.IsAny<String>())).Returns(() => Task.FromResult(_table));
            //_sim = new PathFinder(_table);

            //_model.TimePassed += new EventHandler(Model_TimePassed);
            //_model.SpeedChanged += new EventHandler(Model_SpeedChanged);


        }
        #endregion
        [TestMethod]
        public void CreateEmptyTable()
        {
            _model.NewSimulation();
            Assert.AreEqual(12, _model.SizeX);
            Assert.AreEqual(12, _model.SizeY);
            for (Int32 i = 0; i < _model.SizeX; ++i)
            {
                for (Int32 j = 0; j < _model.SizeY; ++j)
                {
                    Assert.AreEqual(_model[i, j], EntityType.Empty);
                }
            }
        }

        [TestMethod]
        public void StartStepsTest()
        {
            _model.NewSimulation();

            Assert.AreEqual(_model.Steps, 0);
        }

        [TestMethod]
        public void StartEnergyTest()
        {
            _model.NewSimulation();

            Assert.AreEqual(_model.AllEnergy, 0);
        }

        [TestMethod]
        public void StartSpeedTest()
        {
            _model.NewSimulation();

            Assert.AreEqual(_model.Speed, 0);
        }

        [TestMethod]
        public void TimeTest()
        {
            _model.NewSimulation();
            Assert.AreEqual(_model.Time, 0);

            _model.AdvanceTime();
            Assert.AreEqual(_model.Time, 1);
        }

        [TestMethod]
        public void SimulationTest()
        {
            _model.Simulation();
            //TODO
        }

        [TestMethod]
        public async Task GameModelLoadTest()
        {
            // kezdünk egy új játékot
            _model.NewSimulation();

            // majd betöltünk egy játékot
            await _model.LoadSimulationAsync(String.Empty);

            // ellenõrizzük, hogy meghívták-e a Load mûveletet a megadott paraméterrel
            //_mock.Verify(dataAccess => dataAccess.LoadSimulationAsync(String.Empty), Times.Once());
        }


        [TestMethod]
        public void SetSpeedTestSpeedUp()
        {
            int startSpeed = _model.Speed;
            _model.setSpeed(1);
            Assert.AreEqual(startSpeed + 1, _model.Speed);
        }


        [TestMethod]
        public void SetSpeedTestSpeedDown()
        {
            int startSpeed = _model.Speed;

            _model.setSpeed(-1);
            Assert.AreEqual(startSpeed - 1, _model.Speed);
        }

        #region Event handlers
        private void Model_FieldChanged(object sender, RobotMovedEventArgs e)
        {
            //Assert.AreEqual(_model.SizeX, e.X);
            //Assert.AreEqual(_model.SizeX, e.Y);
        }

        private void Model_SpeedChanged(object sender, SpeedChangedEventArgs e)
        {
            Assert.AreEqual(_model.Speed, e.Speed);
        }

        private void Model_TimePassed(object sender, TimePassedEventArgs e)
        {
            Assert.AreEqual(_model.Time, e.Time);
        }

        #endregion

    }
}
