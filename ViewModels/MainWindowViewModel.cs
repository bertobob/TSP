using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using TSP.Config;
using TSP.Mapping;
using TSP.Services;

namespace TSP.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly BruteForceService bruteForceService;
        private readonly NearestNeighbourService nearestNeighbourService;
        private readonly TwoOptService twoOptService;
        private readonly PathCostService pathCostService;

        public Map Map { get; private set; } = new Map(600, 300);

        public MainWindowViewModel(
            PathCostService pathCostService,
            BruteForceService bruteForceService,
            NearestNeighbourService nearestNeighbourService,
            TwoOptService twoOptService)
        {
            this.pathCostService = pathCostService;
            this.bruteForceService = bruteForceService;
            this.nearestNeighbourService = nearestNeighbourService;
            this.twoOptService = twoOptService;

            Map.AddRandomCities(10);
            Map.CreateDistanceMatrix();
        }

        public void GenerateMap(int cityCount)
        {
            Map = new Map(600, 300);
            Map.AddRandomCities(cityCount);
            Map.CreateDistanceMatrix();
        }

        public async Task<(List<int> Path, float Cost, long Ms)> SolveBruteForceAsync(CancellationToken ct)
        {
            var sw = Stopwatch.StartNew();
            var result = await bruteForceService.SolveAsync(Map, ct);
            return (result.Path, result.Cost, sw.ElapsedMilliseconds);
        }

        public async Task<(List<int> Path, float Cost, long Ms)> SolveNearestNeighbourAsync(CancellationToken ct)
        {
            var sw = Stopwatch.StartNew();
            var result = await nearestNeighbourService.SolveAsync(Map, ct);
            return (result.Path, result.Cost, sw.ElapsedMilliseconds);
        }

        public async Task<(List<int> Path, float Cost, long Ms)> SolveTwoOptAsync(CancellationToken ct)
        {
            var sw = Stopwatch.StartNew();
            var result = await twoOptService.SolveAsync(Map, ct);
            return (result.Path, result.Cost, sw.ElapsedMilliseconds);
        }

        public async Task<(List<int> Path, float Cost, long Ms)> SolveGeneticAsync(
            GeneticSolverConfig config, CancellationToken ct)
        {
            var sw = Stopwatch.StartNew();
            var solver = new GeneticSolverService(pathCostService, twoOptService, config);
            var result = await solver.SolveAsync(Map, ct);
            return (result.Path, result.Cost, sw.ElapsedMilliseconds);
        }
    }
}
