# Travelling Salesman Problem Solver

Solves the Travelling Salesman Problem using four different algorithms while comparing and visualizing their runtime and path cost.

<img width="2556" height="1390" alt="grafik" src="https://github.com/user-attachments/assets/f0764698-22e9-4958-8551-000d970b5d58" />

## Algorithms

**Brute Force** — Tries every possible permutation. Optimal solution guaranteed but only feasible for small city counts.

**Nearest Neighbour** — Greedy heuristic. Always moves to the closest unvisited city. Fast but suboptimal.

**2-Opt** — Local search optimization. Iteratively removes crossing edges to improve an initial solution.

**Genetic Algorithm** — Evolutionary approach with configurable parameters:
- Population size
- Tournament selection
- OX crossover
- Mutation (swap, inversion, insertion)
- Elitism
- Optional 2-opt local search per individual

## Tech Stack

- C# / .NET 8
- Avalonia UI
- Dependency Injection (`Microsoft.Extensions.DependencyInjection`)
- `async/await` + `CancellationToken` per algorithm

## Project Structure
```
Mapping/     — Map and distance matrix
Models/      — Individual (route + fitness)
Services/    — Algorithm implementations
Views/       — UI layer
```
## How to Run

```bash
git clone https://github.com/bertobob/TSP.git
cd TSP
dotnet run
```

## Features

- Generate random city maps up to 1000 cities
- Run all four algorithms independently and in parallel
- Cancel any running algorithm at any time
- Compare path cost and computation time side by side
