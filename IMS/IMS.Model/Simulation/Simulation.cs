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

  
        public event EventHandler<EventArgs> SimulationStarted;
        public event EventHandler<EventArgs> SimulationOver;
        public event EventHandler<RobotMovedEventArgs> RobotMoved;
        /// <summary>
        /// Simulation végének eseménye.
        /// </summary>
        public event EventHandler SpeedChanged;
        /// <summary>
        /// Simulation végének eseménye.
        /// </summary>
        public event EventHandler TimePassed;

        /// <summary>
        /// Simulation megnyerésének eseménye.
        /// </summary>
        public event EventHandler<DiaryEventArgs> SimulationWon;

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
        private Dock closestDock(Robot robot)
        {
            Dock closestDock = IMSData.EntityData.DockData[0];
            int shortestDistance = int.MaxValue;
            foreach (Dock dock in IMSData.EntityData.DockData) // iterate over all docks which is closer
            {
                if (shortestDistance > robot.Pos.Distance(dock.Pos))
                {
                    shortestDistance = robot.Pos.Distance(dock.Pos);
                    //closestDockPos = dock.Pos;
                }
            }
            return closestDock;
        }
        //run the calculate paths on the table
        private List<Direction> convertTurn(Pos[] route1, Robot robot)
        {
            Direction direction = new Direction();
            List<Direction> directionList = new List<Direction>();
            directionList.Add(robot.Direction); //add robots initial direction
            for (int i = 0; i < route1.Length; i++)
            {
                switch (route1[i + 1].X - route1[i].X + (route1[i + 1].Y - route1[i].Y) * 2) //x coordinate diff(-1 or 1) plus y  coordinate*2 diff(-2 or 2) and 
                {

                    case -1:// down  
                        direction = Direction.DOWN;
                        break;
                    case 1:// up
                        direction = Direction.UP;
                        break;
                    case -2:// left  
                        direction = Direction.LEFT;
                        break;
                    case 2:// right 
                        direction = Direction.RIGHT;
                        break;
                }
                directionList.Add(direction);
            }
            return directionList;
        }
        //move robot to pod
        public void moveRobotsStartToPod()
        {
            OnSimulationStarted();
            Pos[][] routes = new Pos[4][] ;
            Direction[][] turns = new Direction[4][];
            //make routes and turns
            for (int i = 0; i < routes.GetLength(0); i++)
            {

                //how much energy required from start to dock
                List<Pos> routeStartToPod = astar.FindPath(blocked, IMSData.EntityData.RobotData[i].Pos, IMSData.EntityData.PodData[i].Pos);
                int startToPod = routeStartToPod.Count;
                List<Direction> routeTurnStartToPod = convertTurn(routeStartToPod.ToArray(),IMSData.EntityData.RobotData[i]);
                int turnStartToPod = 0;
                for (int j=0; j<routeTurnStartToPod.Count-1; j++)
                {
                    if (routeTurnStartToPod[j+1]== routeTurnStartToPod[j])
                    {
                        turnStartToPod++;
                    }
                }
                //how much energy required from pod to
                List<Pos> routePodToDest = astar.FindPath(blocked, IMSData.EntityData.PodData[i].Pos, IMSData.EntityData.DestinationData[i].Pos);
                int PodToDest = routePodToDest.Count;
                List<Direction> routeTurnPodToDest = convertTurn(routePodToDest.ToArray(), IMSData.EntityData.RobotData[i]);
                int turnPodToDest = 0;
                for (int j = 0; j < routeTurnPodToDest.Count - 1; j++)
                {
                    if (routeTurnPodToDest[j + 1] == routeTurnPodToDest[j])
                    {
                        turnPodToDest++;
                    }
                }
                while (startToPod + turnStartToPod + PodToDest +turnPodToDest < IMSData.EntityData.RobotData[i].currentCapacity) //enough charge to go to destination
                {
                    //not enough
                    moveRobotToDock(IMSData.EntityData.RobotData[i], closestDock(IMSData.EntityData.RobotData[i]));
                }
                //enough
;
                routes[i] = routeStartToPod.ToArray();
                turns[i] = convertTurn(routes[i], IMSData.EntityData.RobotData[i]).ToArray();
            }

            for (int j = 0; j < routes.GetLength(0); j++) //mindegyik robot egymas utan egyet lep
            {
                //underpod true
                IMSData.EntityData.RobotData[j].UnderPod = true;

            }
        }

        //not enough charge
        public void moveRobotToDock(Robot robot,Dock dock)
        {
            List<Pos> route = new ();
            List<Pos> routeRobotToDock = astar.FindPath(blocked, robot.Pos, dock.Pos);
            List<Direction> turnRobotToDock= convertTurn(routeRobotToDock.ToArray(), robot);

            for (int j = 0; j < routes.GetLength(0); j++) //mindegyik robot egymas utan egyet lep
            {
                robot.Move(route[j]);
                robot.Rotate(turnRobotToDock[j]);
                //onRobotMoved
            }
            for (int j = 0; j < routes.GetLength(0); j++) //mindegyik robot egymas utan egyet lep
            {
                
                //IMSData.EntityData.RobotData[j].UnderPod = true;
                //wait 5 sec
            }
        }

        //after charged it does its job (picks up shelf)
        public void moveDockToPod(Robot robot,Pod pod)
        {
            OnSimulationStarted();
            Pos[][] routes = new Pos[4][];
            Direction[][] turns = new Direction[4][];
            List<Pos> randomList = new();
            //make routes and turns
            for (int i = 0; i < routes.GetLength(0); i++)
            {
                randomList = astar.FindPath(blocked, IMSData.EntityData.RobotData[0].Pos, IMSData.EntityData.PodData[0].Pos);
                routes[i] = randomList.ToArray();
                turns[i] = convertTurn(routes[i], IMSData.EntityData.RobotData[i]).ToArray();
            }

            for (int j = 0; j < routes.GetLength(0); j++) //mindegyik robot egymas utan egyet lep
            {
                //underpod true
                IMSData.EntityData.RobotData[j].UnderPod = true;
                //wait 5 sec
            }
        }
        //move robot from pod to dest
        public void moveRobotsPodToDest()
        {
            routes[0] = astar.FindPath(blocked, IMSData.EntityData.RobotData[0].Pos, IMSData.EntityData.DestinationData[0].Pos).ToArray();
            routes[1] = astar.FindPath(blocked, IMSData.EntityData.RobotData[1].Pos, IMSData.EntityData.DestinationData[1].Pos).ToArray();
            routes[2] = astar.FindPath(blocked, IMSData.EntityData.RobotData[2].Pos, IMSData.EntityData.DestinationData[2].Pos).ToArray();
            routes[3] = astar.FindPath(blocked, IMSData.EntityData.RobotData[3].Pos, IMSData.EntityData.DestinationData[3].Pos).ToArray();

            for (int i = 0; i < routes.GetLength(1); i++) //elso lepesben
            {
                for (int j = 0; j < routes.GetLength(0); j++) //mindegyik robot egymas utan egyet lep
                {
                    IMSData.EntityData.RobotData[j].Move(routes[j][i]);
                    //onorobotmoved
                }
                //onTableChanged   
            }
        }
        //vissaviszik a polcokat a helyukre
        public void moveRobotsPodtoDest(/*Pos[,] routes*/)
        {
            
        }
        /// <summary>
        /// Játék kezdetének eseménykiváltása.
        /// </summary>
        private void OnSimulationStarted()
        {
            if (SimulationStarted != null)
                SimulationStarted(this, EventArgs.Empty);
        }
        /// <summary>
        /// Simulation megnyerésének eseménykiváltása.
        /// </summary>
        /// <param name="Entity">A győztes Simulationos.</param>
        private void OnTimePassed(Int32 time)
        {
            if (SimulationWon != null)
                TimePassed(this, new TimePassedEventArgs(time));
        }
        /// <summary>
        /// Mezőváltozás eseménykiváltása.
        /// </summary>
        /// <param name="x">Oszlop index.</param>
        /// <param name="y">Sor index.</param>
        /// <param name="Entity">Játékos.</param>
        private void OnRobotMoved(Int32 x, Int32 y, Entity Entity)
        {
            if (RobotMoved != null)
                RobotMoved(this, new RobotMovedEventArgs(x, y, Entity));
        }
    }
}
