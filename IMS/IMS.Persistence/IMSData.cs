using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMS.Persistence.Entities;

namespace IMS.Persistence
{
    public class IMSData
    {
        private EntityData _entityData;
        private int _time;
        private int _totalEnergyConsumption;
        private int _sizeX;
        private int _sizeY;

        public EntityData EntityData { get { return _entityData; } }
        public Int32 Time { get { return _time; } }
        public Int32 TotalEnergyConsumption { get { return _totalEnergyConsumption; } }
        public Int32 SizeX { get { return _sizeX; } }
        public Int32 SizeY { get { return _sizeY; } }

        public int StepCount { get; set; }

        //public Int32 Time { get; set; }
        //public Int32 TotalEnergyConsumption { get; set; }
        //public Int32 SizeX { get; set; }
        //public Int32 SizeY { get; set; }



        public IMSData(EntityData entityData, int sizeX, int sizeY, int time, int totalEnergyConsumption)
        {
            _sizeX = sizeX;
            _sizeY = sizeY;
            _entityData = entityData;
            _time = time;
            _totalEnergyConsumption = totalEnergyConsumption;
            StepCount = 0;
        }

        public IMSData(List<Pod> podData, List<Destination> destinationData, List<Dock> dockData, List<Robot> robotData, List<RobotUnderPod> robotUnderPodData, int sizeX, int sizeY, int time,int totalEnergyConsumption) : this(new EntityData(podData,destinationData,dockData,robotData,robotUnderPodData), sizeX, sizeY, time, totalEnergyConsumption)
        {
        }

        public IMSData(Int32 sizeX, Int32 sizeY) : this(new List<Pod>(), new List<Destination>(), new List<Dock>(), new List<Robot>(), new List<RobotUnderPod>(),sizeX,sizeY,0,0)
        {
        }
    }
}
