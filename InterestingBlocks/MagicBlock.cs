using Engine;
using Engine.Graphics;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using TemplatesDatabase;

namespace Game
{
	[PluginLoader("", "", 0)]
	public class MagicBlock : Block
	{
		public const int Index = 324;

		public static new void Initialize()
		{
			FurnitureDesign.CreateGeometry1 += CreateGeometry;
		}

		public static void CreateGeometry(FurnitureDesign design)
		{
			int m_resolution = design.m_resolution;
			design.m_geometry = new FurnitureGeometry();
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
						point4 = new Point3(m_resolution, m_resolution, 0);
						point5 = new Point3(m_resolution - 1, m_resolution - 1, 0);
						break;
					case 1:
						point = new Point3(1, 0, 0);
						point2 = new Point3(0, 0, 1);
						point3 = new Point3(0, -1, 0);
						point4 = new Point3(0, m_resolution, 0);
						point5 = new Point3(0, m_resolution - 1, 0);
						break;
					case 2:
						point = new Point3(0, 0, -1);
						point2 = new Point3(1, 0, 0);
						point3 = new Point3(0, -1, 0);
						point4 = new Point3(0, m_resolution, m_resolution);
						point5 = new Point3(0, m_resolution - 1, m_resolution - 1);
						break;
					case 3:
						point = new Point3(-1, 0, 0);
						point2 = new Point3(0, 0, -1);
						point3 = new Point3(0, -1, 0);
						point4 = new Point3(m_resolution, m_resolution, m_resolution);
						point5 = new Point3(m_resolution - 1, m_resolution - 1, m_resolution - 1);
						break;
					case 4:
						point = new Point3(0, 1, 0);
						point2 = new Point3(-1, 0, 0);
						point3 = new Point3(0, 0, 1);
						point4 = new Point3(m_resolution, 0, 0);
						point5 = new Point3(m_resolution - 1, 0, 0);
						break;
					default:
						point = new Point3(0, -1, 0);
						point2 = new Point3(-1, 0, 0);
						point3 = new Point3(0, 0, -1);
						point4 = new Point3(m_resolution, m_resolution, m_resolution);
						point5 = new Point3(m_resolution - 1, m_resolution - 1, m_resolution - 1);
						break;
				}
				BlockMesh blockMesh = new BlockMesh();
				BlockMesh blockMesh2 = new BlockMesh();
				for (int j = 0; j < m_resolution; j++)
				{
					var array = new FurnitureDesign.Cell[m_resolution * m_resolution];
					for (int k = 0; k < m_resolution; k++)
					{
						for (int l = 0; l < m_resolution; l++)
						{
							int num2 = j * point.X + k * point3.X + l * point2.X + point5.X;
							int num3 = j * point.Y + k * point3.Y + l * point2.Y + point5.Y;
							int num4 = j * point.Z + k * point3.Z + l * point2.Z + point5.Z;
							int num5 = num2 + num3 * m_resolution + num4 * m_resolution * m_resolution;
							int num6 = design.m_values[num5];
							var cell = default(FurnitureDesign.Cell);
							cell.Value = num6;
							if (j > 0 && num6 != 0)
							{
								int num7 = num2 - point.X + (num3 - point.Y) * m_resolution + (num4 - point.Z) * m_resolution * m_resolution;
								int value = design.m_values[num7];
								if (!FurnitureDesign.IsValueTransparent(value) || Terrain.ExtractContents(num6) == Terrain.ExtractContents(value))
									cell.Value = 0;
							}
							array[l + k * m_resolution] = cell;
						}
					}
					for (int m = 0; m < m_resolution; m++)
					{
						for (int n = 0; n < m_resolution; n++)
						{
							int value2 = array[n + m * m_resolution].Value;
							if (value2 == 0)
								continue;
							Point2 point6 = design.FindLargestSize(array, new Point2(n, m), value2);
							if (!(point6 == Point2.Zero))
							{
								design.MarkUsed(array, new Point2(n, m), point6);
								float num8 = 0.0005f * m_resolution;
								float num9 = n - num8;
								float num10 = n + point6.X + num8;
								float num11 = m - num8;
								float num12 = m + point6.Y + num8;
								float x = j * point.X + num11 * point3.X + num9 * point2.X + point4.X;
								float y = j * point.Y + num11 * point3.Y + num9 * point2.Y + point4.Y;
								float z = j * point.Z + num11 * point3.Z + num9 * point2.Z + point4.Z;
								float x2 = j * point.X + num11 * point3.X + num10 * point2.X + point4.X;
								float y2 = j * point.Y + num11 * point3.Y + num10 * point2.Y + point4.Y;
								float z2 = j * point.Z + num11 * point3.Z + num10 * point2.Z + point4.Z;
								float x3 = j * point.X + num12 * point3.X + num10 * point2.X + point4.X;
								float y3 = j * point.Y + num12 * point3.Y + num10 * point2.Y + point4.Y;
								float z3 = j * point.Z + num12 * point3.Z + num10 * point2.Z + point4.Z;
								float x4 = j * point.X + num12 * point3.X + num9 * point2.X + point4.X;
								float y4 = j * point.Y + num12 * point3.Y + num9 * point2.Y + point4.Y;
								float z4 = j * point.Z + num12 * point3.Z + num9 * point2.Z + point4.Z;
								BlockMesh blockMesh3 = blockMesh;
								int num13 = Terrain.ExtractContents(value2);
								Block block = BlocksManager.Blocks[num13];
								int num14 = block.GetFaceTextureSlot(i, value2);
								bool isEmissive = false;
								Color color = Color.White;
								if (block is IPaintableBlock paintableBlock)
								{
									int? paintColor = paintableBlock.GetPaintColor(value2);
									color = SubsystemPalette.GetColor(design.m_subsystemTerrain, paintColor);
								}
								else if (block is WaterBlock)
								{
									color = BlockColorsMap.WaterColorsMap.Lookup(12, 12);
									num14 = 189;
								}
								else if (block is CarpetBlock)
								{
									int color2 = CarpetBlock.GetColor(Terrain.ExtractData(value2));
									color = SubsystemPalette.GetFabricColor(design.m_subsystemTerrain, color2);
								}
								else if (block is TrueColorBlock)
								{
									color = TrueColorBlock.GetColor(value2);
								}
								else if (block is TorchBlock || block is WickerLampBlock)
								{
									isEmissive = true;
									num14 = 31;
								}
								else if (block is GlassBlock)
								{
									blockMesh3 = blockMesh2;
								}
								int num15 = num14 % 16;
								int num16 = num14 / 16;
								int count = blockMesh3.Vertices.Count;
								blockMesh3.Vertices.Count += 4;
								BlockMeshVertex[] array2 = blockMesh3.Vertices.Array;
								float x5 = ((n + 0.01f) / m_resolution + num15) / 16f;
								float x6 = ((n + point6.X - 0.01f) / m_resolution + num15) / 16f;
								float y5 = ((m + 0.01f) / m_resolution + num16) / 16f;
								float y6 = ((m + point6.Y - 0.01f) / m_resolution + num16) / 16f;
								BlockMeshVertex blockMeshVertex = array2[count] = new BlockMeshVertex
								{
									Position = new Vector3(x, y, z) / m_resolution,
									Color = color,
									Face = (byte)num,
									TextureCoordinates = new Vector2(x5, y5),
									IsEmissive = isEmissive
								};
								blockMeshVertex = (array2[count + 1] = new BlockMeshVertex
								{
									Position = new Vector3(x2, y2, z2) / m_resolution,
									Color = color,
									Face = (byte)num,
									TextureCoordinates = new Vector2(x6, y5),
									IsEmissive = isEmissive
								});
								blockMeshVertex = (array2[count + 2] = new BlockMeshVertex
								{
									Position = new Vector3(x3, y3, z3) / m_resolution,
									Color = color,
									Face = (byte)num,
									TextureCoordinates = new Vector2(x6, y6),
									IsEmissive = isEmissive
								});
								blockMeshVertex = (array2[count + 3] = new BlockMeshVertex
								{
									Position = new Vector3(x4, y4, z4) / m_resolution,
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
					design.m_geometry.SubsetOpaqueByFace[i] = blockMesh;
				}
				if (blockMesh2.Indices.Count > 0)
				{
					blockMesh2.Trim();
					blockMesh2.GenerateSidesData();
					design.m_geometry.SubsetAlphaTestByFace[i] = blockMesh2;
				}
			}
		}

		public static Block GetTargetBlock(ref int value)
		{
			if (SubsystemMagicBlockBehavior.ItemsData == null)
				return BlocksManager.Blocks[1];
			int data = Terrain.ExtractData(value), index;
			var arr = SubsystemMagicBlockBehavior.ItemsData.Array;
			return BlocksManager.Blocks[data < arr.Length && !(BlocksManager.Blocks[(index = Terrain.ExtractContents(value = (int)arr[data]))] is MagicBlock) ? index : 72];
		}

		public override CraftingRecipe GetAdHocCraftingRecipe(SubsystemTerrain terrain, string[] ingredients, float heatLevel)
		{
			if (heatLevel != 0f)
				return null;
			for (int i = 0, count = 0; i < ingredients.Length; i++)
			{
				if (string.IsNullOrEmpty(ingredients[i])) count++;
				else if (count == 8)
				{
					CraftingRecipesManager.DecodeIngredient(ingredients[i], out string craftingId, out int? data);
					Block block = BlocksManager.FindBlocksByCraftingId(craftingId)[0];
					count = data ?? 0;
					return new CraftingRecipe
					{
						ResultValue = Terrain.ReplaceData(
								craftingId == BlocksManager.Blocks[Index].CraftingId ? HugeBlock.Index :
								craftingId == BlocksManager.Blocks[HugeBlock.Index].CraftingId ? Cell2DBlock.Index :
								craftingId == BlocksManager.Blocks[Cell2DBlock.Index].CraftingId ? Cell3DBlock.Index : Index,
								SubsystemMagicBlockBehavior.StoreItemData(block is MagicBlock ? (int)SubsystemMagicBlockBehavior.ItemsData.Array[count] : Terrain.ReplaceData(block.BlockIndex, count))),
						ResultCount = 4,
						Description = "Make magic block",
						Ingredients = (string[])ingredients.Clone()
					};
				}
			}
			return null;
		}

		public override string GetDisplayName(SubsystemTerrain subsystemTerrain, int value)
		{
			return GetTargetBlock(ref value).GetDisplayName(subsystemTerrain, value);
		}

		public override string GetDescription(int value)
		{
			return GetTargetBlock(ref value).GetDescription(value);
		}

		public override bool IsInteractive(SubsystemTerrain subsystemTerrain, int value)
		{
			return GetTargetBlock(ref value).IsInteractive(subsystemTerrain, value);
		}

		public override bool IsFaceTransparent(SubsystemTerrain subsystemTerrain, int face, int value)
		{
			return GetTargetBlock(ref value).IsFaceTransparent(subsystemTerrain, face, value);
		}

		public override bool ShouldGenerateFace(SubsystemTerrain subsystemTerrain, int face, int value, int neighborValue)
		{
			return GetTargetBlock(ref value).ShouldGenerateFace(subsystemTerrain, face, value, neighborValue);
		}

		public override int GetShadowStrength(int value)
		{
			return GetTargetBlock(ref value).GetShadowStrength(value);
		}

		public override int GetFaceTextureSlot(int face, int value)
		{
			return GetTargetBlock(ref value).GetFaceTextureSlot(face, value);
		}

		public override string GetSoundMaterialName(SubsystemTerrain subsystemTerrain, int value)
		{
			return GetTargetBlock(ref value).GetSoundMaterialName(subsystemTerrain, value);
		}

		public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometrySubsets geometry, int value, int x, int y, int z)
		{
			GetTargetBlock(ref value).GenerateTerrainVertices(generator, geometry, value, x, y, z);
		}

		public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
		{
			GetTargetBlock(ref value).DrawBlock(primitivesRenderer, value, color, size, ref matrix, environmentData);
		}

		public override BlockPlacementData GetPlacementValue(SubsystemTerrain subsystemTerrain, ComponentMiner componentMiner, int value, TerrainRaycastResult raycastResult)
		{
			int value2 = value;
			return GetTargetBlock(ref value2).GetPlacementValue(subsystemTerrain, componentMiner, value, raycastResult);
		}

		public override BlockPlacementData GetDigValue(SubsystemTerrain subsystemTerrain, ComponentMiner componentMiner, int value, int toolValue, TerrainRaycastResult raycastResult)
		{
			return GetTargetBlock(ref value).GetDigValue(subsystemTerrain, componentMiner, value, toolValue, raycastResult);
		}

		public override void GetDropValues(SubsystemTerrain subsystemTerrain, int oldValue, int newValue, int toolLevel, List<BlockDropValue> dropValues, out bool showDebris)
		{
			GetTargetBlock(ref oldValue).GetDropValues(subsystemTerrain, oldValue, newValue, toolLevel, dropValues, out showDebris);
		}

		public override int GetDamage(int value)
		{
			return 1;
		}

		public override int SetDamage(int value, int damage)
		{
			return value;
		}

		public override float GetSicknessProbability(int value)
		{
			return GetTargetBlock(ref value).GetSicknessProbability(value);
		}

		public override float GetMeleePower(int value)
		{
			return GetTargetBlock(ref value).GetMeleePower(value);
		}

		public override float GetMeleeHitProbability(int value)
		{
			return GetTargetBlock(ref value).GetMeleeHitProbability(value);
		}

		public override float GetProjectilePower(int value)
		{
			return GetTargetBlock(ref value).GetProjectilePower(value);
		}

		public override float GetHeat(int value)
		{
			return GetTargetBlock(ref value).GetHeat(value);
		}

		public override float GetExplosionPressure(int value)
		{
			return GetTargetBlock(ref value).GetExplosionPressure(value);
		}

		public override bool GetExplosionIncendiary(int value)
		{
			return GetTargetBlock(ref value).GetExplosionIncendiary(value);
		}

		public override Vector3 GetIconBlockOffset(int value, DrawBlockEnvironmentData environmentData)
		{
			return GetTargetBlock(ref value).GetIconBlockOffset(value, environmentData);
		}

		public override Vector3 GetIconViewOffset(int value, DrawBlockEnvironmentData environmentData)
		{
			return GetTargetBlock(ref value).GetIconViewOffset(value, environmentData);
		}

		public override float GetIconViewScale(int value, DrawBlockEnvironmentData environmentData)
		{
			return GetTargetBlock(ref value).GetIconViewScale(value, environmentData);
		}

		public override BlockDebrisParticleSystem CreateDebrisParticleSystem(SubsystemTerrain subsystemTerrain, Vector3 position, int value, float strength)
		{
			return GetTargetBlock(ref value).CreateDebrisParticleSystem(subsystemTerrain, position, value, strength);
		}

		public override BoundingBox[] GetCustomCollisionBoxes(SubsystemTerrain terrain, int value)
		{
			return GetTargetBlock(ref value).GetCustomCollisionBoxes(terrain, value);
		}

		public override BoundingBox[] GetCustomInteractionBoxes(SubsystemTerrain terrain, int value)
		{
			return GetTargetBlock(ref value).GetCustomInteractionBoxes(terrain, value);
		}

		public override int GetEmittedLightAmount(int value)
		{
			return GetTargetBlock(ref value).GetEmittedLightAmount(value);
		}

		public override float GetNutritionalValue(int value)
		{
			return GetTargetBlock(ref value).GetNutritionalValue(value);
		}

		public override bool ShouldAvoid(int value)
		{
			return GetTargetBlock(ref value).ShouldAvoid(value);
		}

		public override bool IsSwapAnimationNeeded(int oldValue, int newValue)
		{
			return GetTargetBlock(ref oldValue).IsSwapAnimationNeeded(oldValue, newValue);
		}

		public override bool IsHeatBlocker(int value)
		{
			return GetTargetBlock(ref value).IsHeatBlocker(value);
		}
	}

	public class SubsystemMagicBlockBehavior : SubsystemBlockBehavior
	{
		public static DynamicArray<long> ItemsData;
		public static bool Collected;
		public SubsystemBlockBehaviors SubsystemBlockBehaviors;

		public override int[] HandledBlocks { get { return new int[0]; } }

		static SubsystemMagicBlockBehavior()
		{
		}

		public static int StoreItemData(int value)
		{
			int i = Terrain.ExtractContents(value);
			if (BlocksManager.Blocks[i] is MagicBlock)
				return 0;
			var arr = ItemsData.Array;
			for (i = 1; i < ItemsData.Count; i++)
				if (arr[i] == 0 || arr[i] == value)
					break;
			if (i == 262144)
				for (i = 1; i < 262144 && (arr[i] >> 32) == 0; i++)
				{
				}
			if (i == 262144)
				return 0;
			ItemsData.Count++;
			ItemsData.Array[i] = value;
			return i;
		}

		public override void Load(ValuesDictionary valuesDictionary)
		{
			base.Load(valuesDictionary);
			Project.FindSubsystem<SubsystemItemsScanner>(true).ItemsScanned += GarbageCollectItems;
			Collected = false;
			SubsystemBlockBehaviors = Project.FindSubsystem<SubsystemBlockBehaviors>(true);
			var arr = valuesDictionary.GetValue("ItemsData", "1").Split(',');
			ItemsData = new DynamicArray<long>(arr.Length);
			for (int i = 0; i < arr.Length; i++)
				ItemsData.Add(long.Parse(arr[i], CultureInfo.CurrentCulture));
		}

		public override void Save(ValuesDictionary valuesDictionary)
		{
			base.Save(valuesDictionary);
			var sb = new StringBuilder(ItemsData.Count);
			var values = ItemsData.Array;
			sb.Append(values[0].ToString());
			for (int i = 1; i < ItemsData.Count; i++)
			{
				sb.Append(',');
				sb.Append(values[i].ToString());
			}
			valuesDictionary.SetValue("ItemsData", sb.ToString());
		}

		public SubsystemBlockBehavior[] GetBlockBehaviors(ref int value)
		{
			int data = Terrain.ExtractData(value);
			return SubsystemBlockBehaviors.GetBlockBehaviors(data < ItemsData.Count ? Terrain.ExtractContents(value = (int)ItemsData.Array[data]) : 72);
		}

		public override void OnBlockGenerated(int value, int x, int y, int z, bool isLoaded)
		{
			int i = Terrain.ExtractData(value);
			if (i < ItemsData.Count)
				ItemsData.Array[i] &= int.MaxValue;
			var blockBehaviors = GetBlockBehaviors(ref value);
			for (i = 0; i < blockBehaviors.Length; i++)
				blockBehaviors[i].OnBlockGenerated(value, x, y, z, isLoaded);
		}

		public override void OnBlockAdded(int value, int oldValue, int x, int y, int z)
		{
			int i = Terrain.ExtractData(value);
			if (i < ItemsData.Count)
				ItemsData.Array[i] &= int.MaxValue;
			var blockBehaviors = GetBlockBehaviors(ref value);
			for (i = 0; i < blockBehaviors.Length; i++)
				blockBehaviors[i].OnBlockAdded(value, oldValue, x, y, z);
		}

		public override void OnBlockRemoved(int value, int newValue, int x, int y, int z)
		{
			int i = Terrain.ExtractData(value);
			if (i < ItemsData.Count)
				ItemsData.Array[i] |= 2147483648L;
			var blockBehaviors = GetBlockBehaviors(ref value);
			for (i = 0; i < blockBehaviors.Length; i++)
				blockBehaviors[i].OnBlockRemoved(value, newValue, x, y, z);
		}

		public override void OnBlockModified(int value, int oldValue, int x, int y, int z)
		{
			var blockBehaviors = GetBlockBehaviors(ref value);
			for (int i = 0; i < blockBehaviors.Length; i++)
				blockBehaviors[i].OnBlockModified(value, oldValue, x, y, z);
		}

		public override void OnNeighborBlockChanged(int x, int y, int z, int neighborX, int neighborY, int neighborZ)
		{
			int i = SubsystemTerrain.Terrain.GetCellValue(x, y, z);
			var blockBehaviors = GetBlockBehaviors(ref i);
			for (i = 0; i < blockBehaviors.Length; i++)
				blockBehaviors[i].OnNeighborBlockChanged(x, y, z, neighborX, neighborY, neighborZ);
		}

		public override bool OnUse(Vector3 start, Vector3 direction, ComponentMiner componentMiner)
		{
			var nullable = componentMiner.PickTerrainForDigging(start, direction);
			if (nullable.HasValue)
			{
				var inventory = componentMiner.Inventory;
				int value = componentMiner.ActiveBlockValue;
				if (Terrain.ExtractContents(value) < 323 && Terrain.ExtractData(value) == 0)
				{
					if (Terrain.ExtractData(value = Terrain.ReplaceData(value, StoreItemData(nullable.Value.Value))) != 0)
					{
						componentMiner.RemoveActiveTool(1);
						inventory.AddSlotItems(inventory.ActiveSlotIndex, value, 1);
						return true;
					}
				}
				else
				{
					var blockBehaviors = GetBlockBehaviors(ref value);
					for (int i = 0; i < blockBehaviors.Length; i++)
						blockBehaviors[i].OnUse(start, direction, componentMiner);
				}
			}
			return false;
		}

		public override bool OnInteract(TerrainRaycastResult raycastResult, ComponentMiner componentMiner)
		{
			var blockBehaviors = GetBlockBehaviors(ref raycastResult.Value);
			for (int i = 0; i < blockBehaviors.Length; i++)
				if (blockBehaviors[i].OnInteract(raycastResult, componentMiner))
					return true;
			return false;
		}

		public override bool OnAim(Vector3 start, Vector3 direction, ComponentMiner componentMiner, AimState state)
		{
			int i = componentMiner.ActiveBlockValue;
			var blockBehaviors = GetBlockBehaviors(ref i);
			for (i = 0; i < blockBehaviors.Length; i++)
				if (blockBehaviors[i].OnAim(start, direction, componentMiner, state))
					return true;
			return false;
		}

		public override bool OnEditBlock(int x, int y, int z, int value, ComponentPlayer componentPlayer)
		{
			var blockBehaviors = GetBlockBehaviors(ref value);
			for (int i = 0; i < blockBehaviors.Length; i++)
				if (blockBehaviors[i].OnEditBlock(x, y, z, value, componentPlayer))
					return true;
			return false;
		}

		public override bool OnEditInventoryItem(IInventory inventory, int slotIndex, ComponentPlayer componentPlayer)
		{
			int i = inventory.GetSlotValue(slotIndex);
			var blockBehaviors = GetBlockBehaviors(ref i);
			for (i = 0; i < blockBehaviors.Length; i++)
				if (blockBehaviors[i].OnEditInventoryItem(inventory, slotIndex, componentPlayer))
					return true;
			return false;
		}

		public override void OnItemPlaced(int x, int y, int z, ref BlockPlacementData placementData, int itemValue)
		{
			int i = SubsystemTerrain.Terrain.GetCellValue(x, y, z);
			var blockBehaviors = GetBlockBehaviors(ref i);
			for (i = 0; i < blockBehaviors.Length; i++)
				blockBehaviors[i].OnItemPlaced(x, y, z, ref placementData, itemValue);
		}

		public override void OnItemHarvested(int x, int y, int z, int blockValue, ref BlockDropValue dropValue, ref int newBlockValue)
		{
			var blockBehaviors = GetBlockBehaviors(ref blockValue);
			for (int i = 0; i < blockBehaviors.Length; i++)
				blockBehaviors[i].OnItemHarvested(x, y, z, blockValue, ref dropValue, ref newBlockValue);
		}

		public override void OnCollide(CellFace cellFace, float velocity, ComponentBody componentBody)
		{
			int i = SubsystemTerrain.Terrain.GetCellValue(cellFace.X, cellFace.Y, cellFace.Z);
			var blockBehaviors = GetBlockBehaviors(ref i);
			for (i = 0; i < blockBehaviors.Length; i++)
				blockBehaviors[i].OnCollide(cellFace, velocity, componentBody);
		}

		public override void OnExplosion(int value, int x, int y, int z, float damage)
		{
			var blockBehaviors = GetBlockBehaviors(ref value);
			for (int i = 0; i < blockBehaviors.Length; i++)
				blockBehaviors[i].OnExplosion(value, x, y, z, damage);
		}

		public override void OnFiredAsProjectile(Projectile projectile)
		{
			var blockBehaviors = GetBlockBehaviors(ref projectile.Value);
			for (int i = 0; i < blockBehaviors.Length; i++)
				blockBehaviors[i].OnFiredAsProjectile(projectile);
		}

		public override bool OnHitAsProjectile(CellFace? cellFace, ComponentBody componentBody, WorldItem worldItem)
		{
			var blockBehaviors = GetBlockBehaviors(ref worldItem.Value);
			for (int i = 0; i < blockBehaviors.Length; i++)
				if (blockBehaviors[i].OnHitAsProjectile(cellFace, componentBody, worldItem))
					return true;
			return false;
		}

		public override void OnHitByProjectile(CellFace cellFace, WorldItem worldItem)
		{
			int i = SubsystemTerrain.Terrain.GetCellValue(cellFace.X, cellFace.Y, cellFace.Z);
			var blockBehaviors = GetBlockBehaviors(ref i);
			for (i = 0; i < blockBehaviors.Length; i++)
				blockBehaviors[i].OnHitByProjectile(cellFace, worldItem);
		}

		public override int GetProcessInventoryItemCapacity(IInventory inventory, int slotIndex, int value)
		{
			int i = inventory.GetSlotValue(slotIndex);
			var blockBehaviors = GetBlockBehaviors(ref i);
			for (i = 0; i < blockBehaviors.Length; i++)
			{
				int processInventoryItemCapacity = blockBehaviors[i].GetProcessInventoryItemCapacity(inventory, slotIndex, value);
				if (processInventoryItemCapacity > 0)
					return processInventoryItemCapacity;
			}
			return 0;
		}

		public override void ProcessInventoryItem(IInventory inventory, int slotIndex, int value, int count, int processCount, out int processedValue, out int processedCount)
		{
			int slotValue = inventory.GetSlotValue(slotIndex);
			if (inventory.GetSlotCount(slotIndex) > 0 && slotValue != 0)
			{
				var blockBehaviors = GetBlockBehaviors(ref slotValue);
				for (int i = 0; i < blockBehaviors.Length; i++)
				{
					int processInventoryItemCapacity = blockBehaviors[i].GetProcessInventoryItemCapacity(inventory, slotIndex, value);
					if (processInventoryItemCapacity > 0)
					{
						blockBehaviors[i].ProcessInventoryItem(inventory, slotIndex, value, count, MathUtils.Min(processInventoryItemCapacity, processCount), out processedValue, out processedCount);
						return;
					}
				}
			}
			processedValue = 0;
			processedCount = 0;
		}

		public static void GarbageCollectItems(ReadOnlyList<ScannedItemData> allExistingItems)
		{
			if (Collected)
				return;
			int i = 0, count = ItemsData.Count;
			var arr = ItemsData.Array;
			for (; i < count; i++)
				if (arr[i] == 0 || (arr[i] >> 32) != 0)
					ItemsData.Remove(arr[i]);
			count = allExistingItems.Count;
			for (; i < count; i++)
			{
				int value = allExistingItems[i].Value, block = Terrain.ExtractContents(value);
				if (block == MagicBlock.Index || block == HugeBlock.Index)
				{
					value = Terrain.ExtractData(value);
					if (value < ItemsData.Count)
						ItemsData.Array[value] |= 2147483648L;
				}
			}
			Collected = ItemsData.Count < 200000;
		}
	}
}