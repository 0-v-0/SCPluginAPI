using System;
using Engine;
using Engine.Serialization;
using TemplatesDatabase;

namespace Game
{
	public class SubsystemPaintBucketBlockBehavior : SubsystemBlockBehavior
	{
		public override int[] HandledBlocks
		{
			get
			{
				return new int[]
				{
					129
				};
			}
		}
		private static readonly string[] ColorNames = {
			"White",
			"Pale_Cyan",
			"Pink",
			"Pale_Blue",
			"Pale_Yellow",
			"Pale_Green",
			"Salmon",
			"Light_Gray",
			"Gray",
			"Cyan",
			"Purple",
			"Blue",
			"Yellow",
			"Green",
			"Red",
			"Black"
		};

		public override bool OnEditInventoryItem(IInventory inventory, int slotIndex)
		{
			int i = PaintBucketBlock.GetColor(TerrainData.ExtractData(inventory.GetSlotValue(slotIndex)));
			DialogsManager.ShowDialog(new EditRGBColorValueDialog((int)PaintBucketBlock.PaintColors[i].PackedValue, delegate(int newColor)
				PaintBucketBlock.PaintColors[i] = new Color((uint)newColor);));
			return true;
		}

		public override bool OnUse(Vector3 start, Vector3 direction, ComponentMiner componentMiner)
		{
			BodyRaycastResult? bodyRaycastResult = componentMiner.PickBody(start, direction);
			TerrainRaycastResult? terrainRaycastResult = componentMiner.PickTerrainForDigging(start, direction);
			if (bodyRaycastResult.HasValue && (!terrainRaycastResult.HasValue || terrainRaycastResult.Value.Distance > bodyRaycastResult.Value.Distance))
			{
				ComponentHealth componentHealth = bodyRaycastResult.Value.ComponentBody.Entity.FindComponent<ComponentHealth>();
				if (componentHealth == null || componentHealth.Health > 0f)
				{
					Entity entity = this.m_subsystemEntityFactory.CreateEntity(bodyRaycastResult.Value.ComponentBody.Entity.ValuesDictionary.DatabaseObject.Name + "_" + ColorNames[PaintBucketBlock.GetColor(TerrainData.ExtractData(componentMiner.ActiveBlockValue))], false);
					if (entity != null)
					{
						ComponentBody expr_B7 = entity.FindComponent<ComponentBody>(true);
						expr_B7.Position = bodyRaycastResult.Value.ComponentBody.Position;
						expr_B7.Rotation = bodyRaycastResult.Value.ComponentBody.Rotation;
						expr_B7.Velocity = bodyRaycastResult.Value.ComponentBody.Velocity;
						entity.FindComponent<ComponentSpawn>(true).SpawnDuration = 0f;
						base.Project.RemoveEntity(bodyRaycastResult.Value.ComponentBody.Entity);
						base.Project.AddEntity(entity);
						this.m_subsystemAudio.PlaySound("Audio/BlockPlaced", 1f, this.m_random.UniformFloat(-0.1f, 0.1f), start, 1f, true);
						componentMiner.DamageActiveTool(1);
					}
				}
				return true;
			}
			return false;
		}

		protected override void Load(ValuesDictionary valuesDictionary)
		{
			try{
				Color[] colors = HumanReadableConverter.ValuesListFromString<Color>(';', valuesDictionary.GetValue<string>("Colors"));
				string[] names = valuesDictionary.GetValue<string>("Names").Split(new char[]{';'});
				for (int i = 0; i < 16; i++){
					if (colors[i].PackedValue != 0)
						PaintBucketBlock.PaintColors[i] = colors[i];
					PaintBucketBlock.ColorDisplayNames[i] = names[i];
				}
				BlocksManager.Blocks[208].Initialize();
			}catch{}
			base.Load(valuesDictionary);
		}

		protected override void Save(ValuesDictionary valuesDictionary)
		{
			base.Save(valuesDictionary);
			valuesDictionary.SetValue<string>("Colors", HumanReadableConverter.ValuesListToString<Color>(';', PaintBucketBlock.PaintColors));
			valuesDictionary.SetValue<string>("Names", string.Join(";", PaintBucketBlock.ColorDisplayNames));
		}
	}
}
