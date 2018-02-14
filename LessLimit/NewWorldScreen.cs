namespace Game
{
	public class XNewWorldScreen : NewWorldScreen
	{
		public override void Enter(object[] parameters)
		{
			if (ScreensManager.PreviousScreen.GetType() != typeof(XWorldOptionsScreen))
			{
				m_worldSettings = new WorldSettings
				{
					Name = WorldsManager.NewWorldNames[m_random.UniformInt(0, WorldsManager.NewWorldNames.Count - 1)],
					OriginalSerializationVersion = VersionsManager.SerializationVersion
				};
			}
		}

		public override void Update()
		{
			if (m_gameModeButton.IsClicked)
			{
				var enumValues = EnumUtils.GetEnumValues(typeof(GameMode));
				m_worldSettings.GameMode = (GameMode)((enumValues.IndexOf((int)m_worldSettings.GameMode) + 1) % enumValues.Count);
				while (m_worldSettings.GameMode == GameMode.Adventure)
					m_worldSettings.GameMode = (GameMode)((enumValues.IndexOf((int)m_worldSettings.GameMode) + 1) % enumValues.Count);
			}
			bool flag = WorldsManager.ValidateWorldName(m_worldSettings.Name);
			m_nameTextBox.Text = m_worldSettings.Name;
			m_seedTextBox.Text = m_worldSettings.Seed;
			m_gameModeButton.Text = m_worldSettings.GameMode.ToString();
			m_playButton.IsVisible = flag;
			m_errorLabel.IsVisible = !flag;
			m_blankSeedLabel.IsVisible = m_worldSettings.Seed.Length == 0 && !m_seedTextBox.HasFocus;
			m_descriptionLabel.Text = StringsManager.GetString("GameMode." + m_worldSettings.GameMode + ".Description");
			if (m_worldOptionsButton.IsClicked)
				ScreensManager.SwitchScreen("WorldOptions", m_worldSettings, false);
			if (m_playButton.IsClicked && WorldsManager.ValidateWorldName(m_nameTextBox.Text))
			{
				WorldInfo worldInfo = WorldsManager.CreateWorld(m_worldSettings);
				ScreensManager.SwitchScreen("GameLoading", worldInfo, null);
			}
			if (Input.Back || Input.Cancel || Children.Find<ButtonWidget>("TopBar.Back", true).IsClicked)
				ScreensManager.SwitchScreen("Play");
		}
	}
}
