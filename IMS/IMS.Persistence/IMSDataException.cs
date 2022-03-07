using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Persistence
{
    public class IMSDataException: Exception
    {
        /// <summary>
        /// Tic-Tac-Toe adat kivétel példányosítása.
        /// </summary>
        public IMSDataException(String message) : base(message) { }
    }
}
