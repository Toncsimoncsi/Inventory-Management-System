using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMS.Persistence;
using IMS.Persistence.Entities;
using MoreComplexDataStructures;

namespace IMS.Model.Simulation
{
    //    Constraint Tree

    //The core of the algorithm is the maintenance of a constraint tree(a binary min-heap in my implementation). The nodes in the constraint tree have 3 component:

    //    constraints - detailing what each agent should avoid in space-time
    //    solution - path for each agent
    //    cost - the sum of the cost of individual paths

    //The low-level STA* planner can take the constraints for an agent and calculate a collison-free path for that agent.
    //node = Find paths for individual agents with no constraints.
    //Add node to the constraint tree.

    //while constraint tree is not empty:
    //  best = node with the lowest cost in the constraint tree

    //  Validate the solution in best until a conflict occurs.
    //  if there is no conflict:
    //    return best

    //  conflict = Find the first 2 agents with conflicting paths.

    //  new_node1 = node where the 1st agent avoid the 2nd agent
    //  Add new_node1 to the constraint tree.

    //  new_node2 = node where the 2nd agent avoid the 1st agent
    //  Add new_node2 to the constraint tree.
    //https://github.com/GavinPHR/Multi-Agent-Path-Finding
    public class ConflictBasedSearch
    {
        public IMSData IMSData { get; set; }
        public ConflictBasedSearch()
        {

        }

        private Dictionary<Robot, List<Pos>> routes;
        private Dictionary<Robot, List<Direction>> rotations;
        private Dictionary<Robot, Dictionary<int, HashSet<Pos>>> blocked;
        private Dictionary<Int32, Pos> constraint = new Dictionary<Int32, Pos>();
        private MinHeap<CTNode> constraintTree;
        /// <summary>
        /// return time of conflict or -1 if no conflicts
        /// </summary>
        /// <param name="route1">robot 1 route</param>
        /// <param name="route2">robot 2 route </param>
        /// <returns></returns>
        private int hasConflict(Dictionary<Robot, List<Pos>> solution, Robot robot1, Robot robot2)
        {
            int min_index = Math.Min(solution[robot1].Count, solution[robot2].Count);
            for (int i = 0; i < min_index; i++)
            {
                if (solution[robot1][i] == solution[robot2][i])
                    return i;
            }
            return -1;
        }

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

        //private Boolean isTurn(Pos[] route1)

        private Boolean canCharge(Robot robot)
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

        private void checkConflicts(CTNode best)
        {
            //CTNode ctnode = new CTNode();
            //solution = dict((agent, self.calculate_path(agent, constraints, None)) for agent in self.agents)
            foreach (Robot robot1 in IMSData.EntityData.RobotData)
            {
                foreach (Robot robot2 in IMSData.EntityData.RobotData)
                {
                    int conflictTime = hasConflict(routes, robot1, robot2);
                    if (conflictTime != -1)
                    {
                        // Calculate new constraints

                        Constraints robot1Constraint = calculateConstraints(best, robot2, robot1, conflictTime);
                        Constraints robot2Constraint = calculateConstraints(best, robot1, robot2, conflictTime);
                        // Calculate new paths
                        //TODO: convert constraints to Dictionary<int, HashSet<Pos>>

                        List<Pos> robot1Path = new AstarSpacetime().FindPath(robot1Constraint.Agent_Constraints[robot2], conflictTime, robot1.Pos, robot1.Pos);
                        List<Pos> robot2Path= new AstarSpacetime().FindPath(robot1Constraint.Agent_Constraints[robot1], conflictTime, robot1.Pos, robot1.Pos);
                        //Replace old paths with new ones in solution

                        Dictionary<Robot,List<Pos>> solution_1 = best.Solution;
                        Dictionary<Robot, List<Pos>> solution_2 = new Dictionary<Robot, List<Pos>>(solution_1);
                        solution_1[robot1] = robot1Path;
                        solution_2[robot2] = robot2Path;
                        //add nodes to min heap

                        CTNode node_1 = new CTNode(robot1Constraint, solution_1);
                        CTNode node_2 = new CTNode(robot2Constraint, solution_2);

                        constraintTree.Insert(node_1);
                        constraintTree.Insert(node_2);
                    }
                }
            }
            // If there is not conflict, validate_paths returns (None, None, -1)



        }
        private Constraints calculateConstraints(CTNode ctnode, Robot robot1, Robot robot2, int time)
        {
            List<Pos> contrained_path = ctnode.Solution[robot1];
            List<Pos> unchanged_path = ctnode.Solution[robot2];

            Pos pivot = unchanged_path[time];
            return ctnode.Constraints.Extend(robot1, pivot, time, time);
        }
        private Constraints calculatePath()
        {
            return null;
        }

        private int[] calculateGoalTimes()
        {
            return null;
        }


    }
}
