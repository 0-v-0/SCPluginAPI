#define zh_cn

using Engine;
using Engine.Graphics;
using Engine.Media;
using Game;
using System;
using System.IO;
using TemplatesDatabase;

[PluginLoader("LessLimit", "Less limit, more flexible", 0)]
public class LastBlock : CubeBlock
{
	public const int Index = 1023;
	public static LoadingScreen LoadingScreen;
	public static bool AlwaysActive;

	public static new void Initialize()
	{
		BlocksTexturesManager.ValidateBlocksTexture1 += ValidateBlocksTexture;
		CharacterSkinsManager.LoadTexture1 += LoadTexture;
		CharacterSkinsManager.ImportCharacterSkin1 += ImportCharacterSkin;
		SettingsManager.set_Brightness1 += SetBrightness;
		WorldsManager.ValidateWorldName1 += ValidateWorldName;
		WorldsManager.ImportWorld1 += ImportWorld;
		WorldOptionsScreen.m_islandSizes = new[]
		{
			2f, 5f, 10f, 15f, 20f, 25f, 30f, 40f, 50f, 60f, 80f, 100f, 120f, 150f, 200f, 250f, 300f, 400f, 500f, 600f, 800f, 1000f, 1200f, 1500f, 2000f, 2500f, 3000f, 5000f, 8000f, 10000f
		};
		WorldOptionsScreen.m_biomeSizes = new[]
		{
			0.0001f, 0.05f, 0.1f, 0.2f, 0.33f, 0.5f, 0.75f, 1f, 1.5f, 2f, 3f, 5f, 8f, 10f, 12f, 15f, 20f, 50f, 80f, 100f
		};
		ScreensManager.Initialized += Init;
		SettingsPerformanceScreen.m_visibilityRanges.Insert(0, 16);
		SettingsPerformanceScreen.m_visibilityRanges.AddRange(new[] { 384, 448, 512, 576, 640, 704, 768, 832, 896, 960, 1024 });
		if (WorldSettings.ResetOptionsForNonCreativeMode1 == null)
			WorldSettings.ResetOptionsForNonCreativeMode1 = DoNothing;
		WorldSettings.Save1 = Save + WorldSettings.Save1;
		PlayerData.set_PlayerClass1 += SetPlayerClass;
		FurnitureDesign.Resize1 += Resize;
		FurnitureDesign.SetValues1 += SetValues;
		if (AlwaysActive)
			Window.Deactivated += Active;
		ModsManager.ConfigSaved += SaveConfig;
	}

	public static void SaveConfig(StreamWriter writer)
	{
		if (AlwaysActive)
			writer.WriteLine("AlwaysActive");
	}

	public static void Active()
	{
		Window.m_state = Window.State.Active;
	}

	public static void Resize(FurnitureDesign design, int resolution)
	{
		int m_resolution = design.m_resolution;
		if (resolution == m_resolution)
			return;
		var array = new int[resolution * resolution * resolution];
		for (int i = 0; i < resolution; i++)
		{
			for (int j = 0; j < resolution; j++)
			{
				for (int k = 0; k < resolution; k++)
				{
					if (k >= 0 && k < m_resolution && j >= 0 && j < m_resolution && i >= 0 && i < m_resolution)
						array[k + j * resolution + i * resolution * resolution] = design.m_values[k + j * m_resolution + i * m_resolution * m_resolution];
				}
			}
		}
		design.SetValues(resolution, array);
	}

	public static void SetValues(FurnitureDesign design, int resolution, int[] values)
	{
		if (resolution < 2 || resolution > 128)
			throw new ArgumentException("resolution");
		if (values.Length != resolution * resolution * resolution)
			throw new ArgumentException("values");
		design.m_resolution = resolution;
		if (design.m_values == null || design.m_values.Length != resolution * resolution * resolution)
			design.m_values = new int[resolution * resolution * resolution];
		values.CopyTo(design.m_values, 0);
		design.m_hash = null;
		design.m_geometry = null;
		design.m_box = null;
		design.m_collisionBoxesByRotation = null;
		design.m_interactionBoxesByRotation = null;
		design.m_torchPointsByRotation = null;
		design.m_mainValue = 0;
		design.m_mountingFacesMask = -1;
		design.m_transparentFacesMask = -1;
	}

	public static void SetPlayerClass(PlayerData data, PlayerClass value)
	{
		data.m_playerClass = value;
	}

	public static void ValidateBlocksTexture(Stream stream)
	{
		var image = Image.Load(stream);
		if (image.Width > 32768 || image.Height > 32768)
			throw new InvalidOperationException(string.Format(
#if zh_cn
				"材质纹理大于 32768x32768 像素。"
#else
				"Blocks texture is larger than 32768x32768 pixels"
#endif
				+ " (size={0}x{1})", image.Width, image.Height));
		if (!MathUtils.IsPowerOf2(image.Width) || !MathUtils.IsPowerOf2(image.Height))
			throw new InvalidOperationException(string.Format(
#if zh_cn
				"材质纹理的大小不是 2 的整数倍。"
#else
				"Blocks texture does not have power-of-two size"
#endif
				+ " (size={0}x{1})", image.Width, image.Height));
	}

	public static void ValidateCharacterSkin(Stream stream)
	{
		var image = Image.Load(stream);
		if (image.Width > 32768 || image.Height > 32768)
			throw new InvalidOperationException(string.Format(
#if zh_cn
				"角色皮肤大于 32768x32768 像素。"
#else
				"Character skin is larger than 32768x32768 pixels"
#endif
				+ " (size={0}x{1})", image.Width, image.Height));
		if (!MathUtils.IsPowerOf2(image.Width) || !MathUtils.IsPowerOf2(image.Height))
			throw new InvalidOperationException(string.Format(
#if zh_cn
				"角色皮肤的大小不是 2 的整数倍。"
#else
				"Character skin does not have power-of-two size"
#endif
				+ " (size={0}x{1})", image.Width, image.Height));
	}

	public static Texture2D LoadTexture(string name)
	{
		Texture2D texture2D = null;
		try
		{
			string fileName = CharacterSkinsManager.GetFileName(name);
			if (!string.IsNullOrEmpty(fileName))
			{
				using (var stream = Storage.OpenFile(fileName, OpenFileMode.Read))
				{
					ValidateCharacterSkin(stream);
					stream.Position = 0L;
					texture2D = Texture2D.Load(stream, false);
				}
			}
			else
				texture2D = ContentManager.Get<Texture2D>("Textures/Creatures/Human" + name.Substring(1).Replace(" ", ""));
		}
		catch (Exception ex)
		{
			Log.Warning(string.Format("Could not load character skin \"{0}\". Reason: {1}.", name, ex.Message));
		}
		if (texture2D == null)
			texture2D = ContentManager.Get<Texture2D>("Textures/Creatures/HumanMale1");
		return texture2D;
	}

	public static string ImportCharacterSkin(string name, Stream stream)
	{
		var ex = ExternalContentManager.VerifyExternalContentName(name);
		if (ex != null)
			throw ex;
		if (Storage.GetExtension(name) != ".scskin")
			name += ".scskin";
		ValidateCharacterSkin(stream);
		stream.Position = 0L;
		using (var destination = Storage.OpenFile(CharacterSkinsManager.GetFileName(name), OpenFileMode.Create))
		{
			stream.CopyTo(destination);
			return name;
		}
	}

	public static void SetBrightness(float value)
	{
		value = MathUtils.Clamp(value, 0f, 9f);
		if (value != SettingsManager.m_brightness)
		{
			SettingsManager.m_brightness = value;
			SettingsManager.settingChanged?.Invoke("Brightness");
		}
	}

	public static string ImportWorld(Stream sourceStream)
	{
		if (MarketplaceManager.IsTrialMode)
			throw new InvalidOperationException("Cannot import worlds in trial mode.");
		var unusedWorldDirectoryName = WorldsManager.GetUnusedWorldDirectoryName();
		Storage.CreateDirectory(unusedWorldDirectoryName);
		WorldsManager.UnpackWorld(unusedWorldDirectoryName, sourceStream, true);
		if (!WorldsManager.TestXmlFile(Storage.CombinePaths(unusedWorldDirectoryName, "Project.xml"), "Project"))
		{
			try
			{
				WorldsManager.DeleteWorld(unusedWorldDirectoryName);
			}
			catch
			{
			}
			throw new InvalidOperationException("Cannot import world because it does not contain valid world data.");
		}
		return unusedWorldDirectoryName;
	}

	public static bool ValidateWorldName(string name)
	{
		return name.Length != 0 && name.Length <= 20;
	}

	public static void DoNothing(WorldSettings ws)
	{
	}

	public static void Save(WorldSettings ws, ValuesDictionary valuesDictionary, bool liveModifiableParametersOnly)
	{
		valuesDictionary.SetValue("WorldName", ws.Name);
		valuesDictionary.SetValue("OriginalSerializationVersion", ws.OriginalSerializationVersion);
		valuesDictionary.SetValue("GameMode", ws.GameMode);
		valuesDictionary.SetValue("EnvironmentBehaviorMode", ws.EnvironmentBehaviorMode);
		valuesDictionary.SetValue("TimeOfDayMode", ws.TimeOfDayMode);
		valuesDictionary.SetValue("AreWeatherEffectsEnabled", ws.AreWeatherEffectsEnabled);
		valuesDictionary.SetValue("IsAdventureRespawnAllowed", ws.IsAdventureRespawnAllowed);
		valuesDictionary.SetValue("AreAdventureSurvivalMechanicsEnabled", ws.AreAdventureSurvivalMechanicsEnabled);
		valuesDictionary.SetValue("AreSupernaturalCreaturesEnabled", ws.AreSupernaturalCreaturesEnabled);
		valuesDictionary.SetValue("WorldSeedString", ws.Seed);
		valuesDictionary.SetValue("TerrainGenerationMode", ws.TerrainGenerationMode);
		valuesDictionary.SetValue("IslandSize", ws.IslandSize);
		valuesDictionary.SetValue("TerrainLevel", ws.TerrainLevel);
		valuesDictionary.SetValue("TerrainBlockIndex", ws.TerrainBlockIndex);
		valuesDictionary.SetValue("TerrainOceanBlockIndex", ws.TerrainOceanBlockIndex);
		valuesDictionary.SetValue("TemperatureOffset", ws.TemperatureOffset);
		valuesDictionary.SetValue("HumidityOffset", ws.HumidityOffset);
		valuesDictionary.SetValue("SeaLevelOffset", ws.SeaLevelOffset);
		valuesDictionary.SetValue("BiomeSize", ws.BiomeSize);
		valuesDictionary.SetValue("BlockTextureName", ws.BlocksTextureName);
		valuesDictionary.SetValue("Palette", ws.Palette.Save());
	}

	public static void Init()
	{
		((LoadingScreen)ScreensManager.CurrentScreen).AddLoadAction(ReplaceScreen);
	}

	public static void ReplaceScreen()
	{
		ScreensManager.m_screens["NewWorld"] = new XNewWorldScreen();
		ScreensManager.m_screens["ModifyWorld"] = new XModifyWorldScreen();
		ScreensManager.m_screens["Play"] = new XPlayScreen();
		ScreensManager.m_screens["WorldOptions"] = new XWorldOptionsScreen();
		ScreensManager.m_screens["Player"] = new XPlayerScreen();
		ScreensManager.m_screens["Players"] = new XPlayersScreen();
	}
}