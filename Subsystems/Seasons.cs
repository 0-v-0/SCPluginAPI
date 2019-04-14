using Engine;
using Engine.Serialization;
using GameEntitySystem;
using System;
using System.Collections.Generic;
using System.Text;
using TemplatesDatabase;

namespace Game
{
	[Serializable]
	public enum Climate
	{
		None,
		Polar, //极地冰原气候
		Tundra, //苔原气候
		Plateau, //高原气候
		ConiferousForest, //亚寒带针叶林气候
		TemperateContinental, //温带大陆性气候
		TemperateMonsoon, //温带季风气候
		Mediterranean, //地中海气候
		TemperateMaritime, //温带海洋性气候
		SubtropicalMonsoon, //亚热带季风气候
		DesertClimate, //热带沙漠气候
		SavannaClimate, //热带草原气候
		TropicalMonsoon, //热带季风气候
		TropicalRainforest, //热带雨林气候
	}

	public class SubsystemSeasons : SubsystemBlockBehavior
	{
		public static Dictionary<Point2, Climate> Chunks = new Dictionary<Point2, Climate>();
		public static Dictionary<Point2, byte[]> Plateaus = new Dictionary<Point2, byte[]>();
		public SubsystemGameInfo SubsystemGameInfo;

		public static Climate[] ClimateTable = new[]
		{
			Climate.Polar,Climate.Polar,Climate.Polar,Climate.Tundra,
			Climate.Polar,Climate.Tundra,Climate.ConiferousForest,Climate.ConiferousForest,
			Climate.TemperateContinental,Climate.TemperateMonsoon,Climate.TemperateMaritime,Climate.TemperateMaritime,
			Climate.TemperateContinental,Climate.TemperateMonsoon,Climate.TemperateMaritime,Climate.TemperateMaritime,
			Climate.TemperateContinental,Climate.Mediterranean,Climate.SubtropicalMonsoon,Climate.SubtropicalMonsoon,
			Climate.DesertClimate,Climate.Mediterranean,Climate.SubtropicalMonsoon,Climate.SubtropicalMonsoon,
			Climate.DesertClimate,Climate.SavannaClimate,Climate.TropicalMonsoon,Climate.TropicalRainforest,
			Climate.DesertClimate,Climate.SavannaClimate,Climate.TropicalMonsoon,Climate.TropicalRainforest,
		};

		public static int[] OffsetTable = new[]{
			0,0,0,0,0,0,0,0,0,0,0,0,
			0,0,0,0,3,0,0,0,0xD,0,0,0,
			0,0,0,0,3,0,0,0,0xD,0,0,0,
			0,0,0,0,0,0,0,0,0,0,0,0,
			0,0,0,1,3,0,0,0,0xD,0xF,0,0,
			0,0,0,0x33,0x22,0,0,0,0,0xEE,0xDD,0,
			0,0,0,0x33,0x23,0x10,0,0,0xF0,0xED,0xDD,0,
			0,0,0,0xE2,0xE1,0x1,0,0,0xF,0x2F,0x2E,0,
			0,0,0,0,0,0,0,0,0,0,0,0,
			0,0,0,0x33,0x11,0,0,0,0,0xFF,0xDD,0,
			0,0,0,0,0,0,0,0,0,0,0,0,
			0,0,0,0x30,0x20,0x10,0,0xF0,0xE0,0xD0,0,0,
			0,0,0,0x30,0x10,0,0,0,0xF0,0xD0,0,0,
			0,0,0,0,0,0,0,0,0,0,0,0,
		};

		public override int[] HandledBlocks => new int[0];

		/*public static Color Lookup(BlockColorsMap map, int shaftValue)
		{
			return map.m_map[ExtractTH(shaftValue)];
		}

		public static void Offset(ref int shaftValue)
		{
			//shaftValue += OffsetTable[(int)ClimateTable[ExtractTH(shaftValue)], ((int)subsystemGameInfo.TotalElapsedGameTime / 1200 & 63) >> 1];
		}

		public static int Lookup(int temperature, int humidity)
		{
			return MathUtils.Clamp(temperature, 0, 15) | MathUtils.Clamp(humidity, 0, 15) << 4;
		}*/

		public static Climate GetClimate(TerrainChunk chunk)
		{
			int temperature = 0, humidity = 0, height = 0;
			for (int i = 16; i-- > 0;)
			{
				for (int j = 16; j-- > 0;)
				{
					int value = chunk.GetShaftValueFast(i, j);
					temperature += Terrain.ExtractTemperature(value);
					humidity += Terrain.ExtractHumidity(value);
					height += Terrain.ExtractTopHeight(value);
				}
			}
			return height > (96 << 8) ? Climate.Plateau : ClimateTable[(humidity >> (8 + 2)) | ((temperature >> (8 + 1)) & 0x7) << 2];
		}

		public static int ExtractTH(int value)
		{
			return (value & 0xFF00) >> 8;
		}

		public static int ReplaceTH(int value, int th)
		{
			return value ^ ((value ^ (th << 8)) & 0xFF00);
		}

		public override void Load(ValuesDictionary valuesDictionary)
		{
			Chunks.Clear();
			Plateaus.Clear();
			base.Load(valuesDictionary);
			SubsystemGameInfo = Project.FindSubsystem<SubsystemGameInfo>(true);
			if (SubsystemGameInfo.WorldSettings.EnvironmentBehaviorMode == EnvironmentBehaviorMode.Static)
				return;
			string value = valuesDictionary.GetValue("Data", string.Empty);
			if (string.IsNullOrEmpty(value))
				return;
			var array = value.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
			for (int i = 0; i < array.Length; i++)
			{
				var arr = array[i].Split('=');
				Point2 key = HumanReadableConverter.ConvertFromString<Point2>(arr[0]);
				if (arr.Length == 3 && arr[2].Length > 0 && arr[2][0] == '3')
				{
					var bytes = Convert.FromBase64String(arr[2].Substring(1));
					if (bytes.Length == 256)
						Plateaus.Add(key, bytes);
					Chunks.Add(key, Climate.Plateau);
				}
				else if (arr.Length > 2 && int.TryParse(arr[1], out int r))
					Chunks.Add(key, (Climate)r);
			}
		}

		public override void Save(ValuesDictionary valuesDictionary)
		{
			base.Save(valuesDictionary);
			if (Project.FindSubsystem<SubsystemGameInfo>(true).WorldSettings.EnvironmentBehaviorMode == EnvironmentBehaviorMode.Static)
				return;
			var sb = new StringBuilder();
			var e = Chunks.GetEnumerator();
			while (e.MoveNext())
			{
				var current = e.Current;
				sb.Append(HumanReadableConverter.ConvertToString(current.Key));
				sb.Append('=');
				sb.Append((int)current.Value);
				if (current.Value == Climate.Plateau && Plateaus.TryGetValue(current.Key, out byte[] arr))
					sb.Append(Convert.ToBase64String(arr));
				sb.Append(';');
			}
			valuesDictionary.SetValue("Data", sb.ToString());
		}

		public override void OnChunkInitialized(TerrainChunk chunk)
		{
			Point2 coords = chunk.Coords;
			if (!Chunks.TryGetValue(coords, out Climate climate))
			{
				climate = GetClimate(chunk);
				if (chunk.IsLoaded || chunk.ModificationCounter > 0)
					Chunks.Add(coords, climate);
			}
			if (climate == Climate.None) return;
			int i;
			if (climate == Climate.Plateau)
			{
				if (Plateaus.TryGetValue(coords, out byte[] arr))
					for (i = 0; i < 256; i++)
						chunk.Shafts[i] = ReplaceTH(chunk.Shafts[i], arr[i]);
				else
				{
					arr = new byte[256];
					for (i = 0; i < 256; i++)
						arr[i] = (byte)ExtractTH(chunk.Shafts[i]);
					Plateaus.Add(coords, arr);
				}
			}
			int month = ((int)SubsystemGameInfo.TotalElapsedGameTime / (1200 << 4) + 2) % 12;
			for (i = 256; i-- > 0;)
			{
				int value = chunk.Shafts[i], offset = OffsetTable[(int)climate * 12 + month];
				chunk.Shafts[i] = ReplaceTH(value, ((Terrain.ExtractHumidity(value) + (offset >> 4)) & 15) << 4 | (Terrain.ExtractTemperature(value) + (offset & 15)) & 15);
			}
		}
	}
}