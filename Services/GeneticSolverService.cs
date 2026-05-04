using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MoreLinq;
using TSP.Mapping;
using TSP.Models;

namespace TSP.Services
{
    public class GeneticSolverService
    {
        private readonly PathCostService pathCostService;
        private readonly TwoOptService twoOptService;

        public GeneticSolverService(PathCostService pathCostService, TwoOptService twoOptService)
        {
            this.pathCostService = pathCostService;
            this.twoOptService = twoOptService;
        }

        public async Task<(List<int> Path, float Cost)> SolveAsync(Map map, int populationSize, int RNNCount,
            double RNNProbability, int sequenzLength, double mutationProbability, int sizeTournament,
            double elitismPercentage, int maxGenerations, double twoOptProbability, CancellationToken ct)
        {
            return await Task.Run(() =>
            {
                Random twoOptRandom = new Random();
                Random tournamentRandom = new Random();
                Random mutationRandom = new Random();

                Individual parent1 = new Individual();
                Individual parent2 = new Individual();
                Individual child = new Individual();

                List<Individual> tournament;
                List<Individual> population = new List<Individual>();
                List<Individual> nextGen;
                CreateStartingPopulation(population, map, populationSize, ct, RNNCount, RNNProbability);

                int generationCounter = 0;
                while (generationCounter < maxGenerations && !ct.IsCancellationRequested)
                {
                   
                    nextGen = new List<Individual>();

                    if (elitismPercentage > 0)
                    {
                        population.Sort((a, b) => a.Fitness.CompareTo(b.Fitness));
                        for (int i = 0; i < (int)(elitismPercentage * 0.01 * populationSize); i++)
                        {
                            nextGen.Add(population[i]);
                        }
                    }

                    while (nextGen.Count < population.Count && !ct.IsCancellationRequested)
                    {
                        tournament = new List<Individual>();
                        for (int i = 0; i < sizeTournament; i++)
                        {
                            tournament.Add(population[tournamentRandom.Next(0, population.Count)]);
                        }
                        parent1 = tournament.MinBy(a => a.Fitness)!;

                        tournament = new List<Individual>();
                        for (int i = 0; i < sizeTournament; i++)
                        {
                            tournament.Add(population[tournamentRandom.Next(0, population.Count)]);
                        }
                        parent2 = tournament.MinBy(a => a.Fitness)!;

                        child = GetChildOX(parent1, parent2, sequenzLength, map);
                        if (mutationRandom.NextDouble() < mutationProbability)
                        {
                            Mutate(child.Route);
                            child.Fitness = pathCostService.GetPathCost(child.Route, map);
                        }
                        if (twoOptRandom.NextDouble() < twoOptProbability)
                        {
                            twoOptService.TwoOpt(child.Route, map);
                            child.Fitness = pathCostService.GetPathCost(child.Route, map);
                        }
                        nextGen.Add(child);
                    }
                    population = nextGen;
                    generationCounter++;
                }

                Individual winner = population.MinBy(a => a.Fitness)!;
                return (winner.Route, winner.Fitness);
            }, ct);
        }

        private void CreateStartingPopulation(List<Individual> pop, Map map, int populationSize, CancellationToken ct, int RNNCount = 2, double RNNProbability = 0)
        {
            Random rnd = new Random();
            for (int i = 0; i < populationSize; i++)
            {
                if(ct.IsCancellationRequested) { break; }
                if (rnd.NextDouble() < RNNProbability)
                    pop.Add(DoRNN(map, RNNCount));
                else
                    pop.Add(DoRandom(map));
            }
        }

        private Individual DoRNN(Map map, int RNNCount)
        {
            Random rand = new Random();
            int currentCity = 0;
            int nextCity = 0;
            float pathCost = 0;
            List<int> path = new List<int>();
            List<(int stadt, float distance)> connectAllCities = new List<(int, float)>();
            path.Add(currentCity);

            for (int i = 0; i < map.cities.Count - 1; i++)
            {
                connectAllCities.Clear();
                for (int j = 0; j < map.cities.Count; j++)
                {
                    if (!path.Contains(j))
                        connectAllCities.Add((j, map.distanceMatrix[currentCity][j]));
                }
                connectAllCities = connectAllCities.OrderBy(x => x.distance).Take(RNNCount).ToList();
                nextCity = connectAllCities[rand.Next(connectAllCities.Count)].stadt;
                pathCost += map.distanceMatrix[nextCity][currentCity];
                currentCity = nextCity;
                path.Add(currentCity);
            }
            pathCost += map.distanceMatrix[currentCity][0];
            path.Add(0);

            return new Individual(path, pathCost);
        }

        private Individual DoRandom(Map map)
        {
            List<int> route = Enumerable.Range(1, map.cities.Count - 1).ToList();
            Random rand = new Random();
            int n = route.Count;
            int k;
            while (n > 0)
            {
                k = rand.Next(n--);
                (route[n], route[k]) = (route[k], route[n]);
            }
            route.Insert(0, 0);
            route.Add(0);
            return new Individual(route, pathCostService.GetPathCost(route, map));
        }

        private Individual GetChildOX(Individual parent1, Individual parent2, int sequenzLength, Map map)
        {
            Random r = new Random();
            List<int> childRoute = new List<int>();
            List<int> parent2Route = new List<int>(parent2.Route);
            int sequenzPosition = r.Next(2, parent1.Route.Count - sequenzLength - 1);
            List<int> sequenz = parent1.Route.GetRange(sequenzPosition, sequenzLength);
            parent2Route.RemoveAll(x => sequenz.Contains(x));
            childRoute.AddRange(parent2Route.GetRange(0, sequenzPosition - 1));
            childRoute.AddRange(sequenz);
            childRoute.AddRange(parent2Route.Skip(sequenzPosition - 1));
            return new Individual(childRoute, pathCostService.GetPathCost(childRoute, map));
        }

        private void Mutate(List<int> route)
        {
            Random rand = new Random();
            int len = route.Count - 1;
            double roll = rand.NextDouble();

            if (roll < 0.1)
            {
                int i = rand.Next(1, len - 1);
                int j = rand.Next(1, len - 1);
                (route[i], route[j]) = (route[j], route[i]);
            }
            else if (roll < 0.7)
            {
                int start = rand.Next(1, len - 2);
                int end = rand.Next(start + 1, len - 1);
                route.Reverse(start, end - start + 1);
            }
            else
            {
                int from = rand.Next(1, len - 1);
                int to = rand.Next(1, len - 1);
                if (from != to)
                {
                    int city = route[from];
                    route.RemoveAt(from);
                    if (to > from) to--;
                    route.Insert(to, city);
                }
            }
        }
    }
}
