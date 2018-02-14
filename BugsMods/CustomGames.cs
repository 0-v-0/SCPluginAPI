using Engine;
using System;
using Game;
using System.Collections.Generic;

[PluginLoader("CustomGames", "", 0)]
public class CustomGames
{
	public static string Path;
	public static void Initialize()
	{
		GamesManager.ScanGames1 += ScanGames;
	}
	public static void ScanGames()
	{
		var x = new GamesManager.c__DisplayClass5_0
		{
			games = GamesManager.m_games.ToArray()
		};
		GamesManager.m_games.Clear();
		Dispatcher.Dispatch(x.ScanGames_b__0, true);
		var list = new List<string>
		{
			"app:/Data/Games/Bugs"
		};
		list.AddRange(Storage.ListDirectoryNames("data:/Games"));
		for (var enumerator = list.GetEnumerator(); enumerator.MoveNext();)
		{
			Dispatcher.Dispatch(new GamesManager.c__DisplayClass5_1
			{
				directoryName = enumerator.Current
			}.ScanGames_b__1, true);
		}
		list.Clear();
		for (var enumerator = ModsManager.GetEntries(".xml").GetEnumerator(); enumerator.MoveNext();)
		{
			Path = enumerator.Current.Filename;
			Dispatcher.Dispatch(LoadGame, true);
		}
	}
	public static void LoadGame()
	{
		try
		{
			GamesManager.m_games.Add(new GameData(Path));
		}
		catch (Exception ex)
		{
			Log.Warning("Failed to load game from \""+ Path + "\". " + ex.ToString());
		}
	}
}