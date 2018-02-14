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
		public string[] AuthorList;
		public string Credits;
		public string Logo;
		public string[] Screenshots;
		public string Parent;
		public string[] Dependency;
		public string[] Dependants;
		public bool UseDependencyInfo;
		public ModInfo(
			string name,
			string description,
			uint version,
			string scversion,
			string url,
			string updateUrl,
			string[] authorList,
			string credits,
			string logo,
			string[] screenshots,
			string parent,
			string[] dependency,
			string[] dependants = null,
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
			return obj is ModInfo && this.ToString() == ((ModInfo)obj).ToString();
		}
		public bool Equals(ModInfo other)
		{
			return this.ToString() == other.ToString();
		}
		public override int GetHashCode()
		{
			return this.ToString().GetHashCode();
		}
		public override string ToString()
		{
			return string.Format("{0} {1} - {2} ({3})", new object[]
			{
				this.Name,
				this.Version,
				this.Description,
				this.Url
			});
		}
	}
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
	public class PluginLoaderAttribute : Attribute
	{
		protected ModInfo info;
		protected int index;
		public int Index {
			get { return index; }
		}
		public ModInfo ModInfo{ get{ return info ;} }
		public PluginLoaderAttribute(string name, string description, uint version, string scversion, string url, string updateUrl, string[] authorList, string credits, string logo, string[] screenshots, string parent, string[] dependency, int index = 0, string[] dependants = null, bool usedependencyInfo = false)
		{
			this.info = new ModInfo(name, description, version, scversion, url, updateUrl, authorList, credits, logo, screenshots, parent, dependency, dependants, usedependencyInfo);
			this.index = index;
		}

		public PluginLoaderAttribute(string name, string description, uint version, int index)
		{
			this.info.Name = name;
			this.info.Description = description;
			this.info.Version = version;
			this.index = index;
		}
	}

	public static class ModsManager
	{
		private static List<Assembly> loadedAssemblies = new List<Assembly>();
		private static Dictionary<string, int> loadedMods = new Dictionary<string, int>();
		private static List<string> assembliesList;

		public static List<string> DisabledMods;

		private static int loadedModCount;

		public static ReadOnlyList<Assembly> LoadedAssemblies
		{
			get
			{
				return new ReadOnlyList<Assembly>(ModsManager.loadedAssemblies);
			}
		}

		public static Dictionary<string, int> LoadedMods
		{
			get
			{
				return ModsManager.loadedMods;
			}
		}

		public static ReadOnlyList<string> AssemblyNames
		{
			get
			{
				return new ReadOnlyList<string>(ModsManager.assembliesList);
			}
		}

		public static void Initialize()
		{
			ModsManager.LoadMods();
		}

		public static bool IsAssembly(string name)
		{
			return Storage.GetExtension(name).Equals(".dll", StringComparison.OrdinalIgnoreCase);
		}

		public static void LoadMods()
		{
			ModsManager.UpdateAssembliesList();
			try
			{
				foreach (string current in ModsManager.assembliesList)
				{
					try
					{
						ModsManager.LoadMod(Assembly.LoadFrom(current, null));
						Log.Information("Loaded Mod \"{0}\"", new object[]
						{
							current
						});
					}
					catch (Exception ex)
					{
						Log.Warning(string.Format("Mod \"{0}\" could not be loaded. Reason: {1}", current, ex.Message));
					}
				}
			}
			catch (Exception e)
			{
				ExceptionManager.ReportExceptionToUser("Loading Mods failed.", e);
			}
		}

		public static void LoadMod(Assembly asm)
		{
			Type delegateType = typeof(Action);
			Type attr = typeof(PluginLoaderAttribute);
			ModsManager.loadedAssemblies.Add(asm);
			Type[] types = asm.GetTypes();
			for (int i = 0; i < types.Length; i++)
			{
				PluginLoaderAttribute pluginLoaderAttribute = (PluginLoaderAttribute)Attribute.GetCustomAttribute(types[i], attr);
				if (pluginLoaderAttribute != null)
				{
					ModInfo modInfo = pluginLoaderAttribute.ModInfo;
					if (!ModsManager.DisabledMods.Contains(modInfo.Name))
					{
						var c = types[i].GetMethods(BindingFlags.Static)[pluginLoaderAttribute.Index];
						if (c != null)
						{
							c.Invoke(null, null);
							ModsManager.loadedMods.Add(modInfo.Name, ModsManager.loadedModCount++);
						}
					}
				}
			}
		}

		public static void UpdateAssembliesList()
		{
			ModsManager.assembliesList = new List<string>(Directory.EnumerateFiles(ContentManager.Path).Where(new Func<string, bool>(ModsManager.IsAssembly)));
		}
	}
}