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
	public class FileEntry
	{
		public string Filename;
		public Stream Stream;
	}
	public static class ModsManager
	{
		static List<Assembly> loadedAssemblies;
		static List<ModInfo> loadedMods;
		static string extension;

		public static HashSet<string> DisabledMods;
		public static bool ReadZip, ReadSubfolders;
		public static IEnumerable<string> Files, Directories;
		public static string CacheDir;

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
			ReadZip = true;
			ReadSubfolders = true;
			try
			{
				Files = Directory.EnumerateFiles(ContentManager.Path);
				var str = Path.Combine(ContentManager.Path, "mods.cfg");
				if (File.Exists(str))
					using (var reader = new StreamReader(str))
					{
						if ((str = reader.ReadLine()) != "Version 1.0" || (str = reader.ReadLine()) == null)
							throw new NotSupportedException();
						ReadZip = str.Contains("ReadZip");
						ReadSubfolders = str.Contains("ReadSubfolders");
						do
							if ((str = reader.ReadLine()) == null)
								throw new NotSupportedException();
						while (str != "Disabled:");
						while (!(str = reader.ReadLine()).StartsWith("Last succeed:") && str != null)
							DisabledMods.Add(str);
					}
				if (ReadZip)
					Directory.CreateDirectory(CacheDir = Path.Combine(ContentManager.Path, "Cache"));
				if (ReadSubfolders)
					Directories = Directory.EnumerateDirectories(ContentManager.Path);
				IEnumerator<FileEntry> enumerator = GetEntries(".dll").GetEnumerator();
				while (enumerator.MoveNext())
				{
					str = enumerator.Current.Filename;
					try
					{
						LoadMod(Assembly.LoadFrom(Path.Combine(ContentManager.Path, str), null));
					}
					catch (Exception e)
					{
						Log.Warning(string.Format("Loading mod \"{0}\" failed: {1}", str, e));
					}
				}
			}
			catch (Exception e)
			{
				ExceptionManager.ReportExceptionToUser("Initialize failed.", e);
			}
			Log.Information(string.Format("Loaded {0} dlls ({1} mods)", loadedAssemblies.Count, loadedMods.Count));
			Window.Deactivated += SaveConfig;
		}

		public static void LoadMod(Assembly asm)
		{
			if(DisabledMods.Contains(asm.ToString()))
				return;
			Type attr = typeof(PluginLoaderAttribute);
			var types = asm.GetTypes();
			for (int i = 0; i < types.Length; i++)
			{
				var pluginLoaderAttribute = (PluginLoaderAttribute)Attribute.GetCustomAttribute(types[i], attr);
				if (pluginLoaderAttribute != null)
				{
					MethodInfo m;
					if ((m = types[i].GetMethod("Initialize", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)) != null)
						m.Invoke(Activator.CreateInstance(types[i]), null);
					loadedMods.Add(pluginLoaderAttribute.ModInfo);
				}
			}
			loadedAssemblies.Add(asm);
		}

		public static void SaveConfig()
		{
			var writer = new StreamWriter(Path.Combine(ContentManager.Path, "mods.cfg"), false);
			try
			{
				writer.WriteLine("Version 1.0");
				writer.Write("Flags:");
				if (ReadZip) writer.Write(" ReadZip");
				if (ReadSubfolders) writer.Write(" ReadSubfolders");
				writer.WriteLine();
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
			catch (Exception e)
			{
				Log.Warning(e);
			}
			finally
			{
				if (writer != null)
					writer.Close();
			}
		}
/*
		public static List<string> Check(string file)
		{
			using (var reader = new StreamReader(Path.Combine(ContentManager.Path, file), false))
			{
				string line;
				var list = new List<string>();
				do
					if ((line = reader.ReadLine()) == null)
						return list;
				while (line != "FullName\tLastModify");
				var notExist = new DateTime(1601, 1, 1).ToLocalTime();
				while ((line = reader.ReadLine()).Length != 0)
				{
					var name = line.Remove(line.IndexOf('\t'));
					var time = File.GetLastWriteTimeUtc(name);
					if (time != notExist && time.ToString() != line.Substring(line.IndexOf('\t') + 1))
						list.Add(name);
				}
				return list;
			}
		}
*/
		public static bool IsTargetFile(string name)
		{
			return Storage.GetExtension(name).Equals(extension, StringComparison.OrdinalIgnoreCase);
		}

		public static List<string> GetFiles(string ext, IEnumerable<string> files = null)
		{
			extension = ext;
			return new List<string>((files ?? Files).Where(IsTargetFile));
		}

		public static List<FileEntry> GetEntriesInZip(string ext, bool toExtract = false)
		{
			var list = new List<FileEntry>();
			var enumerator = GetFiles(".zip").GetEnumerator();
			extension = ext;
			while (enumerator.MoveNext())
			{
				using (ZipArchive zipArchive = ZipArchive.Open(File.OpenRead(enumerator.Current)))
				{
					var enumerator2 = zipArchive.ReadCentralDir().GetEnumerator();
					while (enumerator2.MoveNext())
					{
						ext = enumerator2.Current.FilenameInZip;
						if (IsTargetFile(ext))
						{
							var entry = new FileEntry();
							Stream stream;
							if (toExtract)
								stream = File.Open(entry.Filename = Path.Combine(CacheDir, ext), FileMode.Create);
							else
							{
								stream = new MemoryStream();
								entry.Filename = Path.Combine(enumerator.Current, ext);
							}
							zipArchive.ExtractFile(enumerator2.Current, stream);
							stream.Position = 0L;
							if (toExtract)
								stream.Close();
							else
							{
								entry.Stream = stream;
								list.Add(entry);
							}
						}
					}
				}
			}
			return list;
		}

		public static List<FileEntry> GetEntries(string ext)
		{
			var list = new List<FileEntry>();
			IEnumerator<string> enumerator = GetFiles(ext).GetEnumerator();
			FileEntry entry;
			while (enumerator.MoveNext())
			{
				entry = new FileEntry();
				entry.Filename = Storage.GetFileName(enumerator.Current);
				entry.Stream = File.OpenRead(enumerator.Current);
				list.Add(entry);
			}
			if (ReadZip)
				list.AddRange(GetEntriesInZip(ext, ext == ".dll"));
			if (ReadSubfolders)
			{
				enumerator = Directories.GetEnumerator();
				while (enumerator.MoveNext())
				{
					var enumerator2 = GetFiles(ext, Directory.EnumerateFiles(enumerator.Current)).GetEnumerator();
					string dirname = enumerator.Current;
					while (enumerator2.MoveNext())
					{
						entry = new FileEntry();
						entry.Filename = Path.Combine(dirname, enumerator2.Current);
						entry.Stream = File.OpenRead(enumerator2.Current);
						list.Add(entry);
					}
				}
			}
			return list;
		}
	}
}