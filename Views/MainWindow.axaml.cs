using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TSP.Config;
using TSP.ViewModels;

namespace TSP.Views
{
    public partial class MainWindow : Window
    {
        private readonly MainWindowViewModel _vm;

        private CancellationTokenSource? _ctsBruteForce;
        private CancellationTokenSource? _ctsNearestNeighbour;
        private CancellationTokenSource? _ctsTwoOpt;
        private CancellationTokenSource? _ctsEvolution;

        private Dictionary<int, (Canvas canvas, TextBlock cost, TextBlock compute)> _widgets;

        public MainWindow(MainWindowViewModel viewModel)
        {
            InitializeComponent();

            _vm = viewModel;

            _widgets = new Dictionary<int, (Canvas canvas, TextBlock cost, TextBlock compute)>
            {
                { 1, (Canvas1, Cost1, Compute1) },
                { 2, (Canvas2, Cost2, Compute2) },
                { 3, (Canvas3, Cost3, Compute3) },
                { 4, (Canvas4, Cost4, Compute4) }
            };

            DrawMap();
        }

        public void GenerateMap(object? sender, RoutedEventArgs args)
        {
            if (int.TryParse(cityCount.Text, out var anzahl))
            {
                anzahl = Math.Clamp(anzahl, 3, 1000);
                cityCount.Text = anzahl.ToString();
                _vm.GenerateMap(anzahl);
                DrawMap();
                ResetStats();
            }
        }

        private void ResetStats()
        {
            foreach (var widget in _widgets.Values)
            {
                widget.cost.Text = "—";
                widget.compute.Text = "—";
            }
        }

        public async void DoBruteForce(object sender, RoutedEventArgs args)
        {
            _ctsBruteForce = new CancellationTokenSource();
            BruteForceButton.IsEnabled = false;
            CancelBruteForce.IsEnabled = true;
            try
            {
                var result = await _vm.SolveBruteForceAsync(_ctsBruteForce.Token);
                DrawPath(1, result.Path, result.Cost, result.Ms);
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

        public void cancelBruteForce(object sender, RoutedEventArgs args) => _ctsBruteForce?.Cancel();

        public async void DoNearestNeighbour(object sender, RoutedEventArgs args)
        {
            _ctsNearestNeighbour = new CancellationTokenSource();
            NearestNeighbourButton.IsEnabled = false;
            CancelNearestNeighbour.IsEnabled = true;
            try
            {
                var result = await _vm.SolveNearestNeighbourAsync(_ctsNearestNeighbour.Token);
                DrawPath(2, result.Path, result.Cost, result.Ms);
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

        public void cancelNearestNeighbour(object sender, RoutedEventArgs args) => _ctsNearestNeighbour?.Cancel();

        public async void doTwoOpt(object sender, RoutedEventArgs args)
        {
            _ctsTwoOpt = new CancellationTokenSource();
            TwoOptButton.IsEnabled = false;
            CancelTwoOpt.IsEnabled = true;
            try
            {
                var result = await _vm.SolveTwoOptAsync(_ctsTwoOpt.Token);
                DrawPath(3, result.Path, result.Cost, result.Ms);
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

        public void cancelTwoOpt(object sender, RoutedEventArgs args) => _ctsTwoOpt?.Cancel();

        public async void doEvolution(object sender, RoutedEventArgs args)
        {
            _ctsEvolution = new CancellationTokenSource();
            EvolutionButton.IsEnabled = false;
            CancelEvolution.IsEnabled = true;

            int populationSize = (int)Math.Round(startPop.Value) * _vm.Map.cities.Count;
            var config = new GeneticSolverConfig
            {
                PopulationSize = populationSize,
                RNNCount = (int)Math.Round(RNN.Value),
                RNNProbability = 0.5,
                SequenzLength = (int)(Math.Round(OXSequenzLength.Value) * 0.01 * _vm.Map.cities.Count),
                MutationProbability = mutationProb.Value,
                SizeTournament = Math.Max(2, (int)(Math.Round(tournamentSize.Value) * 0.01 * populationSize)),
                ElitismPercentage = elitism.Value,
                MaxGenerations = (int)Math.Round(maxGens.Value),
                TwoOptProbability = twooptProb.Value / 100
            };

            try
            {
                var result = await _vm.SolveGeneticAsync(config, _ctsEvolution.Token);
                DrawPath(4, result.Path, result.Cost, result.Ms);
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

        public void cancelEvolution(object sender, RoutedEventArgs args) => _ctsEvolution?.Cancel();

        private void DrawMap()
        {
            IBrush defaultColor = new SolidColorBrush(Color.Parse("#546E7A"));
            IBrush startColor = new SolidColorBrush(Color.Parse("#E53935"));

            for (int i = 1; i <= _widgets.Count; i++)
                _widgets[i].canvas.Children.Clear();

            for (int i = 0; i < _vm.Map.cities.Count; i++)
            {
                IBrush color = i == 0 ? startColor : defaultColor;
                for (int j = 1; j <= _widgets.Count; j++)
                {
                    var dot = new Ellipse { Width = 6, Height = 6, Fill = color };
                    _widgets[j].canvas.Children.Add(dot);
                    Canvas.SetLeft(dot, _vm.Map.cities[i].Item1 - 3);
                    Canvas.SetTop(dot, _vm.Map.cities[i].Item2 - 3);
                }
            }
        }

        private void DrawPath(int canvas, List<int> path, float cost, long duration)
        {
            _widgets[canvas].cost.Text = ((int)cost).ToString();
            _widgets[canvas].compute.Text = ((double)duration / 1000).ToString("F3") + "s";

            var toRemove = _widgets[canvas].canvas.Children.OfType<Line>().ToList();
            foreach (var line in toRemove)
                _widgets[canvas].canvas.Children.Remove(line);

            for (int i = 0; i < path.Count - 1; i++)
            {
                var line = new Line
                {
                    StartPoint = new Point(_vm.Map.cities[path[i]].Item1, _vm.Map.cities[path[i]].Item2),
                    EndPoint = new Point(_vm.Map.cities[path[i + 1]].Item1, _vm.Map.cities[path[i + 1]].Item2),
                    Stroke = new SolidColorBrush(Color.Parse("#37474F")),
                    StrokeThickness = 1
                };
                _widgets[canvas].canvas.Children.Add(line);
            }
        }
    }
}
