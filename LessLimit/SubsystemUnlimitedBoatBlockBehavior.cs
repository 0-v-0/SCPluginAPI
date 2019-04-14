using Engine;
using GameEntitySystem;

namespace Game
{
	public class SubsystemUnlimitedBoatBlockBehavior : SubsystemBoatBlockBehavior
	{
		public override bool OnUse(Vector3 start, Vector3 direction, ComponentMiner componentMiner)
		{
			if (Terrain.ExtractContents(componentMiner.ActiveBlockValue) == 178)
			{
				TerrainRaycastResult? nullable = componentMiner.PickTerrainForDigging(start, direction);
				if (nullable.HasValue)
				{
					Vector3 vector = nullable.Value.RaycastStart + Vector3.Normalize(nullable.Value.RaycastEnd - nullable.Value.RaycastStart) * nullable.Value.Distance;
					Entity entity = DatabaseManager.CreateEntity(Project, "Boat", true);
					entity.FindComponent<ComponentFrame>(true).Position = vector;
					entity.FindComponent<ComponentFrame>(true).Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, m_random.UniformFloat(0f, 6.28318548f));
					entity.FindComponent<ComponentSpawn>(true).SpawnDuration = 0f;
					Project.AddEntity(entity);
					componentMiner.RemoveActiveTool(1);
					m_subsystemAudio.PlaySound("Audio/BlockPlaced", 1f, 0f, vector, 3f, true);
					return true;
				}
			}
			return false;
		}
	}
}