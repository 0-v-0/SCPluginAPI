namespace Game
{
	public class XPlayScreen : PlayScreen
	{
		public override void Leave()
		{
			UnlimitedGamesWidget.ScreenLayoutChanged = true;
		}

		public override void Update()
		{
			if (m_worldsListWidget.SelectedItem != null && WorldsManager.WorldInfos.IndexOf((WorldInfo)m_worldsListWidget.SelectedItem) < 0)
				m_worldsListWidget.SelectedItem = null;
			Children.Find<LabelWidget>("TopBar.Label").Text = $"Existing Worlds ({m_worldsListWidget.Items.Count})";
			Children.Find("Play").IsEnabled = m_worldsListWidget.SelectedItem != null;
			Children.Find("Properties").IsEnabled = m_worldsListWidget.SelectedItem != null;
			if (Children.Find<ButtonWidget>("Play").IsClicked && m_worldsListWidget.SelectedItem != null)
				Play(m_worldsListWidget.SelectedItem);
			if (Children.Find<ButtonWidget>("NewWorld").IsClicked)
			{
				ScreensManager.SwitchScreen("NewWorld");
				m_worldsListWidget.SelectedItem = null;
			}
			if (Children.Find<ButtonWidget>("Properties").IsClicked && m_worldsListWidget.SelectedItem != null)
			{
				var worldInfo = (WorldInfo)m_worldsListWidget.SelectedItem;
				ScreensManager.SwitchScreen("ModifyWorld", worldInfo.DirectoryName, worldInfo.WorldSettings);
			}
			if (Input.Back || Input.Cancel || Children.Find<ButtonWidget>("TopBar.Back").IsClicked)
			{
				ScreensManager.SwitchScreen("MainMenu");
				m_worldsListWidget.SelectedItem = null;
			}
		}
	}
}