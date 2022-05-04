using System.Collections.Generic;
using IMS.Persistence.Entities;
using IMS.Model;
using System;


namespace IMS.Model.Simulation
{
    public class Assign
    {
        /// <summary>
        /// Assign robots to destinations
        /// </summary>
        private int[,] costs;
        // Hungarian algorithm for global minimal cost
        public int[] Assigner(List<Pos> starts, List<Pos> goals)
        {
            //check for equal amount of start positions vs goal positions
            if (starts.Count != goals.Count)
                return null;

            costs = new int[starts.Count, goals.Count];
            // 1 9
            // 9 1
            //
            for (int i = 0; i < starts.Count; i++)
            {
                for (int j = 0; j < goals.Count; j++)
                {
                    costs[i, j] = -starts[i].Distance(goals[j]);
                }
            }
            int[] result = HungarianAlgorithm.FindAssignments(costs);
            return result;

        }


    }
}
