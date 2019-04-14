using System;
using System.Text;
using Engine;

namespace Game
{
	public class ChestData : IEditableItemData
	{
		public DynamicArray<int> Data = new DynamicArray<int>();

		public IEditableItemData Copy()
		{
			return new MemoryBankData
			{
				Data = new DynamicArray<int>(Data),
				LastOutput = LastOutput
			};
		}

		public void LoadString(string data)
		{
			string[] array = data.Split(new []{';'}, StringSplitOptions.RemoveEmptyEntries);
			if (array.Length >= 1)
			{
				string text = array[0];
				text = text.TrimEnd('0');
				Data.Clear();
				for (int i = 0; i < MathUtils.Min(text.Length, 256); i++)
				{
					int num = m_hexChars.IndexOf(char.ToUpperInvariant(text[i]));
					if (num < 0)
						num = 0;
					Data.Add(num);
				}
			}
		}

		public string SaveString()
		{
			var stringBuilder = new StringBuilder();
			int num = 0;
			for (int i = 0; i < Data.Count; i++)
				if (Data.Array[i] != 0)
					num = i + 1;
			for (int j = 0; j < num; j++)
			{
				int index = MathUtils.Clamp(Data.Array[j], 0, 15);
				stringBuilder.Append(m_hexChars[index]);
			}
			return stringBuilder.ToString();
		}
	}

	public class SubsystemUnlimitedChestBlockBehavior : SubsystemEditableItemBehavior<ChestData>
	{
		public override int[] HandledBlocks
		{
			get { return new int[1] { 266 }; }
		}
		public SubsystemUnlimitedChestBlockBehavior() : base(UnlimitedChestBlock.Index)
		{
		}
		public new int StoreItemDataAtUniqueId(ChestData t)
		{
			int num = FindFreeItemId();
			m_itemsData[num] = t;
			return num;
		}
		public override void OnItemHarvested(int x, int y, int z, int blockValue, ref BlockDropValue dropValue, ref int newBlockValue)
		{
			ChestData blockData = GetBlockData(new Point3(x, y, z));
			if (blockData != null)
			{
				int num = FindFreeItemId();
				m_itemsData.Add(num, (ChestData)blockData.Copy());
				dropValue.Value = Terrain.ReplaceData(dropValue.Value, num);
			}
		}
		public new int FindFreeItemId()
		{
			for (int i = 1; i < 65536; i++)
				if (!m_itemsData.ContainsKey(i))
					return i;
			return 0;
		}
	}

	public class UnlimitedChestBlock : ChestBlock
	{
		public new const int Index = 266;

		public override BlockPlacementData GetPlacementValue(SubsystemTerrain subsystemTerrain, ComponentMiner componentMiner, int value, TerrainRaycastResult raycastResult)
		{
			Vector3 forward = Matrix.CreateFromQuaternion(componentMiner.ComponentCreature.ComponentCreatureModel.EyeRotation).Forward;
			float num = Vector3.Dot(forward, Vector3.UnitZ);
			float num2 = Vector3.Dot(forward, Vector3.UnitX);
			float num3 = Vector3.Dot(forward, -Vector3.UnitZ);
			float num4 = Vector3.Dot(forward, -Vector3.UnitX);
			int data = 0;
			if (num == MathUtils.Max(num, num2, num3, num4))
				data = 2;
			else if (num2 == MathUtils.Max(num, num2, num3, num4))
				data = 3;
			else if (num3 == MathUtils.Max(num, num2, num3, num4))
				data = 0;
			else if (num4 == MathUtils.Max(num, num2, num3, num4))
				data = 1;
			return new BlockPlacementData
			{
				Value = Terrain.MakeBlockValue(Index, 0, TrapdoorBlock.SetRotation(value, data)),
				CellFace = raycastResult.CellFace
			};
		}
	}
}
