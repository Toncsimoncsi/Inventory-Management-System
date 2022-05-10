using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMS.Persistence.Entities;

namespace IMS.Model
{
    public class RobotMovedEventArgs :EventArgs
    {
        public Entity Entity { get; private set; }
        /// <summary>
        /// Oszlop index lekérdezése.
        /// </summary>
        public Int32 X { get; private set; }
        /// <summary>
        /// Sor index lekérdezése.
        /// </summary>
        public Int32 Y { get; private set; }

        /// <summary>
        /// Mezőváltozás eseményargumentum példányosítása.
        /// </summary>
        /// <param name="x">Oszlop index.</param>
        /// <param name="y">Sor index.</param>
        /// <param name="player">Játékos.</param>
        public RobotMovedEventArgs(Int32 x, Int32 y, Entity entity)
        {
            Entity = entity;
            X = x;
            Y = y;
        }
    }
}
