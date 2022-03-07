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
        protected Pos _position;
        protected Boolean _isLocked;
        protected EntityType _type;
        protected Direction _dir;

        public Pos Pos { get { return new Pos(_position.X,_position.Y); } }
        public Boolean IsLocked { get { return _isLocked; } }
        public EntityType Type { get { return _type; } }

        public Entity(int x,int y)
        {
            _type = EntityType.Empty;
            _dir = Direction.NONE;
            _position = new Pos(x, y);
            _isLocked = false;
        }

    }
}
