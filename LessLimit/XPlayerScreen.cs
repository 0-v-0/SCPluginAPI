using Engine;
using Engine.Graphics;
using GameEntitySystem;
using System;
using System.Linq;
using System.Xml.Linq;
using TemplatesDatabase;

namespace Game
{
	public class XPlayerScreen : PlayerScreen
	{
		public override void Enter(object[] parameters)
		{
			m_mode = (Mode)parameters[0];
			m_playerData = m_mode == Mode.Edit ? (PlayerData)parameters[1] : new PlayerData((Project)parameters[1]);
			m_playerClassButton.IsEnabled = true;
			ReadOnlyList<PlayerData> playersData;
			switch (m_mode)
			{
				case Mode.Initial:
					m_addButton.IsVisible = false;
					m_deleteButton.IsVisible = false;
					m_playButton.IsVisible = true;
					playersData = m_playerData.SubsystemPlayers.PlayersData;
					m_addAnotherButton.IsVisible = true;
					break;
				case Mode.Add:
					m_addButton.IsVisible = true;
					m_deleteButton.IsVisible = false;
					m_playButton.IsVisible = false;
					m_addAnotherButton.IsVisible = false;
					break;
				case Mode.Edit:
					m_addButton.IsVisible = false;
					playersData = m_playerData.SubsystemPlayers.PlayersData;
					m_deleteButton.IsVisible = true;
					m_playButton.IsVisible = false;
					m_addAnotherButton.IsVisible = false;
					break;
			}
		}

		public override void Update()
		{
			m_characterSkinsCache.GetTexture(m_playerData.CharacterSkinName);
			m_playerModel.PlayerClass = m_playerData.PlayerClass;
			m_playerModel.CharacterSkinName = m_playerData.CharacterSkinName;
			m_playerClassButton.Text = m_playerData.PlayerClass.ToString();
			if (!m_nameTextBox.HasFocus)
				m_nameTextBox.Text = m_playerData.Name;
			m_characterSkinLabel.Text = CharacterSkinsManager.GetDisplayName(m_playerData.CharacterSkinName);
			m_controlsLabel.Text = PlayerScreen.GetDeviceDisplayName(m_inputDevices.FirstOrDefault(Update_b__21_0));
			ValuesDictionary valuesDictionary = DatabaseManager.FindValuesDictionaryForComponent(DatabaseManager.FindEntityValuesDictionary(m_playerData.GetEntityTemplateName(), true), typeof(ComponentCreature));
			m_descriptionLabel.Text = valuesDictionary.GetValue<string>("Description");
			if (m_playerClassButton.IsClicked)
			{
				m_playerData.PlayerClass = (m_playerData.PlayerClass == PlayerClass.Male) ? PlayerClass.Female : PlayerClass.Male;
				m_playerData.RandomizeCharacterSkin();
				if (m_playerData.IsDefaultName)
					m_playerData.ResetName();
			}
			if (m_characterSkinButton.IsClicked)
			{
				CharacterSkinsManager.UpdateCharacterSkinsList();
				DialogsManager.ShowDialog(null, new ListSelectionDialog("Select Character Skin", CharacterSkinsManager.CharacterSkinsNames.Where(IsTarget), 64f, (Func<object, Widget>)GetItem, Select));
			}
			if (m_controlsButton.IsClicked)
				DialogsManager.ShowDialog(null, new ListSelectionDialog("Select External Input Device", m_inputDevices, 56f, (Func<object, string>)GetDeviceDisplayName, SelectionHandler));
			var subsystem = (SubsystemUnlimitedPlayers)m_playerData.SubsystemPlayers;
			if (m_addButton.IsClicked && VerifyName())
			{
				subsystem.AddPlayerData(m_playerData);
				ScreensManager.SwitchScreen("Players", m_playerData.SubsystemPlayers);
			}
			if (m_deleteButton.IsClicked)
				DialogsManager.ShowDialog(null, new MessageDialog("Warning", "The player will be irrecoverably removed from the world. All items in inventory and stats will be lost.", "OK", "Cancel", Handler));
			if (m_playButton.IsClicked && VerifyName())
			{
				subsystem.AddPlayerData(m_playerData);
				ScreensManager.SwitchScreen("Game");
			}
			if (m_addAnotherButton.IsClicked && VerifyName())
			{
				subsystem.AddPlayerData(m_playerData);
				ScreensManager.SwitchScreen("Player", Mode.Initial, m_playerData.SubsystemPlayers.Project);
			}
			if ((Input.Back || Input.Cancel || Children.Find<ButtonWidget>("TopBar.Back", true).IsClicked) && VerifyName())
			{
				if (m_mode == Mode.Initial)
				{
					GameManager.SaveProject(true, true);
					GameManager.DisposeProject();
					ScreensManager.SwitchScreen("MainMenu");
				}
				else if (m_mode == Mode.Add || m_mode == Mode.Edit)
					ScreensManager.SwitchScreen("Players", m_playerData.SubsystemPlayers);
			}
			m_nameWasInvalid = false;
		}

		public Widget GetItem(object item)
		{
			XElement node = ContentManager.Get<XElement>("Widgets/CharacterSkinItem");
			var obj = (ContainerWidget)WidgetsManager.LoadWidget(this, node, null);
			Texture2D texture = m_characterSkinsCache.GetTexture((string)item);
			obj.Children.Find<LabelWidget>("CharacterSkinItem.Text").Text = CharacterSkinsManager.GetDisplayName((string)item);
			obj.Children.Find<LabelWidget>("CharacterSkinItem.Details").Text = $"{texture.Width}x{texture.Height}";
			PlayerModelWidget playerModelWidget = obj.Children.Find<PlayerModelWidget>("CharacterSkinItem.Model");
			playerModelWidget.PlayerClass = m_playerData.PlayerClass;
			playerModelWidget.CharacterSkinTexture = texture;
			return obj;
		}

		public void Select(object item)
		{
			m_playerData.CharacterSkinName = (string)item;
			if (m_playerData.IsDefaultName)
				m_playerData.ResetName();
		}

		public void SelectionHandler(object d)
		{
			var widgetInputDevice = (WidgetInputDevice)d;
			m_playerData.InputDevice = widgetInputDevice;
			foreach (PlayerData playersDatum in m_playerData.SubsystemPlayers.PlayersData)
			{
				if (playersDatum != m_playerData && (playersDatum.InputDevice & widgetInputDevice) != 0)
					playersDatum.InputDevice &= ~widgetInputDevice;
			}
		}

		public void Handler(MessageDialogButton b)
		{
			if (b == MessageDialogButton.Button1)
			{
				m_playerData.SubsystemPlayers.RemovePlayerData(m_playerData);
				ScreensManager.SwitchScreen("Players", m_playerData.SubsystemPlayers);
			}
		}

		public bool IsTarget(string n)
		{
			if (CharacterSkinsManager.GetPlayerClass(n) != m_playerData.PlayerClass)
				return !CharacterSkinsManager.GetPlayerClass(n).HasValue;
			return true;
		}

		public static string GetDeviceDisplayName(object d)
		{
			return PlayerScreen.GetDeviceDisplayName((WidgetInputDevice)d);
		}
	}
}