using System.Collections.Generic;
using System.Reflection;
using Engine;
using GameEntitySystem;
using TemplatesDatabase;

namespace Game
{
	public class ComponentFissionBehavior : Component, IUpdateable
	{
		private ComponentBody m_componentBody;
		private ComponentCreature m_componentCreature;
		private ComponentHealth m_componentHealth;
		private ComponentSpawn m_componentSpawn;
		private SubsystemEntityFactory m_subsystemEntityFactory;
		private ShapeshiftParticleSystem m_particleSystem;
		private bool m_isSplit;
		private float m_pressure;
		private string m_splitEntityTemplateName;

		public int UpdateOrder
		{
			get
			{
				return 0;
			}
		}

		public void Update(float dt)
		{
			if (!m_isSplit && !m_componentSpawn.IsDespawning && m_componentHealth.Health <= 0f)
			{
				m_isSplit = true;
				string entityTemplateName = base.Entity.ValuesDictionary.DatabaseObject.Name + "_Split";
				Random.GlobalRandom.UniformFloat(2f, 8f);
				Split();
				Entity entity = m_subsystemEntityFactory.CreateEntity(entityTemplateName, false);
			}
			if (!string.IsNullOrEmpty(m_splitEntityTemplateName))
			{
				if (m_particleSystem == null)
				{
					m_particleSystem = new ShapeshiftParticleSystem();
					base.Project.FindSubsystem<SubsystemParticles>(true).AddParticleSystem(m_particleSystem);
				}
				m_particleSystem.BoundingBox = m_componentBody.BoundingBox;
			}
		}

		protected override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap)
		{
			base.Load(valuesDictionary, idToEntityMap);
			m_componentCreature = base.Entity.FindComponent<ComponentCreature>(true);
			m_componentBody = m_componentCreature.ComponentBody;
			m_componentHealth = m_componentCreature.ComponentHealth;
			m_componentSpawn = base.Entity.FindComponent<ComponentSpawn>(true);
			m_subsystemEntityFactory = base.Project.FindSubsystem<SubsystemEntityFactory>(true);
			m_splitEntityTemplateName = valuesDictionary.GetValue<string>("SplitEntityTemplateName", null);
			m_pressure = valuesDictionary.GetValue<bool>("ExplodeWhenSplit", 0f);
			m_isSplit = false;
			if (m_pressure < 0f) m_pressure = m_componentHealth.AttackResilience * 3;
			m_componentSpawn.Despawned += new Action<ComponentSpawn>(ComponentSpawn_Despawned);
		}

		private void Split(string entityTemplateName)
		{
			if (string.IsNullOrEmpty(m_splitEntityTemplateName))
			{
				Vector3 position = m_componentBody.Position;
				m_splitEntityTemplateName = entityTemplateName;
				if (m_pressure > 0f)
					base.Project.FindSubsystem<SubsystemExplosions>(true).AddExplosion((int)position.X, (int)position.Y, (int)position.Z, m_pressure, false, false);
				m_componentSpawn.DespawnDuration = 2f;
				m_componentSpawn.Despawn();
				base.Project.FindSubsystem<SubsystemAudio>(true).PlaySound("Audio/Shapeshift", 1f, 0f, position, 3f, true);
			}
		}

		private void ComponentSpawn_Despawned(ComponentSpawn componentSpawn)
		{
			if (!string.IsNullOrEmpty(m_splitEntityTemplateName))
			{
				Entity entity = m_subsystemEntityFactory.CreateEntity(m_splitEntityTemplateName, true);
				Entity entity2 = m_subsystemEntityFactory.CreateEntity(m_splitEntityTemplateName, true);
				ComponentBody expr_39 = entity.FindComponent<ComponentBody>(true);
				expr_39.Position = m_componentBody.Position;
				expr_39.Rotation = m_componentBody.Rotation;
				expr_39.Velocity = m_componentBody.Velocity;
				entity.FindComponent<ComponentCreature>(true).ComponentBody = expr_39;
				expr_39 = entity2.FindComponent<ComponentCreature>(true).ComponentBody = expr_39;
				expr_39.Position += new Vector3(1.2f);
				entity.FindComponent<ComponentSpawn>(true).SpawnDuration = entity2.FindComponent<ComponentSpawn>(true).SpawnDuration = 0.5f;
				base.Project.AddEntity(entity);
				base.Project.AddEntity(entity2);
			}
			if (m_particleSystem != null)
				m_particleSystem.Stopped = true;
		}
	}
}
