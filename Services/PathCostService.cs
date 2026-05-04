using System.Collections.Generic;
using System.Linq;
using TSP.Mapping;

namespace TSP.Services
{
    public class PathCostService
    {
        public float GetPathCost(List<int> path, Map map)
        {
            float cost = 0;
            for (int i = 0; i < path.Count() - 1; i++)
            {
                cost += map.distanceMatrix[path.ElementAt(i)][path.ElementAt(i + 1)];
            }
            return cost;
        }
    }
}
