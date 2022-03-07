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

        public Int32 TotalEnergyUsed { get { return _totalEnergyUsed; } }
        public Int32 Capacity { get { return _capacity; } }
        public Direction Direction { get { return _dir; } }

        public Robot(int x, int y, Direction direction, int capacity) : base(x, y)
        {
            _type = EntityType.Robot;
            _capacity = capacity;
            _totalEnergyUsed = 0;
            _dir=direction;
        }
    }
}
