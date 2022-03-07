using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Persistence.Entities
{
    public class Robot : Entity
    {
        public enum Direction { LEFT, UP, RIGHT, DOWN };
        private int _TotalEnergyUsed;
        private int _Capacity;
        private Direction _Dir;
        public Robot(int x, int y, Direction direction,int Capacity) : base(x, y)
        {
            _Capacity = Capacity;
            _TotalEnergyUsed = 0;
            _Dir=direction;
        }
    }
}
