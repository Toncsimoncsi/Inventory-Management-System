using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Persistence.Entities
{
    public class RobotUnderPod : Entity
    {
        private Int32 _energyConsumption;
        private Int32 _capacity;
        private Int32 _energyLeft;
        private Int32 _destinationID;

        public Int32 EnergyConsumption { get { return _energyConsumption; } }
        public Int32 Capacity { get { return _capacity; } }
        public Int32 EnergyLeft { get { return _energyLeft; } }
        public Int32 DestinationID { get { return _destinationID; } }
        public Dictionary<Int32, Int32> Products { get; set; }

        //public Int32 EnergyConsumption { get; }
        //public Int32 Capacity { get; }
        //public Int32 EnergyLeft { get; }
        //public Int32 DestinationID { get; }
        //public Dictionary<Int32, Int32> Products { get;  }


        public RobotUnderPod(int x, int y, Direction direction, int capacity, int energyLeft, int destinationID, int energyConsumption, Dictionary<Int32, Int32> products) : base(x, y)
        {
            _type = EntityType.RobotUnderPod;
            _capacity = capacity;
            _energyLeft = energyLeft;
            _energyConsumption = energyConsumption;
            _dir = direction;
            Products = products;
        }

        public RobotUnderPod(int x, int y, Direction direction, int capacity, int energyLeft, int destinationID, Dictionary<Int32, Int32> products) : this(x, y, direction, capacity, energyLeft, destinationID, 0, products)
        {
        }

        public RobotUnderPod(int x, int y, Robot robot, Pod pod) : this(x, y, robot.Direction, robot.Capacity, robot.EnergyLeft, robot.DestinationID, robot.EnergyConsumption, pod.Products)
        {
        }

        //public RobotUnderPod(Robot robot, Pod pod) : this(robot.Pos.X,robot.Pos.Y, robot.Direction, robot.Capacity, robot.EnergyLeft, robot.DestinationID, robot.EnergyConsumption, pod.Products)


        public RobotUnderPod(int x, int y) : this(x, y, Direction.NONE,1,1,-1,new Dictionary<int,int>())
        {

        }

        public void RemoveProduct(int productID)
        {
        }

    }
}
