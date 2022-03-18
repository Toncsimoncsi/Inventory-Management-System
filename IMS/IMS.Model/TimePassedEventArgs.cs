using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Model
{
    class TimePassedEventArgs : EventArgs
    {
        /// <summary>
        /// Ido lekérdezése.
        /// </summary>
        public Int32 Time { get; private set; }

        /// <summary>
        /// Ido eltelesenek eseményargumentuma
        /// </summary>
        /// <param name="player"></param>
        public TimePassedEventArgs(Int32 time) { Time = time; }
    }
}
