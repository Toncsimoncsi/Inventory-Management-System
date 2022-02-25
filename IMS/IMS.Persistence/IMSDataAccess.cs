using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Persistence
{
    public class IMSDataAccess : IIMSDataAccess
    {
        public Task<Dictionary<Int32, Entity.Entity>> LoadAsync(String path)
        {
            return null;
        }

        public Task SaveAsync(String path, Dictionary<Int32, Entity.Entity> table)
        {
            return null;
        }
    }
}
