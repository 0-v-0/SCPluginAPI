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

			m_topicsList.ItemWidgetFactory = delegate(object item)
			{
				var helpTopic3 = (HelpTopic)item;
				XElement node2 = ContentManager.Get<XElement>("Widgets/HelpTopicItem");
				var obj = (ContainerWidget)WidgetsManager.LoadWidget(this, node2, null);
				obj.Children.Find<LabelWidget>("HelpTopicItem.Title", true).Text = helpTopic3.Title;
				return obj;
			};
			m_topicsList.ItemClicked += delegate(object item)
			{
				if (item is HelpTopic helpTopic2)
					ShowTopic(helpTopic2);
			};
			for (var i = ContentManager.CombineXml(ContentManager.Get<XElement>("Help"), ModsManager.GetEntries(".hlp"), "Title", "Name", "Topic").Elements().GetEnumerator(); i.MoveNext();)
			{
				var element = i.Current;
				var strArray = XmlUtils.GetAttributeValue(element, "DisabledPlatforms", string.Empty).Split(',');
				if (strArray.FirstOrDefault(Match) == null)
				{
					var attributeValue1 = XmlUtils.GetAttributeValue(element, "Name", string.Empty);
					var attributeValue2 = XmlUtils.GetAttributeValue<string>(element, "Title");
					var str1 = string.Empty;
					strArray = element.Value.Split('\n');
					for (int i1 = 0; i1 < strArray.Length; i1++)
						str1 = str1 + strArray[i1].Trim() + " ";
					var helpTopic = new HelpTopic { Name = attributeValue1, Title = attributeValue2, Text = str1.Replace("\r", "").Replace("’", "'").Replace("\\n", "\n") };
					if (!string.IsNullOrEmpty(helpTopic.Name))
						m_topics.Add(helpTopic.Name, helpTopic);
					m_topicsList.AddItem(helpTopic);
				}
			}
		}
		bool Match(string s) => s.Trim().ToLower() == VersionsManager.Platform.ToString().ToLower();

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
				ScreensManager.SwitchScreen("HelpTopic", (object)helpTopic);
		}
	}
}