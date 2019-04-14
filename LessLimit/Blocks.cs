namespace Game
{
	public class CollapsingWoodBlock : WoodBlock
	{
		public enum WoodType
		{
			Wood,
			DeadWood = 4,
			LightWood,
			HardWood
		}
		public override string GetDisplayName(SubsystemTerrain subsystemTerrain, int value)
		{
			return DefaultDisplayName.Replace("Wood", GetWoodType(value).ToString());
		}
		public static WoodType GetWoodType(int value)
		{
			return value < 4 ? WoodType.Wood : (WoodType)value;
		}
	}
	public class StickBlock : CubeBlock
	{
		public enum StickType
		{
			Stick,
			HardStick = 33,
			Trunnel
		}
		public override string GetDisplayName(SubsystemTerrain subsystemTerrain, int value)
		{
			return GetStickType(value).ToString();
		}
		public static StickType GetStickType(int value)
		{
			return value < 33 ? StickType.Stick : (StickType)value;
		}
	}
	public class BombBlock : Game.BombBlock
	{
		public new const int Index = 201;
		public enum BombType
		{
			Bomb,
			TNT = 5,
			TimeBomb,
			Amorce,
			FlourBomb,
			ABomb,
			NBomb
		}
		public override string GetDisplayName(SubsystemTerrain subsystemTerrain, int value)
		{
			return GetBombType(value).ToString();
		}
		public static BombType GetBombType(int value)
		{
			return value < 5 ? BombType.Bomb : (BombType)value;
		}
	}
	public class WaterBlock : Game.WaterBlock
	{
		public new const int Index = 18;
		public enum WaterType
		{
			Water,
			DrinkingWater = 33,	//饮用水
			MineralWater,		//矿泉水
			SodaWater,			//苏打水
			Seawater,			//海水
			Brine,				//盐水
			Sewage,				//污水
		}
		public override string GetDisplayName(SubsystemTerrain subsystemTerrain, int value)
		{
			return GetWaterType(value).ToString();
		}
		public static WaterType GetWaterType(int value)
		{
			return value < 33 ? WaterType.Water : (WaterType)value;
		}
	}
}