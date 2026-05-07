using Avalonia;
using Microsoft.Extensions.DependencyInjection;
using System;
using TSP.Config;
using TSP.Services;
using TSP.ViewModels;
using TSP.Views;

namespace TSP
{
    internal sealed class Program
    {
        public static IServiceProvider Services { get; private set; }

        [STAThread]
        public static void Main(string[] args)
        {
            var services = new ServiceCollection();

            services.AddSingleton<PathCostService>();
            services.AddSingleton<TwoOptService>();
            services.AddSingleton<BruteForceService>();
            services.AddSingleton<NearestNeighbourService>();
            services.AddTransient<MainWindowViewModel>();
            services.AddTransient<MainWindow>();

            Services = services.BuildServiceProvider();

            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }

        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace();
    }
}
