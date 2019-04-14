using Engine;
using Engine.Graphics;
using Engine.Input;
using GameEntitySystem;
using System;
using TemplatesDatabase;

namespace Game
{
	public class NewParticleSystem : ParticleSystem<PukeParticleSystem.Particle>
	{
		public static Random m_random = new Random();

		public SubsystemTerrain m_subsystemTerrain;
		public SubsystemBodies m_subsystemBodies;

		public float m_duration;

		public float m_toGenerate;

		public Vector3 Position;

		public Vector3 Direction;

		public bool IsStopped;
		public bool Big;

		public NewParticleSystem(SubsystemTerrain terrain, SubsystemBodies bodies, bool big) : base(big ? 128 : 60)
		{
			m_subsystemTerrain = terrain;
			m_subsystemBodies = bodies;
			Texture = ContentManager.Get<Texture2D>("Textures/WaterSplashParticle");
			TextureSlotsCount = 1;
			Big = big;
		}

		public bool Act(ComponentBody body, float distance)
		{
			return Vector3.DistanceSquared(body.m_position, Position) > 1.4f;
		}

		public override bool Simulate(float dt)
		{
			dt = MathUtils.Clamp(dt, 0f, 0.1f);
			m_duration += dt;
			if (m_duration > 4f)
			{
				IsStopped = true;
			}
			m_toGenerate += 60f * dt;
			bool flag = true;
			float num4 = MathUtils.Pow(0.03f, dt);
			float num5 = MathUtils.Saturate(1.3f * SimplexNoise.Noise(3f * m_duration + GetHashCode() % 100) - 0.3f);
			for (int i = 0; i < Particles.Length; i++)
			{
				var particle = Particles[i];
				if (particle.IsActive)
				{
					flag = false;
					particle.TimeToLive -= dt * 2f;
					if (particle.TimeToLive > 0f)
					{
						Vector3 position = particle.Position;
						Vector3 vector = position + particle.Velocity * dt;
						TerrainRaycastResult? terrainRaycastResult = m_subsystemTerrain.Raycast(position, vector, false, true, PukeParticleSystem.c._.Simulate_b__18_0);
						if (terrainRaycastResult.HasValue)
						{
							vector = position;
							Vector3 normal = terrainRaycastResult.Value.CellFace.CalculatePlane().Normal;
							if (normal.X != 0f)
							{
								particle.Velocity *= new Vector3(-0.05f, 0.05f, 0.05f);
							}
							if (normal.Y != 0f)
							{
								particle.Velocity *= new Vector3(0.05f, -0.05f, 0.05f);
							}
							if (normal.Z != 0f)
							{
								particle.Velocity *= new Vector3(0.05f, 0.05f, -0.05f);
							}
						}
						else if (m_subsystemBodies != null)
						{
							var body = m_subsystemBodies.Raycast(position, vector, 0f, Act);
							if (body.HasValue)
								vector = position;
						}
						particle.Position = vector;
						if (!Big)
						{
							particle.Velocity.Y += -9.81f * dt;
							particle.Velocity *= num4;
						}
						//particle.Color *= MathUtils.Saturate(particle.TimeToLive);
						particle.TextureSlot = 0;//(int)MathUtils.Saturate(3f - particle.TimeToLive);
					}
					else
					{
						particle.IsActive = false;
					}
				}
				else if (!IsStopped && m_toGenerate >= 1f)
				{
					Vector3 v = m_random.UniformVector3(0f, 1f, true);
					particle.IsActive = true;
					particle.Position = Position + 0.05f * v;
					particle.Color = Color.White;
					particle.Velocity = MathUtils.Lerp(1f, 2.5f, num5 * 3f) * Vector3.Normalize(Direction);
					particle.TimeToLive = 3f;
					particle.Size = new Vector2(0.03f);
					//particle.FlipX = m_random.Bool();
					//particle.FlipY = m_random.Bool();
					m_toGenerate -= 1f;
				}
			}
			return m_duration > 4f ? flag : false;
		}
	}
	public class ComponentWave : Component, IUpdateable
	{
		DynamicArray<ComponentBody> bodies;
		public ComponentBody ComponentBody;
		public ComponentCreatureModel ComponentCreatureModel;
		protected ComponentMount m_componentMount;
		public ComponentHealth ComponentHealth;
		public NewParticleSystem m_pukeParticleSystem;

		public SubsystemTime SubsystemTime;
		public SubsystemPlayers SubsystemPlayers;
		public bool Loaded;
		public static bool Stop,
					B;
		public int UpdateOrder => 0;

		public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
		{
			ComponentBody = Entity.FindComponent<ComponentBody>(true);
			m_componentMount = Entity.FindComponent<ComponentMount>(true);
			ComponentHealth = Entity.FindComponent<ComponentHealth>(true);
			ComponentCreatureModel = Entity.FindComponent<ComponentCreatureModel>(true);
			Loaded = Entity.FindComponent<ComponentPlayer>() == null;
			SubsystemTime = Project.FindSubsystem<SubsystemTime>(true);
			SubsystemPlayers = Project.FindSubsystem<SubsystemPlayers>(true);
			if (Loaded)
			{
				ComponentBody.CollidedWithBody += CollidedWithBody;
			}
			base.Load(valuesDictionary, idToEntityMap);
		}

		private void CollidedWithBody(ComponentBody body)
		{
			var miner = body.Entity.FindComponent<ComponentMiner>();
			ComponentBody p;
			if (miner != null && body.Entity.FindComponent<ComponentWave>() == null && miner.ActiveBlockValue == 0 || miner.ActiveBlockValue == ChristmasTreeBlock.Index)
			{
				p = body;
				while (p.ParentBody != null)
				{
					p = p.ParentBody;
					if (p.Entity == Entity) return;
				}
				p = body;
				while (p.ChildBodies.Count > 0) p = p.ChildBodies[0];
				if (p.Entity == Entity) return;
				ComponentBody.ParentBody = p;
			}
			if (m_componentMount.Rider != null)
			{
				miner = m_componentMount.Rider.Entity.FindComponent<ComponentMiner>();
				if (miner != null && miner.ActiveBlockValue == SaddleBlock.Index && ComponentBody.ParentBody == null)
				{
					p = body;
					while (p.ParentBody != null)
					{
						p = p.ParentBody;
						if (p.Entity == Entity) return;
					}
					ComponentBody.ParentBody = body;
				}
			}
		}

		public void Update(float dt)
		{
			var p = SubsystemPlayers.FindNearestPlayer(ComponentBody.m_position);
			if (Loaded)
			{
				if (ComponentHealth.m_componentOnFire.IsOnFire)
					Project.RemoveEntity(Entity, true);
				if (ComponentBody.ParentBody == null)
				{
					if (m_componentMount.Rider == null && SubsystemTime.PeriodicGameTimeEvent(0.3, 0))
						ComponentBody.ApplyImpulse(Vector3.Normalize(p.ComponentBody.m_position - new Vector3(0.1f, 0, 0) - ComponentBody.m_position));
					return;
				}
				if (ComponentHealth.Health <= 0)
					ComponentBody.ParentBodyPositionOffset = new Vector3 { X = 0.4f };
				//ComponentBody.ParentBodyRotationOffset *= Quaternion.CreateFromAxisAngle(Vector3.UnitY, MathUtils.PI / 10);
				//	Vector3.Normalize(ComponentBody.Rotation.ToForwardVector()) * 0.03f * MathUtils.Sin((float)SubsystemTime.GameTime + ComponentBody.Position.X + ComponentBody.Position.Z);
				//Loaded = true;
				var miner = ComponentBody.ParentBody.Entity.FindComponent<ComponentMiner>();
				if (miner != null && miner.ActiveBlockValue == BulletBlock.Index)
					ComponentBody.ParentBody = null;
			}
			else if (Keyboard.IsKeyDownOnce(Key.J))
				Stop = !Stop;
			if (Keyboard.IsKeyDownOnce(Key.K))
				B = !B;
			if (m_pukeParticleSystem != null)
			{
				Vector3 v = ComponentCreatureModel.EyeRotation.ToUpVector();
				Vector3 vector = ComponentCreatureModel.EyeRotation.ToForwardVector();
				m_pukeParticleSystem.Position = ComponentCreatureModel.EyePosition - 0.6f * v + 0.1f * vector;
				m_pukeParticleSystem.Direction = Vector3.Normalize(vector + 0.5f * v);
				if (m_pukeParticleSystem.IsStopped)
					m_pukeParticleSystem = null;
				return;
			}
			if (Stop)
			{
				m_pukeParticleSystem = null;
				return;
			}
			if (!SubsystemTime.PeriodicGameTimeEvent(0.3, 0))
				return;
			m_pukeParticleSystem = new NewParticleSystem(ComponentBody.m_subsystemTerrain, Vector3.DistanceSquared(p.ComponentBody.m_position, ComponentBody.m_position) < 1f ? ComponentBody.m_subsystemBodies : null, !Loaded & B);
			ComponentBody.m_subsystemParticles.AddParticleSystem(m_pukeParticleSystem);
		}
	}

	public class ComponentNHumanModel : ComponentHumanModel
	{
		public override void Animate()
		{
			Vector3 position = m_componentCreature.ComponentBody.Position;
			Vector3 vector = m_componentCreature.ComponentBody.Rotation.ToYawPitchRoll();
			if (m_lieDownFactorModel == 0f)
			{
				ComponentMount componentMount = m_componentRider?.Mount;
				float num = MathUtils.Sin((float)Math.PI * 2f * MovementAnimationPhase);
				position.Y += Bob;
				vector.X += m_headingOffset;
				float num2 = (float)MathUtils.Remainder(0.75 * m_subsystemGameInfo.TotalElapsedGameTime + (GetHashCode() & 0xFFFF), 10000.0);
				float x = MathUtils.Clamp(MathUtils.Lerp(-0.3f, 0.3f, SimplexNoise.Noise(1.02f * num2 - 100f)) + m_componentCreature.ComponentLocomotion.LookAngles.X + 1f * m_componentCreature.ComponentLocomotion.LastTurnOrder.X + m_headingOffset, 0f - MathUtils.DegToRad(80f), MathUtils.DegToRad(80f));
				float y = MathUtils.Clamp(MathUtils.Lerp(-0.3f, 0.3f, SimplexNoise.Noise(0.96f * num2 - 200f)) + m_componentCreature.ComponentLocomotion.LookAngles.Y, 0f - MathUtils.DegToRad(45f), MathUtils.DegToRad(45f));
				float num3 = 0f;
				float y2 = 0f;
				float x2 = 0f;
				float y3 = 0f;
				float num4 = 0f;
				float num5 = 0f;
				float num6 = 0f;
				float num7 = 0f;
				if (componentMount != null)
				{
					if (componentMount.Entity.ValuesDictionary.DatabaseObject.Name == "Boat")
					{
						position.Y -= 0.2f;
						vector.X += (float)Math.PI;
						num4 = 0.4f;
						num6 = 0.4f;
						num5 = 0.2f;
						num7 = -0.2f;
						num3 = 1.1f;
						x2 = 1.1f;
						y2 = 0.2f;
						y3 = -0.2f;
					}
					else
					{
						num4 = 0.5f;
						num6 = 0.5f;
						num5 = 0.15f;
						num7 = -0.15f;
						y2 = 0.55f;
						y3 = -0.55f;
					}
				}
				else if (m_componentCreature.ComponentLocomotion.IsCreativeFlyEnabled)
				{
					float num8 = m_componentCreature.ComponentLocomotion.LastWalkOrder.HasValue ? MathUtils.Min(new Vector2(0.1f * m_componentCreature.ComponentBody.Velocity.X, m_componentCreature.ComponentBody.Velocity.Z).Length(), 0.5f) : 0f;
					num3 = -0.1f - num8;
					x2 = num3;
					y2 = MathUtils.Lerp(0f, 0.25f, SimplexNoise.Noise(1.07f * num2 + 400f));
					y3 = 0f - MathUtils.Lerp(0f, 0.25f, SimplexNoise.Noise(0.93f * num2 + 500f));
				}
				else if (MovementAnimationPhase != 0f)
				{
					num4 = -0.5f * num;
					num6 = 0.5f * num;
					num3 = m_walkLegsAngle * num;
					x2 = 0f - num3;
				}
				float num9 = 0f;
				if (m_componentMiner != null)
				{
					float num10 = MathUtils.Sin(MathUtils.Sqrt(m_componentMiner.PokingPhase) * (float)Math.PI);
					num9 = (m_componentMiner.ActiveBlockValue == 0) ? (1f * num10) : (0.3f + 1f * num10);
				}
				float num11 = (m_punchPhase != 0f) ? ((0f - MathUtils.DegToRad(90f)) * MathUtils.Sin((float)Math.PI * 2f * MathUtils.Sigmoid(m_punchPhase, 4f))) : 0f;
				float num12 = ((m_punchCounter & 1) == 0) ? num11 : 0f;
				float num13 = ((m_punchCounter & 1) != 0) ? num11 : 0f;
				float num14 = 0f;
				float num15 = 0f;
				float num16 = 0f;
				float num17 = 0f;
				if (m_rowLeft || m_rowRight)
				{
					float num18 = 0.6f * (float)MathUtils.Sin(6.91150426864624 * m_subsystemTime.GameTime);
					float num19 = 0.2f + 0.2f * (float)MathUtils.Cos(6.91150426864624 * (m_subsystemTime.GameTime + 0.5));
					if (m_rowLeft)
					{
						num14 = num18;
						num15 = num19;
					}
					if (m_rowRight)
					{
						num16 = num18;
						num17 = 0f - num19;
					}
				}
				float num20 = 0f;
				float num21 = 0f;
				float num22 = 0f;
				float num23 = 0f;
				if (m_aimHandAngle != 0f)
				{
					num20 = 1.5f;
					num21 = -0.7f;
					num22 = m_aimHandAngle * 1f;
					num23 = 0f;
				}
				float num24 = (!m_componentCreature.ComponentLocomotion.IsCreativeFlyEnabled) ? 1 : 4;
				num4 += MathUtils.Lerp(-0.1f, 0.1f, SimplexNoise.Noise(num2)) + num12 + num14 + num20;
				num5 += MathUtils.Lerp(0f, num24 * 0.15f, SimplexNoise.Noise(1.1f * num2 + 100f)) + num15 + num21;
				num6 += num9 + MathUtils.Lerp(-0.1f, 0.1f, SimplexNoise.Noise(0.9f * num2 + 200f)) + num13 + num16 + num22;
				num7 += 0f - MathUtils.Lerp(0f, num24 * 0.15f, SimplexNoise.Noise(1.05f * num2 + 300f)) + num17 + num23;
				float s = MathUtils.Min(12f * m_subsystemTime.GameTimeDelta, 1f);
				m_headAngles += s * (new Vector2(x, y) - m_headAngles);
				m_handAngles1 += s * (new Vector2(num4, num5) - m_handAngles1);
				m_handAngles2 += s * (new Vector2(num6, num7) - m_handAngles2);
				m_legAngles1 += s * (new Vector2(num3, y2) - m_legAngles1);
				m_legAngles2 += s * (new Vector2(x2, y3) - m_legAngles2);
				SetBoneTransform(m_bodyBone.Index, Matrix.CreateRotationY(vector.X) * Matrix.CreateTranslation(position));
				SetBoneTransform(m_headBone.Index, Matrix.CreateRotationX(m_headAngles.Y) * Matrix.CreateRotationZ(0f - m_headAngles.X));
				SetBoneTransform(m_hand1Bone.Index, Matrix.CreateRotationY(m_handAngles1.Y) * Matrix.CreateRotationX(m_handAngles1.X));
				SetBoneTransform(m_hand2Bone.Index, Matrix.CreateRotationY(m_handAngles2.Y) * Matrix.CreateRotationX(m_handAngles2.X));
				m_leg1Bone.m_transform *= Matrix.CreateRotationZ(m_legAngles1.Y);
				m_leg2Bone.m_transform *= Matrix.CreateRotationZ(m_legAngles2.Y);
				//SetBoneTransform(m_leg1Bone.Index, Matrix.CreateRotationY(m_legAngles1.Y) * Matrix.CreateRotationX(m_legAngles1.X));
				//SetBoneTransform(m_leg2Bone.Index, Matrix.CreateRotationY(m_legAngles2.Y) * Matrix.CreateRotationX(m_legAngles2.X));
			}
			else
			{
				float num25 = MathUtils.Max(DeathPhase, m_lieDownFactorModel);
				float num26 = 1f - num25;
				Vector3 position2 = position + num25 * 0.5f * m_componentCreature.ComponentBody.BoxSize.Y * Vector3.Normalize(m_componentCreature.ComponentBody.Matrix.Forward * new Vector3(1f, 0f, 1f)) + num25 * Vector3.UnitY * m_componentCreature.ComponentBody.BoxSize.Z * 0.1f;
				SetBoneTransform(m_bodyBone.Index, Matrix.CreateFromYawPitchRoll(vector.X, (float)Math.PI / 2f * num25, 0f) * Matrix.CreateTranslation(position2));
				SetBoneTransform(m_headBone.Index, Matrix.Identity);
				SetBoneTransform(m_hand1Bone.Index, Matrix.CreateRotationY(m_handAngles1.Y * num26) * Matrix.CreateRotationX(m_handAngles1.X * num26));
				SetBoneTransform(m_hand2Bone.Index, Matrix.CreateRotationY(m_handAngles2.Y * num26) * Matrix.CreateRotationX(m_handAngles2.X * num26));
				SetBoneTransform(m_leg1Bone.Index, Matrix.CreateRotationY(m_legAngles1.Y * num26) * Matrix.CreateRotationX(m_legAngles1.X * num26));
				SetBoneTransform(m_leg2Bone.Index, Matrix.CreateRotationY(m_legAngles2.Y * num26) * Matrix.CreateRotationX(m_legAngles2.X * num26));
			}
			base.Animate();
		}
	}
}