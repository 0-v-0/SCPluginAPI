using Engine;
using System;
using System.Collections.Generic;

namespace Game
{
	public class MyTerrainContentsGeneratorBH2 : TerrainContentsGenerator, ITerrainContentsGenerator
	{
		private WorldSettings worldSettings;

		private int worldSeed;

		//private SubsystemBottomSuckerBlockBehavior subsystemBottomSuckerBlockBehavior;

		private Vector2 oceanCorner;

		private Vector2 temperatureOffset;

		private Vector2 riversOffset;

		private Vector2 myHighLandOffset;

		private Vector2 myBasinOffset;

		private Vector2 myFireMountainsOffset;

		private Vector2 humidityOffset;

		private Vector2 mountainsOffset;

		private Vector2 skyLandOffset;

		private Random terrainRandom;

		private int randomX;

		private int randomZ;

		private ITerrainContentsGenerator systemTerrainContentsGenerator;

		public float MyBasinHeight;

		public float MyFireMountainsSize;

		public float MyHighLand;

		public float MySkyLand;

		public MyTerrainContentsGeneratorBH2(SubsystemTerrain subsystemTerrain) : base(subsystemTerrain)
		{
			systemTerrainContentsGenerator = m_subsystemTerrain.TerrainContentsGenerator;
			//subsystemBottomSuckerBlockBehavior = subsystemTerrain.Project.FindSubsystem<SubsystemBottomSuckerBlockBehavior>(true);
			SubsystemGameInfo subsystemGameInfo = subsystemTerrain.Project.FindSubsystem<SubsystemGameInfo>(true);
			worldSettings = subsystemGameInfo.WorldSettings;
			worldSeed = subsystemGameInfo.WorldSeed;
			var random = new Random(worldSeed);
			float num = float.MaxValue;
			oceanCorner = new Vector2(random.UniformFloat(-100f, -100f), random.UniformFloat(-100f, -100f));
			temperatureOffset = new Vector2(random.UniformFloat(-2000f, 2000f), random.UniformFloat(-2000f, 2000f));
			humidityOffset = new Vector2(random.UniformFloat(-2000f, 2000f), random.UniformFloat(-2000f, 2000f));
			mountainsOffset = new Vector2(random.UniformFloat(-2000f, 2000f), random.UniformFloat(-2000f, 2000f));
			riversOffset = new Vector2(random.UniformFloat(-2000f, 2000f), random.UniformFloat(-2000f, 2000f));
			myHighLandOffset = new Vector2(random.UniformFloat(-4000f, 4000f), random.UniformFloat(-4000f, 4000f));
			myBasinOffset = new Vector2(random.UniformFloat(-4000f, 4000f), random.UniformFloat(-4000f, 4000f));
			myFireMountainsOffset = new Vector2(random.UniformFloat(-4000f, 4000f), random.UniformFloat(-4000f, 4000f));
			skyLandOffset = new Vector2(random.UniformFloat(-4000f, 4000f), random.UniformFloat(-4000f, 4000f));
			TGNewBiomeNoise = true;
			TGBiomeScaling = worldSettings.BiomeSize;
			TGShoreFluctuations = MathUtils.Clamp(2f * num, 0f, 150f);
			TGShoreFluctuationsScaling = MathUtils.Clamp(0.04f * num, 0.5f, 3f);
			TGOceanSlope = 0.006f;
			TGOceanSlopeVariation = 0.004f;
			TGIslandsFrequency = 0.01f;
			TGDensityBias = 55f;
			TGHeightBias = 1f;
			TGRiversStrength = 2.8f;
			TGMountainsStrength = 75f;
			TGMountainsPeriod = 0.0009f;
			TGMountainsPercentage = 0.15f;
			TGHillsStrength = 8f;
			TGTurbulenceStrength = 35f;
			TGTurbulenceTopOffset = 0f;
			TGTurbulencePower = 0.3f;
			TGSurfaceMultiplier = 2f;
			terrainRandom = new Random(worldSeed);
			randomX = terrainRandom.UniformInt(-100000, 100000);
			randomZ = terrainRandom.UniformInt(-100000, 100000);
			MyBasinHeight = 35f;
			MyFireMountainsSize = 105f;
			MyHighLand = 55f;
			MySkyLand = 55f;
		}

		public new int CalculateHumidity(float x, float z)
		{
			if (TGNewBiomeNoise)
			{
				return MathUtils.Clamp((int)(MathUtils.Saturate(4f * SimplexNoise.OctavedNoise(x + humidityOffset.X, z + humidityOffset.Y, 0.0012f / TGBiomeScaling, 5, 2f, 0.7f) - 1.2f + worldSettings.HumidityOffset / 16f) * 16f), 0, 15);
			}
			return MathUtils.Clamp((int)((MathUtils.Saturate(4f * SimplexNoise.OctavedNoise(x + humidityOffset.X, z + humidityOffset.Y, 0.0008f / TGBiomeScaling, 5, 1.97f, 1f) - 1.5f) + worldSettings.HumidityOffset / 16f) * 16f), 0, 15);
		}

		public new float CalculateOceanShoreDistance(float x, float z)
		{
			float num = CalculateOceanShoreX(z);
			float num2 = CalculateOceanShoreZ(x);
			return MathUtils.Min(x - num, z - num2);
		}

		private new float CalculateOceanShoreX(float z)
		{
			return oceanCorner.X + TGShoreFluctuations * SimplexNoise.OctavedNoise(z, 0f, 0.005f / TGShoreFluctuationsScaling, 4, 1.95f, 1f);
		}

		private new float CalculateOceanShoreZ(float x)
		{
			return oceanCorner.Y + TGShoreFluctuations * SimplexNoise.OctavedNoise(0f, x, 0.005f / TGShoreFluctuationsScaling, 4, 1.95f, 1f);
		}

		public new int CalculateTemperature(float x, float z)
		{
			if (TGNewBiomeNoise)
			{
				return MathUtils.Clamp((int)(MathUtils.Saturate(4f * SimplexNoise.OctavedNoise(x + temperatureOffset.X, z + temperatureOffset.Y, 0.0015f / TGBiomeScaling, 5, 2f, 0.7f) - 1.6f + worldSettings.TemperatureOffset / 16f) * 16f), 0, 15);
			}
			return MathUtils.Clamp((int)((MathUtils.Saturate(4f * SimplexNoise.OctavedNoise(x + temperatureOffset.X, z + temperatureOffset.Y, 0.0006f / TGBiomeScaling, 4, 1.93f, 1f) - 1.6f) + worldSettings.TemperatureOffset / 16f) * 16f), 0, 15);
		}

		public new Vector3 FindCoarseSpawnPosition()
		{
			return systemTerrainContentsGenerator.FindCoarseSpawnPosition();
		}

		public new void GenerateChunkContentsPass1(TerrainChunk chunk)
		{
			GenerateMySurfaceParameters(chunk);
			GenerateTerrain(chunk, 0, 0, 16, 8);
		}

		public new void GenerateChunkContentsPass2(TerrainChunk chunk)
		{
			GenerateTerrain(chunk, 0, 8, 16, 16);
		}

		public new void GenerateChunkContentsPass3(TerrainChunk chunk)
		{
			GenerateMyBasin(chunk);
			GenerateMyFireMountains(chunk);
			GenerateCaves(chunk);
			GenerateDesertRemoveWater(chunk);
			GenerateSurface(chunk);
			GenerateMinerals(chunk);
			GenerateMyArea(chunk);
			GenerateGrassAndPlants(chunk);
			GenerateMySkyLand(chunk);
			GeneratePockets(chunk);
			PropagateFluidsDownwards(chunk);
		}

		public new void GenerateChunkContentsPass4(TerrainChunk chunk)
		{
			GenerateTreesAndLogs(chunk);
			systemTerrainContentsGenerator.GenerateChunkContentsPass4(chunk);
		}

		/*private void GenerateSurfaceParameters(TerrainChunk chunk, int x1, int z1, int x2, int z2)
		{
			for (int i = x1; i < x2; i++)
			{
				for (int j = z1; j < z2; j++)
				{
					int num = i + chunk.Origin.X;
					int num2 = j + chunk.Origin.Y;
					int temperature = CalculateTemperature(num, num2);
					int humidity = CalculateHumidity(num, num2);
					chunk.SetTemperatureFast(i, j, temperature);
					chunk.SetHumidityFast(i, j, humidity);
				}
			}
		}*/

		public new float CalculateMountainRangeFactor(float x, float z)
		{
			return 1f - MathUtils.Abs(1.05f * SimplexNoise.OctavedNoise(x + mountainsOffset.X, z + mountainsOffset.Y, TGMountainsPeriod / TGBiomeScaling, 1, 4f, 0.85f) - 1f);
		}

		private float CalculateBasinHeight(float x, float z)
		{
			float num = MathUtils.Abs(MathUtils.Saturate(3f * SimplexNoise.OctavedNoise(x + myBasinOffset.X, z + myBasinOffset.Y, 0.001f / TGBiomeScaling, 1, 1.5f, 1f)) - 1f);
			return num * MyBasinHeight;
		}

		private float CalculateFireMountainsHeight(float x, float z)
		{
			float num = 1.45f * SimplexNoise.OctavedNoise(x, z, 0.004f, 4, 1.98f, 0.9f) - 0.5f;
			float num2 = MathUtils.Abs(MathUtils.Saturate(10f * SimplexNoise.OctavedNoise(x + myFireMountainsOffset.X, z + myFireMountainsOffset.Y, 0.002f / TGBiomeScaling, 1, 2.75f, 1f)) - 1f);
			float num3 = num2 * MyFireMountainsSize;
			float num4 = num3 * num;
			return num3 + num4;
		}

		private float CalculateSkyLand(float x, float z)
		{
			float num = 1.25f * SimplexNoise.OctavedNoise(x, z, 0.004f, 4, 1.98f, 0.9f) - 0.5f;
			float num2 = MathUtils.Abs(MathUtils.Saturate(10f * SimplexNoise.OctavedNoise(x + skyLandOffset.X, z + skyLandOffset.Y, 0.001f / TGBiomeScaling, 1, 3.25f, 2f)) - 1f);
			float num3 = num2 * MySkyLand;
			float num4 = num3 * num;
			return num3 + num4;
		}

		private float CalculateHighLandHeight(float x, float z)
		{
			float num = 1.5f * SimplexNoise.OctavedNoise(x, z, 0.004f, 4, 1.98f, 0.9f) - 0.5f;
			float num2 = MathUtils.Abs(MathUtils.Saturate(5.5f * SimplexNoise.OctavedNoise(x + myHighLandOffset.X, z + myHighLandOffset.Y, 0.001f / TGBiomeScaling, 1, 2.5f, 1f)) - 1f);
			float num3 = num2 * MyHighLand;
			float num4 = num3 * num;
			return num3 + num4;
		}

		public new float CalculateHeight(float x, float z)
		{
			float num = TGOceanSlope + TGOceanSlopeVariation * MathUtils.PowSign(2f * SimplexNoise.OctavedNoise(x + mountainsOffset.X, z + mountainsOffset.Y, 0.01f, 1, 2f, 0.5f) - 1f, 0.5f);
			float num2 = CalculateOceanShoreDistance(x, z);
			float num3 = MathUtils.Saturate(1f - 0.05f * MathUtils.Abs(num2));
			float num4 = MathUtils.Saturate(MathUtils.Sin(TGIslandsFrequency * num2));
			float num5 = MathUtils.Saturate(MathUtils.Saturate((0f - num) * num2) - 0.85f * num4);
			float num6 = MathUtils.Saturate(MathUtils.Saturate(0.05f * (0f - num2 - 10f)) - num4);
			float num7 = CalculateMountainRangeFactor(x, z);
			float f = (1f - num3) * SimplexNoise.OctavedNoise(x, z, 0.001f / TGBiomeScaling, 2, 1.97f, 0.8f);
			float f2 = (1f - num3) * SimplexNoise.OctavedNoise(x, z, 0.0017f / TGBiomeScaling, 2, 1.93f, 0.7f);
			float num8 = (1f - num6) * (1f - num3) * MathUtils.Saturate((num7 - 0.6f) / 0.4f);
			float num9 = (1f - num6) * MathUtils.Saturate((num7 - (1f - TGMountainsPercentage)) / TGMountainsPercentage);
			float num10 = 2f * SimplexNoise.OctavedNoise(x, z, 0.02f, 3, 1.93f, 0.8f) - 1f;
			float num11 = 1.5f * SimplexNoise.OctavedNoise(x, z, 0.004f, 4, 1.98f, 0.9f) - 0.5f;
			float num12 = MathUtils.Lerp(60f, 30f, MathUtils.Saturate(1f * num9 + 0.5f * num8 + MathUtils.Saturate(1f - num2 / 30f)));
			float x2 = MathUtils.Lerp(-2f, -4f, MathUtils.Saturate(num9 + 0.5f * num8));
			float num13 = MathUtils.Saturate(1.5f - num12 * MathUtils.Abs(2f * SimplexNoise.OctavedNoise(x + riversOffset.X, z + riversOffset.Y, 0.001f, 4, 2f, 0.5f) - 1f));
			float num14 = -50f * num5 + TGHeightBias;
			float num15 = MathUtils.Lerp(0f, 8f, f);
			float num16 = MathUtils.Lerp(0f, -6f, f2);
			float num17 = TGHillsStrength * num8 * num10;
			float num18 = TGMountainsStrength * num9 * num11;
			float f3 = TGRiversStrength * num13;
			float num19 = CalculateHighLandHeight(x, z);
			float num20 = num14 + num15 + num16 + num18 + num17 + num19;
			float num21 = MathUtils.Min(MathUtils.Lerp(num20, x2, f3), num20);
			return MathUtils.Clamp(64f + num21, 10f, 123f);
		}

		private new void GenerateTerrain(TerrainChunk chunk, int x1, int z1, int x2, int z2)
		{
			int num = x2 - x1;
			int num2 = z2 - z1;
			int num3 = chunk.Origin.X + x1;
			int num4 = chunk.Origin.Y + z1;
			var grid2d = new Grid2d(num, num2);
			var grid2d2 = new Grid2d(num, num2);
			for (int i = 0; i < num2; i++)
			{
				for (int j = 0; j < num; j++)
				{
					grid2d.Set(j, i, CalculateOceanShoreDistance(j + num3, i + num4));
					grid2d2.Set(j, i, CalculateMountainRangeFactor(j + num3, i + num4));
				}
			}
			var grid3d = new Grid3d(num / 4 + 1, 17, num2 / 4 + 1);
			for (int k = 0; k < grid3d.SizeX; k++)
			{
				for (int l = 0; l < grid3d.SizeZ; l++)
				{
					int num5 = k * 4 + num3;
					int num6 = l * 4 + num4;
					float num7 = CalculateHeight(num5, num6);
					float num8 = CalculateMountainRangeFactor(num5, num6);
					float num9 = MathUtils.Saturate(0.9f * (num8 - 0.8f) / 0.2f + 0.1f);
					for (int m = 0; m < grid3d.SizeY; m++)
					{
						int num10 = m * 8;
						float num11 = num7 - TGTurbulenceTopOffset;
						float num12 = MathUtils.Lerp(0f, TGTurbulenceStrength * num9, MathUtils.Saturate((num11 - (float)num10) * 0.2f)) * MathUtils.PowSign(2f * SimplexNoise.OctavedNoise(num5, num10 + 1000, num6, 0.008f, 3, 2f, 0.75f) - 1f, TGTurbulencePower);
						float num13 = (float)num10 + num12;
						float num14 = num7 - num13;
						num14 += MathUtils.Max(4f * (TGDensityBias - (float)num10), 0f);
						grid3d.Set(k, m, l, num14);
					}
				}
			}
			int oceanLevel = OceanLevel;
			for (int n = 0; n < grid3d.SizeX - 1; n++)
			{
				for (int num15 = 0; num15 < grid3d.SizeZ - 1; num15++)
				{
					for (int num16 = 0; num16 < grid3d.SizeY - 1; num16++)
					{
						grid3d.Get8(n, num16, num15, out float v, out float v2, out float v3, out float v4, out float v5, out float v6, out float v7, out float v8);
						float num17 = (v2 - v) / 4f;
						float num18 = (v4 - v3) / 4f;
						float num19 = (v6 - v5) / 4f;
						float num20 = (v8 - v7) / 4f;
						float num21 = v;
						float num22 = v3;
						float num23 = v5;
						float num24 = v7;
						for (int num25 = 0; num25 < 4; num25++)
						{
							float num26 = (num23 - num21) / 4f;
							float num27 = (num24 - num22) / 4f;
							float num28 = num21;
							float num29 = num22;
							for (int num30 = 0; num30 < 4; num30++)
							{
								float num31 = (num29 - num28) / 8f;
								float num32 = num28;
								int num33 = num25 + n * 4;
								int num34 = num30 + num15 * 4;
								int x3 = x1 + num33;
								int z3 = z1 + num34;
								float x4 = grid2d.Get(num33, num34);
								float num35 = grid2d2.Get(num33, num34);
								int temperatureFast = chunk.GetTemperatureFast(x3, z3);
								int humidityFast = chunk.GetHumidityFast(x3, z3);
								float f = num35 - 0.01f * (float)humidityFast;
								float num36 = MathUtils.Lerp(100f, 0f, f);
								float num37 = MathUtils.Lerp(300f, 30f, f);
								bool flag = (temperatureFast > 5 && humidityFast < 9 && num35 < 0.95f) || (MathUtils.Abs(x4) < 12f && num35 < 0.9f);
								int num38 = TerrainChunk.CalculateCellIndex(x3, 0, z3);
								for (int num39 = 0; num39 < 8; num39++)
								{
									int num40 = num39 + num16 * 8;
									int value = 0;
									if (num32 < 0f)
									{
										if (num40 <= oceanLevel)
										{
											value = 18;
										}
									}
									else
									{
										value = (flag ? ((!(num32 >= num36)) ? 4 : ((num32 >= num37) ? 67 : 3)) : ((num32 >= num37) ? 67 : 3));
									}
									chunk.SetCellValueFast(num38 + num40, value);
									num32 += num31;
								}
								num28 += num26;
								num29 += num27;
							}
							num21 += num17;
							num22 += num18;
							num23 += num19;
							num24 += num20;
						}
					}
				}
			}
		}

		private void GenerateMySurfaceParameters(TerrainChunk chunk)
		{
			GenerateMyHumidity(chunk);
			GenerateMyTemperature(chunk);
		}

		private void GenerateMyHumidity(TerrainChunk chunk)
		{
			int y = chunk.Coords.Y;
			int num = y % 16;
			if (num < 0)
			{
				num += 16;
			}
			int num2 = y - num;
			int num3 = Math.Abs(num2);
			int num4 = new Random(worldSeed + 4143 * num3).UniformInt(0, 15);
			int num5 = new Random(worldSeed + 4143 * (num3 + 16)).UniformInt(0, 15);
			int num6 = new Random(worldSeed + 4143 * (num3 - 16)).UniformInt(0, 15);
			for (int i = 0; i < 16; i++)
			{
				float num7 = SimplexNoise.OctavedNoise(i + chunk.Origin.X + num2 * 16 + randomZ, 0f, 0.005f / TGBiomeScaling, 4, 1.85f, 2f);
				for (int j = 0; j < 16; j++)
				{
					int humidity = num4;
					if (num7 * 100f > (float)(j + num * 16))
					{
						humidity = ((num2 > 0) ? num6 : num5);
					}
					chunk.SetHumidityFast(i, j, humidity);
				}
			}
		}

		private int GetMyHumidity(TerrainChunk chunk, int x, int z)
		{
			int y = chunk.Coords.Y;
			int num = y % 16;
			if (num < 0)
			{
				num += 16;
			}
			int num2 = y - num;
			int num3 = Math.Abs(num2);
			int num4 = new Random(worldSeed + 4143 * num3).UniformInt(0, 15);
			int num5 = new Random(worldSeed + 4143 * (num3 + 16)).UniformInt(0, 15);
			int num6 = new Random(worldSeed + 4143 * (num3 - 16)).UniformInt(0, 15);
			float num7 = SimplexNoise.OctavedNoise(x + chunk.Origin.X + num2 * 16 + randomZ, 0f, 0.005f / TGBiomeScaling, 4, 1.85f, 2f);
			int result = num4;
			if (num7 * 100f > (float)(z + num * 16))
			{
				result = ((num2 > 0) ? num6 : num5);
			}
			return result;
		}

		private void GenerateMyTemperature(TerrainChunk chunk)
		{
			for (int i = 0; i < 16; i++)
			{
				for (int j = 0; j < 16; j++)
				{
					int num = i + chunk.Origin.X;
					int num2 = j + chunk.Origin.Y;
					float num3 = SimplexNoise.OctavedNoise((float)num + temperatureOffset.X + (float)randomX, (float)num2 + temperatureOffset.Y + (float)randomZ, 0.0004f / TGBiomeScaling, 2, 1f, 1f);
					chunk.SetTemperatureFast(i, j, (int)(num3 / 0.0725f));
				}
			}
		}

		private void GenerateMyBasin(TerrainChunk chunk)
		{
			for (int i = 0; i < 16; i++)
			{
				for (int j = 0; j < 16; j++)
				{
					int num = i + chunk.Origin.X;
					int num2 = j + chunk.Origin.Y;
					int num3 = (int)CalculateBasinHeight(num, num2);
					if (num3 > 25)
					{
						num3 = 25;
					}
					int value = 0;
					int num4 = 0;
					bool flag = false;
					for (int num5 = 127; num5 > 0; num5--)
					{
						if (chunk.GetCellContentsFast(i, num5, j) != 0)
						{
							int num6 = 0;
							while (num6 < num3)
							{
								if (chunk.GetCellContentsFast(i, num5, j) == 18 && !flag)
								{
									num4++;
								}
								else if (!flag)
								{
									flag = true;
									value = chunk.GetCellContentsFast(i, num5, j);
								}
								chunk.SetCellValueFast(i, num5, j, 0);
								num6++;
								num5--;
							}
							if (num4 > 0)
							{
								int num7 = 0;
								while (num7 < num4 && num7 <= 6)
								{
									chunk.SetCellValueFast(i, num5, j, 18);
									num7++;
									num5--;
								}
							}
							if (flag)
							{
								chunk.SetCellValueFast(i, num5, j, value);
							}
							break;
						}
					}
				}
			}
		}

		private void GenerateMyHouses(TerrainChunk chunk)
		{
			int x = chunk.Coords.X;
			int y = chunk.Coords.Y;
			var random = new Random(worldSeed + x * 1000 + 2764 * y);
			var random2 = new Random(worldSeed + x * random.UniformInt(1, 1000) + y * random.UniformInt(0, 2000));
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

		private void GenerateMyArea(TerrainChunk chunk)
		{
			int x = chunk.Coords.X;
			int y = chunk.Coords.Y;
			var random = new Random(worldSeed + x / 10 * 1000 + 2764 * (y / 10));
			if (random.Bool(0.0625f) && Math.Abs(x) % 10 < 5 && Math.Abs(y) % 10 < 5)
			{
				GenerateMyHouses(chunk);
			}
		}

		private void GenerateMyFireMountains(TerrainChunk chunk)
		{
			var random = new Random(worldSeed + chunk.Coords.X * 222 + 3543 * chunk.Coords.Y);
			for (int i = 0; i < 16; i++)
			{
				for (int j = 0; j < 16; j++)
				{
					int num = i + chunk.Origin.X;
					int num2 = j + chunk.Origin.Y;
					int num3 = (int)CalculateFireMountainsHeight(num, num2);
					int num4 = 0;
					for (int num5 = 127; num5 > 0; num5--)
					{
						int cellContentsFast = chunk.GetCellContentsFast(i, num5, j);
						if (cellContentsFast != 18 && cellContentsFast != 0)
						{
							num4 = num5;
							break;
						}
					}
					if (num3 <= 0)
					{
						continue;
					}
					for (int k = 0; k < num3; k++)
					{
						int num6 = 120 - num3;
						if (num3 > 57 && k > num6)
						{
							if (num6 < 45)
							{
								for (int num7 = 45; num7 > num6; num7--)
								{
									chunk.SetCellValueFast(i, num4 + num7, j, 92);
								}
							}
						}
						else
						{
							if (k > 50)
							{
								continue;
							}
							if (num3 > 54)
							{
								float num8 = random.UniformFloat(0f, 1.25f);
								int value = 67;
								if (num8 < 0.0001f)
								{
									value = 126;
								}
								else if (num8 < 0.0008f)
								{
									value = 112;
								}
								else if (num8 < 0.03f)
								{
									value = 39;
								}
								else if (num8 < 0.06f)
								{
									value = 148;
								}
								else if (num8 < 0.08f)
								{
									value = 101;
								}
								chunk.SetCellValueFast(i, num4 + k, j, value);
							}
							else
							{
								float num9 = random.UniformFloat(0f, 1.15f);
								int value2 = 3;
								if ((double)num9 < 0.0005)
								{
									value2 = 46;
								}
								else if (num9 < 0.002f)
								{
									value2 = 150;
								}
								else if (num9 < 0.003f)
								{
									value2 = 71;
								}
								else if (num9 < 0.007f)
								{
									value2 = 41;
								}
								else if (num9 < 0.015f)
								{
									value2 = 16;
								}
								chunk.SetCellValueFast(i, num4 + k, j, value2);
							}
						}
					}
				}
			}
		}

		private void GenerateMySkyLand(TerrainChunk chunk)
		{
			var random = new Random(worldSeed + chunk.Coords.X * 111 + 3143 * chunk.Coords.Y);
			for (int i = 0; i < 16; i++)
			{
				for (int j = 0; j < 16; j++)
				{
					int num = i + chunk.Origin.X + randomZ;
					int num2 = j + chunk.Origin.Y + randomX;
					float num3 = CalculateSkyLand(num, num2);
					if (!(num3 > 0f))
					{
						continue;
					}
					for (int k = 0; (float)k < num3; k++)
					{
						int value = 3;
						float num4 = random.UniformFloat(0f, 1.25f);
						if (num4 < 0.002f)
						{
							value = 126;
						}
						else if (num4 < 0.005f)
						{
							value = 46;
						}
						else if (num4 < 0.009f)
						{
							value = 47;
						}
						else if (num4 < 0.012f)
						{
							value = 71;
						}
						else if (num4 < 0.02f)
						{
							value = 41;
						}
						else if (num4 < 0.035f)
						{
							value = 16;
						}
						if ((float)k >= num3 - 3f || k == 0)
						{
							value = 2;
						}
						chunk.SetCellValueFast(i, 80 + k - (int)num3 / 3, j, value);
					}
					GrassAndPlantsHander(chunk, i, j, random);
				}
			}
		}

		private new void GenerateCaves(TerrainChunk chunk)
		{
			var list = new List<CavePoint>();
			int x = chunk.Coords.X;
			int y = chunk.Coords.Y;
			for (int i = x - 2; i <= x + 2; i++)
			{
				for (int j = y - 2; j <= y + 2; j++)
				{
					list.Clear();
					var random = new Random(worldSeed + i + 9973 * j);
					int num = i * 16 + random.UniformInt(0, 15);
					int num2 = j * 16 + random.UniformInt(0, 15);
					float probability = 0.5f;
					if (!random.Bool(probability))
					{
						continue;
					}
					int num3 = (int)CalculateHeight(num, num2);
					int num4 = (int)CalculateHeight(num + 3, num2);
					int num5 = (int)CalculateHeight(num, num2 + 3);
					var position = new Vector3(num, num3 - 1, num2);
					var v = new Vector3(3f, num4 - num3, 0f);
					var v2 = new Vector3(0f, num5 - num3, 3f);
					var direction = Vector3.Normalize(Vector3.Cross(v, v2));
					if (direction.Y > -0.6f)
					{
						list.Add(new CavePoint
						{
							Position = position,
							Direction = direction,
							BrushType = 0,
							Length = random.UniformInt(80, 240)
						});
					}
					int num6 = i * 16 + 8;
					int num7 = j * 16 + 8;
					int num8 = 0;
					while (num8 < list.Count)
					{
						CavePoint cavePoint = list[num8];
						List<TerrainBrush> list2 = TerrainContentsGenerator.m_caveBrushesByType[cavePoint.BrushType];
						list2[random.UniformInt(0, list2.Count - 1)].PaintFastAvoidWater(chunk, Terrain.ToCell(cavePoint.Position.X), Terrain.ToCell(cavePoint.Position.Y), Terrain.ToCell(cavePoint.Position.Z));
						cavePoint.Position += 2f * cavePoint.Direction;
						cavePoint.StepsTaken += 2;
						float num9 = cavePoint.Position.X - (float)num6;
						float num10 = cavePoint.Position.Z - (float)num7;
						if (random.Bool(0.5f))
						{
							var v3 = Vector3.Normalize(random.Vector3(1f, true));
							if ((num9 < -25.5f && v3.X < 0f) || (num9 > 25.5f && v3.X > 0f))
							{
								v3.X = 0f - v3.X;
							}
							if ((num10 < -25.5f && v3.Z < 0f) || (num10 > 25.5f && v3.Z > 0f))
							{
								v3.Z = 0f - v3.Z;
							}
							if ((cavePoint.Direction.Y < -0.5f && v3.Y < -10f) || (cavePoint.Direction.Y > 0.1f && v3.Y > 0f))
							{
								v3.Y = 0f - v3.Y;
							}
							cavePoint.Direction = Vector3.Normalize(cavePoint.Direction + 0.5f * v3);
						}
						if (cavePoint.StepsTaken > 20 && random.Bool(0.06f))
						{
							cavePoint.Direction = Vector3.Normalize(random.Vector3(1f, true) * new Vector3(1f, 0.33f, 1f));
						}
						if (cavePoint.StepsTaken > 20 && random.Bool(0.05f))
						{
							cavePoint.Direction.Y = 0f;
							cavePoint.BrushType = MathUtils.Min(cavePoint.BrushType + 2, TerrainContentsGenerator.m_caveBrushesByType.Count - 1);
						}
						if (cavePoint.StepsTaken > 30 && random.Bool(0.03f))
						{
							cavePoint.Direction.X = 0f;
							cavePoint.Direction.Y = -1f;
							cavePoint.Direction.Z = 0f;
						}
						if (cavePoint.StepsTaken > 30 && cavePoint.Position.Y < 30f && random.Bool(0.02f))
						{
							cavePoint.Direction.X = 0f;
							cavePoint.Direction.Y = 1f;
							cavePoint.Direction.Z = 0f;
						}
						if (random.Bool(0.33f))
						{
							cavePoint.BrushType = (int)(MathUtils.Pow(random.UniformFloat(0f, 0.999f), 7f) * (float)TerrainContentsGenerator.m_caveBrushesByType.Count);
						}
						if (random.Bool(0.06f) && list.Count < 12 && cavePoint.StepsTaken > 20 && cavePoint.Position.Y < 58f)
						{
							list.Add(new CavePoint
							{
								Position = cavePoint.Position,
								Direction = Vector3.Normalize(random.UniformVector3(1f, 1f) * new Vector3(1f, 0.33f, 1f)),
								BrushType = (int)(MathUtils.Pow(random.UniformFloat(0f, 0.999f), 7f) * (float)TerrainContentsGenerator.m_caveBrushesByType.Count),
								Length = random.UniformInt(40, 180)
							});
						}
						if (cavePoint.StepsTaken >= cavePoint.Length || MathUtils.Abs(num9) > 34f || MathUtils.Abs(num10) > 34f || cavePoint.Position.Y < 5f || cavePoint.Position.Y > 118f)
						{
							num8++;
						}
						else if (cavePoint.StepsTaken % 20 == 0)
						{
							float num11 = CalculateHeight(cavePoint.Position.X, cavePoint.Position.Z);
							if (cavePoint.Position.Y > num11 + 1f)
							{
								num8++;
							}
						}
					}
				}
			}
		}

		private new void GeneratePockets(TerrainChunk chunk)
		{
			for (int i = -1; i <= 1; i++)
			{
				for (int j = -1; j <= 1; j++)
				{
					int num = i + chunk.Coords.X;
					int num2 = j + chunk.Coords.Y;
					var random = new Random(worldSeed + num + 71 * num2);
					int num3 = random.UniformInt(0, 10);
					for (int k = 0; k < num3; k++)
					{
						random.UniformInt(0, 1);
					}
					float num4 = CalculateMountainRangeFactor(num * 16, num2 * 16);
					for (int l = 0; l < 3; l++)
					{
						int x = num * 16 + random.UniformInt(0, 15);
						int y = random.UniformInt(50, 100);
						int z = num2 * 16 + random.UniformInt(0, 15);
						TerrainContentsGenerator.m_dirtPocketBrushes[random.UniformInt(0, TerrainContentsGenerator.m_dirtPocketBrushes.Count - 1)].PaintFastSelective(chunk, x, y, z, 3);
					}
					for (int m = 0; m < 10; m++)
					{
						int x2 = num * 16 + random.UniformInt(0, 15);
						int y2 = random.UniformInt(20, 80);
						int z2 = num2 * 16 + random.UniformInt(0, 15);
						TerrainContentsGenerator.m_gravelPocketBrushes[random.UniformInt(0, TerrainContentsGenerator.m_gravelPocketBrushes.Count - 1)].PaintFastSelective(chunk, x2, y2, z2, 3);
					}
					for (int n = 0; n < 2; n++)
					{
						int x3 = num * 16 + random.UniformInt(0, 15);
						int y3 = random.UniformInt(20, 120);
						int z3 = num2 * 16 + random.UniformInt(0, 15);
						TerrainContentsGenerator.m_limestonePocketBrushes[random.UniformInt(0, TerrainContentsGenerator.m_limestonePocketBrushes.Count - 1)].PaintFastSelective(chunk, x3, y3, z3, 3);
					}
					for (int num5 = 0; num5 < 1; num5++)
					{
						int x4 = num * 16 + random.UniformInt(0, 15);
						int y4 = random.UniformInt(50, 70);
						int z4 = num2 * 16 + random.UniformInt(0, 15);
						TerrainContentsGenerator.m_clayPocketBrushes[random.UniformInt(0, TerrainContentsGenerator.m_clayPocketBrushes.Count - 1)].PaintFastSelective(chunk, x4, y4, z4, 3);
					}
					for (int num6 = 0; num6 < 6; num6++)
					{
						int x5 = num * 16 + random.UniformInt(0, 15);
						int y5 = random.UniformInt(40, 80);
						int z5 = num2 * 16 + random.UniformInt(0, 15);
						TerrainContentsGenerator.m_sandPocketBrushes[random.UniformInt(0, TerrainContentsGenerator.m_sandPocketBrushes.Count - 1)].PaintFastSelective(chunk, x5, y5, z5, 4);
					}
					for (int num7 = 0; num7 < 4; num7++)
					{
						int x6 = num * 16 + random.UniformInt(0, 15);
						int y6 = random.UniformInt(40, 60);
						int z6 = num2 * 16 + random.UniformInt(0, 15);
						TerrainContentsGenerator.m_basaltPocketBrushes[random.UniformInt(0, TerrainContentsGenerator.m_basaltPocketBrushes.Count - 1)].PaintFastSelective(chunk, x6, y6, z6, 4);
					}
					for (int num8 = 0; num8 < 3; num8++)
					{
						int x7 = num * 16 + random.UniformInt(0, 15);
						int y7 = random.UniformInt(20, 40);
						int z7 = num2 * 16 + random.UniformInt(0, 15);
						TerrainContentsGenerator.m_basaltPocketBrushes[random.UniformInt(0, TerrainContentsGenerator.m_basaltPocketBrushes.Count - 1)].PaintFastSelective(chunk, x7, y7, z7, 3);
					}
					for (int num9 = 0; num9 < 6; num9++)
					{
						int x8 = num * 16 + random.UniformInt(0, 15);
						int y8 = random.UniformInt(4, 50);
						int z8 = num2 * 16 + random.UniformInt(0, 15);
						TerrainContentsGenerator.m_granitePocketBrushes[random.UniformInt(0, TerrainContentsGenerator.m_granitePocketBrushes.Count - 1)].PaintFastSelective(chunk, x8, y8, z8, 67);
					}
					if (random.Bool(0.02f + 0.01f * num4))
					{
						int num10 = num * 16;
						int num11 = random.UniformInt(15, 35);
						int num12 = num2 * 16;
						int num13 = random.UniformInt(1, 3);
						for (int num14 = 0; num14 < num13; num14++)
						{
							Vector2 vector = random.Vector2(7f);
							int num15 = 8 + (int)MathUtils.Round(vector.X);
							int num16 = 0;
							int num17 = 8 + (int)MathUtils.Round(vector.Y);
							TerrainContentsGenerator.m_waterPocketBrushes[random.UniformInt(0, TerrainContentsGenerator.m_waterPocketBrushes.Count - 1)].PaintFast(chunk, num10 + num15, num11 + num16, num12 + num17);
						}
					}
					if (random.Bool(0.06f + 0.05f * num4))
					{
						int num18 = num * 16;
						int num19 = random.UniformInt(10, 27);
						int num20 = num2 * 16;
						int num21 = random.UniformInt(1, 2);
						for (int num22 = 0; num22 < num21; num22++)
						{
							Vector2 vector2 = random.Vector2(7f);
							int num23 = 8 + (int)MathUtils.Round(vector2.X);
							int num24 = random.UniformInt(0, 1);
							int num25 = 8 + (int)MathUtils.Round(vector2.Y);
							TerrainContentsGenerator.m_magmaPocketBrushes[random.UniformInt(0, TerrainContentsGenerator.m_magmaPocketBrushes.Count - 1)].PaintFast(chunk, num18 + num23, num19 + num24, num20 + num25);
						}
					}
				}
			}
		}

		private new void GenerateMinerals(TerrainChunk chunk)
		{
			int x = chunk.Coords.X;
			int y = chunk.Coords.Y;
			for (int i = x - 1; i <= x + 1; i++)
			{
				for (int j = y - 1; j <= y + 1; j++)
				{
					var random = new Random(worldSeed + i + 119 * j);
					int num = random.UniformInt(0, 10);
					for (int k = 0; k < num; k++)
					{
						random.UniformInt(0, 1);
					}
					float num2 = CalculateMountainRangeFactor(i * 16, j * 16);
					int num3 = (int)(5f + 2f * num2 * SimplexNoise.OctavedNoise(i, j, 0.33f, 1, 1f, 1f));
					for (int l = 0; l < num3; l++)
					{
						int x2 = i * 16 + random.UniformInt(0, 15);
						int y2 = random.UniformInt(5, 80);
						int z = j * 16 + random.UniformInt(0, 15);
						TerrainContentsGenerator.m_coalBrushes[random.UniformInt(0, TerrainContentsGenerator.m_coalBrushes.Count - 1)].PaintFastSelective(chunk, x2, y2, z, 3);
					}
					int num4 = (int)(6f + 2f * num2 * SimplexNoise.OctavedNoise(i + 1211, j + 396, 0.33f, 1, 1f, 1f));
					for (int m = 0; m < num4; m++)
					{
						int x3 = i * 16 + random.UniformInt(0, 15);
						int y3 = random.UniformInt(20, 65);
						int z2 = j * 16 + random.UniformInt(0, 15);
						TerrainContentsGenerator.m_copperBrushes[random.UniformInt(0, TerrainContentsGenerator.m_copperBrushes.Count - 1)].PaintFastSelective(chunk, x3, y3, z2, 3);
					}
					int num5 = (int)(5f + 2f * num2 * SimplexNoise.OctavedNoise(i + 713, j + 211, 0.33f, 1, 1f, 1f));
					for (int n = 0; n < num5; n++)
					{
						int x4 = i * 16 + random.UniformInt(0, 15);
						int y4 = random.UniformInt(2, 40);
						int z3 = j * 16 + random.UniformInt(0, 15);
						TerrainContentsGenerator.m_ironBrushes[random.UniformInt(0, TerrainContentsGenerator.m_ironBrushes.Count - 1)].PaintFastSelective(chunk, x4, y4, z3, 67);
					}
					int num6 = (int)(3f + 2f * num2 * SimplexNoise.OctavedNoise(i + 915, j + 272, 0.33f, 1, 1f, 1f));
					for (int num7 = 0; num7 < num6; num7++)
					{
						int x5 = i * 16 + random.UniformInt(0, 15);
						int y5 = random.UniformInt(50, 70);
						int z4 = j * 16 + random.UniformInt(0, 15);
						TerrainContentsGenerator.m_saltpeterBrushes[random.UniformInt(0, TerrainContentsGenerator.m_saltpeterBrushes.Count - 1)].PaintFastSelective(chunk, x5, y5, z4, 4);
					}
					int num8 = (int)(3f + 2f * num2 * SimplexNoise.OctavedNoise(i + 711, j + 1194, 0.33f, 1, 1f, 1f));
					for (int num9 = 0; num9 < num8; num9++)
					{
						int x6 = i * 16 + random.UniformInt(0, 15);
						int y6 = random.UniformInt(2, 40);
						int z5 = j * 16 + random.UniformInt(0, 15);
						TerrainContentsGenerator.m_sulphurBrushes[random.UniformInt(0, TerrainContentsGenerator.m_sulphurBrushes.Count - 1)].PaintFastSelective(chunk, x6, y6, z5, 67);
					}
					int num10 = (int)(0.5f + 2f * num2 * SimplexNoise.OctavedNoise(i + 432, j + 907, 0.33f, 1, 1f, 1f));
					for (int num11 = 0; num11 < num10; num11++)
					{
						int x7 = i * 16 + random.UniformInt(0, 15);
						int y7 = random.UniformInt(2, 15);
						int z6 = j * 16 + random.UniformInt(0, 15);
						TerrainContentsGenerator.m_diamondBrushes[random.UniformInt(0, TerrainContentsGenerator.m_diamondBrushes.Count - 1)].PaintFastSelective(chunk, x7, y7, z6, 67);
					}
					int num12 = (int)(3f + 2f * num2 * SimplexNoise.OctavedNoise(i + 799, j + 131, 0.33f, 1, 1f, 1f));
					for (int num13 = 0; num13 < num12; num13++)
					{
						int x8 = i * 16 + random.UniformInt(0, 15);
						int y8 = random.UniformInt(2, 50);
						int z7 = j * 16 + random.UniformInt(0, 15);
						TerrainContentsGenerator.m_germaniumBrushes[random.UniformInt(0, TerrainContentsGenerator.m_germaniumBrushes.Count - 1)].PaintFastSelective(chunk, x8, y8, z7, 67);
					}
				}
			}
		}

		private new void GenerateSurface(TerrainChunk chunk)
		{
			Terrain terrain = m_subsystemTerrain.Terrain;
			var random = new Random(worldSeed + chunk.Coords.X + 101 * chunk.Coords.Y);
			for (int i = 0; i < 16; i++)
			{
				for (int j = 0; j < 16; j++)
				{
					int num = i + chunk.Origin.X;
					int num2 = j + chunk.Origin.Y;
					int num3 = TerrainChunk.CalculateCellIndex(i, 126, j);
					int num4 = 126;
					while (num4 >= 0)
					{
						int num5 = Terrain.ExtractContents(chunk.GetCellValueFast(num3));
						if (!BlocksManager.Blocks[num5].IsTransparent)
						{
							float num6 = CalculateMountainRangeFactor(num, num2);
							int temperature = terrain.GetTemperature(num, num2);
							int humidity = terrain.GetHumidity(num, num2);
							float f = MathUtils.Saturate(MathUtils.Saturate((num6 - 0.9f) / 0.1f) - MathUtils.Saturate(((float)humidity - 3f) / 12f) + TerrainContentsGenerator.TGSurfaceMultiplier * MathUtils.Saturate(((float)num4 - 101f) * 0.05f));
							int min = (int)MathUtils.Lerp(4f, 0f, f);
							int max = (int)MathUtils.Lerp(7f, 0f, f);
							int num7 = MathUtils.Min(random.UniformInt(min, max), num4);
							int contents;
							switch (num5)
							{
							case 4:
								contents = ((temperature < 3) ? 6 : 7);
								break;
							default:
							{
								int num8 = temperature / 4;
								int num9 = (num4 + 1 < 255) ? chunk.GetCellContentsFast(i, num4 + 1, j) : 0;
								contents = (((num4 >= 66 && num4 != 84 + num8 && num4 != 103 + num8) || humidity != 9 || temperature % 6 != 1) ? ((num9 != 18 || humidity <= 8 || humidity % 2 != 0 || temperature % 3 != 0) ? 2 : 72) : 66);
								break;
							}
							case 67:
								contents = 67;
								break;
							}
							int num10 = TerrainChunk.CalculateCellIndex(i, num4 + 1, j);
							for (int k = num10 - num7; k < num10; k++)
							{
								if (Terrain.ExtractContents(chunk.GetCellValueFast(k)) != 0)
								{
									int value = Terrain.ReplaceContents(0, contents);
									chunk.SetCellValueFast(k, value);
								}
							}
							break;
						}
						num4--;
						num3--;
					}
				}
			}
		}

		private new void PropagateFluidsDownwards(TerrainChunk chunk)
		{
			for (int i = 0; i < 16; i++)
			{
				for (int j = 0; j < 16; j++)
				{
					int num = TerrainChunk.CalculateCellIndex(i, 127, j);
					int num2 = 0;
					int num3 = 127;
					while (num3 >= 0)
					{
						int num4 = Terrain.ExtractContents(chunk.GetCellValueFast(num));
						if (num4 == 0 && num2 != 0 && BlocksManager.FluidBlocks[num2] != null)
						{
							chunk.SetCellValueFast(num, num2);
							num4 = num2;
						}
						num2 = num4;
						num3--;
						num--;
					}
				}
			}
		}

		private void GrassAndPlantsHander(TerrainChunk chunk, int i, int j, Random random)
		{
			int num = 126;
			int cellValueFast;
			int num2;
			while (true)
			{
				if (num >= 0)
				{
					cellValueFast = chunk.GetCellValueFast(i, num, j);
					num2 = Terrain.ExtractContents(cellValueFast);
					if (num2 != 0)
					{
						break;
					}
					num--;
					continue;
				}
				return;
			}
			if (!(BlocksManager.Blocks[num2] is FluidBlock))
			{
				int temperatureFast = chunk.GetTemperatureFast(i, j);
				int humidityFast = chunk.GetHumidityFast(i, j);
				int num3 = GenerateRandomPlantValue(random, cellValueFast, temperatureFast, humidityFast, num + 1);
				if (num3 != 0)
				{
					chunk.SetCellValueFast(i, num + 1, j, num3);
				}
				if (num2 == 2)
				{
					chunk.SetCellValueFast(i, num, j, Terrain.MakeBlockValue(8, 0, 0));
				}
			}
		}

		private new void GenerateGrassAndPlants(TerrainChunk chunk)
		{
			var random = new Random(worldSeed + chunk.Coords.X + 3943 * chunk.Coords.Y);
			for (int i = 0; i < 16; i++)
			{
				for (int j = 0; j < 16; j++)
				{
					GrassAndPlantsHander(chunk, i, j, random);
				}
			}
		}

		public static int GenerateRandomPlantValue(Random random, int groundValue, int temperature, int humidity, int y)
		{
			Func<Random, int, int, int, int, int> generateRandomPlantValue = PlantsManager.GenerateRandomPlantValue1;
			if (generateRandomPlantValue != null)
			{
				return generateRandomPlantValue(random, groundValue, temperature, humidity, y);
			}
			switch (Terrain.ExtractContents(groundValue))
			{
			default:
				return 0;
			case 7:
				if (humidity >= 8 || random.UniformFloat(0f, 1f) >= 0.01f)
				{
					return 0;
				}
				if (random.UniformFloat(0f, 1f) < 0.05f)
				{
					return Terrain.MakeBlockValue(99, 0, 0);
				}
				return Terrain.MakeBlockValue(28, 0, 0);
			case 2:
			case 8:
				if (humidity >= 6)
				{
					if (random.UniformFloat(0f, 1f) < (float)humidity / 60f)
					{
						int result = Terrain.MakeBlockValue(19, 0, TallGrassBlock.SetIsSmall(0, false));
						if (!SubsystemWeather.IsPlaceFrozen(temperature, y))
						{
							if (temperature > 5)
							{
								float num = random.UniformFloat(0f, 1f);
								if (num < 0.1f)
								{
									result = Terrain.MakeBlockValue(20);
								}
								else if (num < 0.2f)
								{
									result = Terrain.MakeBlockValue(24);
								}
								else if (num < 0.3f)
								{
									result = Terrain.MakeBlockValue(25);
								}
								else if (num < 0.4f)
								{
									result = Terrain.MakeBlockValue(174, 0, RyeBlock.SetIsWild(RyeBlock.SetSize(0, 7), true));
								}
								else if (num < 0.5f)
								{
									result = Terrain.MakeBlockValue(204, 0, CottonBlock.SetIsWild(CottonBlock.SetSize(0, 2), true));
								}
							}
							else
							{
								float num2 = random.UniformFloat(0f, 1f);
								if (num2 < 0.01f)
								{
									result = Terrain.MakeBlockValue(20);
								}
								else if (num2 < 0.02f)
								{
									result = Terrain.MakeBlockValue(24);
								}
								else if (num2 < 0.03f)
								{
									result = Terrain.MakeBlockValue(25);
								}
								else if (num2 < 0.14f)
								{
									result = Terrain.MakeBlockValue(174, 0, RyeBlock.SetIsWild(RyeBlock.SetSize(0, 7), true));
								}
								else if (num2 < 0.3f)
								{
									result = Terrain.MakeBlockValue(204, 0, CottonBlock.SetIsWild(CottonBlock.SetSize(0, 2), true));
								}
							}
						}
						return result;
					}
				}
				else if (random.UniformFloat(0f, 1f) < 0.025f)
				{
					if (random.UniformFloat(0f, 1f) < 0.2f)
					{
						return Terrain.MakeBlockValue(99, 0, 0);
					}
					return Terrain.MakeBlockValue(28, 0, 0);
				}
				return 0;
			}
		}

		private new void GenerateTreesAndLogs(TerrainChunk chunk)
		{
			Terrain terrain = m_subsystemTerrain.Terrain;
			int x = chunk.Origin.X;
			int num = x + 16;
			int y = chunk.Origin.Y;
			int num2 = y + 16;
			int x2 = chunk.Coords.X;
			int y2 = chunk.Coords.Y;
			var random = new Random(worldSeed + x2 + 3943 * y2);
			int num3 = CalculateHumidity(x2 * 16, y2 * 16);
			int temperature = CalculateTemperature(x2 * 16, y2 * 16);
			float num4 = MathUtils.Saturate((SimplexNoise.OctavedNoise(x2, y2, 0.1f, 3, 2f, 0.5f) - 0.25f) / 0.2f + (random.Bool(0.25f) ? 0.5f : 0f)) * MathUtils.Saturate(((float)num3 - 4f) / 11f);
			int num5 = 0;
			if (num4 > 0.95f)
			{
				num5 = 1 + (random.Bool(0.25f) ? 1 : 0);
			}
			else if (num4 > 0.5f)
			{
				num5 = (random.Bool(0.25f) ? 1 : 0);
			}
			int num6 = 0;
			for (int i = 0; i < 8; i++)
			{
				if (num6 >= num5)
				{
					break;
				}
				int num7 = x2 * 16 + random.UniformInt(0, 15);
				int num8 = y2 * 16 + random.UniformInt(0, 15);
				int num9 = terrain.CalculateTopmostCellHeight(num7, num8);
				if (num9 < 66)
				{
					continue;
				}
				int cellContentsFast = terrain.GetCellContentsFast(num7, num9, num8);
				if (cellContentsFast != 2 && cellContentsFast != 8)
				{
					continue;
				}
				num9++;
				int num10 = random.UniformInt(3, 7);
				Point3 point = CellFace.FaceToPoint3(random.UniformInt(0, 3));
				if (point.X < 0 && num7 - num10 + 1 < 0)
				{
					point.X *= -1;
				}
				if (point.X > 0 && num7 + num10 - 1 > 15)
				{
					point.X *= -1;
				}
				if (point.Z < 0 && num8 - num10 + 1 < 0)
				{
					point.Z *= -1;
				}
				if (point.Z > 0 && num8 + num10 - 1 > 15)
				{
					point.Z *= -1;
				}
				bool flag = true;
				bool flag2 = false;
				bool flag3 = false;
				for (int j = 0; j < num10; j++)
				{
					int num11 = num7 + point.X * j;
					int num12 = num8 + point.Z * j;
					if (num11 < x + 1 || num11 >= num - 1 || num12 < y + 1 || num12 >= num2 - 1)
					{
						flag = false;
						break;
					}
					if (BlocksManager.Blocks[terrain.GetCellContentsFast(num11, num9, num12)].IsCollidable)
					{
						flag = false;
						break;
					}
					if (BlocksManager.Blocks[terrain.GetCellContentsFast(num11, num9 - 1, num12)].IsCollidable)
					{
						if (j <= MathUtils.Max(num10 / 2, 0))
						{
							flag2 = true;
						}
						if (j >= MathUtils.Min(num10 / 2 + 1, num10 - 1))
						{
							flag3 = true;
						}
					}
				}
				if (!((flag && flag2) & flag3))
				{
					continue;
				}
				Point3 point2 = (point.X != 0) ? new Point3(0, 0, 1) : new Point3(1, 0, 0);
				TreeType treeType = PlantsManager.GenerateRandomTreeType(random, temperature, num3, num9);
				int treeTrunkValue = PlantsManager.GetTreeTrunkValue(treeType);
				treeTrunkValue = Terrain.ReplaceData(treeTrunkValue, WoodBlock.SetCutFace(Terrain.ExtractData(treeTrunkValue), (point.X != 0) ? 1 : 0));
				int treeLeavesValue = PlantsManager.GetTreeLeavesValue(treeType);
				for (int k = 0; k < num10; k++)
				{
					int num13 = num7 + point.X * k;
					int num14 = num8 + point.Z * k;
					terrain.SetCellValueFast(num13, num9, num14, treeTrunkValue);
					if (k > num10 / 2)
					{
						if (random.Bool(0.3f) && !BlocksManager.Blocks[terrain.GetCellContentsFast(num13 + point2.X, num9, num14 + point2.Z)].IsCollidable)
						{
							terrain.SetCellValueFast(num13 + point2.X, num9, num14 + point2.Z, treeLeavesValue);
						}
						if (random.Bool(0.05f) && !BlocksManager.Blocks[terrain.GetCellContentsFast(num13 + point2.X, num9, num14 + point2.Z)].IsCollidable)
						{
							terrain.SetCellValueFast(num13 + point2.X, num9, num14 + point2.Z, treeTrunkValue);
						}
						if (random.Bool(0.3f) && !BlocksManager.Blocks[terrain.GetCellContentsFast(num13 - point2.X, num9, num14 - point2.Z)].IsCollidable)
						{
							terrain.SetCellValueFast(num13 - point2.X, num9, num14 - point2.Z, treeLeavesValue);
						}
						if (random.Bool(0.05f) && !BlocksManager.Blocks[terrain.GetCellContentsFast(num13 - point2.X, num9, num14 - point2.Z)].IsCollidable)
						{
							terrain.SetCellValueFast(num13 - point2.X, num9, num14 - point2.Z, treeTrunkValue);
						}
						if (random.Bool(0.1f) && !BlocksManager.Blocks[terrain.GetCellContentsFast(num13, num9 + 1, num14)].IsCollidable)
						{
							terrain.SetCellValueFast(num13, num9 + 1, num14, treeLeavesValue);
						}
					}
				}
				num6++;
			}
			int num15 = (int)(5f * num4);
			int num16 = 0;
			for (int l = 0; l < 32; l++)
			{
				if (num16 >= num15)
				{
					break;
				}
				int num17 = x2 * 16 + random.UniformInt(2, 13);
				int num18 = y2 * 16 + random.UniformInt(2, 13);
				int num19 = terrain.CalculateTopmostCellHeight(num17, num18);
				if (num19 < 70)
				{
					continue;
				}
				int cellContentsFast2 = terrain.GetCellContentsFast(num17, num19, num18);
				if (cellContentsFast2 == 2 || cellContentsFast2 == 8)
				{
					num19++;
					if (!BlocksManager.Blocks[terrain.GetCellContentsFast(num17 + 1, num19, num18)].IsCollidable && !BlocksManager.Blocks[terrain.GetCellContentsFast(num17 - 1, num19, num18)].IsCollidable && !BlocksManager.Blocks[terrain.GetCellContentsFast(num17, num19, num18 + 1)].IsCollidable && !BlocksManager.Blocks[terrain.GetCellContentsFast(num17, num19, num18 - 1)].IsCollidable)
					{
						ReadOnlyList<TerrainBrush> treeBrushes = PlantsManager.GetTreeBrushes(PlantsManager.GenerateRandomTreeType(random, temperature, num3, num19));
						treeBrushes[random.UniformInt(0, treeBrushes.Count - 1)].PaintFast(chunk, num17, num19, num18);
						num16++;
					}
				}
			}
			int num20 = x2 * 16 + random.UniformInt(2, 13);
			int num21 = y2 * 16 + random.UniformInt(2, 13);
			int num22 = terrain.CalculateTopmostCellHeight(num20, num21);
			if (num22 < 40 || !random.Bool(0.05f))
			{
				return;
			}
			int cellContentsFast3 = terrain.GetCellContentsFast(num20, num22, num21);
			if (cellContentsFast3 == 2 || cellContentsFast3 == 8)
			{
				num22++;
				if (!BlocksManager.Blocks[terrain.GetCellContentsFast(num20 + 1, num22, num21)].IsCollidable && !BlocksManager.Blocks[terrain.GetCellContentsFast(num20 - 1, num22, num21)].IsCollidable && !BlocksManager.Blocks[terrain.GetCellContentsFast(num20, num22, num21 + 1)].IsCollidable && !BlocksManager.Blocks[terrain.GetCellContentsFast(num20, num22, num21 - 1)].IsCollidable)
				{
					ReadOnlyList<TerrainBrush> treeBrushes2 = PlantsManager.GetTreeBrushes(PlantsManager.GenerateRandomTreeType(random, temperature, num3, num22));
					treeBrushes2[random.UniformInt(0, treeBrushes2.Count - 1)].PaintFast(chunk, num20, num22, num21);
				}
			}
		}

		private void GenerateDesertRemoveWater(TerrainChunk chunk)
		{
			for (int i = 0; i < 16; i++)
			{
				for (int j = 0; j < 16; j++)
				{
					int num = i + chunk.Origin.X;
					int num2 = j + chunk.Origin.Y;
					if (num <= 50 || num2 <= 50)
					{
						continue;
					}
					int num3 = 10 - GetMyHumidity(chunk, num, num2);
					for (int num4 = 127; num4 > 0 && num3 < 10 && num3 > 0; num4--)
					{
						switch (chunk.GetCellContentsFast(i, num4, j))
						{
						case 18:
							chunk.SetCellValueFast(i, num4, j, 0);
							num3--;
							continue;
						case 0:
							continue;
						}
						break;
					}
				}
			}
		}
	}
}
