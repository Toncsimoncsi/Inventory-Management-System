using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Persistence.Entities
{
    public class Dock : Entity
    {
        public Dock(int x, int y) :base (x,y)
        {
            isLocked = true ;
        }
    }
}
