using System;
using Engine;

namespace Game
{
	public class SubsystemFastMagmaBlockBehavior : SubsystemMagmaBlockBehavior, IUpdateable
	{
		public new void Update(float dt)
		{
			if (SubsystemTime.PeriodicGameTimeEvent(0.2, 0.0))
				SpreadFluid();
			if (SubsystemTime.PeriodicGameTimeEvent(0.1, 0.075))
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
}