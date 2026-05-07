using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSP.Config
{
    public class GeneticSolverConfig
    {
        public int PopulationSize { get; set; }
        public int RNNCount { get; set; }
        public double RNNProbability { get; set; }
        public int SequenzLength { get; set; }
        public double MutationProbability { get; set; }
        public int SizeTournament { get; set; }
        public double ElitismPercentage { get; set; }
        public int MaxGenerations { get; set; }
        public double TwoOptProbability { get; set; }
    }
}
