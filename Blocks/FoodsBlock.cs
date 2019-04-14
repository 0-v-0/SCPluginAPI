using System;
using System.IO;

namespace Game
{
	public class FoodsBlock : CustomTextureBlock
	{
		public new const int Index = 265;

		public override void Initialize()
		{
			TexturePath = Path.Combine(ContentManager.Path, "FoodsBlock.png");
			base.Initialize();
		}

		public override System.Collections.Generic.IEnumerable<int> GetCreativeValues()
		{
			var array = new int[208];
			for (int i = 0; i < 208; i++)
				array[i] = Terrain.MakeBlockValue(Index, 0, i);
			return array;
		}

		public override int GetFaceTextureSlot(int face, int value)
		{
			return Terrain.ExtractData(value) & 0xFF;
		}
	}
}