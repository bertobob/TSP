using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TSP.Mapping;

namespace TSP.Interfaces
{
    public interface ITspSolver
    {
        Task<(List<int> Path, float Cost)> SolveAsync(Map map, CancellationToken ct);
    }
}
