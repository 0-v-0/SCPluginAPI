using System;
using Engine;
using Engine.Graphics;
using TemplatesDatabase;

namespace Game
{
	public class SubsystemClearSky : SubsystemSky, IDrawable
	{
		public Shader OpaqueShader;
		//public Shader AlphaTestedShader;
		//public Shader TransparentShader;
		public TerrainChunkGeometry m_geometry;

		public new void Draw(Camera camera, int drawOrder)
		{
			if (drawOrder == m_drawOrders[0])
			{
				ViewUnderWaterDepth = 0f;
				ViewUnderMagmaDepth = 0f;
				Vector3 viewPosition = camera.ViewPosition;
				int x = Terrain.ToCell(viewPosition.X);
				int y = Terrain.ToCell(viewPosition.Y);
				int z = Terrain.ToCell(viewPosition.Z);
				FluidBlock surfaceFluidBlock;
				float? surfaceHeight = m_subsystemFluidBlockBehavior.GetSurfaceHeight(x, y, z, out surfaceFluidBlock);
				if (surfaceHeight.HasValue)
					if (surfaceFluidBlock is WaterBlock)
					{
						ViewUnderWaterDepth = surfaceHeight.Value + 0.1f - viewPosition.Y;
					else if (surfaceFluidBlock is MagmaBlock)
						ViewUnderMagmaDepth = surfaceHeight.Value + 1f - viewPosition.Y;
				}
				if (ViewUnderWaterDepth > 0f)
				{
					int humidity = m_subsystemTerrain.Terrain.GetHumidity(x, z);
					int temperature = m_subsystemTerrain.Terrain.GetTemperature(x, z);
					Color c = BlockColorsMap.WaterColorsMap.Lookup(temperature, humidity);
					float num = MathUtils.Lerp(1f, 0.5f, (float)humidity / 15f);
					float num2 = MathUtils.Lerp(1f, 0.2f, MathUtils.Saturate(0.075f * (ViewUnderWaterDepth - 2f)));
					float num3 = MathUtils.Lerp(0.33f, 1f, SkyLightIntensity);
					m_viewFogRange.X = 0f;
					m_viewFogRange.Y = MathUtils.Lerp(4f, VisibilityRange, num * num2 * num3);
					m_viewFogColor = Color.MultiplyColorOnly(c, 0.66f * num2 * num3);
					m_viewIsSkyVisible = false;
				}
				else if (ViewUnderMagmaDepth > 0f)
				{
					m_viewFogRange.X = 0f;
					m_viewFogRange.Y = VisibilityRange;
					m_viewFogColor = new Color(40, 255, 40);
					m_viewIsSkyVisible = false;
				}
				else
				{
					int temperature2 = m_subsystemTerrain.Terrain.GetTemperature(Terrain.ToCell(viewPosition.X), Terrain.ToCell(viewPosition.Z));
					float num4 = MathUtils.Lerp(0.5f, 0f, m_subsystemWeather.GlobalPrecipitationIntensity);
					float num5 = MathUtils.Lerp(1f, 0.8f, m_subsystemWeather.GlobalPrecipitationIntensity);
					m_viewFogRange.X = VisibilityRange * num4;
					m_viewFogRange.Y = VisibilityRange * num5;
					m_viewFogColor = CalculateSkyColor(new Vector3(camera.ViewDirection.X, 0f, camera.ViewDirection.Z), m_subsystemTimeOfDay.TimeOfDay, m_subsystemWeather.GlobalPrecipitationIntensity, temperature2);
					m_viewIsSkyVisible = true;
				}
				if (!DrawSkyEnabled || !m_viewIsSkyVisible || SettingsManager.SkyRenderingMode == SkyRenderingMode.Disabled)
				{
					GameViewWidget gameViewWidget = camera.View.GameWidget.GameViewWidget;
					FlatBatch2D flatBatch2D = m_primitivesRenderer2d.FlatBatch(-1, DepthStencilState.None, RasterizerState.CullNoneScissor, BlendState.Opaque);
					int count = flatBatch2D.TriangleVertices.Count;
					flatBatch2D.QueueQuad(Vector2.Zero, gameViewWidget.ActualSize, 0f, m_viewFogColor);
					flatBatch2D.TransformTriangles(camera.WidgetMatrix, count, -1);
					m_primitivesRenderer2d.Flush(true);
				}
			}
			else if (drawOrder == m_drawOrders[1])
			{
				DrawOpaque(camera);
				//DrawAlphaTested(camera);
				if (DrawSkyEnabled && m_viewIsSkyVisible && SettingsManager.SkyRenderingMode != SkyRenderingMode.Disabled)
				{
					DrawSkydome(camera);
					DrawStars(camera);
					DrawSunAndMoon(camera);
					DrawClouds(camera);
					m_primitivesRenderer3d.Flush(camera.ViewProjectionMatrix, true, 2147483647);
				}
			}
			else
			{
				//DrawTransparent(camera);
				DrawLightning(camera);
				m_primitivesRenderer3d.Flush(camera.ViewProjectionMatrix, true, 2147483647);
			}
		}
		public void GenerateChunkVertices(Terrain terrain, TerrainChunk chunk, int x1, int z1, int x2, int z2)
		{
			var coords = chunk.Coords;
			TerrainChunk chunkAtCoords = terrain.GetChunkAtCoords(coords.X - 1, coords.Y - 1);
			TerrainChunk chunkAtCoords2 = terrain.GetChunkAtCoords(coords.X, coords.Y - 1);
			TerrainChunk chunkAtCoords3 = terrain.GetChunkAtCoords(coords.X + 1, coords.Y - 1);
			TerrainChunk chunkAtCoords4 = terrain.GetChunkAtCoords(coords.X - 1, coords.Y);
			TerrainChunk chunkAtCoords5 = terrain.GetChunkAtCoords(coords.X + 1, coords.Y);
			TerrainChunk chunkAtCoords6 = terrain.GetChunkAtCoords(coords.X - 1, coords.Y + 1);
			TerrainChunk chunkAtCoords7 = terrain.GetChunkAtCoords(coords.X, coords.Y + 1);
			TerrainChunk chunkAtCoords8 = terrain.GetChunkAtCoords(coords.X + 1, coords.Y + 1);
			if (chunkAtCoords4 == null)
				x1 = MathUtils.Max(x1, 1);
			if (chunkAtCoords2 == null)
				z1 = MathUtils.Max(z1, 1);
			if (chunkAtCoords5 == null)
				x2 = MathUtils.Min(x2, 15);
			if (chunkAtCoords7 == null)
				z2 = MathUtils.Min(z2, 15);
			for (int i = x1; i < x2; i++)
			{
				for (int j = z1; j < z2; j++)
					switch (i)
					{
					case 0:
						if ((j == 0 && chunkAtCoords == null) || (j == 15 && chunkAtCoords6 == null))
						{
							break;
						goto default;
					case 15:
						if ((j == 0 && chunkAtCoords3 == null) || (j == 15 && chunkAtCoords8 == null))
							break;
						goto default;
					default:
					{
						int num = i + chunk.Origin.X;
						int num2 = j + chunk.Origin.Y;
						int bottomHeightFast = chunk.GetBottomHeightFast(i, j);
						int bottomHeight = terrain.GetBottomHeight(num - 1, num2);
						int bottomHeight2 = terrain.GetBottomHeight(num + 1, num2);
						int bottomHeight3 = terrain.GetBottomHeight(num, num2 - 1);
						int bottomHeight4 = terrain.GetBottomHeight(num, num2 + 1);
						int x3 = MathUtils.Min(bottomHeightFast - 1, MathUtils.Min(bottomHeight, bottomHeight2, bottomHeight3, bottomHeight4));
						int topHeightFast = chunk.GetTopHeightFast(i, j);
						topHeightFast = MathUtils.Min(topHeightFast, 126);
						int num4 = TerrainChunk.CalculateCellIndex(i, 0, j);
						for (int k = MathUtils.Max(x3, 1); k <= topHeightFast; k++)
						{
							int cellValueFast = chunk.GetCellValueFast(num4 + k);
							int num5 = Terrain.ExtractContents(cellValueFast);
							if (num5 != 0)
							{
								var block = BlocksManager.Blocks[num5] as CustomTextureBlock;
								if (block != null)
								{
									block.GenerateTerrainVertices(m_subsystemTerrain.BlockGeometryGenerator, m_geometry, cellValueFast, num, k, num2);
									chunk.GeometryMinY = MathUtils.Min(chunk.GeometryMinY, (float)k);
									chunk.GeometryMaxY = MathUtils.Max(chunk.GeometryMaxY, (float)k);
								}
							}
						}
						break;
					}
					}
				}
			}
			ushort[] array = movingBlockSet.Indices.Array;
			int count3 = movingBlockSet.Indices.Count;
			Vector3 vector = movingBlockSet.Position + movingBlockSet.GeometryOffset;
			TerrainVertex[] array2 = movingBlockSet.Vertices.Array;
			int count2 = movingBlockSet.Vertices.Count;
			for (int i = 0; i < count2; i++)
			{
				TerrainVertex item = array2[i];
				item.X += vector.X;
				item.Y += vector.Y;
				item.Z += vector.Z;
				m_vertices.Add(item);
			}
			for (int j = 0; j < movingBlockSet.Indices.Count; j++)
				m_indices.Add((ushort)(array[j] + count));
	ushort num = 0;
	int num2 = 0;
	int num3 = 0;
	TerrainGeometrySubset[] allSubsets = chunk.Geometry.AllSubsets;
	foreach (TerrainGeometrySubset terrainGeometrySubset in allSubsets)
	{
		if (num > 0)
		{
			int count = terrainGeometrySubset.Indices.Count;
			ushort[] array = terrainGeometrySubset.Indices.Array;
			for (int j = 0; j < count; j++)
				array[j] += num;
		}
		num = (ushort)(num + (ushort)terrainGeometrySubset.Vertices.Count);
		num2 += terrainGeometrySubset.Vertices.Count;
		num3 += terrainGeometrySubset.Indices.Count;
	}
	if (num2 > 0 && num3 > 0)
	{
		
	}
		}
		public void DrawOpaque(Camera camera)
		{
			m_geometry = new TerrainChunkGeometry();
			Display.BlendState = BlendState.Opaque;
			Display.DepthStencilState = DepthStencilState.Default;
			Display.RasterizerState = RasterizerState.CullCounterClockwiseScissor;
			OpaqueShader.GetParameter("u_worldViewProjectionMatrix", false).SetValue(camera.ViewProjectionMatrix);
			OpaqueShader.GetParameter("u_viewPosition", false).SetValue(camera.ViewPosition);
			OpaqueShader.GetParameter("u_texture", false).SetValue(block.m_texture);
			OpaqueShader.GetParameter("u_samplerState", false).SetValue(SamplerState.PointClamp);
			OpaqueShader.GetParameter("u_fogColor", false).SetValue(new Vector3(ViewFogColor));
			ShaderParameter parameter = OpaqueShader.GetParameter("u_fogStartInvLength", false);
			Vector3 viewPosition = camera.ViewPosition;
			int viewIndex = camera.View.ViewIndex;
			var chunksToDraw = m_subsystemTerrain.TerrainRenderer.m_chunksToDraw;
			for (int i = 0; i < chunksToDraw.Count; i++)
			{
				TerrainChunk chunk = chunksToDraw[i];
				float num = MathUtils.Min(chunk.FogEnds[viewIndex], ViewFogRange.Y);
				float num2 = MathUtils.Min(ViewFogRange.X, num - 1f);
				parameter.SetValue(new Vector2(num2, 1f / (num - num2)));
				int num3 = 0;
				if (viewPosition.Z > chunk.BoundingBox.Min.Z)
					num3 |= chunk.Geometry.OpaqueSubsetsByFace[0].SubsetMask;
				if (viewPosition.X > chunk.BoundingBox.Min.X)
					num3 |= chunk.Geometry.OpaqueSubsetsByFace[1].SubsetMask;
				if (viewPosition.Z < chunk.BoundingBox.Max.Z)
					num3 |= chunk.Geometry.OpaqueSubsetsByFace[2].SubsetMask;
				if (viewPosition.X < chunk.BoundingBox.Max.X)
					num3 |= chunk.Geometry.OpaqueSubsetsByFace[3].SubsetMask;
				num3 |= chunk.Geometry.SubsetOpaque.SubsetMask;
				m_subsystemTerrain.TerrainRenderer.DrawTerrainChunkGeometrySubsets(OpaqueShader, m_geometry, num3);
			}
		}
		/*public void DrawAlphaTested(Camera camera)
		{
			
		}
		public void DrawTransparent(Camera camera)
		{
			
		}*/
		public override void Load(ValuesDictionary valuesDictionary)
		{
			OpaqueShader = ContentManager.Get<Shader>("Shaders/Opaque");
			//AlphaTestedShader = ContentManager.Get<Shader>("Shaders/AlphaTested");
			//TransparentShader = ContentManager.Get<Shader>("Shaders/Transparent");
			base.Load(valuesDictionary);
		}
	}
}
