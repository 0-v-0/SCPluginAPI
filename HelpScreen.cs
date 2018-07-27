using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using XmlUtilities;

namespace Game
{
	public class HelpScreen : Screen
	{
		readonly ButtonWidget m_bestiaryButton;
		Screen m_previousScreen;
		readonly ButtonWidget m_recipaediaButton;
		readonly Dictionary<string, HelpTopic> m_topics = new Dictionary<string, HelpTopic>();
		readonly ListPanelWidget m_topicsList;

		public HelpScreen()
		{
			WidgetsManager.LoadWidgetContents(this, this, ContentManager.Get<XElement>("Screens/HelpScreen"));
			m_topicsList = Children.Find<ListPanelWidget>("TopicsList", true);
			m_recipaediaButton = Children.Find<ButtonWidget>("RecipaediaButton", true);
			m_bestiaryButton = Children.Find<ButtonWidget>("BestiaryButton", true);

			this.m_topicsList.ItemWidgetFactory = delegate(object item)
			{
				HelpTopic helpTopic3 = (HelpTopic)item;
				XElement node2 = ContentManager.Get<XElement>("Widgets/HelpTopicItem");
				ContainerWidget obj = (ContainerWidget)WidgetsManager.LoadWidget(this, node2, null);
				obj.Children.Find<LabelWidget>("HelpTopicItem.Title", true).Text = helpTopic3.Title;
				return obj;
			};
			this.m_topicsList.ItemClicked += delegate(object item)
			{
				HelpTopic helpTopic2 = item as HelpTopic;
				if (helpTopic2 != null)
					this.ShowTopic(helpTopic2);
			};
			foreach (var element in ContentManager.ConbineXElements(ContentManager.Get<XElement>("Help"), ModsManager.GetEntries(".hlp"), "Title", "Name", "Topic").Elements())
			{
				var strArray = XmlUtils.GetAttributeValue(element, "DisabledPlatforms", string.Empty).Split(',');
				var predicate = (Func<string, bool>)(s => s.Trim().ToLower() == VersionsManager.Platform.ToString().ToLower());
				if (strArray.FirstOrDefault(predicate) == null)
				{
					var attributeValue1 = XmlUtils.GetAttributeValue(element, "Name", string.Empty);
					var attributeValue2 = XmlUtils.GetAttributeValue<string>(element, "Title");
					var str1 = string.Empty;
					var str2 = element.Value;
					foreach (var str3 in str2.Split('\n'))
						str1 = str1 + str3.Trim() + " ";
					var str4 = str1.Replace("\r", "").Replace("’", "'").Replace("\\n", "\n");
					var helpTopic = new HelpTopic {Name = attributeValue1, Title = attributeValue2, Text = str4};
					if (!string.IsNullOrEmpty(helpTopic.Name))
						m_topics.Add(helpTopic.Name, helpTopic);
					m_topicsList.AddItem(helpTopic);
				}
			}
		}

		public override void Enter(object[] parameters)
		{
			if (ScreensManager.PreviousScreen == ScreensManager.FindScreen<Screen>("HelpTopic") ||
				ScreensManager.PreviousScreen == ScreensManager.FindScreen<Screen>("Recipaedia") ||
				ScreensManager.PreviousScreen == ScreensManager.FindScreen<Screen>("Bestiary"))
				return;
			m_previousScreen = ScreensManager.PreviousScreen;
		}

		public override void Leave()
		{
			m_topicsList.SelectedItem = null;
		}

		public override void Update()
		{
			if (m_recipaediaButton.IsClicked)
				ScreensManager.SwitchScreen("Recipaedia");
			if (m_bestiaryButton.IsClicked)
				ScreensManager.SwitchScreen("Bestiary");
			if (!Input.Back && !Input.Cancel && !Children.Find<ButtonWidget>("TopBar.Back", true).IsClicked)
				return;

			ScreensManager.SwitchScreen(m_previousScreen);
		}

		public HelpTopic GetTopic(string name)
		{
			return m_topics[name];
		}

		private void ShowTopic(HelpTopic helpTopic)
		{
			if (helpTopic.Name == "Keyboard")
				DialogsManager.ShowDialog(null, new KeyboardHelpDialog());
			else if (helpTopic.Name == "Gamepad")
				DialogsManager.ShowDialog(null, new GamepadHelpDialog());
			else
				ScreensManager.SwitchScreen("HelpTopic", (object) helpTopic);
		}
	}
}