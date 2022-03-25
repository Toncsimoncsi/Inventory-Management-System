using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Persistence.Entities
{
    public class Pod : Entity
    {
        private Dictionary<Int32, Int32> _products;

        public Dictionary<Int32, Int32> Products { get { return _products; } }
        
        //public Dictionary<Int32,Int32> Products{get;}
        public Pod(int x, int y, Dictionary<Int32, Int32> products) : base(x, y)
        {
            _type = EntityType.Pod;
            _products = products;
        }
    }
}
