using Engine;
using Engine.Graphics;
using System;
using System.Xml.Linq;

namespace Game
{
	public class XWorldOptionsScreen : WorldOptionsScreen
	{
		public static WorldOptionsScreen WorldOptionsScreen;
		public void SelectionHandler(object e)
		{
			m_worldSettings.TerrainGenerationMode = (TerrainGenerationMode)e;
			m_descriptionLabel.Text = StringsManager.GetString("TerrainGenerationMode." + m_worldSettings.TerrainGenerationMode + ".Description");
		}

		public static string GetName(object e)
		{
			return ((TerrainGenerationMode)e).ToString();
		}

		public override void Update()
		{
			if (m_terrainGenerationButton.IsClicked /*&& !m_isExistingWorld*/)
				DialogsManager.ShowDialog(null, new ListSelectionDialog("Select World Type", EnumUtils.GetEnumValues(typeof(TerrainGenerationMode)), 56f, new Func<object, string>(GetName), SelectionHandler));
			if (m_islandSizeEW.IsSliding /*&& !m_isExistingWorld*/)
				m_worldSettings.IslandSize.X = m_islandSizes[MathUtils.Clamp((int)m_islandSizeEW.Value, 0, m_islandSizes.Length - 1)];
			if (m_islandSizeNS.IsSliding /*&& !m_isExistingWorld*/)
				m_worldSettings.IslandSize.Y = m_islandSizes[MathUtils.Clamp((int)m_islandSizeNS.Value, 0, m_islandSizes.Length - 1)];
			if (m_flatTerrainLevelSlider.IsSliding /*&& !m_isExistingWorld*/)
			{
				m_worldSettings.TerrainLevel = (int)m_flatTerrainLevelSlider.Value;
				m_descriptionLabel.Text = StringsManager.GetString("FlatTerrainLevel.Description");
			}
			if (m_flatTerrainBlockButton.IsClicked /*&& !m_isExistingWorld*/)
			{
				var arr = new int[255];
				for (int i = 0; i < 255; i++)
					arr[i] = i;
				DialogsManager.ShowDialog(null, new ListSelectionDialog("Select Block", arr, 72f, delegate (object index)
				{
					var obj = (ContainerWidget)WidgetsManager.LoadWidget(null, ContentManager.Get<XElement>("Widgets/SelectBlockItem"), null);
					obj.Children.Find<BlockIconWidget>("SelectBlockItem.Block", true).Contents = (int)index;
					obj.Children.Find<LabelWidget>("SelectBlockItem.Text", true).Text = BlocksManager.Blocks[(int)index].GetDisplayName(null, Terrain.MakeBlockValue((int)index));
					return obj;
				}, delegate (object index)
				{
					m_worldSettings.TerrainBlockIndex = (int)index;
				}));
			}
			if (m_flatTerrainMagmaOceanCheckbox.IsClicked)
			{
				m_worldSettings.TerrainOceanBlockIndex = m_worldSettings.TerrainOceanBlockIndex == 18 ? 92 : 18;
				m_descriptionLabel.Text = StringsManager.GetString("FlatTerrainMagmaOcean.Description");
			}
			if (m_seaLevelOffsetSlider.IsSliding /*&& !m_isExistingWorld*/)
			{
				m_worldSettings.SeaLevelOffset = (int)m_seaLevelOffsetSlider.Value;
				m_descriptionLabel.Text = StringsManager.GetString("SeaLevelOffset.Description");
			}
			if (m_temperatureOffsetSlider.IsSliding /*&& !m_isExistingWorld*/)
			{
				m_worldSettings.TemperatureOffset = m_temperatureOffsetSlider.Value;
				m_descriptionLabel.Text = StringsManager.GetString("TemperatureOffset.Description");
			}
			if (m_humidityOffsetSlider.IsSliding /*&& !m_isExistingWorld*/)
			{
				m_worldSettings.HumidityOffset = m_humidityOffsetSlider.Value;
				m_descriptionLabel.Text = StringsManager.GetString("HumidityOffset.Description");
			}
			if (m_biomeSizeSlider.IsSliding /*&& !m_isExistingWorld*/)
			{
				m_worldSettings.BiomeSize = m_biomeSizes[MathUtils.Clamp((int)m_biomeSizeSlider.Value, 0, m_biomeSizes.Length - 1)];
				m_descriptionLabel.Text = StringsManager.GetString("BiomeSize.Description");
			}
			if (m_blocksTextureButton.IsClicked)
			{
				BlocksTexturesManager.UpdateBlocksTexturesList();
				DialogsManager.ShowDialog(null, new ListSelectionDialog("Select Blocks Texture", BlocksTexturesManager.BlockTexturesNames, 64f, delegate (object item)
				{
					var containerWidget = (ContainerWidget)WidgetsManager.LoadWidget(this, ContentManager.Get<XElement>("Widgets/BlocksTextureItem"), null);
					Texture2D texture2 = m_blockTexturesCache.GetTexture((string)item);
					containerWidget.Children.Find<LabelWidget>("BlocksTextureItem.Text", true).Text = BlocksTexturesManager.GetDisplayName((string)item);
					containerWidget.Children.Find<LabelWidget>("BlocksTextureItem.Details", true).Text = string.Format("{0}x{1}", new object[2]
					{
						texture2.Width,
						texture2.Height
					});
					containerWidget.Children.Find<RectangleWidget>("BlocksTextureItem.Icon", true).Subtexture = new Subtexture(texture2, Vector2.Zero, Vector2.One);
					return containerWidget;
				}, delegate (object item)
				{
					m_worldSettings.BlocksTextureName = (string)item;
				}));
				m_descriptionLabel.Text = StringsManager.GetString("BlocksTexture.Description");
			}
			if (m_paletteButton.IsClicked)
			{
				DialogsManager.ShowDialog(null, new EditPaletteDialog(m_worldSettings.Palette));
			}
			if (m_supernaturalCreaturesButton.IsClicked)
			{
				m_worldSettings.AreSupernaturalCreaturesEnabled = !m_worldSettings.AreSupernaturalCreaturesEnabled;
				m_descriptionLabel.Text = StringsManager.GetString("SupernaturalCreatures." + m_worldSettings.AreSupernaturalCreaturesEnabled.ToString());
			}
			if (m_environmentBehaviorButton.IsClicked)
			{
				var enumValues2 = EnumUtils.GetEnumValues(typeof(EnvironmentBehaviorMode));
				m_worldSettings.EnvironmentBehaviorMode = (EnvironmentBehaviorMode)((enumValues2.IndexOf((int)m_worldSettings.EnvironmentBehaviorMode) + 1) % enumValues2.Count);
				m_descriptionLabel.Text = StringsManager.GetString("EnvironmentBehaviorMode." + m_worldSettings.EnvironmentBehaviorMode + ".Description");
			}
			if (m_timeOfDayButton.IsClicked)
			{
				var enumValues3 = EnumUtils.GetEnumValues(typeof(TimeOfDayMode));
				m_worldSettings.TimeOfDayMode = (TimeOfDayMode)((enumValues3.IndexOf((int)m_worldSettings.TimeOfDayMode) + 1) % enumValues3.Count);
				m_descriptionLabel.Text = StringsManager.GetString("TimeOfDayMode." + m_worldSettings.TimeOfDayMode + ".Description");
			}
			if (m_weatherEffectsButton.IsClicked)
			{
				m_worldSettings.AreWeatherEffectsEnabled = !m_worldSettings.AreWeatherEffectsEnabled;
				m_descriptionLabel.Text = StringsManager.GetString("WeatherMode." + m_worldSettings.AreWeatherEffectsEnabled.ToString());
			}
			if (m_adventureRespawnButton.IsClicked)
			{
				m_worldSettings.IsAdventureRespawnAllowed = !m_worldSettings.IsAdventureRespawnAllowed;
				m_descriptionLabel.Text = StringsManager.GetString("AdventureRespawnMode." + m_worldSettings.IsAdventureRespawnAllowed.ToString());
			}
			if (m_adventureSurvivalMechanicsButton.IsClicked)
			{
				m_worldSettings.AreAdventureSurvivalMechanicsEnabled = !m_worldSettings.AreAdventureSurvivalMechanicsEnabled;
				m_descriptionLabel.Text = StringsManager.GetString("AdventureSurvivalMechanics." + m_worldSettings.AreAdventureSurvivalMechanicsEnabled.ToString());
			}
			m_creativeModePanel.IsVisible = m_worldSettings.GameMode == GameMode.Creative;
			//m_newWorldOnlyPanel.IsVisible = !m_isExistingWorld;
			m_continentTerrainPanel.IsVisible = m_worldSettings.TerrainGenerationMode == TerrainGenerationMode.Continent;
			m_islandTerrainPanel.IsVisible = m_worldSettings.TerrainGenerationMode == TerrainGenerationMode.Island;
			m_flatTerrainPanel.IsVisible = m_worldSettings.TerrainGenerationMode == TerrainGenerationMode.Flat;
			m_terrainGenerationLabel.Text = m_worldSettings.TerrainGenerationMode.ToString();
			m_islandSizeEW.Value = FindNearestIndex(m_islandSizes, m_worldSettings.IslandSize.X);
			m_islandSizeEW.Text = m_worldSettings.IslandSize.X.ToString();
			m_islandSizeNS.Value = FindNearestIndex(m_islandSizes, m_worldSettings.IslandSize.Y);
			m_islandSizeNS.Text = m_worldSettings.IslandSize.Y.ToString();
			m_flatTerrainLevelSlider.Value = m_worldSettings.TerrainLevel;
			m_flatTerrainLevelSlider.Text = m_worldSettings.TerrainLevel.ToString();
			m_flatTerrainBlock.Contents = m_worldSettings.TerrainBlockIndex;
			m_flatTerrainMagmaOceanCheckbox.IsChecked = m_worldSettings.TerrainOceanBlockIndex == 92;
			var text = (BlocksManager.Blocks[m_worldSettings.TerrainBlockIndex] != null) ? BlocksManager.Blocks[m_worldSettings.TerrainBlockIndex].GetDisplayName(null, Terrain.MakeBlockValue(m_worldSettings.TerrainBlockIndex)) : string.Empty;
			m_flatTerrainBlockLabel.Text = (text.Length > 10) ? (text.Substring(0, 10) + "...") : text;
			Texture2D texture = m_blockTexturesCache.GetTexture(m_worldSettings.BlocksTextureName);
			m_blocksTextureIcon.Subtexture = new Subtexture(texture, Vector2.Zero, Vector2.One);
			m_blocksTextureLabel.Text = BlocksTexturesManager.GetDisplayName(m_worldSettings.BlocksTextureName);
			m_blocksTextureDetails.Text = string.Format("{0}x{1}", new object[]
			{
				texture.Width,
				texture.Height
			});
			m_seaLevelOffsetSlider.Value = m_worldSettings.SeaLevelOffset;
			m_seaLevelOffsetSlider.Text = FormatOffset(m_worldSettings.SeaLevelOffset);
			m_temperatureOffsetSlider.Value = m_worldSettings.TemperatureOffset;
			m_temperatureOffsetSlider.Text = FormatOffset(m_worldSettings.TemperatureOffset);
			m_humidityOffsetSlider.Value = m_worldSettings.HumidityOffset;
			m_humidityOffsetSlider.Text = FormatOffset(m_worldSettings.HumidityOffset);
			m_biomeSizeSlider.Value = FindNearestIndex(m_biomeSizes, m_worldSettings.BiomeSize);
			m_biomeSizeSlider.Text = m_worldSettings.BiomeSize + "x";
			m_environmentBehaviorButton.Text = m_worldSettings.EnvironmentBehaviorMode.ToString();
			m_timeOfDayButton.Text = m_worldSettings.TimeOfDayMode.ToString();
			m_weatherEffectsButton.Text = m_worldSettings.AreWeatherEffectsEnabled ? "Enabled" : "Disabled";
			m_adventureRespawnButton.Text = m_worldSettings.IsAdventureRespawnAllowed ? "Allowed" : "Not Allowed";
			m_adventureSurvivalMechanicsButton.Text = m_worldSettings.AreAdventureSurvivalMechanicsEnabled ? "Enabled" : "Disabled";
			m_supernaturalCreaturesButton.Text = m_worldSettings.AreSupernaturalCreaturesEnabled ? "Enabled" : "Disabled";
			if (Input.Back || Input.Cancel || Children.Find<ButtonWidget>("TopBar.Back", true).IsClicked)
				ScreensManager.SwitchScreen(ScreensManager.PreviousScreen);
		}

		public override void Leave()
		{
			if (WorldOptionsScreen == null)
				WorldOptionsScreen = new WorldOptionsScreen();
			ScreensManager.PreviousScreen = WorldOptionsScreen;
			base.Leave();
		}
	}
}