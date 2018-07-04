using Engine;
using System;
using System.IO;

namespace Game
{
	public static class WorldsManager
	{
		private static ReadOnlyList<string> m_newWorldNames;
		private static string WorldsDirectoryName = "data:/Worlds";
		public static ReadOnlyList<string> FileList;
		// Replace WorldsManager.Initialize
		public static void Initialize()
		{
			Storage.CreateDirectory(WorldsManager.WorldsDirectoryName);
			string text = ContentManager.Get<string>("NewWorldNames");
			var enumerator = (FileList = new ReadOnlyList<string>(ModsManager.GetFiles(".nwn"))).GetEnumerator();
			while (enumerator.MoveNext())
			{
				var reader = new StreamReader(enumerator.Current);
				try
				{
					text += reader.ReadToEnd();
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
			WorldsManager.m_newWorldNames = new ReadOnlyList<string>(text.Split(new char[]
			{
				'\n',
				'\r'
			}, StringSplitOptions.RemoveEmptyEntries));
		}
	}
}