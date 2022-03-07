using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMS.Persistence.Entities;

namespace IMS.Persistence
{
    public class IMSDataAccess : IIMSDataAccess
    {
        public Task<Entity[]> LoadSimulationAsync(String path)
        {
            return null;
        }

        public Task SaveSimulationAsync(String path, Entity[] values)
        {
            return null;
        }
    }
}
