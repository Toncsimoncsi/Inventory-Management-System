using IMS.Persistence;
using IMS.Persistence.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Model.Simulation
{
    public class Simulation
    {
        public IMSData _IMSData { get; set; }

        public Boolean hasEnoughEnergy(Robot robot)
        {
            return robot.EnergyLeft>0;
        }

    }
}
