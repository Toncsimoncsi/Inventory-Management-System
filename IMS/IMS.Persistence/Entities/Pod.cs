using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Persistence.Entities
{
    public class Pod : Entity
    {
        private int _ProductID;
        public Pod(int x, int y,int productID) : base(x, y)
        {
            _ProductID = productID;
        }
    }
}
