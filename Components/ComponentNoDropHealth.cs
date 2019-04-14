using Engine;

namespace Game
{
	public class ComponentNoDropHealth : ComponentHealth, IUpdateable
	{
		public new void Update(float dt)
		{
			Vector3 position = m_componentCreature.ComponentBody.Position;
			if (Health > 0f && Health < 1f)
			{
				float num = 0f;
				if (m_componentPlayer != null)
				{
					if (m_subsystemGameInfo.WorldSettings.GameMode == GameMode.Harmless)
						num = 0.0166666675f;
					else if (m_componentPlayer.ComponentSleep.SleepFactor == 1f && m_componentPlayer.ComponentVitalStats.Food > 0f)
					{
						num = 0.00166666671f;
					}
					else if (m_componentPlayer.ComponentVitalStats.Food > 0.5f)
					{
						num = 0.00111111114f;
					}
				}
				else
					num = 0.00111111114f;
				Heal(m_subsystemGameInfo.TotalElapsedGameTimeDelta * num);
			}
			if (BreathingMode == BreathingMode.Air)
			{
				int cellContents = m_subsystemTerrain.Terrain.GetCellContents(Terrain.ToCell(position.X), Terrain.ToCell(m_componentCreature.ComponentCreatureModel.EyePosition.Y), Terrain.ToCell(position.Z));
				if (BlocksManager.Blocks[cellContents] is FluidBlock || position.Y > 131f)
					Air = MathUtils.Saturate(Air - dt / AirCapacity);
				else
					Air = 1f;
			}
			else if (BreathingMode == BreathingMode.Water)
			{
				if (m_componentCreature.ComponentBody.ImmersionFactor > 0.25f)
					Air = 1f;
				else
					Air = MathUtils.Saturate(Air - dt / AirCapacity);
			}
			if (m_componentCreature.ComponentBody.ImmersionFactor > 0f && m_componentCreature.ComponentBody.ImmersionFluidBlock is MagmaBlock)
			{
				Injure(2f * m_componentCreature.ComponentBody.ImmersionFactor * dt, null, false, "Burned by magma");
				float num2 = 1.1f + 0.1f * (float)MathUtils.Sin(12.0 * m_subsystemTime.GameTime);
				m_redScreenFactor = MathUtils.Max(m_redScreenFactor, num2 * 1.5f * m_componentCreature.ComponentBody.ImmersionFactor);
			}
			float num3 = MathUtils.Abs(m_componentCreature.ComponentBody.CollisionVelocityChange.Y);
			if (!m_wasStanding && num3 > FallResilience)
			{
				float num4 = MathUtils.Sqr(MathUtils.Max(num3 - FallResilience, 0f)) / 15f;
				if (m_componentPlayer != null)
					num4 /= m_componentPlayer.ComponentLevel.ResilienceFactor;
				Injure(num4, null, false, "Impact with the ground");
			}
			m_wasStanding = m_componentCreature.ComponentBody.StandingOnValue.HasValue || m_componentCreature.ComponentBody.StandingOnBody != null;
			if ((position.Y < 0f || position.Y > 168f) && m_subsystemTime.PeriodicGameTimeEvent(2.0, 0.0))
			{
				Injure(0.1f, null, true, "Left the world");
				m_componentPlayer?.ComponentGui.DisplaySmallMessage("Come back!", true, false);
			}
			bool num5 = m_subsystemTime.PeriodicGameTimeEvent(1.0, 0.0);
			if (num5 && Air == 0f)
			{
				float num6 = 0.12f;
				if (m_componentPlayer != null)
					num6 /= m_componentPlayer.ComponentLevel.ResilienceFactor;
				Injure(num6, null, false, "Suffocated");
			}
			if (num5 && (m_componentOnFire.IsOnFire || m_componentOnFire.TouchesFire))
			{
				float num7 = 1f / FireResilience;
				if (m_componentPlayer != null)
					num7 /= m_componentPlayer.ComponentLevel.ResilienceFactor;
				Injure(num7, m_componentOnFire.Attacker, false, "Burned to death");
			}
			if (num5 && CanStrand && m_componentCreature.ComponentBody.ImmersionFactor < 0.25f && (m_componentCreature.ComponentBody.StandingOnValue != 0 || m_componentCreature.ComponentBody.StandingOnBody != null))
				Injure(0.05f, null, false, "Stranded on land");
			HealthChange = Health - m_lastHealth;
			m_lastHealth = Health;
			if (m_redScreenFactor > 0.01f)
				m_redScreenFactor *= MathUtils.Pow(0.2f, dt);
			else
				m_redScreenFactor = 0f;
			if (HealthChange < 0f)
			{
				m_componentCreature.ComponentCreatureSounds.PlayPainSound();
				m_redScreenFactor += -4f * HealthChange;
				m_componentPlayer?.ComponentGui.HealthBarWidget.Flash(MathUtils.Clamp((int)((0f - HealthChange) * 30f), 0, 10));
			}
			if (m_componentPlayer != null)
				m_componentPlayer.ComponentScreenOverlays.RedoutFactor = MathUtils.Max(m_componentPlayer.ComponentScreenOverlays.RedoutFactor, m_redScreenFactor);
			if (m_componentPlayer != null)
				m_componentPlayer.ComponentGui.HealthBarWidget.Value = Health;
			if (Health == 0f && HealthChange < 0f)
			{
				Vector3 position2 = m_componentCreature.ComponentBody.Position + new Vector3(0f, m_componentCreature.ComponentBody.BoxSize.Y / 2f, 0f);
				float x = m_componentCreature.ComponentBody.BoxSize.X;
				m_subsystemParticles.AddParticleSystem(new KillParticleSystem(m_subsystemTerrain, position2, x));
				/*Vector3 position3 = (m_componentCreature.ComponentBody.BoundingBox.Min + m_componentCreature.ComponentBody.BoundingBox.Max) / 2f;
				foreach (IInventory item in base.Entity.FindComponents<IInventory>())
				{
					item.DropAllItems(position3);
				}*/
				DeathTime = m_subsystemGameInfo.TotalElapsedGameTime;
			}
			if (Health <= 0f && CorpseDuration > 0f && m_subsystemGameInfo.TotalElapsedGameTime - DeathTime > (double)CorpseDuration)
				m_componentCreature.ComponentSpawn.Despawn();
		}
	}
}