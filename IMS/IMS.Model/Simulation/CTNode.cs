using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMS.Persistence;
using IMS.Persistence.Entities;

namespace IMS.Model.Simulation
{
    //Constraints Tree node, for the binary tree
    public class CTNode: IComparable<CTNode>
    {
        public CTNode(Constraints constraints,  Dictionary<Robot, List<Pos>> Solution)
        {
            this.Solution = Solution;
            this.Cost = sic();
            this.Constraints = constraints;
        }
        public Constraints Constraints { get; set; }
        public int Cost { get; set; }
        public Dictionary<Robot, List<Pos>> Solution { get; set; }

        public int CompareTo(CTNode incomingCTNode)
        {
            //CTNode incomingCTNode = incomingobject as CTNode;
            return this.Cost.CompareTo(incomingCTNode.Cost);
        }
        public override bool Equals(object obj)
        {
            var item = obj as Pos;
            if (item == null)
            {
                return false;
            }
            return Equals(obj as CTNode);
        }

        public bool Equals(CTNode other)
        {
            return other != null &&
                   Cost == other.Cost &&
                   Solution == other.Solution;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Cost, Solution);
        }
        public static bool operator <(CTNode A, CTNode B)
        {
            if (A is null)
            {
                if (B is null)
                {
                    // null < null = false.
                    return false;
                }

                // Only the left side is null.
                return false;
            }
            return A.Cost <= B.Cost;
        }

        public static bool operator >(CTNode A, CTNode B)
        {
            if (A is null)
            {
                if (B is null)
                {
                    // null < null = false.
                    return false;
                }

                // Only the left side is null.
                return false;
            }
            return A.Cost > B.Cost;
        }
        public static bool operator ==(CTNode A, CTNode B)
        {
            if (A is null)
            {
                if (B is null)
                {
                    // null == null = true.
                    return true;
                }

                // Only the left side is null.
                return false;
            }
            // Equals handles the case of null on right side.
            return A.Equals(B);
        }

        public static bool operator !=(CTNode A, CTNode B) => !(A == B);
        // Sum-of-Individual-Costs heuristics

        private int sic()
        {
            int temp = new();
            foreach(List<Pos> list in Solution.Values)
            {
                temp += list.Count;
            }
            return temp;
        }
    }
}
