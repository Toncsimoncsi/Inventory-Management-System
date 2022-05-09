using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMS.Persistence;
using IMS.Persistence.Entities;

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
    public class CTNode: IComparable<CTNode>
    {
        public CTNode(Constraints constraints, Dictionary<Robot, List<Pos>> solution)
        {
            Solution = solution;
            Cost = sic();
            Constraints = constraints;
        }
        public Constraints Constraints { get;private set; }
        public int Cost { get;private set; }
        public Dictionary<Robot, List<Pos>> Solution { get;private set; }

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
            int temp = 0;
            foreach(List<Pos> list in Solution.Values)
            {
                temp += list.Count;
            }
            return temp;
        }
    }
}
