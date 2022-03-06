using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Persistence.Entity
{
    public class Empty: Entity
    {
        public Empty(int x,int y) : base(x, y)
        {
            isLocked = true;
        }
    }
}
