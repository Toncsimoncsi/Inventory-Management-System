using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Persistence.Entities
{
    public class RobotUnderPod : Entity
    {
        private Int32 _totalEnergyUsed;
        private Int32 _capacity;
        private Int32 _productID;

        public Int32 TotalEnergyUsed { get { return _totalEnergyUsed; } }
        public Int32 Capacity { get { return _capacity; } }
        public Int32 ProductID { get { return _productID; } }
        public Direction Direction { get { return _dir; } }

        public RobotUnderPod(int x, int y, Direction direction, int capacity) : base(x, y)
        {
            _type = EntityType.Robot;
            _capacity = capacity;
            _totalEnergyUsed = 0;
            _dir = direction;
        }

        public RobotUnderPod(int x, int y, Robot robot, Pod pod) : base(x, y)
        {
            _type = EntityType.RobotUnderPod;
            _capacity = robot.Capacity;
            _totalEnergyUsed = robot.TotalEnergyUsed;
            _dir = robot.Direction;
            _productID = pod.ProductID;
        }
    }
}
