using Engine;
using System;
using System.Collections.Generic;

namespace Game
{
	public class LoadingScreen : Screen
	{
		List<Action> m_loadActions = new List<Action>();
		int m_index;
		bool m_loadingStarted;
		bool m_loadingFinished;
		bool m_pauseLoading;
		bool m_loadingErrorsSuppressed;
		// Replace LoadingScreen.Update
		public override void Update()
		{
			if (!m_loadingStarted)
				m_loadingStarted = true;
			else if (!m_loadingFinished)
			{
				double realTime = Time.RealTime;
				while (!m_pauseLoading && m_index < m_loadActions.Count)
				{
					try
					{
						m_loadActions[m_index++]();
					}
					catch (Exception ex)
					{
						Log.Error("Loading error. Reason: " + ex);
						if (!m_loadingErrorsSuppressed)
						{
							m_pauseLoading = true;
							DialogsManager.ShowDialog(WidgetsManager.RootWidget, new MessageDialog("Loading Error", ExceptionManager.MakeFullErrorMessage(ex), "OK", "Suppress", delegate(MessageDialogButton b)
							{
								switch (b)
								{
								case MessageDialogButton.Button1:
									m_pauseLoading = false;
									break;
								case MessageDialogButton.Button2:
									m_loadingErrorsSuppressed = true;
									break;
								}
							}));
						}
					}
					if (Time.RealTime - realTime > 0.1)
						break;
				}
				if (m_index >= m_loadActions.Count)
				{
					m_loadingFinished = true;
					AudioManager.PlaySound("Audio/UI/ButtonClick", 1f, 0f, 0f);
					ScreensManager.SwitchScreen("MainMenu");
				}
			}
		}
	}
}