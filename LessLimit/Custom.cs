using System;
using System.Collections.Generic;
using Engine;
using Engine.Graphics;

namespace Game
{
		public static int DamageItem(int value, int damageCount)
		{
			int num = TerrainData.ExtractContents(value);
			Block block = BlocksManager.Blocks[num];
			if (block.Durability < 0)
				return value;
			if ((num > 29 && num < 35) || (num > 80 && num < 82) || num == 122 || num == 123)
				return 23;
			if (num == 82 || num == 124 || num == 200)
				return 195;
			if (num == 81)
				return 79;
			if (num == 169)
				return 55;
			if (num == 170)
				return 53;
			if ((num > 36 && num < 38) || num == 212)
				return 40;
			if ((num > 113 && num < 116) || num == 125)
				return 111;
			if (num > 218 && num < 222)
				return 42;
			num = block.GetDamage(value) + damageCount;
			if (num <= block.Durability)
				return block.SetDamage(value, num);
			return block.GetDamageDestructionValue(value);
		}
	public class GunpowderKegBlock : PaintedCubeBlock
	{
		public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometrySubsets geometry, int value, int x, int y, int z)
		{
			int? color = PaintedCubeBlock.GetColor(TerrainData.ExtractData(value));
			generator.GenerateMeshVertices(this, x, y, z, this.m_blockMesh, color.HasValue ? PaintBucketBlock.PaintColors[color.Value] : Color.White, null, geometry.SubsetAlphaTest);
		}
		public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
		{
			int? color = PaintedCubeBlock.GetColor(TerrainData.ExtractData(value));
			BlocksManager.DrawMeshBlock(primitivesRenderer, this.m_standaloneBlockMesh, color.HasValue ? PaintBucketBlock.PaintColors[color.Value] : Color.White, size, ref matrix, environmentData);
		}
		public override float GetExplosionPressure(int value)
		{
			TerrainData.ExtractData(value) data >> 5
		}
	}
}