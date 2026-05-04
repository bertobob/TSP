using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using System.Linq;

using TSP.Mapping;
using TSP.Services;

namespace TSP.Views
{
    public partial class MainWindow : Window
    {
        private Map map = new Map(600, 300);

        private readonly PathCostService pathCostService;
        private readonly BruteForceService bruteForceService;
        private readonly NearestNeighbourService nearestNeighbourService;
        private readonly TwoOptService twoOptService;
        private readonly GeneticSolverService geneticSolverService;

        private CancellationTokenSource? _ctsBruteForce;
        private CancellationTokenSource? _ctsNearestNeighbour;
        private CancellationTokenSource? _ctsTwoOpt;
        private CancellationTokenSource? _ctsEvolution;

        public Dictionary<int, (Canvas canvas, TextBlock cost, TextBlock compute)> widgets;

        public MainWindow(
            PathCostService pathCostService,
            BruteForceService bruteForceService,
            NearestNeighbourService nearestNeighbourService,
            TwoOptService twoOptService,
            GeneticSolverService geneticSolverService)
        {
            InitializeComponent();

            this.pathCostService = pathCostService;
            this.bruteForceService = bruteForceService;
            this.nearestNeighbourService = nearestNeighbourService;
            this.twoOptService = twoOptService;
            this.geneticSolverService = geneticSolverService;

            widgets = new Dictionary<int, (Canvas canvas, TextBlock cost, TextBlock compute)>
            {
                {1,(Canvas1,Cost1,Compute1) },
                {2,(Canvas2,Cost2,Compute2) },
                {3,(Canvas3,Cost3,Compute3) },
                {4,(Canvas4,Cost4,Compute4) }
            };

            map.AddRandomCities(10);
            map.CreateDistanceMatrix();
            DrawMap();
        }

        public void generateMap(object? sender, RoutedEventArgs args)
        {
            this.map = new Map(600, 300);
            if (int.TryParse(cityCount.Text, out var anzahl))
            {
                anzahl = Math.Clamp(anzahl, 3, 1000);
                cityCount.Text = anzahl.ToString();
                this.map.AddRandomCities(anzahl);
                this.map.CreateDistanceMatrix();
                DrawMap();
                ResetStats();
            }
        }
        private void ResetStats()
        {
            foreach (var widget in widgets.Values)
            {
                widget.cost.Text = "—";
                widget.compute.Text = "—";
            }
        }
        public async void doBruteForce(object sender, RoutedEventArgs args)
        {
            _ctsBruteForce = new CancellationTokenSource();
            BruteForceButton.IsEnabled = false;
            CancelBruteForce.IsEnabled = true;
            var stopwatch = Stopwatch.StartNew();
            try
            {
                var result = await bruteForceService.SolveAsync(map, _ctsBruteForce.Token);
                DrawPath(1, result.Path, result.Cost, stopwatch.ElapsedMilliseconds);
            }
            catch (OperationCanceledException)
            {
                Cost1.Text = "Cancelled";
                Compute1.Text = "—";
            }
            finally
            {
                BruteForceButton.IsEnabled = true;
                CancelBruteForce.IsEnabled = false;
            }
        }

        public void cancelBruteForce(object sender, RoutedEventArgs args)
        {
            _ctsBruteForce?.Cancel();
        }

        public async void doNearestNeighbour(object sender, RoutedEventArgs args)
        {
            _ctsNearestNeighbour = new CancellationTokenSource();
            NearestNeighbourButton.IsEnabled = false;
            CancelNearestNeighbour.IsEnabled = true;
            var stopwatch = Stopwatch.StartNew();
            try
            {
                var result = await nearestNeighbourService.SolveAsync(map, _ctsNearestNeighbour.Token);
                DrawPath(2, result.Path, result.Cost, stopwatch.ElapsedMilliseconds);
            }
            catch (OperationCanceledException)
            {
                Cost2.Text = "Cancelled";
                Compute2.Text = "—";
            }
            finally
            {
                NearestNeighbourButton.IsEnabled = true;
                CancelNearestNeighbour.IsEnabled = false;
            }
        }

        public void cancelNearestNeighbour(object sender, RoutedEventArgs args)
        {
            _ctsNearestNeighbour?.Cancel();
        }

        public async void doTwoOpt(object sender, RoutedEventArgs args)
        {
            _ctsTwoOpt = new CancellationTokenSource();
            TwoOptButton.IsEnabled = false;
            CancelTwoOpt.IsEnabled = true;
            var stopwatch = Stopwatch.StartNew();
            try
            {
                var result = await twoOptService.SolveAsync(map, _ctsTwoOpt.Token);
                DrawPath(3, result.Path, result.Cost, stopwatch.ElapsedMilliseconds);
            }
            catch (OperationCanceledException)
            {
                Cost3.Text = "Cancelled";
                Compute3.Text = "—";
            }
            finally
            {
                TwoOptButton.IsEnabled = true;
                CancelTwoOpt.IsEnabled = false;
            }
        }

        public void cancelTwoOpt(object sender, RoutedEventArgs args)
        {
            _ctsTwoOpt?.Cancel();
        }

        public async void doEvolution(object sender, RoutedEventArgs args)
        {
            _ctsEvolution = new CancellationTokenSource();
            EvolutionButton.IsEnabled = false;
            CancelEvolution.IsEnabled = true;
            var stopwatch = Stopwatch.StartNew();

            int populationSize = (int)Math.Round(startPop.Value) * this.map.cities.Count;
            int RNNCount = (int)Math.Round(RNN.Value);
            double RNNProbability = 0.5;
            int sequenzLength = (int)(Math.Round(OXSequenzLength.Value) * 0.01 * this.map.cities.Count);
            double mutationProbability = mutationProb.Value;
            int sizeTournament = Math.Max(2, ((int)(Math.Round(tournamentSize.Value) * 0.01 * populationSize)));
            double elitismPercentage = elitism.Value;
            int maxGenerations = (int)Math.Round(maxGens.Value);
            double twoOptProbability = twooptProb.Value / 100;

            try
            {
                var result = await geneticSolverService.SolveAsync(map, populationSize, RNNCount, RNNProbability,
                    sequenzLength, mutationProbability, sizeTournament, elitismPercentage,
                    maxGenerations, twoOptProbability, _ctsEvolution.Token);
                DrawPath(4, result.Path, result.Cost, stopwatch.ElapsedMilliseconds);
            }
            catch (OperationCanceledException)
            {
                Cost4.Text = "Cancelled";
                Compute4.Text = "—";
            }
            finally
            {
                EvolutionButton.IsEnabled = true;
                CancelEvolution.IsEnabled = false;
            }
        }

        public void cancelEvolution(object sender, RoutedEventArgs args)
        {
            _ctsEvolution?.Cancel();
        }

        private void DrawMap()
        {
            IBrush defaultColor = new SolidColorBrush(Color.Parse("#546E7A"));
            IBrush startColor = new SolidColorBrush(Color.Parse("#E53935"));
            for (int i = 1; i <= this.widgets.Count(); i++)
            {
                this.widgets[i].canvas.Children.Clear();
            }
            for (int i = 0; i < map.cities.Count; i++)
            {
                IBrush color = i == 0 ? startColor : defaultColor;
                for (int j = 1; j <= this.widgets.Count(); j++)
                {
                    var dot = new Ellipse
                    {
                        Width = 6,
                        Height = 6,
                        Fill = color,
                    };
                    this.widgets[j].canvas.Children.Add(dot);
                    Canvas.SetLeft(dot, map.cities[i].Item1 - 3);
                    Canvas.SetTop(dot, map.cities[i].Item2 - 3);
                }
            }
        }

        private void DrawPath(int canvas, List<int> path, float cost, long duration)
        {
            this.widgets[canvas].cost.Text = ((int)cost).ToString();
            this.widgets[canvas].compute.Text = ((double)duration / 1000).ToString("F3") + "s";
            var toRemove = this.widgets[canvas].canvas.Children
                           .OfType<Line>()
                           .ToList();

            foreach (var line in toRemove)
                this.widgets[canvas].canvas.Children.Remove(line);

            for (int i = 0; i < path.Count - 1; i++)
            {
                var line = new Line
                {
                    StartPoint = new Point(map.cities[path[i]].Item1, map.cities[path[i]].Item2),
                    EndPoint = new Point(map.cities[path[i + 1]].Item1, map.cities[path[i + 1]].Item2),
                    Stroke = new SolidColorBrush(Color.Parse("#37474F")),
                    StrokeThickness = 1
                };
                this.widgets[canvas].canvas.Children.Add(line);
            }
        }
    }
}
