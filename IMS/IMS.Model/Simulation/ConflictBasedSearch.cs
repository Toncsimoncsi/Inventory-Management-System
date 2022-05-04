using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMS.Persistence;
using IMS.Persistence.Entities;
using System.Diagnostics;

namespace IMS.Model.Simulation
{
    //    MIT License
    //Copyright(c) 2018 YOUR NAME
    //Permission is hereby granted, free of charge, to any person obtaining a copy
    //of this software and associated documentation files (the "Software"), to deal
    //in the Software without restriction, including without limitation the rights
    //to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    //copies of the Software, and to permit persons to whom the Software is
    //furnished to do so, subject to the following conditions:
    //The above copyright notice and this permission notice shall be included in all
    //copies or substantial portions of the Software.
    //THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    //IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    //FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    //AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    //LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    //OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
    //SOFTWARE.
    //https://github.com/GavinPHR/Multi-Agent-Path-Finding
    public class ConflictBasedSearch
    {
        public IMSData IMSData { get; set; }
        public ConflictBasedSearch(IMSData imsdata)
        {
            IMSData = imsdata;
            pathfinder = new PathFinder(IMSData);
            assigner = new Assign();
            constraintTree = new MinHeap(60);
            Rotations = new Dictionary<Robot, List<Direction>>();
        }

        //minheap storing nodes of constraintTree, easy to extract min 
        private MinHeap constraintTree;
        //assign robots to Goals;
        private Assign assigner;
        //Astar with multiple destinations
        private PathFinder pathfinder;


        public Dictionary<Robot, List<Direction>> Rotations { get; private set; }
        public Dictionary<Robot, List<Pos>> CheckConflicts()
        {
            //assign pods to destinations
            Dictionary<int, int> assignmentPodDest = new Dictionary<int, int>();
            for (int i = 0; i < IMSData.EntityData.DestinationData.Count; i++)
            {
                for (int j = 0; j < IMSData.EntityData.PodData.Count; j++)
                {
                    if (IMSData.EntityData.PodData[j].Products.ContainsKey(IMSData.EntityData.DestinationData[i].ID) && !assignmentPodDest.ContainsKey(j)) //matching id and not assigned
                    {
                        assignmentPodDest[j] = i;
                    }
                }
            }

            //     
            //assign robots to pods with hungarian-algorithm (lowest combined cost)
            List<Pos> robotsPos = IMSData.EntityData.RobotData.Select(item => item.Pos).ToList();
            List<Pos> podsPos = IMSData.EntityData.PodData.Select(item => item.Pos).ToList();
            List<Pos> destPos = IMSData.EntityData.DestinationData.Select(item => item.Pos).ToList();



            //int[x0]=y0 where x0 th robot going to y0 th pod;
            int[] assignmentRobPod = assigner.Assigner(robotsPos, podsPos);

            Constraints startConstraints = new Constraints(IMSData.EntityData.RobotData);
            Dictionary<Robot, List<Pos>> Solution = new Dictionary<Robot, List<Pos>>();
            Dictionary<int, HashSet<Pos>> empty = new Dictionary<int, HashSet<Pos>>();
            //better acces to assignment
            Dictionary<Robot, Tuple<Pod, Destination>> assignment = new Dictionary<Robot, Tuple<Pod, Destination>>();

            for (int i = 0; i < assignmentRobPod.Length; i++)
            {
                //find path for each robot
                assignment[IMSData.EntityData.RobotData[i]] = Tuple.Create(IMSData.EntityData.PodData[assignmentRobPod[i]], IMSData.EntityData.DestinationData[assignmentPodDest[assignmentRobPod[i]]]);
                List<Pos> path = pathfinder.CalculatePath(IMSData.EntityData.RobotData[i], assignment[IMSData.EntityData.RobotData[i]].Item1, assignment[IMSData.EntityData.RobotData[i]].Item2,
                    startConstraints.Agent_Constraints[IMSData.EntityData.RobotData[i]], empty);
                Solution[IMSData.EntityData.RobotData[i]] = path;
            }
            //make rootNode
            CTNode rootNode = new CTNode(startConstraints, Solution);
            constraintTree.Add(rootNode);
            //search conflicts
            //if finds conflicts add 2 nodes with 1 representing
            //robot 1 avoding robot 2( robot2 is added to robot1 constraints)
            //robot 2 avoiding robo1 (robot1 is added to robot2 constraint
            while (!constraintTree.IsEmpty())
            {
                CTNode best = constraintTree.Pop();
                //check for conflicts
                Tuple<Robot, Robot, int> conflict = Validate_paths(best);
                Robot robot1 = conflict.Item1;
                Robot robot2 = conflict.Item2;
                int conflictTime = conflict.Item3;
                if (conflictTime == -1) //no conflict
                {

                    foreach (KeyValuePair<Robot, List<Pos>> item in Solution)
                    {
                        Rotations[item.Key] =convertTurn(item.Value, item.Key);
                    }
                    return best.Solution;
                }
                else
                {
                    // Calculate new constraints
                    Debug.WriteLine(conflictTime.ToString(), "conflictTime");
                    Constraints robot1Constraint = CalculateConstraints(best, robot1, robot2, conflictTime);
                    Constraints robot2Constraint = CalculateConstraints(best, robot2, robot1, conflictTime);
                    // Calculate new paths
                    // calculate static obstacles which constitute for robots when they stop they stop
                    Dictionary<int, HashSet<Pos>> goalTimes1 = calculateGoalTimes(best, robot1);
                    Dictionary<int, HashSet<Pos>> goalTimes2 = calculateGoalTimes(best, robot2);

                    List<Pos> robot1Path = pathfinder.CalculatePath(robot1, assignment[robot1].Item1, assignment[robot1].Item2, robot1Constraint.Agent_Constraints[robot1], goalTimes1);
                    List<Pos> robot2Path = pathfinder.CalculatePath(robot2, assignment[robot2].Item1, assignment[robot2].Item2, robot2Constraint.Agent_Constraints[robot2], goalTimes2);

                    //Replace old paths with new ones in solution

                    Dictionary<Robot, List<Pos>> solution1 = best.Solution;
                    Dictionary<Robot, List<Pos>> solution2 = new Dictionary<Robot, List<Pos>>(solution1);
                    solution1[robot1] = robot1Path;
                    solution2[robot2] = robot2Path;
                    //add nodes to min heap

                    CTNode node_1 = new CTNode(robot1Constraint, solution1);
                    CTNode node_2 = new CTNode(robot2Constraint, solution2);

                    constraintTree.Add(node_1);
                    constraintTree.Add(node_2);
                }
            }

            // If there is not conflict, validate_paths returns (None, None, -1)

            return null;

        }
        private List<Direction> convertTurn(List<Pos>route1, Robot robot)
        {
            Direction direction = new Direction();
            List<Direction> directionList = new List<Direction>();
            directionList.Add(robot.Direction); //add robots initial direction
            for (int i = 0; i < route1.Count - 1; i++)
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
        private Constraints CalculateConstraints(CTNode ctnode, Robot robot1, Robot robot2, int time)
        {
            List<Pos> contrainedPath = ctnode.Solution[robot1];
            List<Pos> unchangedPath = ctnode.Solution[robot2];

            Pos pivot = unchangedPath[time];
            //Constraints where robot1 cant be at pivot at the time
            return ctnode.Constraints.Extend(robot1, pivot, time);
        }
        /// <summary>
        /// return time of conflict or -1 if no conflicts
        /// </summary>
        /// <param name="route1">robot 1 route</param>
        /// <param name="route2">robot 2 route </param>
        /// <returns></returns>
        private int HasConflict(Dictionary<Robot, List<Pos>> solution, Robot robot1, Robot robot2)
        {
            int min_index = Math.Min(solution[robot1].Count, solution[robot2].Count);
            for (int i = 0; i < min_index; i++)
            {
                if (solution[robot1][i] == solution[robot2][i])
                {
                    return i;
                }
            }
            //no conflict
            return -1;
        }
        //check for conflicts and return which robot had conflict when
        private Tuple<Robot, Robot, int> Validate_paths(CTNode node)
        {
            Tuple<Robot, Robot, int> conflict = new Tuple<Robot, Robot, int>(null, null, -1);
            for (int j = 0; j < IMSData.EntityData.RobotData.Count(); j++)
            {
                for (int i = 0; i < j; i++)
                {
                    if (i != j)
                    {
                        int conflictTime = HasConflict(node.Solution, IMSData.EntityData.RobotData[i], IMSData.EntityData.RobotData[j]);
                        if (conflictTime != -1)
                        {
                            return Tuple.Create(IMSData.EntityData.RobotData[i], IMSData.EntityData.RobotData[j], conflictTime);
                        }
                    }
                }
            }
            return conflict;
        }
        //static obstacles (robot which has stopped)
        private Dictionary<int, HashSet<Pos>> calculateGoalTimes(CTNode ctnode, Robot robot)
        {
            Dictionary<Robot, List<Pos>> solution = ctnode.Solution;
            Dictionary<int, HashSet<Pos>> goalTimes = new Dictionary<int, HashSet<Pos>>();
            int time;
            foreach (Robot robot1 in IMSData.EntityData.RobotData)
            {
                if (robot1 != robot)
                {
                    time = solution[robot1].Count - 1;
                    if (!goalTimes.ContainsKey(time))
                    {
                        goalTimes[time] = new HashSet<Pos>();
                    }
                    goalTimes[time].Add(solution[robot1][time]);
                }
            }
            return goalTimes;
        }


    }
}
