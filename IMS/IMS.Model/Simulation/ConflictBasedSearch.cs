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
    //runs the a* on 2 robots
    //if find conflict (intersecting at same time/not enough charge reroute reruns it
    //
    public class ConflictBasedSearch
    {
        public IMSData IMSData { get; set; }


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
            return robot.currentCapacity > shortestDistance;
        }
    }
}
