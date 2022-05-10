using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Model
{
    public class SpeedChangedEventArgs:EventArgs
    {
        /// <summary>
        /// Sebesség lekérdezése.
        /// </summary>
        public Int32 Speed { get; private set; }

        /// <summary>
        /// Sebesség eltelesenek eseményargumentuma
        /// </summary>
        /// <param name="speed"></param>
        public SpeedChangedEventArgs(Int32 speed) { Speed = speed; }
    }
}
