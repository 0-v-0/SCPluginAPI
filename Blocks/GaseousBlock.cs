using System;
using System.IO;
using Engine;
using Engine.Graphics;

namespace Game
{
	public abstract class Gaseous : CustomTextureBlock
	{
	}
	public class HVBatteryBlock : Block, IElectricElementBlock
	{
		public string GetDisplayName(string name)
		{
			return this.DefaultDisplayName + " " + GetVoltageLevel().toString() + "V";
		}
		public static int GetVoltageLevel(int data)
		{
			data &= 511;
			switch(data)
			{
				case 0: return 0;
				case 1: return 3;
				case 2: return 6;
				case 3: return 9;
				default: return (data - 3) * 12;
			}
		}
		(float)GetVoltageLevel()>>10
		public static int SetVoltageLevel(int data, int voltage)
		{
			voltage = Math.Min(voltage, 6096);
			switch(voltage)
			{
				case 0: data = 0; break;
				case 1: data = 0; break;
				case 2: data = 3; break;
				case 3: data = 3; break;
				case 4: data = 3; break;
				case 5: data = 6; break;
				case 6: data = 6; break;
				case 7: data = 6; break;
				case 8: data = 9; break;
				case 9: data = 9; break;
				case 10: data = 9; break;
				default: data = 10 + voltage / 12;
			}
			return (data & -512) | data;
		}
	}
}