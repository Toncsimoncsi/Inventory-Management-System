using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMS.Persistence;
using IMS.Persistence.Entities;

namespace IMS.Model.Simulation
{
    //Input:
    //Boolean 2d array of blocked fields - Boolean[,]
    //Starting Position - Pos
    //Goal poisition - Pos
    //Return:
    //Path from Position to Goal- List<Pos>
    //https://www.superscarysnakes.com/blackfuture/2016/10/26/basic-astar/
    public class Astar
    {

        Dictionary<Pos, bool> closedSet = new Dictionary<Pos, bool>();
        Dictionary<Pos, bool> openSet = new Dictionary<Pos, bool>();

        //cost of start to this key node
        Dictionary<Pos, int> gScore = new Dictionary<Pos, int>();
        //cost of start to goal, passing through key node
        Dictionary<Pos, int> fScore = new Dictionary<Pos, int>();

        Dictionary<Pos, Pos> nodeLinks = new Dictionary<Pos, Pos>();

        public List<Pos> FindPath(bool[,] graph, Pos start, Pos goal)
        {

            openSet[start] = true;
            gScore[start] = 0;
            fScore[start] = Heuristic(start, goal);

            while (openSet.Count > 0)
            {
                var current = nextBest();
                if (current.Equals(goal))
                {
                    return Reconstruct(current);
                }


                openSet.Remove(current);
                closedSet[current] = true;

                foreach (var neighbor in Neighbors(graph, current))
                {
                    if (closedSet.ContainsKey(neighbor))
                        continue;

                    var projectedG = getGScore(current) + 1;

                    if (!openSet.ContainsKey(neighbor))
                        openSet[neighbor] = true;
                    else if (projectedG >= getGScore(neighbor))
                        continue;

                    //record it
                    nodeLinks[neighbor] = current;
                    gScore[neighbor] = projectedG;
                    fScore[neighbor] = projectedG + Heuristic(neighbor, goal);

                }
            }


            return new List<Pos>();
        }

        private int Heuristic(Pos start, Pos goal)
        {
            var dx = goal.X - start.X;
            var dy = goal.Y - start.Y;
            return Math.Abs(dx) + Math.Abs(dy);
        }

        private int getGScore(Pos pt)
        {
            int score = int.MaxValue;
            gScore.TryGetValue(pt, out score);
            return score;
        }

        private int getFScore(Pos pt)
        {
            int score = int.MaxValue;
            fScore.TryGetValue(pt, out score);
            return score;
        }

        //diagonal movement not allowed
        public static IEnumerable<Pos> Neighbors(bool[,] graph, Pos center)
        {

            Pos pt = new Pos(center.X, center.Y - 1);
            if (IsValidNeighbor(graph, pt))
                yield return pt;

            //middle row
            pt = new Pos(center.X - 1, center.Y);
            if (IsValidNeighbor(graph, pt))
                yield return pt;

            pt = new Pos(center.X + 1, center.Y);
            if (IsValidNeighbor(graph, pt))
                yield return pt;

            //bottom row

            pt = new Pos(center.X, center.Y + 1);
            if (IsValidNeighbor(graph, pt))
                yield return pt;

        }

        public static bool IsValidNeighbor(bool[,] matrix, Pos pt)
        {
            int x = pt.X;
            int y = pt.Y;
            if (x < 0 || x >= matrix.GetLength(0))
                return false;

            if (y < 0 || y >= matrix.GetLength(1))
                return false;

            return matrix[x,y];

        }

        private List<Pos> Reconstruct(Pos current)
        {
            List<Pos> path = new List<Pos>();
            while (nodeLinks.ContainsKey(current))
            {
                path.Add(current);
                current = nodeLinks[current];
            }

            path.Reverse();
            return path;
        }

        private Pos nextBest()
        {
            int best = int.MaxValue;
            Pos bestPt = null;
            foreach (var node in openSet.Keys)
            {

                var score = getFScore(node);
                if (score < best)
                {
                    bestPt = node;
                    best = score;
                }
            }


            return bestPt;
       }
    }
}
