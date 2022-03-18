using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Persistence.Entities
{
    public class Robot : Entity
    {
        private Int32 _totalEnergyUsed;
        private Int32 _capacity;
        private Int32 _energyLeft;
        private Int32 _destinationID;

        public Int32 TotalEnergyUsed { get { return _totalEnergyUsed; } }
        public Int32 Capacity { get { return _capacity; } }
        public Int32 EnergyLeft { get { return _energyLeft; } }
        public Int32 DestionationID { get { return _destinationID; } }

        public Robot(int x, int y, Direction direction, int capacity, int energyLeft, int destinationID) : this(x, y, direction, capacity, energyLeft, destinationID, 0)
        {
        }

        public Robot(int x, int y, Direction direction, int capacity, int energyLeft, int destinationID, int totalEnergyUsed) : base(x, y)
        {
            _type = EntityType.Robot;
            _capacity = capacity;
            _totalEnergyUsed = totalEnergyUsed;
            _dir = direction;
            _energyLeft = energyLeft;
            _destinationID = destinationID;
        }
    }
}
