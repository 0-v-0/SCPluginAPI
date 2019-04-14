using Engine;
using Engine.Graphics;
using Game;
using System.Collections.Generic;

namespace Game
{
	public abstract class PaintedSoildBlock : SoildBlock, IPaintableBlock
	{
		public override int GetFaceTextureSlot(int face, int value)
		{
			return IsColored(Terrain.ExtractData(value)) ? GetColoredFaceTextureSlot(face, value) : DefaultTextureSlot;
		}

		public abstract int GetColoredFaceTextureSlot(int face, int value);

		public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometrySubsets geometry, int value, int x, int y, int z)
		{
			var color = GetColor(Terrain.ExtractData(value));
			GenerateTerrainVertices(this, generator, geometry, value, x, y, z, color.HasValue ? SubsystemPalette.GetColor(generator, color) : Color.White);
		}

		public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
		{
			base.DrawBlock(primitivesRenderer, value, color * SubsystemPalette.GetColor(environmentData, GetPaintColor(value)), size, ref matrix, environmentData);
		}

		public override IEnumerable<int> GetCreativeValues()
		{
			var list = new List<int>(base.GetCreativeValues());
			for (int i = 0; i < list.Count; i++)
			{
				list[i] = Terrain.MakeBlockValue(BlockIndex, 0, SetColor(list[i], null));
				for (int j = 0; j < 16; j++)
					list.Add(Terrain.MakeBlockValue(BlockIndex, 0, SetColor(list[i], j)));
			}
			return list;
		}

		public override BlockDebrisParticleSystem CreateDebrisParticleSystem(SubsystemTerrain subsystemTerrain, Vector3 position, int value, float strength)
		{
			int data = Terrain.ExtractData(value);
			Color color = SubsystemPalette.GetColor(subsystemTerrain, GetColor(data));
			return new BlockDebrisParticleSystem(subsystemTerrain, position, strength, DestructionDebrisScale, color, GetFaceTextureSlot(0, value));
		}

		public override string GetDisplayName(SubsystemTerrain subsystemTerrain, int value)
		{
			int data = Terrain.ExtractData(value);
			return SubsystemPalette.GetName(subsystemTerrain, GetColor(data), DefaultDisplayName);
		}

		public override string GetCategory(int value)
		{
			return GetColor(Terrain.ExtractData(value)).HasValue ? "Painted" : base.GetCategory(value);
		}

		public int? GetPaintColor(int value)
		{
			return GetColor(Terrain.ExtractData(value));
		}

		public int Paint(SubsystemTerrain terrain, int value, int? color)
		{
			int data = Terrain.ExtractData(value);
			return Terrain.ReplaceData(value, SetColor(data, color));
		}

		public static bool IsColored(int data)
		{
			return (data & 1) != 0;
		}

		public static int? GetColor(int data)
		{
			if ((data & 0x10) != 0)
				return (data >> 5) & 0xF;
			return null;
		}

		public static int SetColor(int data, int? color)
		{
			if (color.HasValue)
				return (data & -497) | 0x10 | ((color.Value & 0xF) << 5);
			return data & -497;
		}
	}
}