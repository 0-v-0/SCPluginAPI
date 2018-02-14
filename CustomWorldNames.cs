using Engine;
using System;
using System.IO;
using Game;

[PluginLoader("CustomWorldNames", "", 0)]
public class CustomWorldNames
{
	public static void Initialize()
	{
		WorldsManager.Initialize1 += Apply;
	}
	public static void Apply()
	{
		StringBuilder sb = new StringBuilder();
		for (var enumerator = ModsManager.GetEntries(".nwn").GetEnumerator(); enumerator.MoveNext();)
		{
			var streamReader = new StreamReader(enumerator.Current.Stream);
			try
			{
				sb.Append(streamReader.ReadToEnd());
			}
			catch (Exception e)
			{
				ModsManager.ErrorHandler(enumerator.Current, e);
			}
			finally
			{
				streamReader.Dispose();
			}
		}
		var arr = sb.ToString().Split('\n', '\r', StringSplitOptions.RemoveEmptyEntries);
		var arr2 = new string[m_newWorldNames.Length + arr.Length];
		m_newWorldNames.CopyTo(arr2, 0);
		arr.CopyTo(arr2, m_newWorldNames.Length);
		m_newWorldNames = new ReadOnlyList<string>(arr2);
	}
}