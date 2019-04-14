using Engine;
using System;
using System.Globalization;
using TemplatesDatabase;

namespace Game
{
	public class SubsystemASWaterBlockBehavior : SubsystemWaterBlockBehavior, IUpdateable
	{
		public double UpdatePeriod;

		public override void Load(ValuesDictionary valuesDictionary)
		{
			base.Load(valuesDictionary);
			UpdatePeriod = valuesDictionary.GetValue("UpdatePeriod", .2);
		}

		public override void Save(ValuesDictionary valuesDictionary)
		{
			base.Save(valuesDictionary);
			valuesDictionary.SetValue("UpdatePeriod", UpdatePeriod);
		}

		public new void Update(float dt)
		{
			if (SubsystemTime.PeriodicGameTimeEvent(UpdatePeriod, 0.0))
				SpreadFluid();
			if (SubsystemTime.PeriodicGameTimeEvent(1.0, 0.25))
			{
				float num = float.MaxValue;
				foreach (Vector3 listenerPosition in SubsystemAudio.ListenerPositions)
				{
					float? num2 = CalculateDistanceToFluid(listenerPosition, 8, flowingFluidOnly: true);
					if (num2.HasValue && num2.Value < num)
						num = num2.Value;
				}
				m_soundVolume = 0.5f * SubsystemAudio.CalculateVolume(num, 2f, 3.5f);
			}
			SubsystemAmbientSounds.WaterSoundVolume = MathUtils.Max(SubsystemAmbientSounds.WaterSoundVolume, m_soundVolume);
		}
	}

	public class SubsystemASMagmaBlockBehavior : SubsystemMagmaBlockBehavior, IUpdateable
	{
		public double UpdatePeriod;

		public override void Load(ValuesDictionary valuesDictionary)
		{
			base.Load(valuesDictionary);
			UpdatePeriod = valuesDictionary.GetValue("UpdatePeriod", 2.0);
		}

		public override void Save(ValuesDictionary valuesDictionary)
		{
			base.Save(valuesDictionary);
			valuesDictionary.SetValue("UpdatePeriod", UpdatePeriod);
		}

		public new void Update(float dt)
		{
			if (SubsystemTime.PeriodicGameTimeEvent(UpdatePeriod, 0.0))
				SpreadFluid();
			if (SubsystemTime.PeriodicGameTimeEvent(1.0, 0.75))
			{
				float num = 3.40282347E+38f;
				foreach (Vector3 listenerPosition in SubsystemAudio.ListenerPositions)
				{
					float? nullable = CalculateDistanceToFluid(listenerPosition, 8, false);
					if (nullable.HasValue && nullable.Value < num)
						num = nullable.Value;
				}
				m_soundVolume = SubsystemAudio.CalculateVolume(num, 2f, 3.5f);
			}
			SubsystemAmbientSounds.MagmaSoundVolume = MathUtils.Max(SubsystemAmbientSounds.MagmaSoundVolume, m_soundVolume);
		}
	}

	public class SubsystemFUPAdjusterBlockBehavior : SubsystemBlockBehavior
	{
		public SubsystemASMagmaBlockBehavior SubsystemASMagmaBlockBehavior;
		public SubsystemASWaterBlockBehavior SubsystemASWaterBlockBehavior;
		public static bool IsWater;

		public override int[] HandledBlocks => new int[0];

		public override void Load(ValuesDictionary valuesDictionary)
		{
			base.Load(valuesDictionary);
			SubsystemASMagmaBlockBehavior = Project.FindSubsystem<SubsystemASMagmaBlockBehavior>(true);
			SubsystemASWaterBlockBehavior = Project.FindSubsystem<SubsystemASWaterBlockBehavior>(true);
		}

		public override bool OnUse(Vector3 start, Vector3 direction, ComponentMiner componentMiner)
		{
			var player = componentMiner.ComponentPlayer;
			if (player == null || player.ComponentBody.ParentBody == null || player.ComponentBody.ParentBody.ImmersionFactor < 0.33f || player.ComponentBody.StandingOnValue.HasValue)
				return false;
			TerrainRaycastResult? terrainRaycastResult = componentMiner.PickTerrainForInteraction(start, direction);
			if (!terrainRaycastResult.HasValue)
			{
				IsWater = componentMiner.ActiveBlockValue == WaterBucketBlock.Index;
				try
				{
					DialogsManager.ShowDialog(player.View.GameWidget, new TextBoxDialog("Set Spread Period", null, 30, SetPeriod));
				}
				catch (Exception e)
				{
					Log.Warning(e);
				}
				return true;
			}
			return false;
		}

		public void SetPeriod(string s)
		{
			if (string.IsNullOrWhiteSpace(s))
				return;
			double period = double.Parse(s.Trim(), CultureInfo.CurrentCulture);
			if (IsWater)
				SubsystemASWaterBlockBehavior.UpdatePeriod = period;
			else
				SubsystemASMagmaBlockBehavior.UpdatePeriod = period;
		}
	}
}