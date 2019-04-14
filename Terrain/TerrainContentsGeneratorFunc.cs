using Engine;
using System;

namespace Game
{
	public class TerrainContentsGeneratorFunc : TerrainContentsGeneratorFlat, ITerrainContentsGenerator
	{
		public Func<float, float, float> GetHeight;
		//private static int A = 1;

		public TerrainContentsGeneratorFunc(SubsystemTerrain subsystemTerrain) : base(subsystemTerrain)
		{
		}

		public new Vector3 FindCoarseSpawnPosition()
		{
			return new Vector3(m_oceanCorner.X, CalculateHeight(m_oceanCorner.X, m_oceanCorner.Y), m_oceanCorner.Y);
		}

		public new void GenerateChunkContentsPass1(TerrainChunk chunk)
		{
			for (int i = 0; i < 16; i++)
			{
				for (int j = 0; j < 16; j++)
				{
					float x = i + chunk.Origin.X, z = j + chunk.Origin.Y;
					chunk.SetTemperatureFast(i, j, CalculateTemperature(x, z));
					chunk.SetHumidityFast(i, j, CalculateHumidity(x, z));
					int num = TerrainChunk.CalculateCellIndex(i, 0, j),
						height = MathUtils.Clamp((int)GetHeight(x, z), 0, 128),
						k = 0;
					for (; k < height; k++)
					{
						chunk.SetCellValueFast(num + k, Terrain.MakeBlockValue(m_worldSettings.TerrainBlockIndex));
					}
					for (;k < 128; k++)
					{
						chunk.SetCellValueFast(num + k, Terrain.MakeBlockValue(0));
					}
				}
			}
		}
	}
}