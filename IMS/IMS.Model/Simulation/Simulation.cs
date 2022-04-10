using IMS.Persistence;
using IMS.Persistence.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Model.Simulation
{
    public class PathFinder
    {
        private Boolean[,] blocked; //convert from imsdata
        //private Boolean[][] blocked; //convert from imsdata
        private Pos[][] routes;
        private Astar astar;
        public IMSData IMSData { get; set; }

        public PathFinder(IMSData data)
        {
            IMSData = data;
            blocked = new Boolean[IMSData.SizeX, IMSData.SizeY];
            //blocked = new Boolean[IMSData.SizeX][IMSData.SizeY];
            foreach (Robot robot in IMSData.EntityData.RobotData)
            {
                int x = robot.Pos.X;
                int y = robot.Pos.Y;
                //blocked[x][y] = true;
                blocked[x, y] = true;
            }
            routes = new Pos[4][];
            astar = new Astar();
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
        public void Run(/*Pos[,] routes*/)
        {
            //put the routes in the table one by one from start to given pod (maybe not enough charge)
            routes[0] = astar.FindPath(blocked, IMSData.EntityData.RobotData[0].Pos, IMSData.EntityData.PodData[0].Pos).ToArray();
            routes[1] = astar.FindPath(blocked, IMSData.EntityData.RobotData[1].Pos, IMSData.EntityData.PodData[1].Pos).ToArray();
            routes[2] = astar.FindPath(blocked, IMSData.EntityData.RobotData[2].Pos, IMSData.EntityData.PodData[2].Pos).ToArray();
            routes[3] = astar.FindPath(blocked, IMSData.EntityData.RobotData[3].Pos, IMSData.EntityData.PodData[3].Pos).ToArray();
            for (int i = 0; i < routes.GetLength(1); i++) //elso lepesben
            {
                for (int j = 0; j < routes.GetLength(0); j++) //mindegyik robot egymas utan egyet lep
                {
                    IMSData.EntityData.RobotData[j].Move(routes[j][i]);
                }
                //onTableChanged   
            }
            for (int j = 0; j < routes.GetLength(0); j++) //mindegyik robot egymas utan egyet lep
            {
                //underpod true
                IMSData.EntityData.RobotData[j].UnderPod=true;
            }
            //from pod to dest
            routes[0] = astar.FindPath(blocked, IMSData.EntityData.RobotData[0].Pos, IMSData.EntityData.DestinationData[0].Pos).ToArray();
            routes[1] = astar.FindPath(blocked, IMSData.EntityData.RobotData[1].Pos, IMSData.EntityData.DestinationData[1].Pos).ToArray();
            routes[2] = astar.FindPath(blocked, IMSData.EntityData.RobotData[2].Pos, IMSData.EntityData.DestinationData[2].Pos).ToArray();
            routes[3] = astar.FindPath(blocked, IMSData.EntityData.RobotData[3].Pos, IMSData.EntityData.DestinationData[3].Pos).ToArray();
            
            for (int i = 0; i < routes.GetLength(1); i++) //elso lepesben
            {
                for (int j = 0; j < routes.GetLength(0); j++) //mindegyik robot egymas utan egyet lep
                {
                    IMSData.EntityData.RobotData[j].Move(routes[j][i]);
                }
                //onTableChanged   
            }
        }
    }
}
