using Engine;
using Engine.Graphics;
using Game;
using System.Collections.Generic;
using TemplatesDatabase;

namespace Game
{
	public class SubsystemComplexFurnitureBlockBehavior : SubsystemFurnitureBlockBehavior, IDrawable
	{
		public static int[] m_drawOrders = { 9 };
		public int[] DrawOrders => m_drawOrders;
		public static Dictionary<Point3, TerrainGeometrySubsets> Data;
		public static SubsystemAnimatedTextures SubsystemAnimatedTextures;
		public static SubsystemSky SubsystemSky;
		public static FurnitureBlock block;
		public static BlockGeometryGenerator BlockGeometryGenerator;

		public SubsystemComplexFurnitureBlockBehavior()
		{
			Data = new Dictionary<Point3, TerrainGeometrySubsets>();
		}

		public void Draw(Camera camera, int drawOrder)
		{
			var e = Data.Values.GetEnumerator();
			while (e.MoveNext())
			{
				Display.BlendState = BlendState.Opaque;
				Display.DepthStencilState = DepthStencilState.Default;
				Display.RasterizerState = RasterizerState.CullCounterClockwiseScissor;
				var c = e.Current;
				var m_shader = SubsystemTerrain.TerrainRenderer.m_opaqueShader;
				var m_subsystemSky = SubsystemSky;
				m_shader.GetParameter("u_texture").SetValue(SubsystemAnimatedTextures.AnimatedBlocksTexture);
				m_shader.GetParameter("u_samplerState").SetValue(SamplerState.PointClamp);
				m_shader.GetParameter("u_fogColor").SetValue(new Vector3(m_subsystemSky.ViewFogColor));
				var value = new Vector2(m_subsystemSky.ViewFogRange.X, 1f / (m_subsystemSky.ViewFogRange.Y - m_subsystemSky.ViewFogRange.X));
				m_shader.GetParameter("u_fogStartInvLength").SetValue(value);
				m_shader.GetParameter("u_worldViewProjectionMatrix").SetValue(camera.ViewProjectionMatrix);
				m_shader.GetParameter("u_viewPosition").SetValue(camera.ViewPosition);
				TerrainGeometrySubset subset = c.SubsetOpaque;
				Display.DrawUserIndexed(PrimitiveType.TriangleList, m_shader, TerrainVertex.VertexDeclaration, subset.Vertices.Array, 0, subset.Vertices.Count, subset.Indices.Array, 0, subset.Indices.Count);
				m_shader = SubsystemTerrain.TerrainRenderer.m_alphaTestedShader;
				m_shader.GetParameter("u_texture").SetValue(SubsystemAnimatedTextures.AnimatedBlocksTexture);
				m_shader.GetParameter("u_samplerState").SetValue(SamplerState.PointClamp);
				m_shader.GetParameter("u_fogColor").SetValue(new Vector3(m_subsystemSky.ViewFogColor));
				m_shader.GetParameter("u_fogStartInvLength").SetValue(value);
				m_shader.GetParameter("u_worldViewProjectionMatrix").SetValue(camera.ViewProjectionMatrix);
				m_shader.GetParameter("u_viewPosition").SetValue(camera.ViewPosition);
				subset = c.SubsetAlphaTest;
				Display.DrawUserIndexed(PrimitiveType.TriangleList, m_shader, TerrainVertex.VertexDeclaration, subset.Vertices.Array, 0, subset.Vertices.Count, subset.Indices.Array, 0, subset.Indices.Count);
			}
		}
		public void AddFurniture(int value, int x, int y, int z)
		{
			var terrainGeometrySubset = new TerrainGeometrySubset(new DynamicArray<TerrainVertex>(), new DynamicArray<ushort>());
			var geometry = new TerrainGeometrySubsets
			{
				SubsetOpaque = terrainGeometrySubset,
				SubsetAlphaTest = terrainGeometrySubset,
				OpaqueSubsetsByFace = new[]
				{
					terrainGeometrySubset, terrainGeometrySubset, terrainGeometrySubset,
					terrainGeometrySubset, terrainGeometrySubset, terrainGeometrySubset
				},
				AlphaTestSubsetsByFace = new[]
				{
					terrainGeometrySubset, terrainGeometrySubset, terrainGeometrySubset,
					terrainGeometrySubset, terrainGeometrySubset, terrainGeometrySubset
				}
			};
			int data = Terrain.ExtractData(value);
			int designIndex = FurnitureBlock.GetDesignIndex(data);
			int rotation = FurnitureBlock.GetRotation(data);
			var generator = BlockGeometryGenerator;
			FurnitureDesign design = generator.SubsystemFurnitureBlockBehavior.GetDesign(designIndex);
			if (design == null)
				return;
			FurnitureGeometry geometry2 = design.Geometry;
			int mountingFacesMask = design.MountingFacesMask;
			for (int i = 0; i < 6; i++)
			{
				int num = CellFace.OppositeFace(i < 4 ? ((i + rotation) % 4) : i);
				byte b = (byte)(LightingManager.LightIntensityByLightValueAndFace[15 + 16 * num] * 255f);
				var color = new Color(b, b, b);
				if (geometry2.SubsetOpaqueByFace[i] != null)
					generator.GenerateShadedMeshVertices(block, x, y, z, geometry2.SubsetOpaqueByFace[i], color, block.m_matrices[rotation], block.m_facesMaps[rotation], geometry.OpaqueSubsetsByFace[num]);
				if (geometry2.SubsetAlphaTestByFace[i] != null)
					generator.GenerateShadedMeshVertices(block, x, y, z, geometry2.SubsetAlphaTestByFace[i], color, block.m_matrices[rotation], block.m_facesMaps[rotation], geometry.AlphaTestSubsetsByFace[num]);
				int num2 = CellFace.OppositeFace((i < 4) ? ((i - rotation + 4) % 4) : i);
				if ((mountingFacesMask & (1 << num2)) != 0)
				{
					generator.GenerateWireVertices(value, x, y, z, i, 0f, Vector2.Zero, geometry.SubsetOpaque);
				}
			}
			Data[new Point3(x, y, z)] = geometry;
		}

		public override void OnBlockAdded(int value, int oldValue, int x, int y, int z)
		{
			AddFurniture(value, x, y, z);
			AddTerrainFurniture(value);
			AddParticleSystems(value, x, y, z);
		}
		public override void OnBlockGenerated(int value, int x, int y, int z, bool isLoaded)
		{
			base.OnBlockGenerated(value, x, y, z, isLoaded);
			AddFurniture(value, x, y, z);
		}
		public override void OnBlockModified(int value, int oldValue, int x, int y, int z)
		{
			base.OnBlockModified(value, oldValue, x, y, z);
			AddFurniture(value, x, y, z);
		}
		public override void OnBlockRemoved(int value, int newValue, int x, int y, int z)
		{
			Data.Remove(new Point3(x, y, z));
			RemoveTerrainFurniture(value);
			RemoveParticleSystems(x, y, z);
		}

		public override void OnChunkDiscarding(TerrainChunk chunk)
		{
			var list = new DynamicArray<Point3>();
			var e = Data.Keys.GetEnumerator();
			while (e.MoveNext())
			{
				var key = e.Current;
				if (key.X >= chunk.Origin.X && key.X < chunk.Origin.X + 16 && key.Z >= chunk.Origin.Y && key.Z < chunk.Origin.Y + 16)
				{
					list.Add(key);
				}
			}
			var e2 = list.GetEnumerator();
			while (e2.MoveNext())
			{
				var item = e2.Current;
				RemoveParticleSystems(item.X, item.Y, item.Z);
			}
		}

		public override void Load(ValuesDictionary valuesDictionary)
		{
			base.Load(valuesDictionary);
			block = (FurnitureBlock)BlocksManager.Blocks[FurnitureBlock.Index];
			SubsystemAnimatedTextures = Project.FindSubsystem<SubsystemAnimatedTextures>(true);
			SubsystemSky = Project.FindSubsystem<SubsystemSky>(true);
			BlockGeometryGenerator = new BlockGeometryGenerator(SubsystemTerrain.Terrain, SubsystemTerrain, Project.FindSubsystem<SubsystemElectricity>(true), SubsystemTerrain.SubsystemFurnitureBlockBehavior, Project.FindSubsystem<SubsystemMetersBlockBehavior>(true), SubsystemTerrain.SubsystemPalette);
		}
	}
}

public class FurnitureBlock : Game.FurnitureBlock
{
	public new const int Index = Game.FurnitureBlock.Index;
	public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometrySubsets geometry, int value, int x, int y, int z)
	{
	}
}