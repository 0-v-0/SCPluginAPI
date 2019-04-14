using Engine.Serialization;
using GameEntitySystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using TemplatesDatabase;

namespace Game
{
	public class SubsystemCustomTerrain : Subsystem
	{
		int A = 1;
		WorldSettings m_worldSettings;
		/// <summary>
		/// 读取键值对文件
		/// </summary>
		/// <param name="dict">读取到的数据</param>
		/// <param name="stream">要读取的流</param>
		/// <param name="separator">分隔符</param>
		/// <param name="commentchar">注释符</param>
		public static void ReadKeyValueFile(Dictionary<string, string> dict, Stream stream, char separator = '=', char commentchar = '#')
		{
			var reader = new StreamReader(stream);
			while (true)
			{
				var line = reader.ReadLine();
				if (line == null) return;
				if (line[0] != commentchar)
				{
					int i = line.IndexOf(separator);
					if (i >= 0)
						dict[line.Substring(0, i)] = line.Substring(i + 1);
				}
			}
		}
		/*public static void ReadYAML(Dictionary<string, string> dict, Stream stream, char separator = '=', char commentchar = '#')
		{
			var reader = new StreamReader(stream);
			string prefix;
			while (true)
			{
				var line = reader.ReadLine();
				if (line == null) return;
				if (line[0] != commentchar)
				{
					int i;
					for (i = 0; i < line.Length; i++)
					if (line[i] != ' ') break;
					i >>= 1;
					i = line.IndexOf(separator);
					if (i >= 0)
					{
						prefix = line.Substring(0, i);
						dict[prefix] = line.Substring(i + 1);
					}
				}
			}
		}*/
		public override void Load(ValuesDictionary valuesDictionary)
		{
			base.Load(valuesDictionary);
			var subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(true);
			if (!(subsystemTerrain.TerrainContentsGenerator is TerrainContentsGenerator g))
				return;
			var enumerator = ModsManager.GetEntries(".tgc").GetEnumerator();
			var d = new Dictionary<string, string>();
			while (enumerator.MoveNext())
				ReadKeyValueFile(d, enumerator.Current.Stream);
			if (d.Count > 0)
			{
				if (d.TryGetValue("TGType", out string s))
				{
					if (s.Contains("BH") && !(subsystemTerrain.TerrainContentsGenerator is MyTerrainContentsGenerator))
					{
						subsystemTerrain.TerrainContentsGenerator = g = new MyTerrainContentsGeneratorBH2(subsystemTerrain);
					}
					if (s.Contains("Sine") && !(subsystemTerrain.TerrainContentsGenerator is TerrainContentsGeneratorFunc))
					{
						subsystemTerrain.TerrainContentsGenerator = new TerrainContentsGeneratorFunc(subsystemTerrain)
						{
							GetHeight = CalculateHeight
						};
						m_worldSettings = subsystemTerrain.m_subsystemGameInfo.WorldSettings;
					}
					if (s.Contains("Hash49") && !(subsystemTerrain.TerrainContentsGenerator is TerrainContentsGeneratorFunc))
					{
						subsystemTerrain.TerrainContentsGenerator = new TerrainContentsGeneratorFunc(subsystemTerrain)
						{
							GetHeight = CalculateHeight2
						};
						m_worldSettings = subsystemTerrain.m_subsystemGameInfo.WorldSettings;
					}
				}
				var arr = g.GetType().GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				for (int i = 0; i < arr.Length; i++)
				{
					FieldInfo f = arr[i];
					if (d.TryGetValue(f.Name, out s))
						f.SetValue(g, HumanReadableConverter.ConvertFromString(f.FieldType, s));
				}
			}
		}

		public float CalculateHeight(float x, float z)
		{
			return m_worldSettings.TerrainLevel + (float)(Math.Sin(x * .1) + Math.Sin(z * .1) + 1) * 16f;
		}

		public float CalculateHeight2(float x, float z)
		{
			int a = A * 10;
			A = a % 499999999;
			A = A * 10 % 499999999;
			return m_worldSettings.TerrainLevel + (a / 499999999) + A / 499999999 * 10;
		}
	}
}