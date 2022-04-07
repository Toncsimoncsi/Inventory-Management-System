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
        private Boolean[,] blocked; //convert from imsdata
        public IMSData IMSData { get; set; }

        public Simulation(IMSData data)
        {
            IMSData = data;
            blocked = new Boolean[IMSData.SizeX, IMSData.SizeY];
            foreach (Robot robot in IMSData.EntityData.RobotData)
            {
                int x = robot.Pos.X;
                int y = robot.Pos.Y;
                blocked[x, y] = true;
            }
        }
        //check if robot has enough energy to finish task
        public Boolean canCharge(Robot robot)
        {
            Pos closestDockPos = new Pos();
            int shortestDistance = int.MaxValue;
            foreach (Dock dock in IMSData.EntityData.DockData) // iterate over all docks which is closer
            {
                if (shortestDistance > robot.Pos.Distance(dock.Pos))
                {
                    shortestDistance = robot.Pos.Distance(dock.Pos);
                    closestDockPos = dock.Pos;
                }
            }
            return robot.EnergyLeft > shortestDistance;
        }
        //run the calculate paths on the table
        public void Run(Pos[,] routes)
        {

            for (int i = 0; i < routes.GetLength(1); i++) //elso lepesben
            {
                for (int j = 0; j < routes.GetLength(0); j++) //mindegyik robot egymas utan egyet lep
                {
                    IMSData.EntityData.RobotData[j].Move(routes[j, i]);
                }
                //onTableChanged   
            }

        }
    }
}
