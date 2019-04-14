using System;
using Game;
using Engine;
using System.Collections.Generic;
using GameEntitySystem;

[PluginLoader("Unlimited Players", "", 0)]
public class B
{
	static void Initialize()
	{
		ScreensManager.Initialize1 = (Action)Delegate.Combine(ScreensManager.Initialize1, (Action)Init);
		PlayerData.ctor1 = ctor;
		PlayerData.set_PlayerClass1 = (Action<PlayerData, PlayerClass>)Delegate.Combine((Action<PlayerData, PlayerClass>)set_PlayerClass1, PlayerData.set_PlayerClass1);
	}
	static void ctor(PlayerData data, Project project)
	{
		data.SubsystemPlayers_ = project.FindSubsystem<SubsystemUnlimitedPlayers>(true);
	}
	static void set_PlayerClass1(PlayerData data, PlayerClass value)
	{
		data.m_playerClass = value;
	}
	static void Init()
	{
		LastBlock.LoadingScreen.AddLoadAction(Player);
	}
	static void Player()
	{
		ScreensManager.m_screens["Player"] = new XPlayerScreen();
	}
}