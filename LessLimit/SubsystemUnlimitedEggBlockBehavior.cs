using Engine;
using GameEntitySystem;

namespace Game
{
	public class SubsystemUnlimitedEggBlockBehavior : SubsystemEggBlockBehavior
	{
		public override bool OnHitAsProjectile(CellFace? cellFace, ComponentBody componentBody, WorldItem worldItem)
		{
			int data = Terrain.ExtractData(worldItem.Value);
			bool isCooked = EggBlock.GetIsCooked(data);
			bool isLaid = EggBlock.GetIsLaid(data);
			if (!isCooked && (m_subsystemGameInfo.WorldSettings.GameMode == GameMode.Creative || m_random.UniformFloat(0f, 1f) <= (isLaid ? 0.2f : 1f)))
			{
				EggBlock.EggType eggType = m_eggBlock.GetEggType(data);
				Entity entity = DatabaseManager.CreateEntity(Project, eggType.TemplateName, true);
				entity.FindComponent<ComponentBody>(true).Position = worldItem.Position;
				entity.FindComponent<ComponentBody>(true).Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, m_random.UniformFloat(0f, 6.28318548f));
				entity.FindComponent<ComponentSpawn>(true).SpawnDuration = 0.25f;
				Project.AddEntity(entity);
			}
			return true;
		}
	}
}
