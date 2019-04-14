using System;
using System.Linq;
using System.Collections.Generic;
using Engine;
using Engine.Graphics;
using Game;

namespace Game
{
	public abstract class PaintedMeshBlock : PaintedCubeBlock
	{
		public BlockMesh m_blockMesh = new BlockMesh();
		public BlockMesh m_standaloneBlockMesh = new BlockMesh();

		protected PaintedMeshBlock() : base(0)
		{
		}
		public override int GetFaceTextureSlot(int face, int value)
		{
			return DefaultTextureSlot;
		}
		public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometrySubsets geometry, int value, int x, int y, int z)
		{
			generator.GenerateMeshVertices(this, x, y, z, m_blockMesh, SubsystemPalette.GetColor(generator, GetColor(Terrain.ExtractData(value))), null, geometry.SubsetAlphaTest);
		}
		public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
		{
			BlocksManager.DrawMeshBlock(primitivesRenderer, m_standaloneBlockMesh, color * SubsystemPalette.GetColor(environmentData, GetColor(Terrain.ExtractData(value))), size, ref matrix, environmentData);
		}
		public override void GetDropValues(SubsystemTerrain subsystemTerrain, int oldValue, int newValue, int toolLevel, List<BlockDropValue> dropValues, out bool showDebris)
		{
			showDebris = true;
			if (toolLevel >= RequiredToolLevel)
				dropValues.Add(new BlockDropValue
				{
					Value = Terrain.MakeBlockValue(DefaultDropContent, 0, Terrain.ExtractData(oldValue)),
					Count = (int)DefaultDropCount
				});
		}
		public static IEnumerable<int> GetCreativeValues(Block block)
		{
			var list = block.GetCreativeValues().ToList();
			int count = list.Count;
			for (int i = 0; i < count; i++)
			{
				for (int j = 0; j < 16; j++)
					list.Add(Terrain.MakeBlockValue(block.BlockIndex, 0, global::ArrowBlock.SetColor(list[i], j)));
			}
			return list;
		}
	}
}
public class ArrowBlock : Game.ArrowBlock, IPaintableBlock
{
	public new const int Index = 192;

	public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometrySubsets geometry, int value, int x, int y, int z)
	{
		int arrowType = (int)GetArrowType(Terrain.ExtractData(value));
		if (arrowType >= 0 && arrowType < m_standaloneBlockMeshes.Count)
		{
			generator.GenerateMeshVertices(this, x, y, z, m_standaloneBlockMeshes[arrowType], SubsystemPalette.GetColor(generator, GetColor(Terrain.ExtractData(value))), null, geometry.SubsetAlphaTest);
		}
	}
	public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
	{
		int arrowType = (int)GetArrowType(Terrain.ExtractData(value));
		if (arrowType >= 0 && arrowType < m_standaloneBlockMeshes.Count)
		{
			BlocksManager.DrawMeshBlock(primitivesRenderer, m_standaloneBlockMeshes[arrowType], color, 2f * size, ref matrix, environmentData);
		}
	}
	public override void GetDropValues(SubsystemTerrain subsystemTerrain, int oldValue, int newValue, int toolLevel, List<BlockDropValue> dropValues, out bool showDebris)
	{
		showDebris = true;
		if (toolLevel >= RequiredToolLevel)
			dropValues.Add(new BlockDropValue
			{
				Value = Terrain.MakeBlockValue(DefaultDropContent, 0, Terrain.ExtractData(oldValue)),
				Count = (int)DefaultDropCount
			});
	}
	public override IEnumerable<int> GetCreativeValues()
	{
		return PaintedMeshBlock.GetCreativeValues(this);
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
	public static int? GetColor(int data)
	{
		if ((data & 0x10) != 0)
		{
			return (data >> 5) & 0xF;
		}
		return null;
	}
	public static int SetColor(int data, int? color)
	{
		if (color.HasValue)
		{
			return (data & -497) | 0x10 | ((color.Value & 0xF) << 5);
		}
		return data & -497;
	}
}
public class ButtonBlock : Game.ButtonBlock, IPaintableBlock
{
	public new const int Index = 142;
	public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometrySubsets geometry, int value, int x, int y, int z)
	{
		int num = Terrain.ExtractData(value);
		if (num < m_blockMeshesByData.Length)
		{
			generator.GenerateMeshVertices(this, x, y, z, m_blockMeshesByData[num], Color.White, null, geometry.SubsetOpaque);
			generator.GenerateWireVertices(value, x, y, z, GetFace(value), 0.25f, Vector2.Zero, geometry.SubsetOpaque);
		}
	}
	public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
	{
		BlocksManager.DrawMeshBlock(primitivesRenderer, m_standaloneBlockMesh, color, 2f * size, ref matrix, environmentData);
	}
	public override IEnumerable<int> GetCreativeValues()
	{
		return PaintedMeshBlock.GetCreativeValues(this);
	}
	public int? GetPaintColor(int value)
	{
		return ArrowBlock.GetColor(Terrain.ExtractData(value));
	}
	public int Paint(SubsystemTerrain terrain, int value, int? color)
	{
		int data = Terrain.ExtractData(value);
		return Terrain.ReplaceData(value, ArrowBlock.SetColor(data, color));
	}
}
public class SwitchBlock : Game.SwitchBlock, IPaintableBlock
{
	public new const int Index = 141;
	public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometrySubsets geometry, int value, int x, int y, int z)
	{
		int num = Terrain.ExtractData(value);
		if (num < m_blockMeshesByData.Length)
		{
			generator.GenerateMeshVertices(this, x, y, z, m_blockMeshesByData[num], Color.White, null, geometry.SubsetOpaque);
			generator.GenerateWireVertices(value, x, y, z, GetFace(value), 0.25f, Vector2.Zero, geometry.SubsetOpaque);
		}
	}
	public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
	{
		BlocksManager.DrawMeshBlock(primitivesRenderer, m_standaloneBlockMesh, color, 2f * size, ref matrix, environmentData);
	}
	public override IEnumerable<int> GetCreativeValues()
	{
		return PaintedMeshBlock.GetCreativeValues(this);
	}
	public int? GetPaintColor(int value)
	{
		return ArrowBlock.GetColor(Terrain.ExtractData(value));
	}
	public int Paint(SubsystemTerrain terrain, int value, int? color)
	{
		int data = Terrain.ExtractData(value);
		return Terrain.ReplaceData(value, ArrowBlock.SetColor(data, color));
	}
}
public class CompassBlock : PaintedMeshBlock
{
	public const int Index = 117;
	public BlockMesh m_caseMesh = new BlockMesh();
	public BlockMesh m_pointerMesh = new BlockMesh();
	public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometrySubsets geometry, int value, int x, int y, int z)
	{
	}
	public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
	{
		float radians = 0f;
		if (environmentData != null && environmentData.SubsystemTerrain != null)
		{
			Vector3 forward = environmentData.InWorldMatrix.Forward;
			Vector3 translation = environmentData.InWorldMatrix.Translation;
			Vector3 v = environmentData.SubsystemTerrain.Project.FindSubsystem<SubsystemMagnetBlockBehavior>(true).FindNearestCompassTarget(translation);
			Vector3 vector = translation - v;
			radians = Vector2.Angle(v2: new Vector2(forward.X, forward.Z), v1: new Vector2(vector.X, vector.Z));
		}
		Matrix matrix2 = matrix;
		Matrix matrix3 = Matrix.CreateRotationY(radians) * matrix;
		BlocksManager.DrawMeshBlock(primitivesRenderer, m_caseMesh, color, size * 6f, ref matrix2, environmentData);
		BlocksManager.DrawMeshBlock(primitivesRenderer, m_pointerMesh, color, size * 6f, ref matrix3, environmentData);
	}
}
public class SaddleBlock : PaintedMeshBlock
{
	public const int Index = 158;
	public override void Initialize()
	{
		Model model = ContentManager.Get<Model>("Models/Saddle");
		Matrix boneAbsoluteTransform = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("Saddle", true).ParentBone);
		BlockMesh standaloneBlockMesh = m_standaloneBlockMesh;
		ReadOnlyList<ModelMeshPart> meshParts = model.FindMesh("Saddle", true).MeshParts;
		standaloneBlockMesh.AppendModelMeshPart(meshParts[0], boneAbsoluteTransform * Matrix.CreateTranslation(0f, -0.2f, 0f), false, false, false, false, new Color(224, 224, 224));
		BlockMesh standaloneBlockMesh2 = m_standaloneBlockMesh;
		meshParts = model.FindMesh("Saddle", true).MeshParts;
		standaloneBlockMesh2.AppendModelMeshPart(meshParts[0], boneAbsoluteTransform * Matrix.CreateTranslation(0f, -0.2f, 0f), false, true, false, false, new Color(96, 96, 96));
		base.Initialize();
	}
	public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
	{
		BlocksManager.DrawMeshBlock(primitivesRenderer, m_standaloneBlockMesh, color * SubsystemPalette.GetColor(environmentData, GetPaintColor(value)), 2f * size, ref matrix, environmentData);
	}
}
public class WhistleBlock : PaintedMeshBlock
{
	public const int Index = 160;

	public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
	{
		BlocksManager.DrawMeshBlock(primitivesRenderer, m_standaloneBlockMesh, color * SubsystemPalette.GetColor(environmentData, GetPaintColor(value)), 9f * size, ref matrix, environmentData);
	}
}
