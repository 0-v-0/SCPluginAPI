using Engine;
using GameEntitySystem;

namespace Game
{
	[PluginLoader("NoCrashOrDrop", "Entity can't be crashed, level won't decrease after died", 0u)]
	public class ComponentNoCrashBody : ComponentBody, IUpdateable
	{
		public static PlayerData PlayerData;

		public static void Initialize()
		{
			PlayerData.ctor1 += Init;
		}

		public static void Init(PlayerData data, Project project)
		{
			PlayerData = data;
			data.m_stateMachine.m_states.Remove("PlayerDead");
			data.m_stateMachine.AddState("PlayerDead", Enter, Update, null);
		}

		public static void Enter()
		{
			PlayerData.View.ActiveCamera = PlayerData.View.FindCamera<DeathCamera>();
			if (PlayerData.ComponentPlayer != null)
			{
				string text = PlayerData.ComponentPlayer.ComponentHealth.CauseOfDeath;
				if (string.IsNullOrEmpty(text))
					text = "未知";
				string arg = "死因: " + text;
				if (PlayerData.m_subsystemGameInfo.WorldSettings.GameMode == GameMode.Cruel)
					PlayerData.ComponentPlayer.ComponentGui.DisplayLargeMessage("你死了", $"{arg}\n\n不能在残酷模式复活", 30f, 1.5f);
				else if (PlayerData.m_subsystemGameInfo.WorldSettings.GameMode == GameMode.Adventure && !PlayerData.m_subsystemGameInfo.WorldSettings.IsAdventureRespawnAllowed)
					PlayerData.ComponentPlayer.ComponentGui.DisplayLargeMessage("你死了", $"{arg}\n\n轻触以重置", 30f, 1.5f);
				else
					PlayerData.ComponentPlayer.ComponentGui.DisplayLargeMessage("你死了", $"{arg}\n\n轻触以复活", 30f, 1.5f);
			}
		}

		public static void Update()
		{
			if (PlayerData.ComponentPlayer == null)
				PlayerData.m_stateMachine.TransitionTo("PrepareSpawn");
			else if ((PlayerData.m_playerDeathTime.HasValue ? Time.RealTime - PlayerData.m_playerDeathTime.Value : Time.RealTime) > 1.5 && !DialogsManager.HasDialogs(PlayerData.ComponentPlayer.View.GameWidget) && PlayerData.ComponentPlayer.View.Input.Any)
				if (PlayerData.m_subsystemGameInfo.WorldSettings.GameMode == GameMode.Cruel)
					DialogsManager.ShowDialog(PlayerData.ComponentPlayer.View.GameWidget, new GameMenuDialog(PlayerData.ComponentPlayer));
				else if (PlayerData.m_subsystemGameInfo.WorldSettings.GameMode == GameMode.Adventure && !PlayerData.m_subsystemGameInfo.WorldSettings.IsAdventureRespawnAllowed)
					ScreensManager.SwitchScreen("GameLoading", GameManager.WorldInfo, "AdventureRestart");
				else
					PlayerData.m_project.RemoveEntity(PlayerData.ComponentPlayer.Entity, true);
		}

		public new void Update(float dt)
		{
			CollisionVelocityChange = Vector3.Zero;
			if (m_totalImpulse.HasValue)
			{
				Velocity += m_totalImpulse.Value;
				m_totalImpulse = null;
			}
			if (m_parentBody != null || m_velocity.LengthSquared() > 9.99999944E-11f)
				m_stoppedTime = 0f;
			else
			{
				m_stoppedTime += dt;
				if (m_stoppedTime > 0.5f && !Time.PeriodicEvent(0.25, 0.0))
					return;
			}
			Vector3 position = Position;
			TerrainChunk chunkAtCell = m_subsystemTerrain.Terrain.GetChunkAtCell(Terrain.ToCell(position.X), Terrain.ToCell(position.Z));
			if (chunkAtCell == null || chunkAtCell.State <= TerrainChunkState.InvalidContents4)
			{
				Velocity = Vector3.Zero;
				return;
			}
			ComponentRider rider = Entity.FindComponent<ComponentRider>();
			if ((rider == null || !rider.m_isAnimating) && IsGravityEnabled && (m_parentBody == null || m_childBodies.Count == 0))
			{
				m_bodiesCollisionBoxes.Clear();
				FindBodiesCollisionBoxes(position, m_bodiesCollisionBoxes);
			}
			m_movingBlocksCollisionBoxes.Clear();
			FindMovingBlocksCollisionBoxes(position, m_movingBlocksCollisionBoxes);
			MoveToFreeSpace();
			if (IsGravityEnabled)
			{
				m_velocity.Y -= 10f * dt;
				if (ImmersionFactor > 0f)
				{
					float num = ImmersionFactor * (1f + 0.03f * MathUtils.Sin((float)MathUtils.Remainder(2.0 * m_subsystemTime.GameTime, 6.2831854820251465)));
					m_velocity.Y += 10f * (1f / Density * num) * dt;
				}
			}
			float num2 = MathUtils.Saturate(AirDrag.X * dt);
			float num3 = MathUtils.Saturate(AirDrag.Y * dt);
			m_velocity.X *= 1f - num2;
			m_velocity.Y *= 1f - num3;
			m_velocity.Z *= 1f - num2;
			if (IsWaterDragEnabled && ImmersionFactor > 0f && ImmersionFluidBlock != null)
			{
				Vector2? vector = m_subsystemFluidBlockBehavior.CalculateFlowSpeed(Terrain.ToCell(position.X), Terrain.ToCell(position.Y), Terrain.ToCell(position.Z));
				Vector3 vector2 = vector.HasValue ? new Vector3(vector.Value.X, 0f, vector.Value.Y) : Vector3.Zero;
				float num4 = 1f;
				if (ImmersionFluidBlock.FrictionFactor != 1f)
					num4 = (SimplexNoise.Noise((float)MathUtils.Remainder(6.0 * Time.FrameStartTime + (double)(GetHashCode() % 1000), 1000.0)) > 0.5f) ? ImmersionFluidBlock.FrictionFactor : 1f;
				float f = MathUtils.Saturate(WaterDrag.X * num4 * ImmersionFactor * dt);
				float f2 = MathUtils.Saturate(WaterDrag.Y * num4 * dt);
				m_velocity.X = MathUtils.Lerp(m_velocity.X, vector2.X, f);
				m_velocity.Y = MathUtils.Lerp(m_velocity.Y, vector2.Y, f2);
				m_velocity.Z = MathUtils.Lerp(m_velocity.Z, vector2.Z, f);
				if (m_parentBody == null && vector.HasValue && !StandingOnValue.HasValue)
				{
					if (WaterTurnSpeed > 0f)
					{
						float s = MathUtils.Saturate(MathUtils.Lerp(1f, 0f, m_velocity.Length()));
						Vector2 vector3 = Vector2.Normalize(vector.Value) * s;
						Rotation *= Quaternion.CreateFromAxisAngle(Vector3.UnitY, WaterTurnSpeed * (-1f * vector3.X + 0.71f * vector3.Y) * dt);
					}
					if (WaterSwayAngle > 0f)
						Rotation *= Quaternion.CreateFromAxisAngle(Vector3.UnitX, WaterSwayAngle * (float)MathUtils.Sin((double)(200f / Mass) * m_subsystemTime.GameTime));
				}
			}
			if (m_parentBody != null)
			{
				Vector3 v = Vector3.Transform(ParentBodyPositionOffset, m_parentBody.Rotation) + m_parentBody.Position - position;
				m_velocity = (dt > 0f) ? (v / dt) : Vector3.Zero;
				Rotation = ParentBodyRotationOffset * m_parentBody.Rotation;
			}
			StandingOnValue = null;
			StandingOnBody = null;
			StandingOnVelocity = Vector3.Zero;
			Vector3 velocity = m_velocity;
			float num5 = m_velocity.Length();
			if (num5 > 0f)
			{
				float x = 0.475f * MathUtils.Min(BoxSize.X, BoxSize.Y, BoxSize.Z) / num5;
				float num7;
				for (float num6 = dt; num6 > 0f; num6 -= num7)
				{
					num7 = MathUtils.Min(num6, x);
					MoveWithCollision(num7);
				}
			}
			CollisionVelocityChange = m_velocity - velocity;
			if (IsGroundDragEnabled && StandingOnValue.HasValue)
				m_velocity = Vector3.Lerp(m_velocity, StandingOnVelocity, 6f * dt);
			if (!StandingOnValue.HasValue)
				IsSneaking = false;
			UpdateImmersionData();
			if (ImmersionFluidBlock is WaterBlock && ImmersionDepth > 0.3f && !m_fluidEffectsPlayed)
			{
				m_fluidEffectsPlayed = true;
				m_subsystemAudio.PlayRandomSound("Audio/WaterFallIn", m_random.UniformFloat(0.75f, 1f), m_random.UniformFloat(-0.3f, 0f), position, 4f, true);
				SubsystemParticles subsystemParticles = m_subsystemParticles;
				SubsystemTerrain subsystemTerrain = m_subsystemTerrain;
				Vector3 position2 = position;
				subsystemParticles.AddParticleSystem(new WaterSplashParticleSystem(subsystemTerrain, position2, (BoundingBox.Max - BoundingBox.Min).Length() > 0.8f));
			}
			else if (ImmersionFluidBlock is MagmaBlock && ImmersionDepth > 0f && !m_fluidEffectsPlayed)
			{
				m_fluidEffectsPlayed = true;
				m_subsystemAudio.PlaySound("Audio/SizzleLong", 1f, 0f, position, 4f, autoDelay: true);
				SubsystemParticles subsystemParticles2 = m_subsystemParticles;
				SubsystemTerrain subsystemTerrain2 = m_subsystemTerrain;
				Vector3 position3 = position;
				subsystemParticles2.AddParticleSystem(new MagmaSplashParticleSystem(subsystemTerrain2, position3, (BoundingBox.Max - BoundingBox.Min).Length() > 0.8f));
			}
			else if (ImmersionFluidBlock == null)
			{
				m_fluidEffectsPlayed = false;
			}
		}

		public new void FindBodiesCollisionBoxes(Vector3 position, DynamicArray<CollisionBox> result)
		{
			m_componentBodies.Clear();
			m_subsystemBodies.FindBodiesAroundPoint(new Vector2(position.X, position.Z), 4f, m_componentBodies);
			for (int i = 0; i < m_componentBodies.Count; i++)
			{
				ComponentBody componentBody = m_componentBodies.Array[i];
				if (componentBody != this && componentBody != m_parentBody && componentBody.m_parentBody != this && componentBody.IsGravityEnabled && componentBody.ParentBodyRotationOffset == Quaternion.Identity)
				{
					result.Add(new CollisionBox
					{
						Box = componentBody.BoundingBox,
						ComponentBody = componentBody
					});
				}
			}
		}
	}
}