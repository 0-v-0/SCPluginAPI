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
		static List<Assembly> loadedAssemblies = new List<Assembly>();
		static List<ModInfo> loadedMods = new List<ModInfo>();
		static string extension;

		public static HashSet<string> DisabledMods = new HashSet<string>();
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
			try
			{
				foreach (string current in (AssembliesList = new ReadOnlyList<string>(ModsManager.GetFiles(".dll"))))
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
			Type attr = typeof(PluginLoaderAttribute);
			ModsManager.loadedAssemblies.Add(asm);
			Type[] types = asm.GetTypes();
			for (int i = 0; i < types.Length; i++)
			{
				PluginLoaderAttribute pluginLoaderAttribute = (PluginLoaderAttribute)Attribute.GetCustomAttribute(types[i], attr);
				if (pluginLoaderAttribute != null)
				{
					ModInfo modInfo;
					if (!ModsManager.DisabledMods.Contains((modInfo = pluginLoaderAttribute.ModInfo).Name))
					{
						MethodInfo c;
						if ((c = types[i].GetMethod("Initialize", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)) != null)
						{
							c.Invoke(Activator.CreateInstance(types[i]), null);
							ModsManager.loadedMods.Add(modInfo);
						}
					}
				}
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