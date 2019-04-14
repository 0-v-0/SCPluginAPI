using Engine;

namespace Game
{
	public class ComponentMagneticRider : ComponentRider, IUpdateable
	{
		public new void Update(float dt)
		{
			if (m_isAnimating)
			{
				float f = 8f * dt;
				ComponentBody componentBody = ComponentCreature.ComponentBody;
				componentBody.ParentBodyPositionOffset = Vector3.Lerp(componentBody.ParentBodyPositionOffset, m_targetPositionOffset, f);
				componentBody.ParentBodyRotationOffset = Quaternion.Slerp(componentBody.ParentBodyRotationOffset, m_targetRotationOffset, f);
				m_animationTime += dt;
				if (Vector3.DistanceSquared(componentBody.ParentBodyPositionOffset, m_targetPositionOffset) < 0.0100000007f || m_animationTime > 0.75f)
				{
					m_isAnimating = false;
					if (m_isDismounting)
					{
						if (componentBody.ParentBody != null)
						{
							componentBody.Velocity = componentBody.ParentBody.Velocity;
							componentBody.ParentBody = null;
						}
					}
					else
					{
						componentBody.ParentBodyPositionOffset = m_targetPositionOffset;
						componentBody.ParentBodyRotationOffset = m_targetRotationOffset;
						m_outOfMountTime = 0f;
					}
				}
			}
			ComponentMount mount = Mount;
			if (mount != null && !m_isAnimating)
			{
				ComponentBody componentBody2 = ComponentCreature.ComponentBody;
				ComponentBody parentBody = ComponentCreature.ComponentBody.ParentBody;
				if (Vector3.DistanceSquared(parentBody.Position + Vector3.Transform(componentBody2.ParentBodyPositionOffset, parentBody.Rotation), componentBody2.Position) > 64f)
					m_outOfMountTime += dt;
				else
					m_outOfMountTime = 0f;
				ComponentHealth componentHealth = mount.Entity.FindComponent<ComponentHealth>();
				if (m_outOfMountTime > 0.1f || (componentHealth != null && componentHealth.Health <= 0f && componentHealth.FallResilience < 1e8f) || (ComponentCreature.ComponentHealth.Health <= 0f && ComponentCreature.ComponentBody.BoxSize.X > 0.1f))
					StartDismounting();
				ComponentCreature.ComponentBody.ParentBodyPositionOffset = mount.MountOffset + m_riderOffset;
				ComponentCreature.ComponentBody.ParentBodyRotationOffset = Quaternion.Identity;
			}
		}
	}
}