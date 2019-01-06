using GameEntitySystem;
using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TemplatesDatabase;

namespace Game
{
	public static class DatabaseManager
	{
		static GameDatabase m_gameDatabase;
		static Dictionary<string, ValuesDictionary> m_valueDictionaries = new Dictionary<string, ValuesDictionary>();
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
				var node = ContentManager.Get<XElement>("Database");
				ContentManager.Dispose("Database");
				ContentManager.CombineXml(node, ModsManager.GetEntries(".xdb"), "Guid", "Name");
				m_gameDatabase = new GameDatabase(XmlDatabaseSerializer.LoadDatabase(node));
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