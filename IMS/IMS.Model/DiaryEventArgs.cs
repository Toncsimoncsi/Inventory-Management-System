using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMS.Persistence;
using IMS.Persistence.Entities;


namespace IMS.Model
{
    public class DiaryEventArgs: EventArgs
    {
        /// <summary>
        /// Entitydata lekérdezése
        /// </summary>
        public EntityData EntityData { get; private set; }

        /// <summary>
        /// Szimulációvégének eseményargumentuma
        /// </summary>
        /// <param name="player"></param>
        public DiaryEventArgs(EntityData entitydata) { EntityData = entitydata; }
    }
}
