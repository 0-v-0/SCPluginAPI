using Engine;
using Engine.Graphics;

using Game;

public class SandBlock : GrassBlock
{
	public new const int Index = 7;

	public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometrySubsets geometry, int value, int x, int y, int z)
	{
		GenerateFuildVertices(generator, this, value, x, y, z, Color.White,
								Color.White,
								Color.White,
								Color.White,
								Color.White, DefaultTextureSlot, geometry.OpaqueSubsetsByFace);
	}

	public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
	{
		BlocksManager.DrawCubeBlock(primitivesRenderer, value, new Vector3(size), ref matrix, color, color, environmentData);
	}
	public override int GetFaceTextureSlot(int face, int value)
	{
		return DefaultTextureSlot;
	}
}
public class GrassBlock : Game.GrassBlock
{
	public new const int Index = 8;

	public static readonly int[] HeightTable = new[]{
		0,1,2,3,4,5,6,7,6,5,4,3,2,1,0,
		1,0,1,2,3,4,5,6,5,4,3,2,1,0,1,
		2,1,0,1,2,3,4,5,4,3,2,1,0,1,2,
		3,2,1,0,1,2,3,4,3,2,1,0,1,2,3,
		4,3,2,1,0,1,2,3,2,1,0,1,2,3,4,
		5,4,3,2,1,0,1,2,1,0,1,2,3,4,5,
		6,5,4,3,2,1,0,1,0,1,2,3,4,5,6,
		7,6,5,4,3,2,1,0,1,2,3,4,5,6,7,
		6,7,6,5,4,3,2,1,2,3,4,5,6,7,6,
		5,6,7,6,5,4,3,2,3,4,5,6,7,6,5,
		4,5,6,7,6,5,4,3,4,5,6,7,6,5,4,
		3,4,5,6,7,6,5,4,5,6,7,6,5,4,3,
		2,3,4,5,6,7,6,5,6,7,6,5,4,3,2,
		1,2,3,4,5,6,7,6,7,6,5,4,3,2,1,
	};
	protected static readonly float[] m_heightByLevel = new float[16];
	private static readonly BoundingBox[][] m_boundingBoxesByLevel = new BoundingBox[16][];
	public const int MaxLevel = 7;
	static GrassBlock()
	{
		for (int i = 0; i < 16; i++)
		{
			m_boundingBoxesByLevel[i] = new[]
			{
				new BoundingBox(Vector3.Zero, new Vector3(1f, m_heightByLevel[i] = MathUtils.Saturate(1f - (float)i / (float)MaxLevel), 1f))
			};
		}
	}

	public override BoundingBox[] GetCustomCollisionBoxes(SubsystemTerrain terrain, int value)
	{
		return m_boundingBoxesByLevel[FluidBlock.GetLevel(Terrain.ExtractData(value)) >> 1];
	}

	public static void GenerateFuildVertices(BlockGeometryGenerator generator, Block block, int value, int x, int y, int z, Color sideColor, Color topColor11, Color topColor21, Color topColor22, Color topColor12, int overrideTopTextureSlot, TerrainGeometrySubset[] subsetsByFace)
	{
		var terrain = generator.Terrain;
		if (!FluidBlock.GetIsTop(Terrain.ExtractData(value)))
		{
			generator.GenerateCubeVertices(block, value, x, y, z, 1f, 1f, 1f, 1f, sideColor, topColor11, topColor21, topColor22, topColor12, overrideTopTextureSlot, subsetsByFace);
			return;
		}
		int cellValue = Terrain.ExtractContents(value);
		var h1 = CalculateNeighborHeight(terrain, cellValue, x - 1, y, z - 1);
		var h2 = CalculateNeighborHeight(terrain, cellValue, x, y, z - 1);
		var h3 = CalculateNeighborHeight(terrain, cellValue, x + 1, y, z - 1);
		var h4 = CalculateNeighborHeight(terrain, cellValue, x - 1, y, z);
		var h5 = CalculateNeighborHeight(terrain, cellValue, x + 1, y, z);
		var h6 = CalculateNeighborHeight(terrain, cellValue, x - 1, y, z + 1);
		var h7 = CalculateNeighborHeight(terrain, cellValue, x, y, z + 1);
		var h8 = CalculateNeighborHeight(terrain, cellValue, x + 1, y, z + 1);
		var levelHeight = CalculateNeighborHeight(terrain, cellValue, x, y, z);
		generator.GenerateCubeVertices(block, value, x, y, z,
			FluidBlock.CalculateFluidVertexHeight(h1, h2, h4, levelHeight),
			FluidBlock.CalculateFluidVertexHeight(h2, h3, levelHeight, h5),
			FluidBlock.CalculateFluidVertexHeight(levelHeight, h5, h7, h8),
			FluidBlock.CalculateFluidVertexHeight(h4, levelHeight, h6, h7),
			sideColor, topColor11, topColor21, topColor22, topColor12, overrideTopTextureSlot, subsetsByFace);
	}

	public static float CalculateNeighborHeight(Terrain terrain, int index, int x, int y, int z)
	{
		int value = terrain.GetCellValueFast(x, y, z);
		int contents = Terrain.ExtractContents(value);
		if (contents == index)
		{
			return contents == 8 && (value & 1 << 14) != 0 ? 1f : m_heightByLevel[FluidBlock.GetLevel(Terrain.ExtractData(value)) >> 1];
		}
		var fluidBlock = BlocksManager.FluidBlocks[contents];
		return fluidBlock != null ? fluidBlock.CalculateNeighborHeight(value) : BlocksManager.Blocks[contents].IsCollidable ? 1f : 0.01f;
	}

	public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
	{
		Color topColor = color * BlockColorsMap.GrassColorsMap.Lookup(environmentData.Temperature, environmentData.Humidity);
		BlocksManager.DrawCubeBlock(primitivesRenderer, value, new Vector3(size), ref matrix, color, topColor, environmentData);
	}

	public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometrySubsets geometry, int value, int x, int y, int z)
	{
		GenerateFuildVertices(generator, this, value, x, y, z, Color.White,
								BlockColorsMap.GrassColorsMap.Lookup(generator.Terrain, x, z),
								BlockColorsMap.GrassColorsMap.Lookup(generator.Terrain, x + 1, z),
								BlockColorsMap.GrassColorsMap.Lookup(generator.Terrain, x + 1, z + 1),
								BlockColorsMap.GrassColorsMap.Lookup(generator.Terrain, x, z + 1), DefaultTextureSlot, geometry.OpaqueSubsetsByFace);
	}
	public override bool ShouldGenerateFace(SubsystemTerrain subsystemTerrain, int face, int value, int neighborValue)
	{
		return BlocksManager.Blocks[Terrain.ExtractContents(neighborValue)] is WoodBlock || base.ShouldGenerateFace(subsystemTerrain, face, value, neighborValue);
	}
	public override int GetFaceTextureSlot(int face, int value)
	{
		switch (face)
		{
			case 4:
				return 0;
			case 5:
				return 2;
			default:
				return (Terrain.ExtractData(value) & 1) == 0 ? 3 : 68;
		}
	}
}

namespace Game
{
	public class SubsystemSlopeBlockBehavior : SubsystemBlockBehavior
	{
		public override int[] HandledBlocks => new[] { 7, 8 };
		public override void OnBlockGenerated(int value, int x, int y, int z, bool isLoaded)
		{
			int data = Terrain.ExtractData(value);
			int contents = Terrain.ExtractContents(value);
			var terrain = SubsystemTerrain.Terrain;
			int contents2 = terrain.GetCellContents(x, y + 1, z);
			if (contents2 != contents)
			{
				int level = 0;
				if (contents != 8 || contents2 != 61)
				{
					level = global::GrassBlock.HeightTable[x & 15 | MathUtils.Abs(z % 13) << 4];
					if (global::GrassBlock.CalculateNeighborHeight(terrain, contents, x, y, z - 1) > 0.5f)
						level--;
					if (global::GrassBlock.CalculateNeighborHeight(terrain, contents, x - 1, y, z) > 0.5f)
						level--;
					if (global::GrassBlock.CalculateNeighborHeight(terrain, contents, x + 1, y, z) > 0.5f)
						level--;
					if (global::GrassBlock.CalculateNeighborHeight(terrain, contents, x, y, z + 1) > 0.5f)
						level--;
				}
				terrain.SetCellValueFast(x, y, z, Terrain.ReplaceData(value, FluidBlock.SetIsTop(FluidBlock.SetLevel(data, MathUtils.Max(level, 0) << 1 | (data & 1)), true)));
			}
		}
		public override void OnNeighborBlockChanged(int x, int y, int z, int neighborX, int neighborY, int neighborZ)
		{
			int value = SubsystemTerrain.Terrain.GetCellValueFast(x, y, z);
			int contents2 = SubsystemTerrain.Terrain.GetCellContents(x, y + 1, z);
			SubsystemTerrain.Terrain.SetCellValueFast(x, y, z, Terrain.ReplaceData(value, FluidBlock.SetIsTop(Terrain.ExtractData(value), contents2 != Terrain.ExtractContents(value) || (Terrain.ExtractContents(value) == 8 && BlocksManager.Blocks[contents2] is WoodBlock))));
		}
	}
	public class SubsystemSlopeGrassBlockBehavior : SubsystemGrassBlockBehavior
	{
		public override void OnNeighborBlockChanged(int x, int y, int z, int neighborX, int neighborY, int neighborZ)
		{
			int cellValue = SubsystemTerrain.Terrain.GetCellValue(x, y + 1, z);
			if (Terrain.ExtractContents(cellValue) == 61)
				SubsystemTerrain.ChangeCell(x, y, z, SubsystemTerrain.Terrain.GetCellValueFast(x, y, z) | 1 << 14, true);
			else
			{
				SubsystemTerrain.ChangeCell(x, y, z, SubsystemTerrain.Terrain.GetCellValueFast(x, y, z) & ~(1 << 14), true);
			}
			if (KillsGrassIfOnTopOfIt(cellValue))
				SubsystemTerrain.ChangeCell(x, y, z, Terrain.MakeBlockValue(2), true);
		}
	}
}