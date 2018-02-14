using Engine;
using Engine.Graphics;
using System.Collections.Generic;

namespace Game
{
	public class TrueColorBlock : CubeBlock
	{
		public const int Index = 801;

		public override void Initialize()
		{
			//FurnitureDesign.CreateGeometry1 = CreateGeometry;
			base.Initialize();
		}

		public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometrySubsets geometry, int value, int x, int y, int z)
		{
			generator.GenerateCubeVertices(this, value, x, y, z, GetColor(value), geometry.OpaqueSubsetsByFace);
			/*var subsetsByFace = geometry.OpaqueSubsetsByFace;
			var counts = new int[6];
			int i = 0;
			for (; i < 6; i++)
				counts[i] = subsetsByFace[i].Vertices.Count;
			base.GenerateTerrainVertices(generator, geometry, value, x, y, z);
			Color color = GetColor(value);
			for (i = 0; i < 6; i++)
			{
				int count = subsetsByFace[i].Vertices.Count;
				if (count > counts[i])
				{
					var arr = subsetsByFace[i].Vertices.Array;
					for (int j = counts[i]; j < count; j++)
						arr[i].Color *= color;
				}
			}*/
		}

		public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
		{
			base.DrawBlock(primitivesRenderer, value, color * GetColor(value), size, ref matrix, environmentData);
		}

		public override void GetDropValues(SubsystemTerrain subsystemTerrain, int oldValue, int newValue, int toolLevel, List<BlockDropValue> dropValues, out bool showDebris)
		{
			dropValues.Add(new BlockDropValue
			{
				Value = Terrain.MakeBlockValue(BlockIndex, 0, Terrain.ExtractData(oldValue)),
				Count = 1
			});
			showDebris = true;
		}

		public override BlockDebrisParticleSystem CreateDebrisParticleSystem(SubsystemTerrain subsystemTerrain, Vector3 position, int value, float strength)
		{
			return new BlockDebrisParticleSystem(subsystemTerrain, position, strength, DestructionDebrisScale, GetColor(value), GetFaceTextureSlot(0, value));
		}

		public override IEnumerable<int> GetCreativeValues()
		{
			return new int[0];
		}

		public override string GetDisplayName(SubsystemTerrain subsystemTerrain, int value)
		{
			return DefaultDisplayName + " #" + ((int)GetColor(value).PackedValue & 16777215).ToString("x6");
		}

		public static Color GetColor(int value)
		{
			return new Color((uint)((Terrain.ExtractContents(value) - 801 & 63) << 18 | Terrain.ExtractData(value)) | 4278190080u);
		}

		public static int SetColor(int color)
		{
			return Terrain.MakeBlockValue((color >> 18 & 63) + Index, 0, color);
		}

		/*public static void CreateGeometry(FurnitureDesign f)
		{
			f.m_geometry = new FurnitureGeometry();
			for (int i = 0; i < 6; i++)
			{
				int num = CellFace.OppositeFace(i);
				Point3 point;
				Point3 point2;
				Point3 point3;
				Point3 point4;
				Point3 point5;
				switch (i)
				{
					case 0:
						point = new Point3(0, 0, 1);
						point2 = new Point3(-1, 0, 0);
						point3 = new Point3(0, -1, 0);
						point4 = new Point3(f.m_resolution, f.m_resolution, 0);
						point5 = new Point3(f.m_resolution - 1, f.m_resolution - 1, 0);
						break;

					case 1:
						point = new Point3(1, 0, 0);
						point2 = new Point3(0, 0, 1);
						point3 = new Point3(0, -1, 0);
						point4 = new Point3(0, f.m_resolution, 0);
						point5 = new Point3(0, f.m_resolution - 1, 0);
						break;

					case 2:
						point = new Point3(0, 0, -1);
						point2 = new Point3(1, 0, 0);
						point3 = new Point3(0, -1, 0);
						point4 = new Point3(0, f.m_resolution, f.m_resolution);
						point5 = new Point3(0, f.m_resolution - 1, f.m_resolution - 1);
						break;

					case 3:
						point = new Point3(-1, 0, 0);
						point2 = new Point3(0, 0, -1);
						point3 = new Point3(0, -1, 0);
						point4 = new Point3(f.m_resolution, f.m_resolution, f.m_resolution);
						point5 = new Point3(f.m_resolution - 1, f.m_resolution - 1, f.m_resolution - 1);
						break;

					case 4:
						point = new Point3(0, 1, 0);
						point2 = new Point3(-1, 0, 0);
						point3 = new Point3(0, 0, 1);
						point4 = new Point3(f.m_resolution, 0, 0);
						point5 = new Point3(f.m_resolution - 1, 0, 0);
						break;

					default:
						point = new Point3(0, -1, 0);
						point2 = new Point3(-1, 0, 0);
						point3 = new Point3(0, 0, -1);
						point4 = new Point3(f.m_resolution, f.m_resolution, f.m_resolution);
						point5 = new Point3(f.m_resolution - 1, f.m_resolution - 1, f.m_resolution - 1);
						break;
				}
				BlockMesh blockMesh = new BlockMesh();
				BlockMesh blockMesh2 = new BlockMesh();
				for (int j = 0; j < f.m_resolution; j++)
				{
					var array = new FurnitureDesign.Cell[f.m_resolution * f.m_resolution];
					for (int k = 0; k < f.m_resolution; k++)
					{
						for (int l = 0; l < f.m_resolution; l++)
						{
							int num2 = j * point.X + k * point3.X + l * point2.X + point5.X;
							int num3 = j * point.Y + k * point3.Y + l * point2.Y + point5.Y;
							int num4 = j * point.Z + k * point3.Z + l * point2.Z + point5.Z;
							int num5 = num2 + num3 * f.m_resolution + num4 * f.m_resolution * f.m_resolution;
							int num6 = f.m_values[num5];
							var cell = default(FurnitureDesign.Cell);
							cell.Value = num6;
							FurnitureDesign.Cell cell2 = cell;
							if (j > 0 && num6 != 0)
							{
								int num7 = num2 - point.X + (num3 - point.Y) * f.m_resolution + (num4 - point.Z) * f.m_resolution * f.m_resolution;
								int value = f.m_values[num7];
								if (!FurnitureDesign.IsValueTransparent(value) || Terrain.ExtractContents(num6) == Terrain.ExtractContents(value))
									cell2.Value = 0;
							}
							array[l + k * f.m_resolution] = cell2;
						}
					}
					for (int m = 0; m < f.m_resolution; m++)
					{
						for (int n = 0; n < f.m_resolution; n++)
						{
							int value2 = array[n + m * f.m_resolution].Value;
							if (value2 != 0)
							{
								Point2 point6 = f.FindLargestSize(array, new Point2(n, m), value2);
								if (point6 == Point2.Zero) continue;
								f.MarkUsed(array, new Point2(n, m), point6);
								float num8 = 0.0005f * (float)f.m_resolution;
								float num9 = (float)n - num8;
								float num10 = (float)(n + point6.X) + num8;
								float num11 = (float)m - num8;
								float num12 = (float)(m + point6.Y) + num8;
								float x = (float)(j * point.X) + num11 * (float)point3.X + num9 * (float)point2.X + (float)point4.X;
								float y = (float)(j * point.Y) + num11 * (float)point3.Y + num9 * (float)point2.Y + (float)point4.Y;
								float z = (float)(j * point.Z) + num11 * (float)point3.Z + num9 * (float)point2.Z + (float)point4.Z;
								float x2 = (float)(j * point.X) + num11 * (float)point3.X + num10 * (float)point2.X + (float)point4.X;
								float y2 = (float)(j * point.Y) + num11 * (float)point3.Y + num10 * (float)point2.Y + (float)point4.Y;
								float z2 = (float)(j * point.Z) + num11 * (float)point3.Z + num10 * (float)point2.Z + (float)point4.Z;
								float x3 = (float)(j * point.X) + num12 * (float)point3.X + num10 * (float)point2.X + (float)point4.X;
								float y3 = (float)(j * point.Y) + num12 * (float)point3.Y + num10 * (float)point2.Y + (float)point4.Y;
								float z3 = (float)(j * point.Z) + num12 * (float)point3.Z + num10 * (float)point2.Z + (float)point4.Z;
								float x4 = (float)(j * point.X) + num12 * (float)point3.X + num9 * (float)point2.X + (float)point4.X;
								float y4 = (float)(j * point.Y) + num12 * (float)point3.Y + num9 * (float)point2.Y + (float)point4.Y;
								float z4 = (float)(j * point.Z) + num12 * (float)point3.Z + num9 * (float)point2.Z + (float)point4.Z;
								BlockMesh blockMesh3 = blockMesh;
								int num13 = Terrain.ExtractContents(value2);
								Block block = BlocksManager.Blocks[num13];
								int num14 = block.GetFaceTextureSlot(i, value2);
								bool isEmissive = false;
								Color color = Color.White;
								IPaintableBlock paintableBlock = block as IPaintableBlock;
								if (paintableBlock != null)
								{
									int? paintColor = paintableBlock.GetPaintColor(value2);
									color = SubsystemPalette.GetColor(f.m_subsystemTerrain, paintColor);
								}
								else if (block is WaterBlock)
								{
									color = BlockColorsMap.WaterColorsMap.Lookup(12, 12);
									num14 = 189;
								}
								else if (block is CarpetBlock)
								{
									int color2 = CarpetBlock.GetColor(Terrain.ExtractData(value2));
									color = SubsystemPalette.GetFabricColor(f.m_subsystemTerrain, color2);
								}
								else if (block is TorchBlock || block is WickerLampBlock)
								{
									isEmissive = true;
									num14 = 31;
								}
								else if (block is GlassBlock)
									blockMesh3 = blockMesh2;
								else if (block is TrueColorBlock)
									color = GetColor(value2);
								int num15 = num14 % 16;
								int num16 = num14 / 16;
								int count = blockMesh3.Vertices.Count;
								blockMesh3.Vertices.Count += 4;
								BlockMeshVertex[] array2 = blockMesh3.Vertices.Array;
								float x5 = (((float)n + 0.01f) / (float)f.m_resolution + (float)num15) / 16f;
								float x6 = (((float)(n + point6.X) - 0.01f) / (float)f.m_resolution + (float)num15) / 16f;
								float y5 = (((float)m + 0.01f) / (float)f.m_resolution + (float)num16) / 16f;
								float y6 = (((float)(m + point6.Y) - 0.01f) / (float)f.m_resolution + (float)num16) / 16f;
								BlockMeshVertex blockMeshVertex = array2[count] = new BlockMeshVertex
								{
									Position = new Vector3(x, y, z) / (float)f.m_resolution,
									Color = color,
									Face = (byte)num,
									TextureCoordinates = new Vector2(x5, y5),
									IsEmissive = isEmissive
								};
								blockMeshVertex = (array2[count + 1] = new BlockMeshVertex
								{
									Position = new Vector3(x2, y2, z2) / (float)f.m_resolution,
									Color = color,
									Face = (byte)num,
									TextureCoordinates = new Vector2(x6, y5),
									IsEmissive = isEmissive
								});
								blockMeshVertex = (array2[count + 2] = new BlockMeshVertex
								{
									Position = new Vector3(x3, y3, z3) / (float)f.m_resolution,
									Color = color,
									Face = (byte)num,
									TextureCoordinates = new Vector2(x6, y6),
									IsEmissive = isEmissive
								});
								blockMeshVertex = (array2[count + 3] = new BlockMeshVertex
								{
									Position = new Vector3(x4, y4, z4) / (float)f.m_resolution,
									Color = color,
									Face = (byte)num,
									TextureCoordinates = new Vector2(x5, y6),
									IsEmissive = isEmissive
								});
								int count2 = blockMesh3.Indices.Count;
								blockMesh3.Indices.Count += 6;
								ushort[] array3 = blockMesh3.Indices.Array;
								array3[count2] = (ushort)count;
								array3[count2 + 1] = (ushort)(count + 1);
								array3[count2 + 2] = (ushort)(count + 2);
								array3[count2 + 3] = (ushort)(count + 2);
								array3[count2 + 4] = (ushort)(count + 3);
								array3[count2 + 5] = (ushort)count;
							}
						}
					}
				}
				if (blockMesh.Indices.Count > 0)
				{
					blockMesh.Trim();
					blockMesh.GenerateSidesData();
					f.m_geometry.SubsetOpaqueByFace[i] = blockMesh;
				}
				if (blockMesh2.Indices.Count > 0)
				{
					blockMesh2.Trim();
					blockMesh2.GenerateSidesData();
					f.m_geometry.SubsetAlphaTestByFace[i] = blockMesh2;
				}
			}
		}*/
	}

	public class TrueColorBlock2 : TrueColorBlock
	{
		public new const int Index = 802;

		public override IEnumerable<int> GetCreativeValues()
		{
			var arr = new int[512];
			for (int i = 0; i < 512; i++)
				arr[i] = SetColor(i >> 6 << 21 | (i >> 3 & 7) << 13 | (i & 7) << 5);
			return arr;
		}
	}

	public class TrueColorBlock3 : TrueColorBlock
	{
		public new const int Index = 803;
	}

	public class TrueColorBlock4 : TrueColorBlock
	{
		public new const int Index = 804;
	}

	public class TrueColorBlock5 : TrueColorBlock
	{
		public new const int Index = 805;
	}

	public class TrueColorBlock6 : TrueColorBlock
	{
		public new const int Index = 806;
	}

	public class TrueColorBlock7 : TrueColorBlock
	{
		public new const int Index = 807;
	}

	public class TrueColorBlock8 : TrueColorBlock
	{
		public new const int Index = 808;
	}

	public class TrueColorBlock9 : TrueColorBlock
	{
		public new const int Index = 809;
	}

	public class TrueColorBlock10 : TrueColorBlock
	{
		public new const int Index = 810;
	}

	public class TrueColorBlock11 : TrueColorBlock
	{
		public new const int Index = 811;
	}

	public class TrueColorBlock12 : TrueColorBlock
	{
		public new const int Index = 812;
	}

	public class TrueColorBlock13 : TrueColorBlock
	{
		public new const int Index = 813;
	}

	public class TrueColorBlock14 : TrueColorBlock
	{
		public new const int Index = 814;
	}

	public class TrueColorBlock15 : TrueColorBlock
	{
		public new const int Index = 815;
	}

	public class TrueColorBlock16 : TrueColorBlock
	{
		public new const int Index = 816;
	}

	public class TrueColorBlock17 : TrueColorBlock
	{
		public new const int Index = 817;
	}

	public class TrueColorBlock18 : TrueColorBlock
	{
		public new const int Index = 818;
	}

	public class TrueColorBlock19 : TrueColorBlock
	{
		public new const int Index = 819;
	}

	public class TrueColorBlock20 : TrueColorBlock
	{
		public new const int Index = 820;
	}

	public class TrueColorBlock21 : TrueColorBlock
	{
		public new const int Index = 821;
	}

	public class TrueColorBlock22 : TrueColorBlock
	{
		public new const int Index = 822;
	}

	public class TrueColorBlock23 : TrueColorBlock
	{
		public new const int Index = 823;
	}

	public class TrueColorBlock24 : TrueColorBlock
	{
		public new const int Index = 824;
	}

	public class TrueColorBlock25 : TrueColorBlock
	{
		public new const int Index = 825;
	}

	public class TrueColorBlock26 : TrueColorBlock
	{
		public new const int Index = 826;
	}

	public class TrueColorBlock27 : TrueColorBlock
	{
		public new const int Index = 827;
	}

	public class TrueColorBlock28 : TrueColorBlock
	{
		public new const int Index = 828;
	}

	public class TrueColorBlock29 : TrueColorBlock
	{
		public new const int Index = 829;
	}

	public class TrueColorBlock30 : TrueColorBlock
	{
		public new const int Index = 830;
	}

	public class TrueColorBlock31 : TrueColorBlock
	{
		public new const int Index = 831;
	}

	public class TrueColorBlock32 : TrueColorBlock
	{
		public new const int Index = 832;
	}

	public class TrueColorBlock33 : TrueColorBlock
	{
		public new const int Index = 833;
	}

	public class TrueColorBlock34 : TrueColorBlock
	{
		public new const int Index = 834;
	}

	public class TrueColorBlock35 : TrueColorBlock
	{
		public new const int Index = 835;
	}

	public class TrueColorBlock36 : TrueColorBlock
	{
		public new const int Index = 836;
	}

	public class TrueColorBlock37 : TrueColorBlock
	{
		public new const int Index = 837;
	}

	public class TrueColorBlock38 : TrueColorBlock
	{
		public new const int Index = 838;
	}

	public class TrueColorBlock39 : TrueColorBlock
	{
		public new const int Index = 839;
	}

	public class TrueColorBlock40 : TrueColorBlock
	{
		public new const int Index = 840;
	}

	public class TrueColorBlock41 : TrueColorBlock
	{
		public new const int Index = 841;
	}

	public class TrueColorBlock42 : TrueColorBlock
	{
		public new const int Index = 842;
	}

	public class TrueColorBlock43 : TrueColorBlock
	{
		public new const int Index = 843;
	}

	public class TrueColorBlock44 : TrueColorBlock
	{
		public new const int Index = 844;
	}

	public class TrueColorBlock45 : TrueColorBlock
	{
		public new const int Index = 845;
	}

	public class TrueColorBlock46 : TrueColorBlock
	{
		public new const int Index = 846;
	}

	public class TrueColorBlock47 : TrueColorBlock
	{
		public new const int Index = 847;
	}

	public class TrueColorBlock48 : TrueColorBlock
	{
		public new const int Index = 848;
	}

	public class TrueColorBlock49 : TrueColorBlock
	{
		public new const int Index = 849;
	}

	public class TrueColorBlock50 : TrueColorBlock
	{
		public new const int Index = 850;
	}

	public class TrueColorBlock51 : TrueColorBlock
	{
		public new const int Index = 851;
	}

	public class TrueColorBlock52 : TrueColorBlock
	{
		public new const int Index = 852;
	}

	public class TrueColorBlock53 : TrueColorBlock
	{
		public new const int Index = 853;
	}

	public class TrueColorBlock54 : TrueColorBlock
	{
		public new const int Index = 854;
	}

	public class TrueColorBlock55 : TrueColorBlock
	{
		public new const int Index = 855;
	}

	public class TrueColorBlock56 : TrueColorBlock
	{
		public new const int Index = 856;
	}

	public class TrueColorBlock57 : TrueColorBlock
	{
		public new const int Index = 857;
	}

	public class TrueColorBlock58 : TrueColorBlock
	{
		public new const int Index = 858;
	}

	public class TrueColorBlock59 : TrueColorBlock
	{
		public new const int Index = 859;
	}

	public class TrueColorBlock60 : TrueColorBlock
	{
		public new const int Index = 860;
	}

	public class TrueColorBlock61 : TrueColorBlock
	{
		public new const int Index = 861;
	}

	public class TrueColorBlock62 : TrueColorBlock
	{
		public new const int Index = 862;
	}

	public class TrueColorBlock63 : TrueColorBlock
	{
		public new const int Index = 863;
	}

	public class TrueColorBlock64 : TrueColorBlock
	{
		public new const int Index = 864;
	}
}