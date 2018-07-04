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
		public static ReadOnlyList<string> DBList;
		public static GameDatabase GameDatabase
		{
			get
			{
				if (DatabaseManager.m_gameDatabase != null)
					return DatabaseManager.m_gameDatabase;
				throw new InvalidOperationException("Database not loaded.");
			}
		}

		// Replace DatabaseManager.Initialize
		public static void Initialize()
		{
			if (DatabaseManager.m_gameDatabase == null)
			{
				XElement node = ContentManager.Get<XElement>("Database");
				ContentManager.Dispose("Database");
				var database = (DatabaseManager.m_gameDatabase = new GameDatabase(XmlDatabaseSerializer.LoadDatabase(node))).Database;
				var enumerator = (DBList = new ReadOnlyList<string>(ModsManager.GetFiles(".xdb"))).GetEnumerator();
				while (enumerator.MoveNext())
				{
					var reader = new StreamReader(enumerator.Current);
					try
					{
						foreach (var item in XmlDatabaseSerializer.LoadDatabaseObjectsList(XmlUtils.LoadXmlFromTextReader(reader, true), database))
						{
							item.NestingParent = database.Root;
						}
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
				foreach (DatabaseObject explicitNestingChild in DatabaseManager.GameDatabase.Database.Root.GetExplicitNestingChildren(DatabaseManager.GameDatabase.EntityTemplateType, false))
				{
					var valuesDictionary = new ValuesDictionary();
					valuesDictionary.PopulateFromDatabaseObject(explicitNestingChild);
					DatabaseManager.m_valueDictionaries.Add(explicitNestingChild.Name, valuesDictionary);
				}
				return;
			}
			throw new InvalidOperationException("Database already loaded.");
		}
	}
}