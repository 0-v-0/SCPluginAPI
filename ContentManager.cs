using Engine;
using Engine.Content;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
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
			using (IEnumerator<string> enumerator = (PAKsList = new ReadOnlyList<string>(ModsManager.GetFiles(".pak"))).GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					ContentCache.AddPackage(enumerator.Current);
				}
			}
		}
	}
	public static class BlocksManager
	{
		public static ReadOnlyList<string> CSVsList;

		public static void Initialize()
		{
			// Insert this into BlocksManager.Initialize and remove 'Index of block type \"{0}\" conflicts with another block' error
			CSVsList = new ReadOnlyList<string>(ModsManager.GetFiles(".csv"));
			using (IEnumerator<string> enumerator = CSVsList.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					BlocksManager.LoadBlocksData(enumerator.Current);
				}
			}
		}
		static void LoadBlocksData(string data)
		{
			// remove the last 'throw'
		}
		public static IEnumerable<TypeInfo> GetBlockTypes()
		{
			var list = new List<TypeInfo>();
			list.AddRange(typeof(BlocksManager).GetTypeInfo().Assembly.DefinedTypes);
			using (IEnumerator<string> enumerator = LoadedAssemblies.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					list.AddRange(enumerator.Current.DefinedTypes);
				}
			}
			return list;
		}
	}
	public static class CraftingRecipesManager
	{
		public static ReadOnlyList<string> CRsList;

		public static void Initialize()
		{
			// Insert this before the CraftingRecipesManager.Initialize
			IEnumerable<XElement> elements = ContentManager.Get<XElement>("CraftingRecipes").Descendants("Recipe");
			CRsList = new ReadOnlyList<string>(ModsManager.GetFiles(".cr"));
			using (IEnumerator<string> enumerator = CRsList.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					try
					{
						elements = elements.Concat(XmlUtils.LoadXmlFromStream(Storage.OpenFile(enumerator.Current, OpenFileMode.Read), null, true).Descendants("Recipe"));
					}
					catch (Exception ex)
					{
						Log.Warning(string.Format("Recipes \"{0}\" could not be loaded. Reason: {1}", enumerator.Current, ex.Message));
					}
				}
			}
			// foreach (XElement current in elements)
		}
	}
}