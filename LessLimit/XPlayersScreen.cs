using System;
using System.Collections.Generic;

namespace Game
{
	public class XPlayersScreen : PlayersScreen
	{
		public override void Update()
		{
			if (m_addPlayerButton.IsClicked)
			{
				SubsystemGameInfo subsystemGameInfo = m_subsystemPlayers.Project.FindSubsystem<SubsystemGameInfo>(true);
				if (subsystemGameInfo.WorldSettings.GameMode == GameMode.Cruel)
					DialogsManager.ShowDialog(null, new MessageDialog("Unavailable", "Cannot add players in cruel mode.", "OK", null, null));
				else if (subsystemGameInfo.WorldSettings.GameMode == GameMode.Adventure)
					DialogsManager.ShowDialog(null, new MessageDialog("Unavailable", "Cannot add players in adventure mode.", "OK", null, null));
				else
					ScreensManager.SwitchScreen("Player", PlayerScreen.Mode.Add, m_subsystemPlayers.Project);
			}
			if (m_screenLayoutButton.IsClicked)
			{
				var list = new List<ScreenLayout>();
				if (m_subsystemPlayers.PlayersData.Count >= 1)
					list.Add(0);
				if (m_subsystemPlayers.PlayersData.Count >= 2)
				{
					list.Add(ScreenLayout.DoubleVertical);
					list.Add(ScreenLayout.DoubleHorizontal);
					list.Add(ScreenLayout.DoubleOpposite);
				}
				if (m_subsystemPlayers.PlayersData.Count >= 3)
				{
					list.Add(ScreenLayout.TripleVertical);
					list.Add(ScreenLayout.TripleHorizontal);
					list.Add(ScreenLayout.TripleEven);
					list.Add(ScreenLayout.TripleOpposite);
				}
				if (m_subsystemPlayers.PlayersData.Count >= 4)
				{
					list.Add(ScreenLayout.Quadruple);
					list.Add(ScreenLayout.QuadrupleOpposite);
				}
				if (list != null)
					DialogsManager.ShowDialog(null, new ListSelectionDialog("Select Screen Layout", list, 80f, (Func<object, Widget>)c._.Update_b__8_0, SelectionHandler));
			}
			if (Input.Back || Input.Cancel || Children.Find<ButtonWidget>("TopBar.Back").IsClicked)
				ScreensManager.SwitchScreen("Game");
		}

		public void SelectionHandler(object o)
		{
			if (o != null)
			{
				UnlimitedGamesWidget.ScreenLayoutChanged = true;
				var layout = (ScreenLayout)o;
				switch (m_subsystemPlayers.PlayersData.Count)
				{
					case 1: SettingsManager.ScreenLayout1 = layout; return;
					case 2: SettingsManager.ScreenLayout2 = layout; return;
					case 3: SettingsManager.ScreenLayout3 = layout; return;
					default: SettingsManager.ScreenLayout4 = layout; return;
				}
			}
		}
	}
}