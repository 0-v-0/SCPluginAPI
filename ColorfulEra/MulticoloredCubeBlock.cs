namespace Game
{
	public abstract class MulticoloredCubeBlock : PaintedCubeBlock
	{
		protected MulticoloredCubeBlock() : base(0)
		{
		}
		public override int GetFaceTextureSlot(int face, int value)
		{
			return DefaultTextureSlot;
		}
	}
	public class IronBlock : MulticoloredCubeBlock
	{
		public const int Index = 46;
	}
	public class CopperBlock : MulticoloredCubeBlock
	{
		public const int Index = 47;
	}
	public class MalachiteBlock : MulticoloredCubeBlock
	{
		public const int Index = 71;
	}
	public class DiamondBlock : MulticoloredCubeBlock
	{
		public const int Index = 126;
	}
	public class SemiconductorBlock : MulticoloredCubeBlock
	{
		public const int Index = 231;
	}
	public class BedrockBlock : MulticoloredCubeBlock
	{
		public const int Index = 1;
	}
	public class LimestoneBlock : MulticoloredCubeBlock
	{
		public const int Index = 66;
	}
	/*public class CopperOreBlock : MulticoloredCubeBlock
	{
		public const int Index = 41;
	}*/
}