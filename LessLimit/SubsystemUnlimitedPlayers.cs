using System;
using System.Linq;
using TemplatesDatabase;

namespace Game
{
	public class SubsystemUnlimitedPlayers : SubsystemPlayers
	{
		public new void AddPlayerData(PlayerData playerData)
		{
			if (m_playersData.Contains(playerData))
				throw new InvalidOperationException("Player already added.");
			m_playersData.Add(playerData);
			playerData.PlayerIndex = m_nextPlayerIndex++;
			playerAdded?.Invoke(playerData);
		}
	}

	public class SubsystemUnlimitedViews : SubsystemViews
	{
		public override void Load(ValuesDictionary valuesDictionary)
		{
			m_subsystemPlayers = Project.FindSubsystem<SubsystemPlayers>(true);
			SubsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(true);
			m_subsystemPlayers.PlayerAdded += AddViewForPlayer;
			m_subsystemPlayers.PlayerRemoved += Load_b__18_1;
			GamesWidget = valuesDictionary.GetValue<UnlimitedGamesWidget>("GamesWidget");
			for (int i = 0; i < m_subsystemPlayers.PlayersData.Count; i++)
				AddViewForPlayer(m_subsystemPlayers.PlayersData[i]);
		}

		public new void AddViewForPlayer(PlayerData playerData)
		{
			var c__DisplayClass20_ = new c__DisplayClass20_0();
			while (c__DisplayClass20_.viewIndex < 3 && m_views.FirstOrDefault(c__DisplayClass20_.AddViewForPlayer_b__0) != null)
				++c__DisplayClass20_.viewIndex;
			var view = new View(this, playerData, c__DisplayClass20_.viewIndex);
			m_views.Add(view);
			GamesWidget.Children.Add(view.GameWidget);
		}
	}
}