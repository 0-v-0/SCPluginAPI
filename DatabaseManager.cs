using Engine;
using Game;
using GameEntitySystem;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using TemplatesDatabase;
using XmlUtilities;

namespace Game
{
	public static class DatabaseManager
	{
		private static GameDatabase m_gameDatabase;
		private static Dictionary<string, ValuesDictionary> m_valueDictionaries = new Dictionary<string, ValuesDictionary>();
		public static GameDatabase GameDatabase
		{
			get
			{
				if (m_gameDatabase != null)
					return m_gameDatabase;
				throw new InvalidOperationException("Database not loaded.");
			}
		}

		// Replace DatabaseManager.Initialize
		public static void Initialize()
		{
			if (m_gameDatabase == null)
			{
				XElement node = ContentManager.Get<XElement>("Database");
				ContentManager.Dispose("Database");
				var database = (m_gameDatabase = new GameDatabase(XmlDatabaseSerializer.LoadDatabase(node))).Database;
				var enumerator = (new ReadOnlyList<FileEntry>(ModsManager.GetEntries(".xdb"))).GetEnumerator();
				while (enumerator.MoveNext())
				{
					var reader = new StreamReader(enumerator.Current.Stream);
					try
					{
						foreach (var item in XmlDatabaseSerializer.LoadDatabaseObjectsList(XmlUtils.LoadXmlFromTextReader(reader, true), database))
						{
							item.NestingParent = database.Root;
						}
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
				foreach (DatabaseObject explicitNestingChild in GameDatabase.Database.Root.GetExplicitNestingChildren(GameDatabase.EntityTemplateType, false))
				{
					var valuesDictionary = new ValuesDictionary();
					valuesDictionary.PopulateFromDatabaseObject(explicitNestingChild);
					m_valueDictionaries[explicitNestingChild.Name] = valuesDictionary;
				}
				return;
			}
			throw new InvalidOperationException("Database already loaded.");
		}
	}
}