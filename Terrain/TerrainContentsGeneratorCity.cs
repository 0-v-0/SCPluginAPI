using Engine;
using System;

namespace Game
{
	/*public class TerrainContentsGeneratorCity : TerrainContentsGeneratorFunc
	{
		public WorldInfo m_worldInfo;
		public int BaseHeight;
		public bool TGExtras;
		public bool TGCaves;
		public City City;//CityGroup

		public TerrainContentsGeneratorCity(SubsystemTerrain subsystemTerrain) : base(subsystemTerrain)
		{
		}

		public new void GenerateChunkContentsPass1(TerrainChunk chunk)
		{
		}
	}*/
	public class TerrainContentsGeneratorMess : TerrainContentsGeneratorFlat, ITerrainContentsGenerator
	{
		protected int[] Cells;
		public new Vector3 FindCoarseSpawnPosition()
		{
			return new Vector3(m_oceanCorner.X, 126, m_oceanCorner.Y);
		}

		public TerrainContentsGeneratorMess(SubsystemTerrain subsystemTerrain) : base(subsystemTerrain)
		{
		}

		public new void GenerateChunkContentsPass1(TerrainChunk chunk)
		{
			int[] a = Cells;
			for (int i = 0; i < 16; i++)
			{
				for (int j = 0; j < 16; j++)
				{
					float x = i + chunk.Origin.X, z = j + chunk.Origin.Y;
					chunk.SetTemperatureFast(i, j, CalculateTemperature(x, z));
					chunk.SetHumidityFast(i, j, CalculateHumidity(x, z));
				}
			}
			if (a != null)
			{
				a = chunk.Cells = Gen(a, 8, 128);
				return;
			}
			a = chunk.Cells;
			a[2+256*54+122] = 1;
			a[3+256*54+122] = 2;
			a[11+256*54+122] = 2;
			a[12+256*54+122] = 1;
			a[257+256*54+122] = 2;
			a[259+256*54+122] = 1;
			a[264+256*54+122] = 2;
			a[265+256*54+122] = 1;
			a[267+256*54+122] = 2;
			a[268+256*54+122] = 1;
			a[514+256*54+122] = 1;
			a[515+256*54+122] = 1;
			a[516+256*54+122] = 2;
			a[519+256*54+122] = 1;
			a[520+256*54+122] = 1;
			a[522+256*54+122] = 2;
			a[523+256*54+122] = 1;
			a[771+256*54+122] = 2;
			a[773+256*54+122] = 1;
			a[775+256*54+122] = 1;
			a[776+256*54+122] = 2;
			a[1026+256*54+122] = 2;
			a[1027+256*54+122] = 2;
			a[1029+256*54+122] = 2;
			a[1032+256*54+122] = 2;
			a[1033+256*54+122] = 1;
			a[1280+256*54+122] = 2;
			a[1281+256*54+122] = 2;
			a[1288+256*54+122] = 1;
			a[1289+256*54+122] = 1;
			a[1536+256*54+122] = 1;
			a[1537+256*54+122] = 1;
			a[1538+256*54+122] = 1;
			a[1540+256*54+122] = 2;
			a[1541+256*54+122] = 2;
			a[1544+256*54+122] = 2;
			a[1545+256*54+122] = 1;
			a[1795+256*54+122] = 2;
			a[1796+256*54+122] = 2;
			a[1799+256*54+122] = 1;
			a[1800+256*54+122] = 2;
			a[2052+256*54+122] = 1;
			a[2058+256*54+122] = 2;
			a[2059+256*54+122] = 1;
			a[2304+256*54+122] = 1;
			a[2305+256*54+122] = 2;
			a[2306+256*54+122] = 2;
			a[2307+256*54+122] = 1;
			a[2315+256*54+122] = 2;
			a[2316+256*54+122] = 1;
			a[2560+256*54+122] = 1;
			a[2561+256*54+122] = 2;
			a[2562+256*54+122] = 2;
			a[2563+256*54+122] = 1;
			a[2571+256*54+122] = 2;
			a[2572+256*54+122] = 1;
			a[2820+256*54+122] = 1;
			a[2826+256*54+122] = 2;
			a[2827+256*54+122] = 1;
			a[3075+256*54+122] = 2;
			a[3076+256*54+122] = 2;
			a[3079+256*54+122] = 1;
			a[3080+256*54+122] = 2;
			a[3328+256*54+122] = 1;
			a[3329+256*54+122] = 1;
			a[3330+256*54+122] = 1;
			a[3332+256*54+122] = 2;
			a[3333+256*54+122] = 2;
			a[3336+256*54+122] = 2;
			a[3337+256*54+122] = 1;
			a[3584+256*54+122] = 2;
			a[3585+256*54+122] = 2;
			a[3592+256*54+122] = 1;
			a[3593+256*54+122] = 1;
			a[3842+256*54+122] = 2;
			a[3843+256*54+122] = 2;
			a[3845+256*54+122] = 2;
			a[3848+256*54+122] = 2;
			a[3849+256*54+122] = 1;
			a[4099+256*54+122] = 2;
			a[4101+256*54+122] = 1;
			a[4103+256*54+122] = 1;
			a[4104+256*54+122] = 2;
			a[4354+256*54+122] = 1;
			a[4355+256*54+122] = 1;
			a[4356+256*54+122] = 2;
			a[4359+256*54+122] = 1;
			a[4360+256*54+122] = 1;
			a[4362+256*54+122] = 2;
			a[4363+256*54+122] = 1;
			a[4609+256*54+122] = 2;
			a[4611+256*54+122] = 1;
			a[4616+256*54+122] = 2;
			a[4617+256*54+122] = 1;
			a[4619+256*54+122] = 2;
			a[4620+256*54+122] = 1;
			a[4866+256*54+122] = 1;
			a[4867+256*54+122] = 2;
			a[4875+256*54+122] = 2;
			a[4876+256*54+122] = 1;
			Cells = chunk.Cells = Gen(a, 8, 128);
		}

		private int[] Gen(int[] a, int ws, int h)
		{
			int w = 1 << ws;
			int[] b = new int[h << ws];
			for (int i = 0; i < w; ++i)
				for (int j = 0; j < h; ++j)
				{
					int count = 0;
					if (i + 1 < w && a[i + 1 | j << ws] != 0)
					{
						count++;
					}
					if (i > 0 && a[i - 1 | j << ws] != 0)
					{
						count++;
					}
					if (j + 1 < h && a[i | j + 1 << ws] != 0)
					{
						count++;
					}
					if (j > 0 && a[i | j - 1 << ws] != 0)
					{
						count++;
					}
					if (i + 1 < w)
					{
						if (j + 1 < h && a[i + 1 | j + 1 << ws] != 0)
						{
							count++;
						}
						if (j > 0 && a[i + 1 | j - 1 << ws] != 0)
						{
							count++;
						}
					}
					if (i > 0)
					{
						if (j + 1 < h && a[i - 1 | j + 1 << ws] != 0)
						{
							count++;
						}
						if (j > 0 && a[i - 1 | j - 1 << ws] != 0)
						{
							count++;
						}
					}
					int m = a[i | j << ws];
					if (count == 2 && m != 2)
					{
						b[i | j << ws] = m + 1;
					}
				}
			return b;
		}
	}
}