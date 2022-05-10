using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMS.Persistence.Entities;

namespace IMS.Persistence
{
    public class EntityData
    {
        private List<Pod> _podData;
        private List<Destination> _destinationData;
        private List<Dock> _dockData;
        private List<Robot> _robotData;
        private List<RobotUnderPod> _robotUnderPodData;

        public List<Pod> PodData { get { return _podData; } }
        public List<Destination> DestinationData { get { return _destinationData; } }
        public List<Dock> DockData { get { return _dockData; } }
        public List<Robot> RobotData { get { return _robotData; } }
        public List<RobotUnderPod> RobotUnderPodData { get { return _robotUnderPodData; } }


        public EntityData(List<Pod> podData, List<Destination> destinationData, List<Dock> dockData, List<Robot> robotData, List<RobotUnderPod> robotUnderPodData)
        {
            _podData = podData;
            _destinationData = destinationData;
            _dockData = dockData;
            _robotData = robotData;
            _robotUnderPodData = robotUnderPodData;
        }
    }
}
