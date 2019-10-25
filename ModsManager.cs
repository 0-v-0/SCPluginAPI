using Engine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Game
{
	/// <summary>
	/// Mod信息
	/// </summary>
	[Serializable]
	public struct ModInfo : IEquatable<ModInfo>
	{
		/// <summary>
		/// 名称
		/// </summary>
		public string Name;

		/// <summary>
		/// 描述
		/// </summary>
		public string Description;

		/// <summary>
		/// 版本
		/// </summary>
		public uint Version;

		/// <summary>
		/// 适用的sc版本
		/// </summary>
		public string ScVersion;

		/// <summary>
		/// url
		/// </summary>
		public string Url;

		/// <summary>
		/// 更新链接
		/// </summary>
		public string UpdateUrl;

		/// <summary>
		/// 作者名单
		/// </summary>
		public string AuthorList;

		/// <summary>
		/// credits
		/// </summary>
		public string Credits;

		/// <summary>
		/// logo
		/// </summary>
		public string Logo;

		/// <summary>
		/// 截图
		/// </summary>
		public string Screenshots;

		/// <summary>
		/// 父集
		/// </summary>
		public string Parent;

		/// <summary>
		/// 依赖项
		/// </summary>
		public string Dependency;

		/// <summary>
		/// 子集
		/// </summary>
		public string Dependants;

		/// <summary>
		/// 使用依赖项信息
		/// </summary>
		public bool UseDependencyInfo;

		/// <summary>
		/// 默认构造函数
		/// </summary>
		/// <param name="name">名称</param>
		/// <param name="description">描述</param>
		/// <param name="version">版本</param>
		/// <param name="scversion">适用的sc版本</param>
		/// <param name="url">url</param>
		/// <param name="updateUrl">更新链接</param>
		/// <param name="authorList">作者名单</param>
		/// <param name="credits">credits</param>
		/// <param name="logo">logo</param>
		/// <param name="screenshots">截图</param>
		/// <param name="parent">父集</param>
		/// <param name="dependency">依赖项</param>
		/// <param name="dependants">子集</param>
		/// <param name="usedependencyInfo">使用依赖项信息</param>
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
		protected readonly ModInfo info;

		/// <summary>
		/// <see cref="Game.ModInfo"/>
		/// </summary>
		public ModInfo ModInfo { get { return info; } }

		/// <summary>
		/// 完整
		/// </summary>
		/// <param name="name">名称</param>
		/// <param name="description">描述</param>
		/// <param name="version">版本</param>
		/// <param name="scversion">适用的sc版本</param>
		/// <param name="url">url</param>
		/// <param name="updateUrl">更新链接</param>
		/// <param name="authorList">作者名单</param>
		/// <param name="credits">credits</param>
		/// <param name="logo">logo</param>
		/// <param name="screenshots">截图</param>
		/// <param name="parent">父集</param>
		/// <param name="dependency">依赖项</param>
		/// <param name="dependants">子集</param>
		/// <param name="usedependencyInfo">使用依赖项信息</param>
		public PluginLoaderAttribute(string name, string description, uint version, string scversion, string url, string updateUrl, string authorList, string credits, string logo, string screenshots, string parent, string dependency = null, string dependants = null, bool usedependencyInfo = false)
		{
			info = new ModInfo(name, description, version, scversion, url, updateUrl, authorList, credits, logo, screenshots, parent, dependency, dependants, usedependencyInfo);
		}

		/// <summary>
		/// 使用依赖项信息
		/// </summary>
		/// <param name="name">名称</param>
		/// <param name="description">描述</param>
		/// <param name="version">版本</param>
		public PluginLoaderAttribute(string name, string description, uint version)
		{
			info.Name = name;
			info.Description = description;
			info.Version = version;
		}
	}

	/// <summary>
	/// 文件条目
	/// </summary>
	public class FileEntry
	{
		/// <summary>
		/// 文件名
		/// </summary>
		public string Filename;

		/// <summary>
		/// 文件流
		/// </summary>
		public Stream Stream;
	}

	/// <summary>
	/// Mods Manager
	/// </summary>
	public static class ModsManager
	{
		private static List<Assembly> loadedAssemblies;

		/// <summary>
		/// Extension name.
		/// </summary>
		public static string Extension;

		/// <summary>
		/// File whose name is in the set won't be loaded.
		/// </summary>
		public static HashSet<string> DisabledMods;

		/// <summary>
		/// Cached Mods
		/// </summary>
		public static HashSet<string> CachedMods;

		/// <summary>
		/// If
		/// </summary>
		public static bool ReadZip, AutoCleanCache;

		/// <summary>
		/// The maximum search pepth, default value is 3.
		/// </summary>
		public static int SearchDepth;

		/// <summary>
		/// The
		/// </summary>
		public static List<string> Files, Directories;

		/// <summary>
		/// Archives
		/// </summary>
		public static Dictionary<string, ZipArchive> Archives;

		/// <summary>
		/// The path of cache directory or <c>null</c> if not used.
		/// </summary>
		public static string CacheDir;

		/// <summary>
		/// Error handler
		/// </summary>
		public static Action<FileEntry, Exception> ErrorHandler;

		/// <summary>
		/// Call after config saved
		/// </summary>
		public static Action<StreamWriter> ConfigSaved;

		/// <summary>
		/// Call before <see cref="Initialize"/> returns.
		/// </summary>
		public static Action Initialized;

		/// <summary>
		/// ModInfo of loaded Mod
		/// </summary>
		public static List<ModInfo> LoadedMods;
/*
		/// <summary>
		/// Loaded Assemblies
		/// </summary>
		public static Dictionary<string, AppDomain> LoadedAssemblies;
*/
		public static void Initialize()
		{
			loadedAssemblies = new List<Assembly>();
			LoadedMods = new List<ModInfo>();
#if ENV_ANDROID
			CachedMods = new HashSet<string>();
#endif
			DisabledMods = new HashSet<string>();
			Files = new List<string>();
			Directories = new List<string>();
			LabelWidget.Strings = new Dictionary<string, string>();
			ReadZip = true;
			AutoCleanCache = true;
			SearchDepth = 3;
			ErrorHandler = LogException;
			try
			{
				var str = Combine(ContentManager.Path, "mods.cfg");
				if (File.Exists(str))
					using (var reader = new StreamReader(str))
						if (string.Equals(reader.ReadLine(), "Ver 1.1") && (str = reader.ReadLine()) != null)
						{
							ReadZip = str.Contains(nameof(ReadZip));
							AutoCleanCache = str.Contains(nameof(AutoCleanCache));
							if (!int.TryParse(reader.ReadLine(), out SearchDepth))
								SearchDepth = 3;
							do
								if ((str = reader.ReadLine()) == null)
									goto loadmods;
							while (!string.Equals(str, "Disabled:"));
							while ((str = reader.ReadLine()) != null && !string.Equals(str, "Last loaded mods:"))
								DisabledMods.Add(str);
							do
								if ((str = reader.ReadLine()) == null)
									goto loadmods;
							while (!string.Equals(str, "File\tModifyTime"));
							var notExist = new DateTime(1601, 1, 1).ToLocalTime();
							while ((str = reader.ReadLine()) != null && str.Length != 0)
							{
#if ENV_ANDROID
								var name = str.
#if Bugs
									Substring(0,
#else
									Remove(
#endif
									str.IndexOf('\t'));
								var time = File.GetLastWriteTimeUtc(Combine(ContentManager.Path, name));
								if (time != notExist && time.ToString() == str.Substring(str.IndexOf('\t') + 1))
									CachedMods.Add(name);
#endif
							}
						}
				loadmods:
				EnumerateDirectory(ContentManager.Path, Files, Directories, SearchDepth);
				if (ReadZip)
				{
					Archives = new Dictionary<string, ZipArchive>();
					var enumerator2 = GetFiles(".zip").GetEnumerator();
					while (enumerator2.MoveNext())
					{
						str = enumerator2.Current;
						Archives.Add(str, ZipArchive.Open(File.OpenRead(str)));
					}
				}
				var enumerator = GetEntries(".dll").GetEnumerator();
				while (enumerator.MoveNext())
				{
					//str = enumerator.Current.Filename;
					//var domain = AppDomain.CreateDomain(str);
					//Assembly asm;
					try
					{
#if ENV_ANDROID
						//asm = domain.Load(str);
						LoadMod(Assembly.LoadFrom(enumerator.Current.Filename, null));
#else
						var buf = new byte[enumerator.Current.Stream.Length];
						enumerator.Current.Stream.Read(buf, 0, buf.Length);
						//asm = domain.Load(buf);
						LoadMod(Assembly.Load(buf));
#endif
						//LoadMod(asm);
						//LoadedAssemblies.Add(str, domain);
					}
					catch (Exception e)
					{
						ErrorHandler(enumerator.Current, e);
					}
				}
			}
			catch (Exception e)
			{
				ExceptionManager.ReportExceptionToUser("Initialize failed.", e);
			}
			Log.Information(string.Format("Found {0} files, loaded {1} dlls ({2} mods)", Files.Count, loadedAssemblies.Count, LoadedMods.Count));
			Initialized?.Invoke();
			SaveConfig();
#if ENV_ANDROID
			if (AutoCleanCache)
				CleanCache();
#endif
		}

		/// <summary>
		/// Default error handler
		/// </summary>
		/// <param name="file">filename</param>
		/// <param name="ex">the caught exception</param>
		public static void LogException(FileEntry file, Exception ex)
		{
			Log.Warning("Loading \"" + file.Filename.Substring(ContentManager.Path.Length + 1) + "\" failed: " + ex.ToString());
			file.Stream.Close();
		}

		/// <summary>
		/// Load a mod
		/// </summary>
		/// <param name="asm">Assembly</param>
		/// <exception cref="ReflectionTypeLoadException"></exception>
		public static void LoadMod(Assembly asm)
		{
			var attr = typeof(PluginLoaderAttribute);
			var types = asm.GetTypes();
			for (int i = 0; i < types.Length; i++)
			{
				var pluginLoaderAttr = (PluginLoaderAttribute)Attribute.GetCustomAttribute(types[i], attr);
				if (pluginLoaderAttr != null)
				{
					MethodInfo m;
					if ((m = types[i].GetMethod("Initialize", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)) != null)
						m.Invoke(Activator.CreateInstance(types[i]), null);
					LoadedMods.Add(pluginLoaderAttr.ModInfo);
				}
			}
			loadedAssemblies.Add(asm);
		}
/*
		/// <summary>
		/// Unload a mod
		/// </summary>
		/// <param name="name"></param>
		/// <exception cref="Exception"></exception>
		/// <exception cref="ArgumentNullException"></exception>
		public static void Unload(string name)
		{
			if (LoadedAssemblies.TryGetValue(name, out AppDomain domain))
			{
				AppDomain.Unload(domain);
				LoadedAssemblies.Remove(name);
			}
		}
*/
		/// <summary>
		/// Save the config
		/// </summary>
		public static void SaveConfig()
		{
			StreamWriter writer = null;
			try
			{
				writer = new StreamWriter(
#if ENV_ANDROID
					File.Open(Combine(ContentManager.Path, "mods.cfg"), FileMode.Create)
#else
					Combine(ContentManager.Path, "mods.cfg"), false
#endif
					);
				writer.WriteLine("Ver 1.1");
				writer.Write("Flags:");
				if (ReadZip) writer.Write(" " + nameof(ReadZip));
				if (AutoCleanCache) writer.Write(" " + nameof(AutoCleanCache));
				writer.WriteLine();
				writer.WriteLine("SearchDepth:" + SearchDepth.ToString());
				writer.WriteLine("Disabled:");
				for (var enumerator = DisabledMods.GetEnumerator(); enumerator.MoveNext();)
					writer.WriteLine(enumerator.Current);
				writer.WriteLine("Last loaded mods:");
				writer.WriteLine("File\tModifyTime");
#if ENV_ANDROID
				for (var enumerator = loadedAssemblies.GetEnumerator(); enumerator.MoveNext();)
					writer.WriteLine(enumerator.Current.Location.Substring(ContentManager.Path.Length + 1) + '\t' + File.GetLastWriteTimeUtc(enumerator.Current.Location));
#endif
				ConfigSaved?.Invoke(writer);
			}
			catch (Exception e)
			{
				Log.Warning("Saving config failed: " + e);
			}
			finally
			{
				writer?.Close();
			}
		}

		/// <summary>
		/// Enumerate the directory
		/// </summary>
		/// <param name="dirName"></param>
		/// <param name="files"></param>
		/// <param name="directories"></param>
		/// <param name="searchDepth">The maximum search pepth</param>
		/// <exception cref="ArgumentException"></exception>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="DirectoryNotFoundException"></exception>
		/// <exception cref="IOException"></exception>
		/// <exception cref="UnauthorizedAccessException"></exception>
		public static void EnumerateDirectory(string dirName, ICollection<string> files = null, ICollection<string> directories = null, int searchDepth = int.MaxValue)
		{
			var q = new Queue<KeyValuePair<string, int>>();
			q.Enqueue(new KeyValuePair<string, int>(dirName, 0));
			try
			{
				while (q.Count > 0)
				{
					var t = q.Dequeue();
					if (t.Value < searchDepth)
					{
						IEnumerator<string> i;
						for (i = Directory.EnumerateDirectories(t.Key).GetEnumerator(); i.MoveNext();)
						{
							q.Enqueue(new KeyValuePair<string, int>(i.Current, t.Value + 1));
							directories?.Add(i.Current);
						}
						if (files != null)
							for (i = Directory.EnumerateFiles(t.Key).GetEnumerator(); i.MoveNext();)
								files.Add(i.Current);
					}
				}
			}
			catch (Exception ex)
			{
				Log.Error("Error enumerating files/directories: " + ex);
			}
		}

		/// <summary>
		/// Delete the cache directory if exists
		/// </summary>
		public static void CleanCache()
		{
#if ENV_ANDROID
			if (!Directory.Exists(CacheDir))
				return;
			var files = new List<string>();
			var directories = new List<string>();
			EnumerateDirectory(CacheDir, files, directories, SearchDepth);
			var enumerator = files.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
					File.Delete(enumerator.Current);
				enumerator = directories.GetEnumerator();
				while (enumerator.MoveNext())
					Directory.Delete(enumerator.Current);
				Directory.Delete(CacheDir);
			}
			catch (Exception e)
			{
				Log.Error("Error deleting " + enumerator.Current + ": " + e.ToString());
			}
#endif
		}

		/// <summary>
		/// Is Target File
		/// </summary>
		/// <param name="name">filename</param>
		/// <returns><c>True</c> if the extension name matches target extension, otherwise <c>False</c>.</returns>
		/// <exception cref="NullReferenceException"></exception>
		public static bool IsTargetFile(string name)
		{
			return Storage.GetExtension(name).Equals(Extension, StringComparison.OrdinalIgnoreCase);
		}

		/// <summary>
		/// Get files with specified extension name
		/// </summary>
		/// <param name="ext">Extension name</param>
		/// <param name="files"></param>
		/// <returns>Files with specified extension name</returns>
		public static IEnumerable<string> GetFiles(string ext, IEnumerable<string> files = null)
		{
			Extension = ext;
			return (files ?? Files).Where(IsTargetFile);
		}

		/// <summary>
		/// Combine two paths quickly without checking
		/// </summary>
		/// <param name="path1"></param>
		/// <param name="path2"></param>
		/// <returns>The combined path</returns>
		/// <exception cref="NullReferenceException"></exception>
		public static string Combine(string path1, string path2)
		{
			var c = path1[path1.Length - 1];
			return path1 + (c != Path.DirectorySeparatorChar && c != Path.AltDirectorySeparatorChar && c != Path.VolumeSeparatorChar
				? char.ToString(Path.DirectorySeparatorChar) + path2
				: path2);
		}

		/// <summary>
		/// Get all entries with specified extension name.
		/// </summary>
		/// <param name="ext">extension name</param>
		/// <returns>an empty list if <paramref name="ext"/> is null</returns>
		/// <exception cref="IOException"></exception>
		/// <exception cref="UnauthorizedAccessException"></exception>
		public static List<FileEntry> GetEntries(string ext)
		{
			var list = new List<FileEntry>();
			FileEntry entry;
			if (ReadZip)
			{
				Extension = ext;
#if ENV_ANDROID
				bool extract = ".dll".Equals(ext, StringComparison.OrdinalIgnoreCase);
#endif
				//Extension = ext;
				var enumerator = Archives.GetEnumerator();
				while (enumerator.MoveNext())
				{
					var zipArchive = enumerator.Current.Value;
					var enumerator2 = zipArchive.ReadCentralDir().GetEnumerator();
					while (enumerator2.MoveNext())
					{
						ext = enumerator2.Current.FilenameInZip;
						if (IsTargetFile(ext) && !DisabledMods.Contains(Storage.GetFileName(ext)))
						{
							entry = new FileEntry();
							Stream stream;
#if ENV_ANDROID
							if (extract)
							{
								if (CacheDir == null)
									Directory.CreateDirectory(CacheDir = Combine(ContentManager.Path, "Cache"));
								stream = File.Open(entry.Filename = Combine(CacheDir, ext), FileMode.OpenOrCreate);
								CachedMods.Add(ext);
							}
							else
#endif
							{
								stream = new MemoryStream();
								entry.Filename = Combine(enumerator.Current.Key, ext);
							}
							zipArchive.ExtractFile(enumerator2.Current, stream);
							stream.Position = 0L;
							entry.Stream = stream;
							list.Add(entry);
						}
					}
				}
			}
			var enumerator3 = GetFiles(Extension).GetEnumerator();
			while (enumerator3.MoveNext())
			{
				ext = enumerator3.Current;
#if ENV_ANDROID
				if (CachedMods.Contains(Storage.GetFileName(ext))) continue;
#endif
				entry = new FileEntry();
				entry.Filename = ext;
				entry.Stream = File.OpenRead(ext);
				list.Add(entry);
			}
			return list;
		}
	}
}