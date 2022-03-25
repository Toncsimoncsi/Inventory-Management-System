using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Persistence.Entities
{
    public class Destination : Entity
    {
        private Int32 _id;

        public Int32 ID { get { return _id; } }
        //public Int32 Id{get;}
        public Destination(int x, int y, int id) : base(x, y)
        {
            _type = EntityType.Destination;
            _id = id;
        }
    }

}
