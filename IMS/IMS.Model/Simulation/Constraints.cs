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
        public Dictionary<Robot, Dictionary<int, HashSet<Pos>>> Agent_Constraints { get; set; }
        //private Dictionary<Robot, Dictionary<int, HashSet<Pos>>> agent_Constraints = new Dictionary<Robot, Dictionary<int, HashSet<Pos>>>();
        public Constraints(Dictionary<Robot, Dictionary<int, HashSet<Pos>>> new_constraints)
        {
            Agent_Constraints = new_constraints;
        }


        public Constraints Extend(Robot robot, Pos obstacle, int start, int end)
        {
            //johnny deep copy of constraints

            Dictionary<Robot, Dictionary<int, HashSet<Pos>>> constraintsCopy = new Dictionary<Robot, Dictionary<int, HashSet<Pos>>>(Agent_Constraints);
            for (int i = start; i < end; i++)
            {
                if (!constraintsCopy.ContainsKey(robot))
                {
                    constraintsCopy[robot] = new Dictionary<int, HashSet<Pos>>();

                }
                if (!constraintsCopy[robot].ContainsKey(i))
                {
                    constraintsCopy[robot][i] = new HashSet<Pos>();
                }
                constraintsCopy[robot][i].Add(obstacle);
            }
            Constraints new_constraints = new Constraints(constraintsCopy);

            return new_constraints;
        }


    }
}
