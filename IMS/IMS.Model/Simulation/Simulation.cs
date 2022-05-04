using IMS.Persistence;
using IMS.Persistence.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace IMS.Model.Simulation
{
    /// <summary>
    /// https://github.com/GavinPHR/Multi-Agent-Path-Finding
    /// 
    /// </summary>
    public class PathFinder
    {

        private AstarSpacetime astar;
        private List<Pos> route;
        private int time;


        public IMSData IMSData { get; set; }


        public Dictionary<Robot, List<Direction>> Rotations { get; set; }
        public PathFinder(IMSData data)
        {
            IMSData = data;
            astar = new AstarSpacetime(data.SizeX,data.SizeY);

        }


        /// <summary>
        /// Find path with given and stops constraints/obstacles/
        /// </summary>
        /// <param name="robot"></param>
        /// <param name="pod"></param>
        /// <param name="dest"></param>
        /// <returns></returns>

        public List<Pos> CalculatePath(Robot robot, Pod pod,Destination dest, Dictionary<int, HashSet<Pos>> constraints, Dictionary<int, HashSet<Pos>> staticObstacles)
        {
            route = new List<Pos>();
            time = 0;
            moveRobotToPod(robot, pod,dest,constraints,staticObstacles);
            robot.UnderPod = true;
            moveRobotPodToDestThenPod(robot, pod, dest, constraints, staticObstacles);
            return route;
        }

        /// <summary>
        /// Check if robot has enough energy
        /// if yes does the job
        /// if not charges and and goes to pod (if not enough charge after charge then fail)
        /// </summary>
        /// <param name="robot"></param>
        /// <param name="pod"></param>
        /// <param name="route"></param>
        private void moveRobotToPod(Robot robot, Pod pod, Destination dest,Dictionary<int, HashSet<Pos>> constraints, Dictionary<int, HashSet<Pos>> staticObstacles)
        {
            //how much energy required from start to pod
            List<Pos> routeStartToPod = new AstarSpacetime(IMSData.SizeX, IMSData.SizeY).FindPath(constraints, staticObstacles, time, robot.Pos, pod.Pos);
            int startToPodMove = routeStartToPod.Count;
            time += startToPodMove;
            List<Direction> turnStartToPod = convertTurn(routeStartToPod.ToArray(), robot);
            int startToPodTurns = convertTurnTime(turnStartToPod);
            time += startToPodTurns;

            //how much energy required from pod to dest and back
            List<Pos> routePodToDest = new AstarSpacetime(IMSData.SizeX, IMSData.SizeY).FindPath(constraints, staticObstacles, time, pod.Pos, dest.Pos);
            int podToDestMove = routePodToDest.Count;
            time += podToDestMove;
            List<Direction> turnPodToDest = convertTurn(routePodToDest.ToArray(), robot);
            int podToDestTurns = convertTurnTime(turnPodToDest);
            time += podToDestTurns;

            if (startToPodMove + startToPodTurns + podToDestMove + (podToDestTurns * 2) > robot.EnergyLeft) //is enough charge to go to destination
            {
                //not enough
                List<Pos> routeRobotToDock = new AstarSpacetime(IMSData.SizeX, IMSData.SizeY).FindPath(constraints, staticObstacles, time, robot.Pos, closestDock(robot).Pos);
                List<Direction> turnRobotToDock = convertTurn(routeRobotToDock.ToArray(), robot);
                route.AddRange(routeRobotToDock);
                time += routeRobotToDock.Count;
                Rotations[robot].AddRange(turnRobotToDock);
                time += convertTurnTime(turnRobotToDock);

                //wait 5


                List<Pos> waitRobot = Enumerable.Repeat(closestDock(robot).Pos, 5).ToList();
                List<Direction> waitRobotRotate = Enumerable.Repeat(turnRobotToDock.Last(), 5).ToList();
                route.AddRange(waitRobot);
                Rotations[robot].AddRange(waitRobotRotate);
                time += 5;

                robot.Charge();
                //to pod
                List<Pos> routeRobotToPod = new AstarSpacetime(IMSData.SizeX, IMSData.SizeY).FindPath(constraints, staticObstacles, time, closestDock(robot).Pos, pod.Pos);
                List<Direction> turnRobotToPod = convertTurn(routeRobotToPod.ToArray(), robot);


                route.AddRange(routeRobotToPod);
                Rotations[robot].AddRange(turnRobotToPod);
                time += routeRobotToPod.Count;
                time += convertTurnTime(turnRobotToPod);
                //return;
            }
            else
            {
                //enough so straight to pod
                route.AddRange(routeStartToPod);
                Rotations[robot].AddRange(turnStartToPod);
                time += routeStartToPod.Count;
                time += convertTurnTime(turnStartToPod);

            }
        }

        //move robot from pod to dest
        private void moveRobotPodToDestThenPod(Robot robot, Pod pod, Destination dest,  Dictionary<int, HashSet<Pos>> constraints, Dictionary<int, HashSet<Pos>> staticObstacles)
        {
            List<Pos> routePodToDest = new AstarSpacetime(IMSData.SizeX, IMSData.SizeY).FindPath(constraints, staticObstacles, time, pod.Pos, dest.Pos);
            List<Direction> turnPodToDest = convertTurn(routePodToDest.ToArray(), robot);
            route.AddRange(routePodToDest);
            Rotations[robot].AddRange(turnPodToDest);
            time += routePodToDest.Count;
            time += convertTurnTime(turnPodToDest);

            List<Pos> routeDesToPod = new AstarSpacetime(IMSData.SizeX, IMSData.SizeY).FindPath(constraints, staticObstacles, time, dest.Pos, pod.Pos);
            //add last field budget fix
            routeDesToPod.Add(pod.Pos);
            List<Direction> turnDesToPod = convertTurn(routeDesToPod.ToArray(), robot);

            route.AddRange(routeDesToPod);
            Rotations[robot].AddRange(turnDesToPod);
            time += routeDesToPod.Count;
            time += convertTurnTime(turnDesToPod);
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
                }
            }
            return closestDock;
        }

        private int convertTurnTime(List<Direction> Rotations)
        {
            int temp = 0;
            for (int j = 0; j < Rotations.Count - 1; j++)
            {
                if (Rotations[j + 1] == Rotations[j])
                {
                    temp++;
                }
            }
            return temp;
        }
        //run the calculate paths on the table
        private List<Direction> convertTurn(Pos[] route1, Robot robot)
        {
            Direction direction = new Direction();
            List<Direction> directionList = new List<Direction>();
            directionList.Add(robot.Direction); //add robots initial direction
            for (int i = 0; i < route1.Length-1; i++)
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
    }
}
