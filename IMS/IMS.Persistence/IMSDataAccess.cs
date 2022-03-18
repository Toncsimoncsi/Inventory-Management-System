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
        public Task<IMSData> LoadSimulationAsync(String path)
        {
            return null;
        }

        public Task SaveSimulationAsync(String path, IMSData values)
        {
            return null;
        }
    }
}
