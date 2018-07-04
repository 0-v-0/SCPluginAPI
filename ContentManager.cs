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

		public static ReadOnlyList<string> PAKsList;

		public static void Initialize()
		{
			// Note: make AndroidSdCardExternalContentProvider.ToInternalPath public
			Directory.CreateDirectory(ContentManager.Path = (ExternalContentManager.Providers[0] as AndroidSdCardExternalContentProvider).ToInternalPath("Mods"));
			ModsManager.Initialize();
			ContentCache.AddPackage("app:Content.pak");
			var enumerator = (PAKsList = new ReadOnlyList<string>(ModsManager.GetFiles(".pak"))).GetEnumerator();
			while (enumerator.MoveNext())
			{
				ContentCache.AddPackage(enumerator.Current);
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
			object obj = ContentManager.Get(name);
			if (!type.GetTypeInfo().IsAssignableFrom(obj.GetType().GetTypeInfo()))
				throw new InvalidOperationException(string.Format("Content \"{0}\" has type {1}, requested type was {2}", name, obj.GetType().FullName, type.FullName));
			return obj;
		}
	
		public static T Get<T>(string name)
		{
			return (T)ContentManager.Get(typeof(T), name);
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

		public static IEnumerable<XElement> ConbineXElements(IEnumerable<XElement> elements, IEnumerable<string> files, string type)
		{
			var enumerator = files.GetEnumerator();
			while (enumerator.MoveNext())
			{
				var reader = new StreamReader(enumerator.Current);
				try
				{
					elements = elements.Concat(XmlUtils.LoadXmlFromTextReader(reader, true).Descendants(type));
				}
				catch (Exception ex)
				{
					Log.Warning(string.Format("\"{0}\": {1}", enumerator.Current.Substring(ContentManager.Path.Length), ex.Message));
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