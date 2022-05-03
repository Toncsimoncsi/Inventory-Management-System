using IMS.Persistence;
using IMS.Persistence.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Model.Simulation
{
    /// <summary>
    /// https://github.com/GavinPHR/Multi-Agent-Path-Finding
    /// </summary>
    public class PathFinder
    {
        //private Boolean[,] constraints; //convert from imsdata
        private Dictionary<Robot, Dictionary<int, HashSet<Pos>>> constraints;
        private List<Pos> static_obstacles; //not moving
        private IMSData _IMSData;
        private AstarSpacetime astar;
        //private List<Pos>[] Routes;
        //private List<Direction>[] Rotations;
        private ConflictBasedSearch cbs;
        private int time;

        private Assign assign;
        private int[] assignment;

        public IMSData IMSData { get; set; }

        public bool IsFinished { get; set; }

        public Dictionary<Robot, List<Pos>> Routes { get; set; }

        public Dictionary<Robot, List<Direction>> Rotations { get; set; }
        public PathFinder(IMSData data, Dictionary<Robot, Dictionary<int, HashSet<Pos>>> constraints)
        {
            IMSData = data;
            constraints = new Dictionary<Robot, Dictionary<int, HashSet<Pos>>>();
            Routes = new Dictionary<Robot, List<Pos>>();
            Rotations = new Dictionary<Robot, List<Direction>>();
            foreach (Robot robot in IMSData.EntityData.RobotData)
            {
                Routes[robot] = new List<Pos>();
                Rotations[robot] = new List<Direction>();
                //constraints[robot.Pos.X, robot.Pos.Y] = true;
            }
            astar = new AstarSpacetime();
            //AstarSpaceTime = new AstarSpacetime();
            assign = new Assign();
        }

        //move robot to pod
        public void FindPaths()
        {

            /////////////////
            //assign with hungarian-algorithm (lowest combined cost)
            List<Pos> robotsPos = IMSData.EntityData.RobotData.Select(item => item.Pos).ToList();
            List<Pos> podsPos = IMSData.EntityData.PodData.Select(item => item.Pos).ToList();

            //int[x0]=y0 where x0 th robot going to y0 th pod;
            assignment = assign.Assigner(robotsPos, podsPos);

            //make Routes and turns
            for (int i = 0; i < _IMSData.EntityData.RobotData.Count; i++) //for every robot
            {
                CalculatePath(_IMSData.EntityData.RobotData[i], _IMSData.EntityData.PodData[assignment[i]], _IMSData.EntityData.DestinationData[assignment[i]], i);
            }
            IsFinished = true;
        }
        /// <summary>
        /// Calculate path for a single robot going to pod and to destination back to pod (whilst checking charging
        /// </summary>
        /// <param name="robot"></param>
        private void CalculatePath(Robot robot, Pod pod, Destination dest, int index)
        {
            updateBlock();
            time = 0;
            moveRobotToPod(robot, pod, index);
            robot.UnderPod = true;
            moveRobotPodToDestThenPod(pod, dest, index, robot);

        }

        /// <summary>
        /// Check if robot has enough energy
        /// if yes does the job
        /// if not charges and and goes to pod (if not enough charge after charge then fail)
        /// </summary>
        /// <param name="robot"></param>
        /// <param name="pod"></param>
        /// <param name="index"></param>
        private void moveRobotToPod(Robot robot, Pod pod, int index)
        {
            //how much energy required from start to pod
            List<Pos> RoutestartToPod = new AstarSpacetime().FindPath(constraints[robot], time, robot.Pos, pod.Pos);
            int startToPodMove = RoutestartToPod.Count;
            time += startToPodMove;
            List<Direction> turnStartToPod = convertTurn(RoutestartToPod.ToArray(), robot);
            int startToPodTurns = convertTurnTime(turnStartToPod);
            time += startToPodTurns;

            //how much energy required from pod to dest and back
            List<Pos> routePodToDest = new AstarSpacetime().FindPath(constraints[robot], time, pod.Pos, _IMSData.EntityData.DestinationData[assignment[index]].Pos);
            int podToDestMove = routePodToDest.Count;
            time += podToDestMove;
            List<Direction> turnPodToDest = convertTurn(routePodToDest.ToArray(), robot);
            int podToDestTurns = convertTurnTime(turnPodToDest);
            time += podToDestTurns;

            if (startToPodMove + startToPodTurns + podToDestMove + (podToDestTurns * 2) > robot.EnergyLeft) //is enough charge to go to destination
            {
                //not enough
                List<Pos> routeRobotToDock = new AstarSpacetime().FindPath(constraints[robot], time, robot.Pos, closestDock(robot).Pos);
                List<Direction> turnRobotToDock = convertTurn(routeRobotToDock.ToArray(), robot);
                Routes[robot].AddRange(routeRobotToDock);
                time += routeRobotToDock.Count;
                Rotations[robot].AddRange(turnRobotToDock);
                time += convertTurnTime(turnRobotToDock);

                //wait 5


                List<Pos> waitRobot = Enumerable.Repeat(closestDock(robot).Pos, 5).ToList();
                List<Direction> waitRobotRotate = Enumerable.Repeat(turnRobotToDock.Last(), 5).ToList();
                Routes[robot].AddRange(waitRobot);
                Rotations[robot].AddRange(waitRobotRotate);
                time += 5;

                //to pod
                List<Pos> routeRobotToPod = new AstarSpacetime().FindPath(constraints[robot], time, closestDock(robot).Pos, pod.Pos);
                List<Direction> turnRobotToPod = convertTurn(routeRobotToPod.ToArray(), robot);


                Routes[robot].AddRange(routeRobotToPod);
                Rotations[robot].AddRange(turnRobotToPod);
                time += routeRobotToPod.Count;
                time += convertTurnTime(turnRobotToPod);
                //return;
            }
            else
            {
                //enough so straight to pod
                Routes[robot].AddRange(RoutestartToPod);
                Rotations[robot].AddRange(turnStartToPod);
                time += RoutestartToPod.Count;
                time += convertTurnTime(turnStartToPod);

            }
        }

        //move robot from pod to dest
        private void moveRobotPodToDestThenPod(Pod pod, Destination dest, int index, Robot robot)
        {
            List<Pos> routePodToDest = new AstarSpacetime().FindPath(constraints[robot], time, pod.Pos, dest.Pos);
            List<Direction> turnPodToDest = convertTurn(routePodToDest.ToArray(), robot);
            Routes[robot].AddRange(routePodToDest);
            Rotations[robot].AddRange(turnPodToDest);
            time += routePodToDest.Count;
            time += convertTurnTime(turnPodToDest);


            //List<Pos> routeRobotToDesReverse = Enumerable.Reverse(routeRobotToDest).ToList();
            List<Pos> routeDesToPod = new AstarSpacetime().FindPath(constraints[robot], time, dest.Pos, pod.Pos);
            //add last field budget fix
            routeDesToPod.Add(pod.Pos);
            //List<Direction> turnRobotToPodReverse = convertTurn(routeRobotToDesReverse.ToArray(), robot);
            List<Direction> turnDesToPod = convertTurn(routeDesToPod.ToArray(), robot);

            Routes[robot].AddRange(routeDesToPod);
            Rotations[robot].AddRange(turnDesToPod);
            time += routeDesToPod.Count;
            time += convertTurnTime(turnDesToPod);
            //cbs.checkConflicts();
        }

        private void constraintTree()
        {

        }

        private void updateBlock()
        {
            foreach (Robot robot in IMSData.EntityData.RobotData)
            {
                //int x = robot.Pos.X;
                //int y = robot.Pos.Y;
                //constraints[robot.Pos.X][robot.Pos.Y;] = true;
                //constraints[robot.Pos.X, robot.Pos.Y] = true;
            }
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
    }
}
