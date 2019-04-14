using System;

namespace Game
{
	public class PlanksBlock : PaintedSoildBlock
	{
		public const int Index = 21;

		public override int GetColoredFaceTextureSlot(int face, int value)
		{
			return 23;
		}
	}
}
