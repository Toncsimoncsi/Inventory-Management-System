using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Persistence.Entities
{
    public enum EntityType { Empty, Pod, Robot, Dock, Destination, RobotUnderPod }
    public enum Direction { LEFT, UP, RIGHT, DOWN, NONE };
    public abstract class Entity
    {
        //protected Pos _position;
        protected EntityType _type;
        protected Direction _dir;

        public Pos Pos { get; set; }
        public EntityType Type { get { return _type; } }
        public Direction Direction { get { return _dir; } }

        public Entity(int x,int y)
        {
            _type = EntityType.Empty;
            _dir = Direction.NONE;
            Pos = new Pos(x, y);
        }

    }
}
