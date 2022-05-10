using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMS.Persistence;
using IMS.Persistence.Entities;


namespace IMS.Model.Simulation
{
    public class Constraints
    {
        //which agents the robots have to avoid at a given time
        public Dictionary<Robot, Dictionary<int, HashSet<Pos>>> Agent_Constraints { get; set; }
        public Constraints()
        {
            Agent_Constraints = new Dictionary<Robot, Dictionary<int, HashSet<Pos>>>();
        }
        public Constraints(List<Robot> Robots)
        {
            Agent_Constraints = new Dictionary<Robot, Dictionary<int, HashSet<Pos>>>();
            foreach (Robot robot in Robots)
            {
                Agent_Constraints[robot] = new Dictionary<int, HashSet<Pos>>();
            }
        }
        public Constraints(Dictionary<Robot, Dictionary<int, HashSet<Pos>>> new_constraints)
        {
            Agent_Constraints = new_constraints;
        }


        public Constraints Extend(Robot robot, Pos obstacle, int start)
        {
            //johnny deep copy of constraints

            Dictionary<Robot, Dictionary<int, HashSet<Pos>>> constraintsCopy = new Dictionary<Robot, Dictionary<int, HashSet<Pos>>>(Agent_Constraints);

            if (!constraintsCopy.ContainsKey(robot))
            {
                constraintsCopy[robot] = new Dictionary<int, HashSet<Pos>>();

            }
            if (!constraintsCopy[robot].ContainsKey(start))
            {
                constraintsCopy[robot][start] = new HashSet<Pos>();
            }
            constraintsCopy[robot][start].Add(obstacle);

            Constraints new_constraints = new Constraints(constraintsCopy);

            return new_constraints;
        }


    }
}
