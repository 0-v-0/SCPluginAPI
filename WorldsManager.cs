using Engine;
using System;
using System.IO;

namespace Game
{
	public static class WorldsManager
	{
		private static ReadOnlyList<string> m_newWorldNames;
		private static string WorldsDirectoryName = "data:/Worlds";
		// Replace WorldsManager.Initialize
		public static void Initialize()
		{
			Storage.CreateDirectory(WorldsManager.WorldsDirectoryName);
			var text = ContentManager.Get<string>("NewWorldNames");
			var enumerator = ModsManager.GetEntries(".nwn").GetEnumerator();
			while (enumerator.MoveNext())
			{
				var reader = new StreamReader(enumerator.Current.Stream);
				try
				{
					text += reader.ReadToEnd();
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
			WorldsManager.m_newWorldNames = new ReadOnlyList<string>(text.Split(new []
			{
				'\n',
				'\r'
			}, StringSplitOptions.RemoveEmptyEntries));
		}
	}
}