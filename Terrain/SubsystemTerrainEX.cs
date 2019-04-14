using Engine;
using Engine.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TemplatesDatabase;

namespace Game
{
	public class SubsystemTerrainEX : SubsystemTerrain, IDrawable, IUpdateable
	{
		public new TerrainUpdaterEX TerrainUpdater;

		public new TerrainRendererEX TerrainRenderer;

		public new TerrainSerializerLZ4 TerrainSerializer;

		public override void Dispose()
		{
			TerrainRenderer.Dispose();
			TerrainUpdater.Dispose();
			TerrainSerializer.Dispose();
		}

		public new void Draw(Camera camera, int drawOrder)
		{
			if (TerrainRenderingEnabled)
			{
				if (drawOrder == m_drawOrders[0])
				{
					TerrainUpdater.PrepareForDrawing(camera);
					TerrainRenderer.PrepareForDrawing(camera);
					TerrainRenderer.DrawOpaque(camera);
					TerrainRenderer.DrawAlphaTested(camera);
				}
				else if (drawOrder == m_drawOrders[1])
				{
					TerrainRenderer.DrawTransparent(camera);
				}
			}
		}

		public override void Load(ValuesDictionary valuesDictionary)
		{
			m_subsystemViews = Project.FindSubsystem<SubsystemViews>(true);
			m_subsystemGameInfo = Project.FindSubsystem<SubsystemGameInfo>(true);
			m_subsystemParticles = Project.FindSubsystem<SubsystemParticles>(true);
			m_subsystemPickables = Project.FindSubsystem<SubsystemPickables>(true);
			m_subsystemBlockBehaviors = Project.FindSubsystem<SubsystemBlockBehaviors>(true);
			SubsystemAnimatedTextures = Project.FindSubsystem<SubsystemAnimatedTextures>(true);
			SubsystemFurnitureBlockBehavior = Project.FindSubsystem<SubsystemFurnitureBlockBehavior>(true);
			SubsystemPalette = Project.FindSubsystem<SubsystemPalette>(true);
			Terrain = new Terrain();
			TerrainRenderer = new TerrainRendererEX(this);
			TerrainUpdater = new TerrainUpdaterEX(this);
			TerrainSerializer = new TerrainSerializerLZ4(Terrain, m_subsystemGameInfo.DirectoryName);
			BlockGeometryGenerator = new BlockGeometryGenerator(Terrain, this, Project.FindSubsystem<SubsystemElectricity>(true), SubsystemFurnitureBlockBehavior, Project.FindSubsystem<SubsystemMetersBlockBehavior>(true), SubsystemPalette);
			TerrainContentsGenerator = m_subsystemGameInfo.WorldSettings.TerrainGenerationMode == TerrainGenerationMode.Flat
				? new TerrainContentsGeneratorFlat(this)
				: (ITerrainContentsGenerator)new TerrainContentsGenerator(this);
		}

		public override void Save(ValuesDictionary valuesDictionary)
		{
			TerrainUpdater.UpdateEvent.WaitOne();
			try
			{
				TerrainChunk[] allocatedChunks = Terrain.AllocatedChunks;
				foreach (TerrainChunk chunk in allocatedChunks)
				{
					TerrainSerializer.SaveChunk(chunk);
				}
			}
			finally
			{
				TerrainUpdater.UpdateEvent.Set();
			}
		}

		public new void Update(float dt)
		{
			TerrainUpdater.Update();
			ProcessModifiedCells();
		}
	}
	public class IndexBufferCacheEX : IndexBufferCache
	{
		public IndexBufferCacheEX()
		{
		}

		public IndexBufferCacheEX(IndexBufferCache cache)
		{
			m_freeIndexBuffers = cache.m_freeIndexBuffers;
			m_usedIndexBuffers = cache.m_usedIndexBuffers;
			UsedIndexBuffersBytes_ = cache.UsedIndexBuffersBytes_;
			FreeIndexBuffersBytes_ = cache.FreeIndexBuffersBytes_;
			IndexBufferAllocations_ = cache.IndexBufferAllocations_;
		}

		public new IndexBuffer GetIndexBuffer(int indicesCount)
		{
			IndexBuffer indexBuffer = null;
			foreach (IndexBuffer freeIndexBuffer in m_freeIndexBuffers)
			{
				if (freeIndexBuffer.IndicesCount >= indicesCount && freeIndexBuffer.IndicesCount / (float)indicesCount < 1.1f && (indexBuffer == null || freeIndexBuffer.IndicesCount < indexBuffer.IndicesCount))
					indexBuffer = freeIndexBuffer;
			}
			if (indexBuffer == null)
			{
				indexBuffer = new IndexBuffer(IndexFormat.ThirtyTwoBits, indicesCount);
				long num = ++IndexBufferAllocations;
			}
			else
			{
				m_freeIndexBuffers.Remove(indexBuffer);
				FreeIndexBuffersBytes -= CalculateBufferSize(indexBuffer);
			}
			m_usedIndexBuffers.Add(indexBuffer);
			UsedIndexBuffersBytes += CalculateBufferSize(indexBuffer);
			return indexBuffer;
		}
	}
	public class TerrainRendererEX : TerrainRenderer
	{
		public IndexBufferCacheEX IndexBufferCache;

		public TerrainRendererEX(SubsystemTerrain subsystemTerrain) : base(subsystemTerrain)
		{
			m_indexBufferCache = IndexBufferCache = new IndexBufferCacheEX(m_indexBufferCache);
		}

		public new void PrepareForDrawing(Camera camera)
		{
			Vector3 viewPosition = camera.ViewPosition;
			float num = MathUtils.Sqr(m_subsystemSky.VisibilityRange);
			BoundingFrustum viewFrustum = camera.ViewFrustum;
			int viewIndex = camera.View.ViewIndex;
			m_chunksToDraw.Clear();
			TerrainChunk[] allocatedChunks = m_subsystemTerrain.Terrain.AllocatedChunks;
			foreach (TerrainChunk terrainChunk in allocatedChunks)
			{
				if (terrainChunk.NewGeometryData)
				{
					lock (terrainChunk.Geometry)
					{
						if (terrainChunk.NewGeometryData)
						{
							terrainChunk.NewGeometryData = false;
							terrainChunk.Geometry.GeometryBoundingBox = terrainChunk.BoundingBox;
							terrainChunk.Geometry.GeometryBoundingBox.Min.Y = terrainChunk.GeometryMinY;
							terrainChunk.Geometry.GeometryBoundingBox.Max.Y = terrainChunk.GeometryMaxY + 1f;
							SetupTerrainChunkGeometryVertexIndexBuffers(terrainChunk);
						}
					}
				}
				terrainChunk.DrawDistanceSquared = Vector2.DistanceSquared(viewPosition.XZ, terrainChunk.Center);
				if (terrainChunk.DrawDistanceSquared <= num)
				{
					if (viewFrustum.Intersection(terrainChunk.Geometry.GeometryBoundingBox))
						m_chunksToDraw.Add(terrainChunk);
					if (terrainChunk.State != TerrainChunkState.Valid)
						continue;
					float num2 = terrainChunk.FogEnds[viewIndex];
					if (num2 != float.MaxValue)
					{
						if (num2 == 0f)
							StartChunkFadeIn(camera, terrainChunk);
						else
							RunChunkFadeIn(camera, terrainChunk);
					}
				}
				else
					terrainChunk.FogEnds[viewIndex] = 0f;
			}
			m_chunksToDraw.Sort(m_chunksDistanceComparer);
			ChunksDrawn = 0;
			ChunkDrawCalls = 0;
			ChunkTrianglesDrawn = 0;
		}

		public new void SetupTerrainChunkGeometryVertexIndexBuffers(TerrainChunk chunk)
		{
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
					{
						array[j] += num;
					}
				}
				num = (ushort)(num + (ushort)terrainGeometrySubset.Vertices.Count);
				num2 += terrainGeometrySubset.Vertices.Count;
				num3 += terrainGeometrySubset.Indices.Count;
			}
			if (num2 > 0 && num3 > 0)
			{
				ReturnTerrainChunkGeometryVertexIndexBuffers(chunk.Geometry);
				VertexBuffer vertexBuffer = m_vertexBufferCache.GetVertexBuffer(num2);
				IndexBuffer indexBuffer = IndexBufferCache.GetIndexBuffer(num3);
				chunk.Geometry.VertexBuffer = vertexBuffer;
				chunk.Geometry.IndexBuffer = indexBuffer;
				int num4 = 0;
				int num5 = 0;
				allSubsets = chunk.Geometry.AllSubsets;
				foreach (TerrainGeometrySubset terrainGeometrySubset2 in allSubsets)
				{
					terrainGeometrySubset2.VertexOffset = num4;
					terrainGeometrySubset2.VerticesCount = terrainGeometrySubset2.Vertices.Count;
					terrainGeometrySubset2.IndexOffset = num5;
					terrainGeometrySubset2.IndicesCount = terrainGeometrySubset2.Indices.Count;
					if (terrainGeometrySubset2.VerticesCount > 0 && terrainGeometrySubset2.IndicesCount > 0)
					{
						chunk.Geometry.VertexBuffer.SetData(terrainGeometrySubset2.Vertices.Array, 0, terrainGeometrySubset2.VerticesCount, num4);
						chunk.Geometry.IndexBuffer.SetData(terrainGeometrySubset2.Indices.Array, 0, terrainGeometrySubset2.IndicesCount, num5);
						num4 += terrainGeometrySubset2.VerticesCount;
						num5 += terrainGeometrySubset2.IndicesCount;
					}
				}
				allSubsets = chunk.Geometry.AllSubsets;
				foreach (TerrainGeometrySubset obj in allSubsets)
				{
					obj.Vertices.Clear();
					obj.Indices.Clear();
					obj.Vertices.Capacity = 0;
					obj.Indices.Capacity = 0;
				}
			}
			chunk.Geometry.GeometryHash = chunk.ContentsHash;
		}
	}
	public class TerrainUpdaterEX : TerrainUpdater
	{
		public SubsystemTerrainEX SubsystemTerrainEX;

		public TerrainUpdaterEX(SubsystemTerrainEX subsystemTerrain) : base(subsystemTerrain)
		{
			SubsystemTerrainEX = subsystemTerrain;
		}

		public new void Update()
		{
			if (m_subsystemSky.SkyLightValue != m_lastSkylightValue)
			{
				m_lastSkylightValue = m_subsystemSky.SkyLightValue;
				DowngradeAllChunksState(TerrainChunkState.InvalidLight, forceGeometryRegeneration: false);
			}
			if (!SettingsManager.MultithreadedTerrainUpdate)
			{
				if (m_task != null)
				{
					m_quitUpdateThread = true;
					UnpauseUpdateThread();
					m_updateEvent.Set();
					m_task.Wait();
					m_task = null;
				}
				double realTime = Time.RealTime;
				while (!SynchronousUpdateFunction() && Time.RealTime - realTime < 0.0099999997764825821)
				{
				}
			}
			else if (m_task == null)
			{
				m_quitUpdateThread = false;
				m_task = Task.Run((Action)ThreadUpdateFunction);
				UnpauseUpdateThread();
				m_updateEvent.Set();
			}
			if (m_pendingLocations.Count > 0)
			{
				m_pauseEvent.Reset();
				if (m_updateEvent.WaitOne(0))
				{
					m_pauseEvent.Set();
					try
					{
						foreach (KeyValuePair<int, UpdateLocation?> pendingLocation in m_pendingLocations)
						{
							if (pendingLocation.Value.HasValue)
								m_updateParameters.Locations[pendingLocation.Key] = pendingLocation.Value.Value;
							else
								m_updateParameters.Locations.Remove(pendingLocation.Key);
						}
						if (AllocateAndFreeChunks(m_updateParameters.Locations.Values.ToArray()))
							m_updateParameters.Chunks = m_terrain.AllocatedChunks;
						m_pendingLocations.Clear();
					}
					finally
					{
						m_updateEvent.Set();
					}
				}
			}
			else
			{
				lock (m_updateParametersLock)
				{
					if (SendReceiveChunkStates())
						UnpauseUpdateThread();
				}
			}
			TerrainChunk[] allocatedChunks = m_terrain.AllocatedChunks;
			foreach (TerrainChunk terrainChunk in allocatedChunks)
			{
				if (terrainChunk.State >= TerrainChunkState.InvalidVertices1 && !terrainChunk.AreBehaviorsNotified)
				{
					terrainChunk.AreBehaviorsNotified = true;
					NotifyBlockBehaviors(terrainChunk);
				}
			}
		}

		public new bool AllocateAndFreeChunks(UpdateLocation[] locations)
		{
			bool result = false;
			TerrainChunk[] allocatedChunks = m_terrain.AllocatedChunks;
			foreach (TerrainChunk terrainChunk in allocatedChunks)
			{
				if (!IsChunkInRange(terrainChunk.Center, locations))
				{
					result = true;
					foreach (SubsystemBlockBehavior blockBehavior in m_subsystemBlockBehaviors.BlockBehaviors)
					{
						blockBehavior.OnChunkDiscarding(terrainChunk);
					}
					m_subsystemTerrain.TerrainSerializer.SaveChunk(terrainChunk);
					m_terrain.FreeChunk(terrainChunk);
					m_subsystemTerrain.TerrainRenderer.ReturnTerrainChunkGeometryVertexIndexBuffers(terrainChunk.Geometry);
				}
			}
			for (int j = 0; j < locations.Length; j++)
			{
				Point2 point = Terrain.ToChunk(locations[j].Center - new Vector2(locations[j].ContentDistance));
				Point2 point2 = Terrain.ToChunk(locations[j].Center + new Vector2(locations[j].ContentDistance));
				for (int k = point.X; k <= point2.X; k++)
				{
					for (int l = point.Y; l <= point2.Y; l++)
					{
						var chunkCenter = new Vector2((k + 0.5f) * 16f, (l + 0.5f) * 16f);
						TerrainChunk chunkAtCoords = m_terrain.GetChunkAtCoords(k, l);
						if (chunkAtCoords == null)
						{
							if (IsChunkInRange(chunkCenter, locations))
							{
								result = true;
								m_terrain.AllocateChunk(k, l);
								DowngradeChunkNeighborhoodState(new Point2(k, l), 0, TerrainChunkState.NotLoaded, forceGeometryRegeneration: false);
								DowngradeChunkNeighborhoodState(new Point2(k, l), 1, TerrainChunkState.InvalidLight, forceGeometryRegeneration: false);
							}
						}
						else if (chunkAtCoords.Coords.X != k || chunkAtCoords.Coords.Y != l)
						{
							Log.Error("Chunk wraparound detected at {0}", chunkAtCoords.Coords);
						}
					}
				}
			}
			return result;
		}

		public new void UpdateChunkSingleStep(TerrainChunk chunk, int skylightValue)
		{
			switch (chunk.ThreadState)
			{
			case TerrainChunkState.NotLoaded:
				{
					double realTime19 = Time.RealTime;
					if (m_subsystemTerrain.TerrainSerializer.LoadChunk(chunk))
					{
						chunk.ThreadState = TerrainChunkState.InvalidLight;
						chunk.WasUpgraded = true;
						double realTime20 = Time.RealTime;
						chunk.IsLoaded = true;
						m_statistics.LoadingCount++;
						m_statistics.LoadingTime += realTime20 - realTime19;
					}
					else
					{
						chunk.ThreadState = TerrainChunkState.InvalidContents1;
						chunk.WasUpgraded = true;
					}
					break;
				}
			case TerrainChunkState.InvalidContents1:
				{
					double realTime17 = Time.RealTime;
					m_subsystemTerrain.TerrainContentsGenerator.GenerateChunkContentsPass1(chunk);
					chunk.ThreadState = TerrainChunkState.InvalidContents2;
					chunk.WasUpgraded = true;
					double realTime18 = Time.RealTime;
					m_statistics.ContentsCount1++;
					m_statistics.ContentsTime1 += realTime18 - realTime17;
					break;
				}
			case TerrainChunkState.InvalidContents2:
				{
					double realTime15 = Time.RealTime;
					m_subsystemTerrain.TerrainContentsGenerator.GenerateChunkContentsPass2(chunk);
					chunk.ThreadState = TerrainChunkState.InvalidContents3;
					chunk.WasUpgraded = true;
					double realTime16 = Time.RealTime;
					m_statistics.ContentsCount2++;
					m_statistics.ContentsTime2 += realTime16 - realTime15;
					break;
				}
			case TerrainChunkState.InvalidContents3:
				{
					double realTime13 = Time.RealTime;
					m_subsystemTerrain.TerrainContentsGenerator.GenerateChunkContentsPass3(chunk);
					chunk.ThreadState = TerrainChunkState.InvalidContents4;
					chunk.WasUpgraded = true;
					double realTime14 = Time.RealTime;
					m_statistics.ContentsCount3++;
					m_statistics.ContentsTime3 += realTime14 - realTime13;
					break;
				}
			case TerrainChunkState.InvalidContents4:
				{
					double realTime7 = Time.RealTime;
					m_subsystemTerrain.TerrainContentsGenerator.GenerateChunkContentsPass4(chunk);
					chunk.ThreadState = TerrainChunkState.InvalidLight;
					chunk.WasUpgraded = true;
					double realTime8 = Time.RealTime;
					m_statistics.ContentsCount4++;
					m_statistics.ContentsTime4 += realTime8 - realTime7;
					break;
				}
			case TerrainChunkState.InvalidLight:
				{
					double realTime3 = Time.RealTime;
					GenerateChunkLightAndHeight(chunk, skylightValue);
					chunk.ThreadState = TerrainChunkState.InvalidPropagatedLight;
					chunk.WasUpgraded = true;
					chunk.LightPropagationMask = 0;
					double realTime4 = Time.RealTime;
					m_statistics.LightCount++;
					m_statistics.LightTime += realTime4 - realTime3;
					break;
				}
			case TerrainChunkState.InvalidPropagatedLight:
				{
					for (int j = -2; j <= 2; j++)
					{
						for (int k = -2; k <= 2; k++)
						{
							TerrainChunk chunkAtCell = m_terrain.GetChunkAtCell(chunk.Origin.X + j * 16, chunk.Origin.Y + k * 16);
							if (chunkAtCell != null && chunkAtCell.ThreadState < TerrainChunkState.InvalidPropagatedLight)
							{
								UpdateChunkSingleStep(chunkAtCell, skylightValue);
								return;
							}
						}
					}
					double realTime9 = Time.RealTime;
					m_lightSources.Clear();
					for (int l = -1; l <= 1; l++)
					{
						for (int m = -1; m <= 1; m++)
						{
							int num = CalculateLightPropagationBitIndex(l, m);
							if (((chunk.LightPropagationMask >> num) & 1) == 0)
							{
								TerrainChunk chunkAtCell2 = m_terrain.GetChunkAtCell(chunk.Origin.X + l * 16, chunk.Origin.Y + m * 16);
								if (chunkAtCell2 != null)
								{
									GenerateChunkLightSources(chunkAtCell2);
									UpdateNeighborsLightPropagationBitmasks(chunkAtCell2);
								}
							}
						}
					}
					double realTime10 = Time.RealTime;
					m_statistics.LightSourcesCount++;
					m_statistics.LightSourcesTime += realTime10 - realTime9;
					double realTime11 = Time.RealTime;
					PropagateLight();
					chunk.ThreadState = TerrainChunkState.InvalidVertices1;
					chunk.WasUpgraded = true;
					double realTime12 = Time.RealTime;
					m_statistics.LightPropagateCount++;
					m_statistics.LightSourceInstancesCount += m_lightSources.Count;
					m_statistics.LightPropagateTime += realTime12 - realTime11;
					break;
				}
			case TerrainChunkState.InvalidVertices1:
				{
					double realTime5 = Time.RealTime;
					lock (chunk.Geometry)
					{
						chunk.ContentsHash = CalculateChunkGeometryHash(chunk);
						if (chunk.Geometry.GeometryHash != 0L && chunk.ContentsHash == chunk.Geometry.GeometryHash)
						{
							chunk.ThreadState = TerrainChunkState.Valid;
							chunk.WasUpgraded = true;
							m_statistics.SkippedVertices++;
							return;
						}
						TerrainGeometrySubset[] allSubsets = chunk.Geometry.AllSubsets;
						foreach (TerrainGeometrySubset obj in allSubsets)
						{
							obj.Vertices.Clear();
							obj.Indices.Clear();
						}
						chunk.GeometryMinY = 128f;
						chunk.GeometryMaxY = 0f;
						GenerateChunkVertices(chunk, 0, 0, 16, 8);
						chunk.NewGeometryData = false;
					}
					double realTime6 = Time.RealTime;
					chunk.ThreadState = TerrainChunkState.InvalidVertices2;
					chunk.WasUpgraded = true;
					m_statistics.VerticesCount1++;
					m_statistics.VerticesTime1 += realTime6 - realTime5;
					break;
				}
			case TerrainChunkState.InvalidVertices2:
				{
					double realTime = Time.RealTime;
					lock (chunk.Geometry)
					{
						GenerateChunkVertices(chunk, 0, 8, 16, 16);
						chunk.NewGeometryData = true;
					}
					chunk.ThreadState = TerrainChunkState.Valid;
					chunk.WasUpgraded = true;
					double realTime2 = Time.RealTime;
					m_statistics.VerticesCount2++;
					m_statistics.VerticesTime2 += realTime2 - realTime;
					break;
				}
			}
		}
	}
}