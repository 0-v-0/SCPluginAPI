using System;
using Engine;

namespace Game
{
	public abstract class FluidlikeBlock : CubeBlock
	{
		private float[] m_heightByLevel = new float[16];
		private BoundingBox[][] m_boundingBoxesByLevel = new BoundingBox[16][];
		public readonly int MaxLevel;

		public FluidlikeBlock(int maxLevel)
		{
			MaxLevel = maxLevel;
			for (int i = 0; i < 16; i++)
			{
				float num = m_heightByLevel[i] = MathUtils.Saturate(1f - (float)i / (float)MaxLevel);
				m_boundingBoxesByLevel[i] = new[]
				{
					new BoundingBox(new Vector3(0f, 0f, 0f), new Vector3(1f, num, 1f))
				};
			}
		}

		public override BoundingBox[] GetCustomCollisionBoxes(SubsystemTerrain terrain, int value)
		{
			return m_boundingBoxesByLevel[FluidBlock.GetLevel(Terrain.ExtractData(value))];
		}

		public void GenerateFuildVertices(BlockGeometryGenerator generator, Block block, int value, int x, int y, int z, float height11, float height21, float height22, float height12, Color sideColor, Color topColor11, Color topColor21, Color topColor22, Color topColor12, int overrideTopTextureSlot, TerrainGeometrySubset[] subsetsByFace)
		{
			var terrain = generator.Terrain;
			int cellValue = terrain.GetCellValue(x, y + 1, z);
			//if (!BlocksManager.Blocks[cellValue].IsFaceTransparent(generator.SubsystemTerrain, 5, cellValue))
			//{
				//generator.GenerateCubeVertices(this, value, x, y, z, 1f, 1f, 1f, 1f, sideColor, topColor11, topColor21, topColor22, topColor12, overrideTopTextureSlot, subsetsByFace);
				//return;
			//}
			var h1 = CalculateNeighborHeight(terrain, x - 1, y, z - 1);
			var h2 = CalculateNeighborHeight(terrain, x, y, z - 1);
			var h3 = CalculateNeighborHeight(terrain, x + 1, y, z - 1);
			var h4 = CalculateNeighborHeight(terrain, x - 1, y, z);
			var h5 = CalculateNeighborHeight(terrain, x + 1, y, z);
			var h6 = CalculateNeighborHeight(terrain, x - 1, y, z + 1);
			var h7 = CalculateNeighborHeight(terrain, x, y, z + 1);
			var h8 = CalculateNeighborHeight(terrain, x + 1, y, z + 1);
			var levelHeight = m_heightByLevel[FluidBlock.GetLevel(value)];
			generator.GenerateCubeVertices(this, value, x, y, z,
				FluidBlock.CalculateFluidVertexHeight(h1, h2, h4, levelHeight),
				FluidBlock.CalculateFluidVertexHeight(h2, h3, levelHeight, h5),
				FluidBlock.CalculateFluidVertexHeight(levelHeight, h5, h7, h8),
				FluidBlock.CalculateFluidVertexHeight(h4, levelHeight, h6, h7),
				sideColor, topColor11, topColor21, topColor22, topColor12, overrideTopTextureSlot, subsetsByFace);
		}

		private float CalculateNeighborHeight(Terrain terrain, int x, int y, int z)
		{
			int contents = terrain.GetCellContentsFast(x, y, z);
			return contents == 0 ? 0.01f : m_heightByLevel[FluidBlock.GetLevel(x * 91 + contents * y ^ z * MaxLevel)];
		}
	}
}