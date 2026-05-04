using System.Collections.Generic;

namespace TSP.Models
{
    public class Individual
    {
        public List<int> Route { get; set; }
        public float Fitness { get; set; }

        public Individual()
        {
            Route = new List<int>();
            Fitness = 0;
        }

        public Individual(List<int> route, float fitness)
        {
            this.Route = new List<int>(route);
            this.Fitness = fitness;
        }
    }
}
