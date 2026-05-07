using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TSP.Interfaces;
using TSP.Mapping;

namespace TSP.Services
{
    public class TwoOptService : ITspSolver
    {
        private readonly PathCostService pathCostService;

        public TwoOptService(PathCostService pathCostService)
        {
            this.pathCostService = pathCostService;
        }

        public async Task<(List<int> Path, float Cost)> SolveAsync(Map map, CancellationToken ct)
        {
            return await Task.Run(() =>
            {
                int n = map.cities.Count;
                List<int> bestPath = Enumerable.Range(0, n).ToList();
                bestPath.Add(0);
                float bestPathCost = pathCostService.GetPathCost(bestPath, map);

                bool improved = true;
                while (improved)
                {
                    improved = false;
                    for (int i = 1; i < n - 1; i++)
                    {
                        for (int j = i + 1; j < n; j++)
                        {
                            if (ct.IsCancellationRequested)
                                return (bestPath, bestPathCost);
                            List<int> newPath = TwoOptSwap(bestPath, i, j);
                            float newCost = pathCostService.GetPathCost(newPath, map);
                            if (newCost < bestPathCost)
                            {
                                bestPath = newPath;
                                bestPathCost = newCost;
                                improved = true;
                            }
                        }
                    }
                }
                return (bestPath, bestPathCost);
            }, ct);
        }

        public void TwoOpt(List<int> route, Map map)
        {
            bool improved = true;
            int size = route.Count;

            while (improved)
            {
                improved = false;
                for (int i = 1; i < size - 2; i++)
                {
                    for (int j = i + 1; j < size - 1; j++)
                    {
                        int a = route[i - 1], b = route[i];
                        int c = route[j], d = route[j + 1];

                        float before = map.distanceMatrix[a][b] + map.distanceMatrix[c][d];
                        float after = map.distanceMatrix[a][c] + map.distanceMatrix[b][d];

                        if (after < before)
                        {
                            route.Reverse(i, j - i + 1);
                            improved = true;
                            goto OuterLoop;
                        }
                    }
                }
            OuterLoop:;
            }
        }

        private List<int> TwoOptSwap(List<int> path, int i, int k)
        {
            List<int> newPath = new List<int>();
            newPath.AddRange(path.Take(i));
            List<int> reversed = path.Skip(i).Take(k - i + 1).Reverse().ToList();
            newPath.AddRange(reversed);
            newPath.AddRange(path.Skip(k + 1));
            return newPath;
        }
    }
}
