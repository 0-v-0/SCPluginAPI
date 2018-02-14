using System;
using System.Collections.Generic;
using System.IO;
using Engine;
using Game;

[PluginLoader("CacheAutoCleaner", "", 0)]
public class CacheAutoCleaner
{
	public static void Clean()
	{
		string directoryName = ModsManager.CacheDir;
		if (!Directory.Exists(directoryName))
			return;
		var list = new List<string>();
		var list2 = new List<string>();
		RecursiveEnumerateDirectory(directoryName, list, list2, null);
		List<string>.Enumerator enumerator = list.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
				File.Delete(enumerator.Current);
		}
		finally
		{
			((IDisposable)enumerator).Dispose();
		}
		enumerator = list2.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
				Directory.Delete(enumerator.Current);
		}
		finally
		{
			((IDisposable)enumerator).Dispose();
		}
		//Directory.Delete(directoryName);
	}
	public static void Initialize()
	{
		Window.Closed += Clean;
	}
	public static void RecursiveEnumerateDirectory(string directoryName, ICollection<string> files, ICollection<string> directories, Func<string, bool> filesFilter)
	{
		try
		{
			string item;
			for (var i = Directory.EnumerateDirectories(directoryName).GetEnumerator(); i.MoveNext();) {
				item = i.Current;
				RecursiveEnumerateDirectory(item, files, directories, filesFilter);
				if (directories != null)
					directories.Add(item);
			}
			if (files != null)
			{
				for (var i = Directory.EnumerateFiles(directoryName).GetEnumerator(); i.MoveNext();) {
					item = i.Current;
					if (filesFilter == null || filesFilter(item))
						files.Add(item);
				}
			}
		}
		catch (Exception ex)
		{
			Log.Error("Error enumerating files/directories. Reason: " + ex.Message);
		}
	}
}