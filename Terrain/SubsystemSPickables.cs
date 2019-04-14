using Engine;
using System;
using TemplatesDatabase;

namespace Game
{
	public class SubsystemSPickables : SubsystemPickables, IUpdateable
	{

		public new void Update(float dt)
		{
			double totalElapsedGameTime = m_subsystemGameInfo.TotalElapsedGameTime;
			float num2 = MathUtils.Pow(0.001f, dt);
			m_tmpPlayers.Clear();
			foreach (ComponentPlayer componentPlayer in m_subsystemPlayers.ComponentPlayers)
			{
				if (componentPlayer.ComponentHealth.Health > 0f)
				{
					m_tmpPlayers.Add(componentPlayer);
				}
			}
			foreach (Pickable pickable in m_pickables)
			{
				if (pickable.ToRemove)
				{
					m_pickablesToRemove.Add(pickable);
				}
				else
				{
					Block block = BlocksManager.Blocks[Terrain.ExtractContents(pickable.Value)];
					int num3 = m_pickables.Count - m_pickablesToRemove.Count;
					float num4 = MathUtils.Lerp(240f, 90f, MathUtils.Saturate((float)num3 / 40f));
					double num5 = totalElapsedGameTime - pickable.CreationTime;
					if (num5 > (double)num4)
					{
						pickable.ToRemove = true;
					}
					else
					{
						TerrainChunk chunkAtCell = m_subsystemTerrain.Terrain.GetChunkAtCell(Terrain.ToCell(pickable.Position.X), Terrain.ToCell(pickable.Position.Z));
						if (chunkAtCell != null && chunkAtCell.State > TerrainChunkState.InvalidContents4)
						{
							Vector3 position = pickable.Position;
							Vector3 vector = position + pickable.Velocity * dt;
							if (!pickable.FlyToPosition.HasValue && num5 > 0.5)
							{
								foreach (ComponentPlayer tmpPlayer in m_tmpPlayers)
								{
									ComponentBody componentBody = tmpPlayer.ComponentBody;
									Vector3 v = componentBody.Position + new Vector3(0f, 0.75f, 0f);
									float num6 = (v - pickable.Position).LengthSquared();
									if (num6 < 3.0625f)
									{
										bool flag = Terrain.ExtractContents(pickable.Value) == 248;
										IInventory inventory = tmpPlayer.ComponentMiner.Inventory;
										if (flag || ComponentInventoryBase.FindAcquireSlotForItem(inventory, pickable.Value) >= 0)
										{
											if (num6 < 1f)
											{
												if (flag)
												{
													tmpPlayer.ComponentLevel.AddExperience(pickable.Count, true);
													pickable.ToRemove = true;
												}
												else
												{
													pickable.Count = ComponentInventoryBase.AcquireItems(inventory, pickable.Value, pickable.Count);
													if (pickable.Count == 0)
													{
														pickable.ToRemove = true;
														m_subsystemAudio.PlaySound("Audio/PickableCollected", 0.7f, -0.4f, pickable.Position, 2f, false);
													}
												}
											}
											else if (!pickable.StuckMatrix.HasValue)
											{
												pickable.FlyToPosition = v + 0.1f * MathUtils.Sqrt(num6) * componentBody.Velocity;
											}
										}
									}
								}
							}
							if (pickable.FlyToPosition.HasValue)
							{
								Vector3 v2 = pickable.FlyToPosition.Value - pickable.Position;
								float num7 = v2.LengthSquared();
								if (num7 >= 0.25f)
								{
									pickable.Velocity = 6f * v2 / MathUtils.Sqrt(num7);
								}
								else
								{
									pickable.FlyToPosition = null;
								}
							}
							else
							{
								FluidBlock surfaceBlock;
								float? surfaceHeight;
								Vector2? vector2 = m_subsystemFluidBlockBehavior.CalculateFlowSpeed(Terrain.ToCell(pickable.Position.X), Terrain.ToCell(pickable.Position.Y + 0.1f), Terrain.ToCell(pickable.Position.Z), out surfaceBlock, out surfaceHeight);
								if (!pickable.StuckMatrix.HasValue)
								{
									TerrainRaycastResult? terrainRaycastResult = m_subsystemTerrain.Raycast(position, vector, false, true, c._.Update_b__32_0);
									if (terrainRaycastResult.HasValue)
									{
										int contents = Terrain.ExtractContents(m_subsystemTerrain.Terrain.GetCellValue(terrainRaycastResult.Value.CellFace.X, terrainRaycastResult.Value.CellFace.Y, terrainRaycastResult.Value.CellFace.Z));
										SubsystemBlockBehavior[] blockBehaviors = m_subsystemBlockBehaviors.GetBlockBehaviors(contents);
										for (int i = 0; i < blockBehaviors.Length; i++)
										{
											blockBehaviors[i].OnHitByProjectile(terrainRaycastResult.Value.CellFace, pickable);
										}
										if (m_subsystemTerrain.Raycast(position, position, false, true, c._.Update_b__32_1).HasValue)
										{
											int num8 = Terrain.ToCell(position.X);
											int num9 = Terrain.ToCell(position.Y);
											int num10 = Terrain.ToCell(position.Z);
											int num11 = 0;
											int num12 = 0;
											int num13 = 0;
											int? num14 = null;
											for (int j = -3; j <= 3; j++)
											{
												for (int k = -3; k <= 3; k++)
												{
													for (int l = -3; l <= 3; l++)
													{
														if (!BlocksManager.Blocks[m_subsystemTerrain.Terrain.GetCellContents(j + num8, k + num9, l + num10)].IsCollidable)
														{
															int num15 = j * j + k * k + l * l;
															if (!num14.HasValue || num15 < num14.Value)
															{
																num11 = j + num8;
																num12 = k + num9;
																num13 = l + num10;
																num14 = num15;
															}
														}
													}
												}
											}
											if (num14.HasValue)
											{
												pickable.FlyToPosition = new Vector3(num11, num12, num13) + new Vector3(0.5f);
											}
											else
											{
												pickable.ToRemove = true;
											}
										}
										else
										{
											Plane plane = terrainRaycastResult.Value.CellFace.CalculatePlane();
											bool flag2 = vector2.HasValue && vector2.Value != Vector2.Zero;
											if (plane.Normal.X != 0f)
											{
												float num16 = (flag2 || MathUtils.Sqrt(MathUtils.Sqr(pickable.Velocity.Y) + MathUtils.Sqr(pickable.Velocity.Z)) > 10f) ? 0.95f : 0.25f;
												pickable.Velocity *= new Vector3(0f - num16, num16, num16);
											}
											if (plane.Normal.Y != 0f)
											{
												float num17 = (flag2 || MathUtils.Sqrt(MathUtils.Sqr(pickable.Velocity.X) + MathUtils.Sqr(pickable.Velocity.Z)) > 10f) ? 0.95f : 0.25f;
												pickable.Velocity *= new Vector3(num17, 0f - num17, num17);
												if (flag2)
												{
													pickable.Velocity.Y += 0.1f * plane.Normal.Y;
												}
											}
											if (plane.Normal.Z != 0f)
											{
												float num18 = (flag2 || MathUtils.Sqrt(MathUtils.Sqr(pickable.Velocity.X) + MathUtils.Sqr(pickable.Velocity.Y)) > 10f) ? 0.95f : 0.25f;
												pickable.Velocity *= new Vector3(num18, num18, 0f - num18);
											}
											vector = position;
										}
									}
								}
								else
								{
									Vector3 vector3 = pickable.StuckMatrix.Value.Translation + pickable.StuckMatrix.Value.Up * block.ProjectileTipOffset;
									if (!m_subsystemTerrain.Raycast(vector3, vector3, false, true, c._.Update_b__32_2).HasValue)
									{
										pickable.Position = pickable.StuckMatrix.Value.Translation;
										pickable.Velocity = Vector3.Zero;
										pickable.StuckMatrix = null;
									}
								}
								if (surfaceBlock is WaterBlock && !pickable.SplashGenerated)
								{
									m_subsystemParticles.AddParticleSystem(new WaterSplashParticleSystem(m_subsystemTerrain, pickable.Position, false));
									m_subsystemAudio.PlayRandomSound("Audio/Splashes", 1f, m_random.UniformFloat(-0.2f, 0.2f), pickable.Position, 6f, true);
									pickable.SplashGenerated = true;
								}
								else if (surfaceBlock is MagmaBlock && !pickable.SplashGenerated)
								{
									m_subsystemParticles.AddParticleSystem(new MagmaSplashParticleSystem(m_subsystemTerrain, pickable.Position, false));
									m_subsystemAudio.PlayRandomSound("Audio/Sizzles", 1f, m_random.UniformFloat(-0.2f, 0.2f), pickable.Position, 3f, true);
									pickable.ToRemove = true;
									pickable.SplashGenerated = true;
									m_subsystemExplosions.TryExplodeBlock(Terrain.ToCell(pickable.Position.X), Terrain.ToCell(pickable.Position.Y), Terrain.ToCell(pickable.Position.Z), pickable.Value);
								}
								else if (surfaceBlock == null)
								{
									pickable.SplashGenerated = false;
								}
								if (m_subsystemTime.PeriodicGameTimeEvent(1.0, (double)(pickable.GetHashCode() % 100) / 100.0) && (m_subsystemTerrain.Terrain.GetCellContents(Terrain.ToCell(pickable.Position.X), Terrain.ToCell(pickable.Position.Y + 0.1f), Terrain.ToCell(pickable.Position.Z)) == 104 || m_subsystemFireBlockBehavior.IsCellOnFire(Terrain.ToCell(pickable.Position.X), Terrain.ToCell(pickable.Position.Y + 0.1f), Terrain.ToCell(pickable.Position.Z))))
								{
									m_subsystemAudio.PlayRandomSound("Audio/Sizzles", 1f, m_random.UniformFloat(-0.2f, 0.2f), pickable.Position, 3f, true);
									pickable.ToRemove = true;
									m_subsystemExplosions.TryExplodeBlock(Terrain.ToCell(pickable.Position.X), Terrain.ToCell(pickable.Position.Y), Terrain.ToCell(pickable.Position.Z), pickable.Value);
								}
								if (!pickable.StuckMatrix.HasValue)
								{
									if (vector2.HasValue && surfaceHeight.HasValue)
									{
										float num19 = surfaceHeight.Value - pickable.Position.Y;
										float num20 = MathUtils.Saturate(3f * num19);
										pickable.Velocity.X += 4f * dt * (vector2.Value.X - pickable.Velocity.X);
										pickable.Velocity.Y -= 10f * dt;
										pickable.Velocity.Y += 10f * (1f / block.Density * num20) * dt;
										pickable.Velocity.Z += 4f * dt * (vector2.Value.Y - pickable.Velocity.Z);
										pickable.Velocity.Y *= num2;
									}
									else
									{
										pickable.Velocity.Y -= 10f * dt;
										pickable.Velocity *= MathUtils.Pow(0.5f, dt);
										pickable.Velocity.X += (float)Math.Sin(totalElapsedGameTime) *.5f;
										pickable.Velocity.Z -= (float)Math.Cos(totalElapsedGameTime) *.5f;
									}
								}
							}
							pickable.Position = vector;
						}
					}
				}
			}
			foreach (Pickable item in m_pickablesToRemove)
			{
				m_pickables.Remove(item);
				pickableRemoved?.Invoke(item);
			}
			m_pickablesToRemove.Clear();
		}
	}
}