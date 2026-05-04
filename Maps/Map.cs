using System;
using System.Collections.Generic;

namespace TSP.Mapping
{
	public class Map
	{
		private int height;
		private int width;
		public List<(int, int)> cities;
		private Random rand = new Random();
		public float[][] distanceMatrix;

		public Map(int height, int width)
		{
			this.height = height;
			this.width = width;
			this.cities = new List<(int, int)>();
		}

		public void AddRandomCities(int cityCount)
		{
			int randomHeight, randomWidth;
			bool alreadyExists = false;
			for (int i = 0; i < cityCount; i++)
			{
				alreadyExists = true;
				while (alreadyExists)
				{
					randomHeight = rand.Next(0, this.height);
					randomWidth = rand.Next(0, this.width);
					if (!this.cities.Contains((randomHeight, randomWidth)))
					{
						this.cities.Add((randomHeight, randomWidth));
						alreadyExists = false;
					}
				}
			}
		}

		public void CreateDistanceMatrix()
		{
			this.distanceMatrix = new float[this.cities.Count][];
			for (int i = 0; i < this.cities.Count; i++)
			{
				this.distanceMatrix[i] = new float[this.cities.Count];
				for (int j = 0; j < this.cities.Count; j++)
				{
					if (i == j) this.distanceMatrix[i][j] = float.PositiveInfinity;
					else distanceMatrix[i][j] = GetDistance(this.cities[i], this.cities[j]);
				}
			}
		}

		public float GetDistance((int x, int y) a, (int x, int y) b)
		{
			return (float)(Math.Sqrt(((a.x - b.x) * (a.x - b.x) + (a.y - b.y) * (a.y - b.y))));
		}
	}
}
