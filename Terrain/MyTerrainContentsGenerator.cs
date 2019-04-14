using Engine;
using System;

namespace Game
{
	public class MyTerrainContentsGenerator : ITerrainContentsGenerator
	{
		private ITerrainContentsGenerator mSystemTerrainContentsGenerator;

		private SubsystemGameInfo mSubsystemGameInfo;

		private SubsystemTerrain mSubsystemTerrain;

		private int mSeed;

		public int OceanLevel
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public MyTerrainContentsGenerator(SubsystemTerrain mSubsystemTerrain)
		{
			this.mSubsystemTerrain = mSubsystemTerrain;
			mSubsystemGameInfo = this.mSubsystemTerrain.Project.FindSubsystem<SubsystemGameInfo>();
			mSeed = mSubsystemGameInfo.WorldSeed;
			mSystemTerrainContentsGenerator = this.mSubsystemTerrain.TerrainContentsGenerator_;
		}

		public float CalculateHeight(float x, float z)
		{
			throw new NotImplementedException();
		}

		public int CalculateHumidity(float x, float z)
		{
			throw new NotImplementedException();
		}

		public float CalculateMountainRangeFactor(float x, float z)
		{
			throw new NotImplementedException();
		}

		public float CalculateOceanShoreDistance(float x, float z)
		{
			throw new NotImplementedException();
		}

		public int CalculateTemperature(float x, float z)
		{
			throw new NotImplementedException();
		}

		public Vector3 FindCoarseSpawnPosition()
		{
			throw new NotImplementedException();
		}

		public void GenerateChunkContentsPass1(TerrainChunk chunk)
		{
			mSystemTerrainContentsGenerator.GenerateChunkContentsPass1(chunk);
		}

		public void GenerateChunkContentsPass2(TerrainChunk chunk)
		{
			mSystemTerrainContentsGenerator.GenerateChunkContentsPass2(chunk);
		}

		public void GenerateChunkContentsPass3(TerrainChunk chunk)
		{
			mSystemTerrainContentsGenerator.GenerateChunkContentsPass3(chunk);
		}

		public void GenerateChunkContentsPass4(TerrainChunk chunk)
		{
			GenerateArea(chunk);
			mSystemTerrainContentsGenerator.GenerateChunkContentsPass4(chunk);
		}

		private void GenerateHouses(TerrainChunk chunk)
		{
			int x = chunk.Coords.X;
			int y = chunk.Coords.Y;
			var random = new Random(mSeed + x * 1000 + 2764 * y);
			var random2 = new Random(mSeed + x * random.UniformInt(1, 1000) + y * random.UniformInt(0, 2000));
			if (!random2.Bool(0.7f))
			{
				return;
			}
			int num = 0;
			for (int num2 = 126; num2 >= 0; num2--)
			{
				if (num2 == 0)
				{
					return;
				}
				num = num2;
				int num3 = Terrain.ExtractContents(chunk.GetCellValueFast(0, num2, 0));
				if (num3 != 0)
				{
					if (num3 != 2 && num3 != 3 && num3 != 7 && num3 != 8 && num3 != 66 && num3 != 61)
					{
						return;
					}
					for (int i = 0; i < 16; i++)
					{
						for (int j = 0; j < 16; j++)
						{
							if (Terrain.ExtractContents(chunk.GetCellValueFast(i, num2 + 1, j)) != 0 || (Terrain.ExtractContents(chunk.GetCellValueFast(i, num2, j)) == 0 && Terrain.ExtractContents(chunk.GetCellValueFast(i, num2 - 1, j)) == 0))
							{
								return;
							}
						}
					}
					break;
				}
			}
			string text = ContentManager.Get<string>("TerrainHouse/Houses/HouseData").Replace("\r", string.Empty);
			string[] array = text.Split(new char[1]
			{
				'\n'
			}, StringSplitOptions.RemoveEmptyEntries);
			int num4 = random2.UniformInt(0, array.Length - 1);
			string[] array2 = ContentManager.Get<string>(array[num4]).Split(new char[1]
			{
				'\n'
			}, StringSplitOptions.RemoveEmptyEntries);
			for (int k = 1; k < array2.Length; k++)
			{
				string[] array3 = array2[k].Split(new char[1]
				{
					','
				}, StringSplitOptions.RemoveEmptyEntries);
				chunk.SetCellValueFast(int.Parse(array3[0]), num + int.Parse(array3[1]) + 1, int.Parse(array3[2]), int.Parse(array3[3]));
			}
		}

		private void GenerateArea(TerrainChunk chunk)
		{
			int x = chunk.Coords.X;
			int y = chunk.Coords.Y;
			var random = new Random(mSeed + x / 10 * 1000 + 2764 * (y / 10));
			if (random.Bool(0.3f) && Math.Abs(x) % 10 < 5 && Math.Abs(y) % 10 < 5)
			{
				GenerateHouses(chunk);
			}
		}
	}
}
