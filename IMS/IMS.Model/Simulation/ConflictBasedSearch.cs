using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMS.Persistence;
using IMS.Persistence.Entities;

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

        private Dictionary<Robot, List<Pos>> routes;
        private Dictionary<Robot, List<Direction>> rotations;
        private Dictionary<int, HashSet<Pos>>[] blocked;
        private Dictionary<Int32, Pos> constraint = new Dictionary<Int32, Pos>();
        //Finds time-space conflict of given routes
        private Boolean hasConflict(Pos[] route1, Pos[] route2)
        {
            int min_index = Math.Min(route1.Length, route2.Length);
            for (int i = 0; i < min_index; i++)
            {
                if (route1[i] == route2[i])
                    return true;
            }
            return false;
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

        private void checkConflicts() { 
        
        
        
        
        
        
        
        
        
        }
    }
}
