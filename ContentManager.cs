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
			var enumerator = ModsManager.GetEntries(".pak").GetEnumerator();
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

		public static XElement ConbineXElements(XElement node, IEnumerable<FileEntry> files, string attr1 = null, string attr2 = null, string type = null)
		{
			var enumerator = files.GetEnumerator();
			while (enumerator.MoveNext())
			{
				var reader = new StreamReader(enumerator.Current.Stream);
				try
				{
					var xml = XmlUtils.LoadXmlFromTextReader(reader, true);
					Modify(node, xml, attr1, attr2, type, false);
					Modify(node, xml, attr1, attr2, type);
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
			return node;
		}

		public static void Modify(XElement dst, XElement src, string attr1 = null, string attr2 = null, string type = null, bool toAdd = true)
		{
			var enumerator = src.DescendantsAndSelf(toAdd ? "ToAdd" : "ToRemove").GetEnumerator();
			while (enumerator.MoveNext())
			{
				var node = enumerator.Current;
				var attr = node.Attribute(attr1);
				var guid = attr == null ? null : attr.Value;
				attr = node.Attribute(attr2);
				var name = attr == null ? null : attr.Value;
				var enumerator2 = dst.DescendantsAndSelf(XmlUtils.GetAttributeValue<string>(node, "Type", type)).GetEnumerator();
				while (enumerator2.MoveNext())
				{
					var current = enumerator2.Current;
					if (guid != null)
					{
						if (current.Attribute(attr1).Value != guid) continue;
					}
					else if (name != null && current.Attribute(attr2).Value != name) continue;
					if (toAdd)
						current.Add(node.Elements());
					else
						current.Remove();
				}
			}
		}
	}
}