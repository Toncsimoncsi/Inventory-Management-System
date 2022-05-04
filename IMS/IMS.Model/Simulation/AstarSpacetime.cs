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
    //Boolean Dictionary of blocked fields - Dictionary<int,hashSet<Pos>>
    //Starting Position - Pos
    //Goal poisition - Pos
    //Return:
    //Path from Position to Goal- List<Pos>
    //node = Find paths for individual agents with no constraints.

    //https://www.superscarysnakes.com/blackfuture/2016/10/26/basic-astar/
    //https://www.davidsilver.uk/wp-content/uploads/2020/03/coop-path-AIWisdom.pdf
    public class AstarSpacetime
    {
        public AstarSpacetime(int x,int y)
        {
            SizeX = x;
            SizeY = y;
        }
        public int SizeX { get; set; }
        public int SizeY { get; set; }

        Dictionary<Pos, bool> closedSet = new Dictionary<Pos, bool>();
        Dictionary<Pos, bool> openSet = new Dictionary<Pos, bool>();

        //cost of start to this key node
        Dictionary<Pos, int> gScore = new Dictionary<Pos, int>();
        //cost of start to goal, passing through key node
        Dictionary<Pos, int> fScore = new Dictionary<Pos, int>();
        //path
        Dictionary<Pos, Pos> nodeLinks = new Dictionary<Pos, Pos>();

        //robots that have stopped
        Dictionary<int, HashSet<Pos>> staticObstacles = new Dictionary<int, HashSet<Pos>>();
        //robots that are moving 
        Dictionary<int, HashSet<Pos>> dynamicObstacles = new Dictionary<int, HashSet<Pos>>();

        public List<Pos> FindPath( Dictionary<int, HashSet<Pos>> dynamicObstacle, Dictionary<int, HashSet<Pos>> robotObstacles, int startTime, Pos start, Pos goal)
        {

            Clear();
            dynamicObstacles = dynamicObstacle;
            staticObstacles = robotObstacles;
            openSet[start] = true;
            gScore[start] = 0;
            fScore[start] = Heuristic(start, goal);
            int localTime = startTime;

            while (openSet.Count > 0)
            {
                var current = nextBest();
                if (current.Equals(goal))
                {
                    return Reconstruct(current);
                }


                openSet.Remove(current);
                closedSet[current] = true;

                foreach (var neighbor in Neighbors(getGraph(getGScore(current)), current))
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


        private void Clear()
        {
            closedSet.Clear();
            openSet.Clear();
            gScore.Clear();
            fScore.Clear();
            nodeLinks.Clear();
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
        //remake graph that has blocked points
        public bool[,] getGraph(int time)
        {
            bool[,] tempGraph = new bool[SizeX, SizeY];
            
            if (dynamicObstacles.ContainsKey(time))
            {
                foreach (Pos positions in dynamicObstacles[time])
                {
                    tempGraph[positions.X, positions.Y] = true;

                }
            }

            foreach (KeyValuePair<int, HashSet<Pos>> entry in staticObstacles)
            {
                if (time > entry.Key)
                {
                    foreach (Pos positions in entry.Value)
                    {
                        tempGraph[positions.X, positions.Y] = true;
                    }
                }
            }
            return tempGraph;
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

            return !matrix[x, y];

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
