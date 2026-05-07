using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TSP.Interfaces;
using TSP.Mapping;

namespace TSP.Services
{
    public class NearestNeighbourService : ITspSolver
    {
        public async Task<(List<int> Path, float Cost)> SolveAsync(Map map, CancellationToken ct)
        {
            return await Task.Run(() =>
            {
                float smallestDistance;
                int currentCity = 0;
                int nextCity = 0;
                float bestPathCost = 0;
                List<int> bestPath = new List<int>();
                bestPath.Add(currentCity);

                for (int i = 0; i < map.cities.Count - 1; i++)
                {
                    if (ct.IsCancellationRequested)
                        return (bestPath, bestPathCost);
                    smallestDistance = float.PositiveInfinity;
                    for (int j = 0; j < map.cities.Count; j++)
                    {
                        if (map.distanceMatrix[currentCity][j] < smallestDistance && !bestPath.Contains(j))
                        {
                            smallestDistance = map.distanceMatrix[currentCity][j];
                            nextCity = j;
                        }
                    }
                    bestPathCost += map.distanceMatrix[nextCity][currentCity];
                    currentCity = nextCity;
                    bestPath.Add(currentCity);
                }
                bestPathCost += map.distanceMatrix[currentCity][0];
                bestPath.Add(0);

                return (bestPath, bestPathCost);
            }, ct);
        }
    }
}
