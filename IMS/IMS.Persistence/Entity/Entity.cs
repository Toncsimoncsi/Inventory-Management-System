using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Persistence.Entity
{
    public abstract class Entity
    {
        public enum Type { Empty,Pod,Robot,Dock,Destination}
        protected Pos position;
        protected Boolean isLocked;
        protected Type type;
        public Entity(int x,int y)
        {
            type = Type.Empty;
            position.X = x;
            position.Y = y;
            isLocked = false;
        }

    }
}
