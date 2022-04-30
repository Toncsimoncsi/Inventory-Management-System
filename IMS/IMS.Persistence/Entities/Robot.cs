using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Persistence.Entities
{
    public class Robot : Entity
    {
        private Int32 _energyConsumption;
        private Int32 _capacity;
        private Int32 _energyLeft;
        private Int32 _destinationID;

        public Int32 EnergyConsumption { get { return _energyConsumption; } }
        public Int32 Capacity { get { return _capacity; } }
        public Int32 EnergyLeft { get { return _energyLeft; } }
        public Int32 DestinationID { get { return _destinationID; } }

        public Robot(int x, int y, Direction direction, int capacity, int energyLeft, int destinationID) : this(x, y, direction, capacity, energyLeft, destinationID, 0)
        {
        }

        public Robot(int x, int y, Direction direction, int capacity, int energyLeft, int destinationID, int energyConsumption) : base(x, y)
        {
            _type = EntityType.Robot;
            _capacity = capacity;
            _energyConsumption = energyConsumption;
            _dir = direction;
            _energyLeft = energyLeft;
            _destinationID = destinationID;
        }

        public Robot(int x, int y) : this(x,y,Direction.UP,1,1,-1,0)
        {

        }

        public Robot(int x, int y, int capacity) : this(x,y,Direction.UP,capacity,capacity,-1)
        {

        }

        public Robot(int x, int y, Direction direction, int capacity) : this(x, y, direction, capacity, capacity, -1)
        {

        }

        /*
        public Robot(int x, int y, Direction direction, int capacity, int energyLeft, int energyConsumption) : this(x, y, direction, capacity, energyLeft, -1, energyConsumption)
        {

        }
        */

        public Robot(Robot robot, Direction direction) : this(robot.Pos.X, robot.Pos.Y, direction, robot.Capacity, robot.EnergyLeft, robot.DestinationID, robot.EnergyConsumption)
        {

        }
    }
}
