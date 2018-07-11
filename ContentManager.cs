using Engine;
using Engine.Content;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using XmlUtilities;

namespace Game
{
	public static class ContentManager
	{
		public static string Path;

		public static void Initialize()
		{
			Directory.CreateDirectory(ContentManager.Path = System.IO.Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, "Survivalcraft", "Mods"));
			//Directory.CreateDirectory(ContentManager.Path = Storage.GetSystemPath("data:Mods"));
			ModsManager.Initialize();
			ContentCache.AddPackage(Storage.OpenFile("app:Content.pak", OpenFileMode.Read));
			var enumerator = (new ReadOnlyList<FileEntry>(ModsManager.GetEntries(".pak"))).GetEnumerator();
			while (enumerator.MoveNext())
			{
				ContentCache.AddPackage(enumerator.Current.Stream);
			}
		}

		public static object Get(string name)
		{
			return ContentCache.Get(name);
		}
	
		public static object Get(Type type, string name)
		{
			if (type == typeof(Subtexture))
				return TextureAtlasManager.GetSubtexture(name);
			if (type == typeof(string) && name.StartsWith("Strings/"))
				return StringsManager.GetString(name.Substring(8));
			object obj = Get(name);
			if (!type.GetTypeInfo().IsAssignableFrom(obj.GetType().GetTypeInfo()))
				throw new InvalidOperationException(string.Format("Content \"{0}\" has type {1}, requested type was {2}", name, obj.GetType().FullName, type.FullName));
			return obj;
		}
	
		public static T Get<T>(string name)
		{
			return (T)Get(typeof(T), name);
		}
	
		public static void Dispose(string name)
		{
			ContentCache.Dispose(name);
		}
	
		public static bool IsContent(object content)
		{
			return ContentCache.IsContent(content);
		}
	
		public static ReadOnlyList<ContentInfo> List()
		{
			return ContentCache.List();
		}
	
		public static ReadOnlyList<ContentInfo> List(string directory)
		{
			return ContentCache.List(directory);
		}

		public static IEnumerable<XElement> ConbineXElements(IEnumerable<XElement> elements, IEnumerable<FileEntry> files, string type)
		{
			var enumerator = files.GetEnumerator();
			while (enumerator.MoveNext())
			{
				var reader = new StreamReader(enumerator.Current.Stream);
				try
				{
					elements = elements.Concat(XmlUtils.LoadXmlFromTextReader(reader, true).Descendants(type));
				}
				catch (Exception e)
				{
					Log.Warning(string.Format("\"{0}\": {1}", enumerator.Current.Filename, e));
				}
				finally
				{
					reader.Dispose();
				}
			}
			return elements;
		}
	}
}