using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TSP.Mapping;

namespace TSP.Services
{
    public class BruteForceService
    {
        private readonly PathCostService pathCostService;

        public BruteForceService(PathCostService pathCostService)
        {
            this.pathCostService = pathCostService;
        }

        public async Task<(List<int> Path, float Cost)> SolveAsync(Map map, CancellationToken ct)
        {
            return await Task.Run(() =>
            {
                var liste = Enumerable.Range(1, map.cities.Count - 1).ToList();
                List<int> bestPath = new List<int>();
                float bestPathCost = float.PositiveInfinity;

                foreach (var element in Permute(liste))
                {
                    if (ct.IsCancellationRequested)
                        return (bestPath, bestPathCost);
                    element.Insert(0, 0);
                    element.Add(0);
                    float cost = pathCostService.GetPathCost(element, map);
                    if (cost < bestPathCost)
                    {
                        bestPathCost = cost;
                        bestPath = element;
                    }
                }
                return (bestPath, bestPathCost);
            }, ct);
        }

        private IEnumerable<List<int>> Permute(List<int> liste)
        {
            if (liste.Count == 1)
            {
                yield return liste;
                yield break;
            }
            foreach (var element in liste)
            {
                List<int> liste1 = new List<int>(liste);
                liste1.Remove(element);
                foreach (var permutation in Permute(liste1))
                {
                    yield return permutation.Prepend(element).ToList();
                }
            }
        }
    }
}
