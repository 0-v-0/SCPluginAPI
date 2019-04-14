using Engine;
using Game;
using System;
using System.IO;
using System.Reflection;

[PluginLoader("DebugTK", "disable saving config", 0)]
public class DebugTK
{
	public static void Initialize()
	{
		ModsManager.ErrorHandler += LogLoadException;
		ModsManager.ConfigSaved = ConfigSaved + ModsManager.ConfigSaved;
	}

	public static void LogLoadException(FileEntry file, Exception ex)
	{
		if (ex is ReflectionTypeLoadException e)
		{
			var le = e.LoaderExceptions;
			for (int i = 0; i < le.Length; i++)
				Log.Warning(le[i]);
		}
	}
	public static void ConfigSaved(StreamWriter writer)
	{
		writer.Flush();
		writer.BaseStream.SetLength(0);
	}
}