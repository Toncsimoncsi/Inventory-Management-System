using System;
using IMS.Persistence;

namespace IMS.Model
{
    public class IMSModel
    {
        private IMSDataAccess _dataAccess;

        public IMSModel(IMSDataAccess dataAccess)
        {
            _dataAccess = dataAccess;
        }
    }
}
