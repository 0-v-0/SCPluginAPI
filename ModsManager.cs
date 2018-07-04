using Engine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Game
{
	[Serializable]
	public struct ModInfo : IEquatable<ModInfo>
	{
		public string Name;
		public string Description;
		public uint Version;
		public string ScVersion;
		public string Url;
		public string UpdateUrl;
		public string AuthorList;
		public string Credits;
		public string Logo;
		public string Screenshots;
		public string Parent;
		public string Dependency;
		public string Dependants;
		public bool UseDependencyInfo;
		public ModInfo(
			string name,
			string description,
			uint version,
			string scversion,
			string url,
			string updateUrl,
			string authorList,
			string credits,
			string logo,
			string screenshots,
			string parent,
			string dependency,
			string dependants = null,
			bool usedependencyInfo = false)
			{
				Name = name;
				Description = description;
				Version = version;
				ScVersion = scversion;
				Url = url;
				UpdateUrl = updateUrl;
				AuthorList = authorList;
				Credits = credits;
				Logo = logo;
				Screenshots = screenshots;
				Parent = parent;
				Dependency = dependency;
				Dependants = dependants;
				UseDependencyInfo = usedependencyInfo;
			}

		public override bool Equals(object obj)
		{
			return obj is ModInfo && ToString() == ((ModInfo)obj).ToString();
		}
		public bool Equals(ModInfo other)
		{
			return ToString() == other.ToString();
		}
		//public bool IsNewerThan(ModInfo other)
		//{
		//	return this.Name == other.Name && this.Version > other.Version;
		//}
		public override int GetHashCode()
		{
			return ToString().GetHashCode();
		}
		public override string ToString()
		{
			return string.Format("{0} {1} - {2} ({3})", new object[]
			{
				Name,
				Version,
				Description,
				Url
			});
		}
	}
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
	public class PluginLoaderAttribute : Attribute
	{
		protected ModInfo info;
		public ModInfo ModInfo{ get{ return info; } }
		public PluginLoaderAttribute(string name, string description, uint version, string scversion, string url, string updateUrl, string authorList, string credits, string logo, string screenshots, string parent, string dependency = null, string dependants = null, bool usedependencyInfo = false)
		{
			info = new ModInfo(name, description, version, scversion, url, updateUrl, authorList, credits, logo, screenshots, parent, dependency, dependants, usedependencyInfo);
		}

		public PluginLoaderAttribute(string name, string description, uint version)
		{
			info.Name = name;
			info.Description = description;
			info.Version = version;
		}
	}
	public static class ModsManager
	{
		static List<Assembly> loadedAssemblies;
		static List<ModInfo> loadedMods;
		static string extension;

		public static HashSet<string> DisabledMods;
		public static ReadOnlyList<string> AssembliesList;

		public static ReadOnlyList<ModInfo> LoadedMods
		{
			get
			{
				return new ReadOnlyList<ModInfo>(ModsManager.loadedMods);
			}
		}
		public static ReadOnlyList<Assembly> LoadedAssemblies
		{
			get
			{
				return new ReadOnlyList<Assembly>(ModsManager.loadedAssemblies);
			}
		}

		public static void Initialize()
		{
			loadedAssemblies = new List<Assembly>();
			loadedMods = new List<ModInfo>();
			DisabledMods = new HashSet<string>();
			try
			{
				if (File.Exists("mods.cfg"))
					using (var reader = new StreamReader(Path.Combine(ContentManager.Path, "mods.cfg")))
					{
						var line = reader.ReadLine();
						while (line != "FullName\tLastModify" && line != null)
						{
							DisabledMods.Add(line);
						}
					}
				var enumerator = (AssembliesList = new ReadOnlyList<string>(GetFiles(".dll"))).GetEnumerator();
				while (enumerator.MoveNext())
				{
					try
					{
						LoadMod(Assembly.LoadFrom(enumerator.Current, null));
					}
					catch (Exception ex)
					{
						Log.Warning(string.Format("Loading mod \"{0}\" failed: {1}", enumerator.Current.Substring(ContentManager.Path.Length), ex.Message));
					}
				}
				Log.Information("Loaded {0} dlls ({1} mods)", AssembliesList.Count, loadedMods.Count);
				SaveConfig("mods.cfg");
			}
			catch (Exception e)
			{
				ExceptionManager.ReportExceptionToUser("Loading mods failed.", e);
			}
		}

		public static void LoadMod(Assembly asm)
		{
			if(DisabledMods.Contains(asm.ToString()))
				return;
			Type attr = typeof(PluginLoaderAttribute);
			loadedAssemblies.Add(asm);
			var types = asm.GetTypes();
			for (int i = 0; i < types.Length; i++)
			{
				var pluginLoaderAttribute = (PluginLoaderAttribute)Attribute.GetCustomAttribute(types[i], attr);
				if (pluginLoaderAttribute != null)
				{
					MethodInfo c;
					if ((c = types[i].GetMethod("Initialize", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)) != null)
					{
						c.Invoke(Activator.CreateInstance(types[i]), null);
					}
					loadedMods.Add(pluginLoaderAttribute.ModInfo);
				}
			}
		}

		public static void SaveConfig(string name)
		{
			using (var writer = new StreamWriter(Path.Combine(ContentManager.Path, name), false))
			{
				writer.WriteLine("Disabled:");
				for (var enumerator = DisabledMods.GetEnumerator(); enumerator.MoveNext();)
				{
					writer.WriteLine(enumerator.Current);
				}
				writer.WriteLine("Last succeed:");
				writer.WriteLine("FullName\tLastModify");
				for (var enumerator = loadedAssemblies.GetEnumerator(); enumerator.MoveNext();)
				{
					writer.WriteLine("{0}\t{1}", enumerator.Current, File.GetLastWriteTimeUtc(enumerator.Current.Location));
				}
				writer.WriteLine();
			}
		}

		public static List<string> Check(string file)
		{
			using (var reader = new StreamReader(Path.Combine(ContentManager.Path, file), false))
			{
				string line;
				var list = new List<string>();
				do
				{
					if ((line = reader.ReadLine()) == null)
					{
						return list;
					}
				}
				while (line != "FullName\tLastModify");
				var notExist = new DateTime(1601, 1, 1).ToLocalTime();
				while ((line = reader.ReadLine()).Length != 0)
				{
					var name = line.Remove(line.IndexOf('\t'));
					var time = File.GetLastWriteTimeUtc(name);
					if (time != notExist && time.ToString() != line.Substring(line.IndexOf('\t') + 1))
					{
						list.Add(name);
					}
				}
				return list;
			}
		}

		public static bool IsTargetFile(string name)
		{
			return Storage.GetExtension(name).Equals(extension, StringComparison.OrdinalIgnoreCase);
		}

		public static List<string> GetFiles(string ext)
		{
			extension = ext;
			return new List<string>(Directory.EnumerateFiles(ContentManager.Path).Where(new Func<string, bool>(ModsManager.IsTargetFile)));
		}
	}
}