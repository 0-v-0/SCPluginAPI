using Engine;
using TemplatesDatabase;

namespace Game
{
	public class SubsystemSProjectiles : SubsystemProjectiles, IUpdateable
	{
		private SubsystemPlayers subsystemPlayers;

		public override void Load(ValuesDictionary valuesDictionary)
		{
			subsystemPlayers = Project.FindSubsystem<SubsystemPlayers>(true);
			//ProjectileRemoved += OnProjectileRemoved;
			base.Load(valuesDictionary);
		}


		/*private void OnProjectileRemoved(Projectile obj)
		{
			var p = Terrain.ToCell(obj.Position);
			m_subsystemTerrain.DestroyCell(0, p.X, p.Y, p.Z, 0, false, true);
		}*/

		public new void Update(float dt)
		{
			double totalElapsedGameTime = m_subsystemGameInfo.TotalElapsedGameTime;
			foreach (Projectile projectile in m_projectiles)
			{
				if (projectile.ToRemove)
				{
					m_projectilesToRemove.Add(projectile);
				}
				else
				{
					Block block = BlocksManager.Blocks[Terrain.ExtractContents(projectile.Value)];
					if (totalElapsedGameTime - projectile.CreationTime > 20.0)
					{
						projectile.ToRemove = true;
					}
					TerrainChunk chunkAtCell = m_subsystemTerrain.Terrain.GetChunkAtCell(Terrain.ToCell(projectile.Position.X), Terrain.ToCell(projectile.Position.Z));
					if (chunkAtCell == null || chunkAtCell.State <= TerrainChunkState.InvalidContents4)
					{
						projectile.NoChunk = true;
						if (projectile.TrailParticleSystem != null)
						{
							projectile.TrailParticleSystem.IsStopped = true;
						}
					}
					else
					{
						projectile.NoChunk = false;
						Vector3 position = projectile.Position;
						Vector3 vector = position + projectile.Velocity * dt;
						Vector3 v = block.ProjectileTipOffset * Vector3.Normalize(projectile.Velocity);
						BodyRaycastResult? bodyRaycastResult = m_subsystemBodies.Raycast(position + v, vector + v, 0.2f, c._.Update_b__38_0);
						TerrainRaycastResult? terrainRaycastResult = m_subsystemTerrain.Raycast(position + v, vector + v, false, true, c._.Update_b__38_1);
						bool flag = block.DisintegratesOnHit;
						if (terrainRaycastResult.HasValue || bodyRaycastResult.HasValue)
						{
							CellFace? cellFace = terrainRaycastResult.HasValue ? new CellFace?(terrainRaycastResult.Value.CellFace) : null;
							ComponentBody componentBody = bodyRaycastResult.HasValue ? bodyRaycastResult.Value.ComponentBody : null;
							SubsystemBlockBehavior[] blockBehaviors = m_subsystemBlockBehaviors.GetBlockBehaviors(Terrain.ExtractContents(projectile.Value));
							for (int i = 0; i < blockBehaviors.Length; i++)
							{
								flag |= blockBehaviors[i].OnHitAsProjectile(cellFace, componentBody, projectile);
							}
							projectile.ToRemove |= flag;
						}
						Vector3? vector2 = null;
						if (bodyRaycastResult.HasValue && (!terrainRaycastResult.HasValue || bodyRaycastResult.Value.Distance < terrainRaycastResult.Value.Distance))
						{
							if (projectile.Velocity.Length() > 10f)
							{
								ComponentMiner.AttackBody(bodyRaycastResult.Value.ComponentBody, projectile.Owner, Vector3.Normalize(projectile.Velocity), block.GetProjectilePower(projectile.Value), false);
								if (projectile.Owner != null && projectile.Owner.PlayerStats != null)
								{
									projectile.Owner.PlayerStats.RangedHits++;
								}
							}
							if (projectile.IsIncendiary)
							{
								bodyRaycastResult.Value.ComponentBody.Entity.FindComponent<ComponentOnFire>()?.SetOnFire(projectile?.Owner, m_random.UniformFloat(6f, 8f));
							}
							vector = position;
							projectile.Velocity *= -0.05f;
							projectile.Velocity += m_random.Vector3(0.33f * projectile.Velocity.Length());
							projectile.AngularVelocity *= -0.05f;
						}
						else if (terrainRaycastResult.HasValue)
						{
							CellFace cellFace2 = terrainRaycastResult.Value.CellFace;
							int cellValue = m_subsystemTerrain.Terrain.GetCellValue(cellFace2.X, cellFace2.Y, cellFace2.Z);
							int num = Terrain.ExtractContents(cellValue);
							Block block2 = BlocksManager.Blocks[num];
							float num2 = projectile.Velocity.Length();
							SubsystemBlockBehavior[] blockBehaviors2 = m_subsystemBlockBehaviors.GetBlockBehaviors(num);
							for (int j = 0; j < blockBehaviors2.Length; j++)
							{
								blockBehaviors2[j].OnHitByProjectile(cellFace2, projectile);
							}
							if (num2 > 10f && m_random.UniformFloat(0f, 1f) > block2.ProjectileResilience)
							{
								m_subsystemTerrain.DestroyCell(0, cellFace2.X, cellFace2.Y, cellFace2.Z, 0, true, false);
								m_subsystemSoundMaterials.PlayImpactSound(cellValue, position, 1f);
							}
							if (projectile.IsIncendiary)
							{
								m_subsystemFireBlockBehavior.SetCellOnFire(terrainRaycastResult.Value.CellFace.X, terrainRaycastResult.Value.CellFace.Y, terrainRaycastResult.Value.CellFace.Z, 1f);
								Vector3 vector3 = projectile.Position - 0.75f * Vector3.Normalize(projectile.Velocity);
								for (int k = 0; k < 8; k++)
								{
									Vector3 v2 = (k == 0) ? Vector3.Normalize(projectile.Velocity) : m_random.Vector3(1.5f);
									TerrainRaycastResult? terrainRaycastResult2 = m_subsystemTerrain.Raycast(vector3, vector3 + v2, false, true, c._.Update_b__38_2);
									if (terrainRaycastResult2.HasValue)
									{
										m_subsystemFireBlockBehavior.SetCellOnFire(terrainRaycastResult2.Value.CellFace.X, terrainRaycastResult2.Value.CellFace.Y, terrainRaycastResult2.Value.CellFace.Z, 1f);
									}
								}
							}
							if (num2 > 5f)
							{
								m_subsystemSoundMaterials.PlayImpactSound(cellValue, position, 1f);
							}
							if (block.IsStickable && num2 > 10f && m_random.Bool(block2.ProjectileStickProbability))
							{
								Vector3 v3 = Vector3.Normalize(projectile.Velocity);
								float s = MathUtils.Lerp(0.1f, 0.2f, MathUtils.Saturate((num2 - 15f) / 20f));
								vector2 = position + terrainRaycastResult.Value.Distance * Vector3.Normalize(projectile.Velocity) + v3 * s;
							}
							else
							{
								Plane plane = cellFace2.CalculatePlane();
								vector = position;
								if (plane.Normal.X != 0f)
								{
									projectile.Velocity *= new Vector3(-0.3f, 0.3f, 0.3f);
								}
								if (plane.Normal.Y != 0f)
								{
									projectile.Velocity *= new Vector3(0.3f, -0.3f, 0.3f);
								}
								if (plane.Normal.Z != 0f)
								{
									projectile.Velocity *= new Vector3(0.3f, 0.3f, -0.3f);
								}
								float num3 = projectile.Velocity.Length();
								projectile.Velocity = num3 * Vector3.Normalize(projectile.Velocity + m_random.UniformVector3(num3 / 6f, num3 / 3f));
								projectile.AngularVelocity *= -0.3f;
							}
							MakeProjectileNoise(projectile);
						}
						if (terrainRaycastResult.HasValue || bodyRaycastResult.HasValue)
						{
							if (flag)
							{
								m_subsystemParticles.AddParticleSystem(block.CreateDebrisParticleSystem(m_subsystemTerrain, projectile.Position, projectile.Value, 1f));
							}
							else if (!projectile.ToRemove && (vector2.HasValue || projectile.Velocity.Length() < 1f))
							{
								if (projectile.ProjectileStoppedAction == ProjectileStoppedAction.TurnIntoPickable)
								{
									int num4 = BlocksManager.DamageItem(projectile.Value, 1);
									if (num4 != 0)
									{
										if (vector2.HasValue)
										{
											CalculateVelocityAlignMatrix(block, vector2.Value, projectile.Velocity, out Matrix matrix);
											m_subsystemPickables.AddPickable(num4, 1, projectile.Position, Vector3.Zero, matrix);
										}
										else
										{
											m_subsystemPickables.AddPickable(num4, 1, position, Vector3.Zero, null);
										}
									}
									projectile.ToRemove = true;
								}
								else if (projectile.ProjectileStoppedAction == ProjectileStoppedAction.Disappear)
								{
									projectile.ToRemove = true;
								}
							}
						}
						float num5 = projectile.IsInWater ? MathUtils.Pow(0.001f, dt) : MathUtils.Pow(block.ProjectileDamping, dt);
						projectile.Velocity.Y += -10f * dt;
						if (projectile.Owner == null)
						{
							var dpos = subsystemPlayers.ComponentPlayers[0].ComponentBody.Position - position;
							if (dpos.LengthSquared() < 5f) projectile.Velocity = Vector3.Zero;
							projectile.Velocity += Vector3.Normalize(dpos) * (-10f / dpos.LengthSquared() + 1);
						}
						projectile.Velocity *= num5;
						projectile.AngularVelocity *= num5;
						projectile.Position = vector;
						projectile.Rotation += projectile.AngularVelocity * dt;
						if (projectile.TrailParticleSystem != null)
						{
							if (!m_subsystemParticles.ContainsParticleSystem((ParticleSystemBase)projectile.TrailParticleSystem))
							{
								m_subsystemParticles.AddParticleSystem((ParticleSystemBase)projectile.TrailParticleSystem);
							}
							Vector3 v4 = (projectile.TrailOffset != Vector3.Zero) ? Vector3.TransformNormal(projectile.TrailOffset, Matrix.CreateFromAxisAngle(Vector3.Normalize(projectile.Rotation), projectile.Rotation.Length())) : Vector3.Zero;
							projectile.TrailParticleSystem.Position = projectile.Position + v4;
							if (projectile.IsInWater)
							{
								projectile.TrailParticleSystem.IsStopped = true;
							}
						}
						bool flag2 = IsWater(projectile.Position);
						if (projectile.IsInWater != flag2)
						{
							if (flag2)
							{
								float num6 = new Vector2(projectile.Velocity.X + projectile.Velocity.Z).Length();
								if (!(num6 > 6f) || !(num6 > 4f * MathUtils.Abs(projectile.Velocity.Y)))
								{
									projectile.Velocity *= 0.2f;
								}
								else
								{
									projectile.Velocity *= 0.5f;
									projectile.Velocity.Y *= -1f;
									flag2 = false;
								}
								float? surfaceHeight = m_subsystemFluidBlockBehavior.GetSurfaceHeight(Terrain.ToCell(projectile.Position.X), Terrain.ToCell(projectile.Position.Y), Terrain.ToCell(projectile.Position.Z));
								if (surfaceHeight.HasValue)
								{
									m_subsystemParticles.AddParticleSystem(new WaterSplashParticleSystem(m_subsystemTerrain, new Vector3(projectile.Position.X, surfaceHeight.Value, projectile.Position.Z), false));
									m_subsystemAudio.PlayRandomSound("Audio/Splashes", 1f, m_random.UniformFloat(-0.2f, 0.2f), projectile.Position, 6f, true);
									MakeProjectileNoise(projectile);
								}
							}
							projectile.IsInWater = flag2;
						}
						if (IsMagma(projectile.Position))
						{
							m_subsystemParticles.AddParticleSystem(new MagmaSplashParticleSystem(m_subsystemTerrain, projectile.Position, false));
							m_subsystemAudio.PlayRandomSound("Audio/Sizzles", 1f, m_random.UniformFloat(-0.2f, 0.2f), projectile.Position, 3f, true);
							projectile.ToRemove = true;
							m_subsystemExplosions.TryExplodeBlock(Terrain.ToCell(projectile.Position.X), Terrain.ToCell(projectile.Position.Y), Terrain.ToCell(projectile.Position.Z), projectile.Value);
						}
						if (m_subsystemTime.PeriodicGameTimeEvent(1.0, (double)(projectile.GetHashCode() % 100) / 100.0) && (m_subsystemFireBlockBehavior.IsCellOnFire(Terrain.ToCell(projectile.Position.X), Terrain.ToCell(projectile.Position.Y + 0.1f), Terrain.ToCell(projectile.Position.Z)) || m_subsystemFireBlockBehavior.IsCellOnFire(Terrain.ToCell(projectile.Position.X), Terrain.ToCell(projectile.Position.Y + 0.1f) - 1, Terrain.ToCell(projectile.Position.Z))))
						{
							m_subsystemAudio.PlayRandomSound("Audio/Sizzles", 1f, m_random.UniformFloat(-0.2f, 0.2f), projectile.Position, 3f, true);
							projectile.ToRemove = true;
							m_subsystemExplosions.TryExplodeBlock(Terrain.ToCell(projectile.Position.X), Terrain.ToCell(projectile.Position.Y), Terrain.ToCell(projectile.Position.Z), projectile.Value);
						}
					}
				}
			}
			foreach (Projectile item in m_projectilesToRemove)
			{
				if (item.TrailParticleSystem != null)
				{
					item.TrailParticleSystem.IsStopped = true;
				}
				m_projectiles.Remove(item);
				projectileRemoved?.Invoke(item);
			}
			m_projectilesToRemove.Clear();
		}
	}
}