using Engine;
using Engine.Graphics;

namespace Game
{
	public class HugeBlock : MagicBlock
	{
		public new const int Index = 323;

		public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
		{
			GetTargetBlock(ref value).DrawBlock(primitivesRenderer, value, color, size == 0.3f ? size * 10f : size, ref matrix, environmentData);
		}
	}
}
