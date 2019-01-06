using Engine;
using Engine.Content;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml.Linq;
using XmlUtilities;

namespace Game
{
	public static class ContentManager
	{
		/// <summary>
		/// 路径
		/// </summary>
		public static string Path;

		public static void Initialize()
		{
			Directory.CreateDirectory(Path =
#if ENV_ANDROID
				ModsManager.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, "Survivalcraft/Mods")
#elif USE_DATA_PATH
				Storage.GetSystemPath("data:Mods")
#else
				"Mods"
#endif
			);
			ModsManager.Initialize();
			ContentCache.AddPackage(Storage.OpenFile("app:Content.pak", OpenFileMode.Read));
			for (var enumerator = ModsManager.GetEntries(".pak").GetEnumerator(); enumerator.MoveNext();)
				ContentCache.AddPackage(enumerator.Current.Stream);
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
			var obj = Get(name);
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

		/// <summary>
		/// 合并xml文件
		/// </summary>
		/// <param name="node"></param>
		/// <param name="files"></param>
		/// <param name="attr1"></param>
		/// <param name="attr2"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public static XElement CombineXml(XElement node, IEnumerable<FileEntry> files, string attr1 = null, string attr2 = null, string type = null)
		{
			var enumerator = files.GetEnumerator();
			while (enumerator.MoveNext())
			{
				try
				{
					var xml = XmlUtils.LoadXmlFromStream(enumerator.Current.Stream, null, true);
					Modify(node, xml, attr1, attr2, type);
				}
				catch (Exception e)
				{
					ModsManager.ErrorHandler(enumerator.Current, e);
				}
			}
			return node;
		}

		/// <summary>
		/// 修改
		/// </summary>
		/// <param name="dst">目标</param>
		/// <param name="src">源</param>
		/// <param name="attr1"></param>
		/// <param name="attr2"></param>
		/// <param name="type"></param>
		public static void Modify(XElement dst, XElement src, string attr1 = null, string attr2 = null, XName type = null)
		{
			var list = new List<XElement>();
			var enumerator = src.Elements().GetEnumerator();
			while (enumerator.MoveNext())
			{
				var node = enumerator.Current;
				var nn = node.Name.LocalName;
				var attr = node.Attribute(attr1);
				var guid = attr?.Value;
				attr = node.Attribute(attr2);
				var name = attr?.Value;
				int startIndex = nn.Length >= 2 && nn[0] == 'r' && nn[1] == '-' ? node.IsEmpty ? 2 : -2 : 0;
				var enumerator2 = dst.DescendantsAndSelf(nn.Length == 2 && startIndex != 0 ? type : node.Name.LocalName.Substring(Math.Abs(startIndex))).GetEnumerator();
				while (enumerator2.MoveNext())
				{
					var current = enumerator2.Current;
					for (var i = current.Attributes().GetEnumerator(); i.MoveNext();)
					{
						nn = i.Current.Name.LocalName;
						string value = i.Current.Value;
						if (guid != null && string.Equals(nn, attr1))
						{
							if (!string.Equals(value, guid)) goto next;
						}
						else if (name != null && string.Equals(nn, attr2))
						{
							if (!string.Equals(value, name)) goto next;
						}
						else if ((attr = node.Attribute(XName.Get("new-" + nn))) != null)
							current.SetAttributeValue(XName.Get(nn), attr.Value);
					}
					if (startIndex < 0)
					{
						current.RemoveNodes();
						current.Add(node.Elements());
					}
					else if (startIndex > 0)
						list.Add(current);
					else if (!node.IsEmpty)
						current.Add(node.Elements());
					next:;
				}
			}
			for (var i = list.GetEnumerator(); i.MoveNext();) i.Current.Remove();
		}
	}
}