using System;
using System.Collections.Generic;
using Engine;
using Engine.Graphics;

namespace Game
{
	public abstract class SoildBlock : ChunkBlock
	{
		public BlockMesh standaloneBlockMesh = new BlockMesh();
		public Texture2D Texture;
		public string TexturePath;
		public bool IsEmissive;

		protected SoildBlock() : base(Matrix.CreateRotationX(1f) * Matrix.CreateRotationZ(2f), Matrix.CreateTranslation(0.875f, 0.1875f, 0f), new Color(255, 255, 255), false)
		{
			Texture = Texture2D.Load(TexturePath);
			IsEmissive = false;
		}

		public override void Initialize()
		{
			Model model = ContentManager.Get<Model>("Models/Snowball");
			Matrix boneAbsoluteTransform = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("Snowball", true).ParentBone);
			standaloneBlockMesh.AppendModelMeshPart(model.FindMesh("Snowball", true).MeshParts[0], boneAbsoluteTransform * Matrix.CreateTranslation(0f, 0f, 0f), false, false, false, false, Color.White);
			base.Initialize();
		}

		public override IEnumerable<int> GetCreativeValues()
		{
			var array = new int[16];
			for (int i = 0; i < 16; i++)
				array[i] = Terrain.MakeBlockValue(BlockIndex, 0, SetVariant(DefaultCreativeData, i));
			return array;
		}
	
		public override void GetDropValues(SubsystemTerrain subsystemTerrain, int oldValue, int newValue, int toolLevel, List<BlockDropValue> dropValues, out bool showDebris)
		{
			int data = Terrain.ExtractData(oldValue);
			if (data != 0)
			{
				showDebris = true;
				if (toolLevel >= RequiredToolLevel)
				{
					dropValues.Add(new BlockDropValue
					{
						Value = Terrain.MakeBlockValue(DefaultDropContent, 0, data),
						Count = (int)DefaultDropCount
					});
				}
			}
			else
			{
				base.GetDropValues(subsystemTerrain, oldValue, newValue, toolLevel, dropValues, out showDebris);
			}
		}

		public override string GetDisplayName(SubsystemTerrain subsystemTerrain, int value)
		{
			switch (GetVariant(Terrain.ExtractData(value)))
			{
				case 1: return DefaultDisplayName + " Chunk";
				case 2: return "Granulated " + DefaultDisplayName;
				case 3: return DefaultDisplayName + " Powder";
				case 4: return DefaultDisplayName + " Slab";
				case 5: return DefaultDisplayName + " Plate";
				case 6: return DefaultDisplayName + " Ball";
				default: return DefaultDisplayName;
			}
		}

		public override string GetCategory(int value)
		{
			return "Materials";
		}

		public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometrySubsets geometry, int value, int x, int y, int z)
		{
			GenerateTerrainVertices(this, generator, geometry, value, x, y, z, Color.White);
		}

		public static void GenerateTerrainVertices(Block block, BlockGeometryGenerator generator, TerrainGeometrySubsets geometry, int value, int x, int y, int z, Color color)
		{
			generator.GenerateCubeVertices(block, value, x, y, z, color, geometry.OpaqueSubsetsByFace);
		}

		public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
		{
			switch (GetVariant(Terrain.ExtractData(value)))
			{
				case 1:
				case 2: BlocksManager.DrawMeshBlock(primitivesRenderer, standaloneBlockMesh, color, 2f * size, ref matrix, environmentData); break;
				case 3: CustomTextureBlock.DrawFlatBlock(primitivesRenderer, value, 1f, ref matrix, Texture, color, IsEmissive, environmentData); break;
				case 4: BlocksManager.DrawMeshBlock(primitivesRenderer, standaloneBlockMesh, color, 2.5f * size, ref matrix, environmentData); break;
				case 5: BlocksManager.DrawMeshBlock(primitivesRenderer, standaloneBlockMesh, color, 2.5f * size, ref matrix, environmentData); break;
				default: BlocksManager.DrawCubeBlock(primitivesRenderer, value, new Vector3(size), ref matrix, color, color, environmentData); break;
			}
		}

		public static int GetVariant(int data)
		{
			return data & 0xF;
		}
	
		public static int SetVariant(int data, int variant)
		{
			return (data & -16) | (variant & 0xF);
		}
	}
}
