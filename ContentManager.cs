using Engine;
using Engine.Content;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Game
{
	public static class ContentManager
	{
		public static string Path;

		private static List<string> paksList;

		public static ReadOnlyList<string> PAKsList
		{
			get
			{
				return new ReadOnlyList<string>(ContentManager.paksList);
			}
		}

		public static bool IsPAK(string name)
		{
			return Storage.GetExtension(name).Equals(".pak", StringComparison.OrdinalIgnoreCase);
		}

		public static void Initialize()
		{
			Directory.CreateDirectory(ContentManager.Path = new AndroidSdCardExternalContentProvider().ToInternalPath("Mods"));
			ModsManager.Initialize();
			ContentCache.AddPackage("app:Content.pak");
			ContentManager.UpdatePAKsList();
			using (IEnumerator<string> enumerator = ContentManager.paksList.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					ContentCache.AddPackage(enumerator.Current);
				}
			}
		}

		public static void UpdatePAKsList()
		{
			ContentManager.paksList = new List<string>(Directory.EnumerateFiles(ContentManager.Path).Where(new Func<string, bool>(ContentManager.IsPAK)));
		}
	}
}