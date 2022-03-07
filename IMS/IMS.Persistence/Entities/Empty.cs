using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Persistence.Entities
{
    public class Empty: Entity
    {
        public Empty(int x,int y) : base(x, y)
        {
            _type = EntityType.Empty;
            _isLocked = true;
        }
    }
}
