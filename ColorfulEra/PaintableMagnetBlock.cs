using Engine;
using Engine.Graphics;
using Game;
using System.Collections.Generic;

public class MagnetBlock : Game.MagnetBlock, IPaintableBlock
{
	public new const int Index = 167;

	public override void Initialize()
	{
		m_meshesByData = new BlockMesh[4];
		m_collisionBoxesByData = new BoundingBox[4][];
		Model model = ContentManager.Get<Model>("Models/Magnet");
		Matrix boneAbsoluteTransform = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("Magnet", true).ParentBone);
		ReadOnlyList<ModelMeshPart> meshParts;
		for (int i = 0; i < 4; i++)
		{
			m_meshesByData[i] = new BlockMesh();
			meshParts = model.FindMesh("Magnet", true).MeshParts;
			m_meshesByData[i].AppendModelMeshPart(meshParts[0], boneAbsoluteTransform * Matrix.CreateRotationY(3.14159274f / 4f * (float)i) * Matrix.CreateTranslation(0.5f, 0f, 0.5f), false, false, true, false, Color.White);
			m_collisionBoxesByData[i] = new BoundingBox[]
			{
				m_meshesByData[i].CalculateBoundingBox()
			};
		}
		BlockMesh standaloneMesh = m_standaloneMesh;
		meshParts = model.FindMesh("Magnet", true).MeshParts;
		standaloneMesh.AppendModelMeshPart(meshParts[0], boneAbsoluteTransform * Matrix.CreateScale(1.5f) * Matrix.CreateTranslation(0f, -0.25f, 0f), false, false, true, false, Color.White);
	}

	public override BoundingBox[] GetCustomCollisionBoxes(SubsystemTerrain terrain, int value)
	{
		return m_collisionBoxesByData[GetType(Terrain.ExtractData(value)) & 3];
	}

	public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometrySubsets geometry, int value, int x, int y, int z)
	{
		int data = Terrain.ExtractData(value);
		value = GetType(data);
		Color color = SubsystemPalette.GetColor(generator, GetColor(data));
		if (value != 0)
		{
			if ((value & 1) != 0)
				generator.GenerateMeshVertices(this, x, y, z, m_meshesByData[0], color, null, geometry.SubsetOpaque);
			if ((value & 2) != 0)
				generator.GenerateMeshVertices(this, x, y, z, m_meshesByData[1], color, null, geometry.SubsetOpaque);
			if ((value & 4) != 0)
				generator.GenerateMeshVertices(this, x, y, z, m_meshesByData[2], color, null, geometry.SubsetOpaque);
			if ((value & 8) != 0)
				generator.GenerateMeshVertices(this, x, y, z, m_meshesByData[3], color, null, geometry.SubsetOpaque);
			return;
		}
		generator.GenerateMeshVertices(this, x, y, z, m_meshesByData[value >> 1 & 1], color, null, geometry.SubsetOpaque);
	}

	public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
	{
		value = Terrain.ExtractData(value);
		color *= SubsystemPalette.GetColor(environmentData, GetColor(value));
		int type = GetType(value);
		if (type != 0)
		{
			if ((type & 1) != 0)
				BlocksManager.DrawMeshBlock(primitivesRenderer, m_meshesByData[0], color, size, ref matrix, environmentData);
			if ((type & 2) != 0)
				BlocksManager.DrawMeshBlock(primitivesRenderer, m_meshesByData[1], color, size, ref matrix, environmentData);
			if ((type & 4) != 0)
				BlocksManager.DrawMeshBlock(primitivesRenderer, m_meshesByData[2], color, size, ref matrix, environmentData);
			if ((type & 8) != 0)
				BlocksManager.DrawMeshBlock(primitivesRenderer, m_meshesByData[3], color, size, ref matrix, environmentData);
			return;
		}
		BlocksManager.DrawMeshBlock(primitivesRenderer, m_standaloneMesh, color, size, ref matrix, environmentData);
	}

	public override BlockPlacementData GetPlacementValue(SubsystemTerrain subsystemTerrain, ComponentMiner componentMiner, int value, TerrainRaycastResult raycastResult)
	{
		if (componentMiner.Project.FindSubsystem<SubsystemMagnetBlockBehavior>(true).MagnetsCount < 8)
		{
			Vector3 forward = Matrix.CreateFromQuaternion(componentMiner.ComponentCreature.ComponentCreatureModel.EyeRotation).Forward;
			return new BlockPlacementData
			{
				CellFace = raycastResult.CellFace,
				Value = Terrain.ReplaceData(value, SetColor(SetType(MathUtils.Abs(forward.X) <= MathUtils.Abs(forward.Z) ? 1 : 0, GetType(Terrain.ExtractData(value))), GetPaintColor(value)))
			};
		}
		var componentPlayer = componentMiner.ComponentPlayer;
		if (componentPlayer != null)
		{
			componentPlayer.ComponentGui.DisplaySmallMessage("Too many magnets", true, false);
		}
		return default(BlockPlacementData);
	}

	public override void GetDropValues(SubsystemTerrain subsystemTerrain, int oldValue, int newValue, int toolLevel, List<BlockDropValue> dropValues, out bool showDebris)
	{
		showDebris = true;
		if (toolLevel >= RequiredToolLevel)
		{
			dropValues.Add(new BlockDropValue
			{
				Value = Terrain.MakeBlockValue(DefaultDropContent, 0, Terrain.ExtractData(oldValue) & -2),
				Count = (int)DefaultDropCount
			});
		}
	}

	public override Vector3 GetIconBlockOffset(int value, DrawBlockEnvironmentData environmentData)
	{
		return GetType(Terrain.ExtractData(value)) != 0 ? new Vector3(-1.2f, -0.3f, -0.5f) : DefaultIconBlockOffset;
	}

	public override float GetIconViewScale(int value, DrawBlockEnvironmentData environmentData)
	{
		return GetType(Terrain.ExtractData(value)) != 0 ? 1.8f : DefaultIconViewScale;
	}

	public override string GetCategory(int value)
	{
		return GetPaintColor(value).HasValue ? "Painted" : base.GetCategory(value);
	}

	public override IEnumerable<int> GetCreativeValues()
	{
		var array = new int[256];
		for (int i = 0; i < 256; i++)
		{
			array[i] = Terrain.MakeBlockValue(Index, 0, i << 1);
		}
		return array;
	}

	public override IEnumerable<CraftingRecipe> GetProceduralCraftingRecipes()
	{
		var recipes = new CraftingRecipe[30 << 4];
		for (int i = 1; i < 16; i++)
		{
			for (int j = 0; j < 32; j += 2)
			{
				var ingredients = new string[36];
				int count = 0;
				var name = "magnet:" + j.ToString();
				if ((i & 1) != 0)
				{
					ingredients[0] = name;
					count++;
				}
				else
				{
					ingredients[0] = "stick";
				}
				if ((i & 2) != 0)
				{
					ingredients[1] = name;
					count++;
				}
				else
				{
					ingredients[1] = "stick";
				}
				if ((i & 4) != 0)
				{
					ingredients[2] = name;
					count++;
				}
				else
				{
					ingredients[2] = "stick";
				}
				if ((i & 8) != 0)
				{
					ingredients[3] = name;
					count++;
				}
				else
				{
					ingredients[3] = "stick";
				}
				recipes[(i - 1) << 5 | j] = new CraftingRecipe
				{
					ResultCount = 1,
					ResultValue = Terrain.ReplaceData(Index, i << 5 | j),
					RequiredHeatLevel = 0f,
					Ingredients = ingredients,
					Description = "Make a Cross Magnet from magnet."
				};
				ingredients = new string[36];
				ingredients[0] = "magnet:" + SetColor(i << 5, j >> 1).ToString();
				recipes[(i - 1) << 5 | j | 1] = new CraftingRecipe
				{
					ResultCount = count,
					ResultValue = Terrain.ReplaceData(Index, j),
					RemainsValue = Terrain.MakeBlockValue(StickBlock.Index),
					RemainsCount = 3 ^ count,
					RequiredHeatLevel = 0f,
					Ingredients = ingredients,
					Description = "Dismantle the Cross Magnet."
				};
			}
		}
		return recipes;
	}

	public override string GetDisplayName(SubsystemTerrain subsystemTerrain, int value)
	{
		return SubsystemPalette.GetName(subsystemTerrain, GetPaintColor(value), GetType(Terrain.ExtractData(value)) != 0 ? "Cross " + DefaultDisplayName : DefaultDisplayName);
	}

	public int? GetPaintColor(int value)
	{
		return GetColor(Terrain.ExtractData(value));
	}

	public int Paint(SubsystemTerrain subsystemTerrain, int value, int? color)
	{
		return Terrain.ReplaceData(value, SetColor(Terrain.ExtractData(value), color));
	}

	public static int? GetColor(int data)
	{
		if ((data & 30) != 0)
		{
			return data >> 1 & 15;
		}
		return null;
	}

	public static int SetColor(int data, int? color)
	{
		data &= -31;
		return color.HasValue ? color == 0 ? data : data | color.Value << 1 : data;
	}

	public static int GetType(int data)
	{
		return data >> 5 & 15;
	}

	public static int SetType(int data, int type)
	{
		return data | (type & 15) << 5;
	}
}