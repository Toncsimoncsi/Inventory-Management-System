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
        private Pos _destination;

        public Int32 EnergyConsumption { get { return _energyConsumption; } }
        public Int32 Capacity { get { return _capacity; } }
        public Int32 EnergyLeft { get { return _energyLeft; } }
        public Int32 DestinationID { get { return _destinationID; } }

        public Pos Destination { get { return _destination; } set { } }
        //move robot to given position and change fuel
        public void MoveRobot(Pos other)
        {
            if (_energyLeft <= _position.Distance(other))
                return;
            _energyLeft += _position.Distance(other);
            _energyConsumption += _position.Distance(other);
            _position.X = other.X;
            _position.Y = _position.Y;
        }
        //public Int32 EnergyConsumption { get; }
        //public Int32 Capacity { get;}
        //public Int32 EnergyLeft { get; }
        //public Int32 DestinationID { get; }

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
    }
}
