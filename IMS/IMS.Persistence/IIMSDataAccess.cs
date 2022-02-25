using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IMS.Persistence
{
    public interface IIMSDataAccess
    {
        Task<Dictionary<Int32, Entity.Entity>> LoadAsync(String path);
        Task SaveAsync(String path, Dictionary<Int32, Entity.Entity> table);
    }
}
