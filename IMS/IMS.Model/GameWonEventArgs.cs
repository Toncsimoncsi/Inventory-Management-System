using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMS.Persistence.Entities;

namespace IMS.Model
{
    public class GameWonEventArgs:EventArgs
    {
        /// <summary>
        /// Játékos lekérdezése.
        /// </summary>
        public Entity Entity { get; private set; }

        /// <summary>
        /// Játék megnyerésének eseményargumentuma
        /// </summary>
        /// <param name="player"></param>
        public GameWonEventArgs(Entity entity) { Entity = entity; }
    }
}
