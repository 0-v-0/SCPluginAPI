#define zh_cn

namespace Game
{
	public class XModifyWorldScreen :
#if zh_cn
		ZHCN.ModifyWorldScreen
#else
		ModifyWorldScreen
#endif
	{
		public override void Enter(object[] parameters)
		{
			if (ScreensManager.PreviousScreen.GetType() != typeof(XWorldOptionsScreen))
			{
				m_directoryName = (string)parameters[0];
				m_worldSettings = (WorldSettings)parameters[1];
				m_originalWorldSettingsData.Clear();
				m_worldSettings.Save(m_originalWorldSettingsData, true);
			}
		}
	}
}
