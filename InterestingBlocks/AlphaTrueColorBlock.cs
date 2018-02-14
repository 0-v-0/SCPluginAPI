using Engine;
using System.Collections.Generic;

namespace Game
{
	public class AlphaTrueColorBlock : TrueColorBlock
	{
		public new const int Index = 865;

		public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometrySubsets geometry, int value, int x, int y, int z)
		{
			generator.GenerateCubeVertices(this, value, x, y, z, new Color(GetColor(value)), geometry.TransparentSubsetsByFace);
			/*var subsetsByFace = geometry.TransparentSubsetsByFace;
			var counts = new int[6];
			int i = 0;
			for (; i < 6; i++)
				counts[i] = subsetsByFace[i].Vertices.Count;
			GetTargetBlock(ref value).GenerateTerrainVertices(generator, geometry, value, x, y, z);
			var color = new Color(GetColor(value));
			for (; i < 6; i++)
			{
				int count = subsetsByFace[i].Vertices.Count;
				if (count > counts[i])
				{
					var arr = subsetsByFace[i].Vertices.Array;
					for (int j = counts[i]; j < count; j++)
						arr[i].Color *= color;
				}
			}*/
		}

		public new static uint GetColor(int value)
		{
			return (uint)((Terrain.ExtractContents(value) - 865 & 63) << 18 | Terrain.ExtractData(value)) | 1u << 31;
		}

		public new static int SetColor(int color)
		{
			return TrueColorBlock.SetColor(color) + 64;
		}
	}

	public class AlphaTrueColorBlock2 : AlphaTrueColorBlock
	{
		public new const int Index = 866;

		public override IEnumerable<int> GetCreativeValues()
		{
			var arr = new int[512];
			for (int i = 0; i < 512; i++)
				arr[i] = SetColor(i >> 6 << 21 | (i >> 3 & 7) << 13 | (i & 7) << 5);
			return arr;
		}
	}

	public class AlphaTrueColorBlock3 : AlphaTrueColorBlock
	{
		public new const int Index = 867;
	}

	public class AlphaTrueColorBlock4 : AlphaTrueColorBlock
	{
		public new const int Index = 868;
	}

	public class AlphaTrueColorBlock5 : AlphaTrueColorBlock
	{
		public new const int Index = 869;
	}

	public class AlphaTrueColorBlock6 : AlphaTrueColorBlock
	{
		public new const int Index = 870;
	}

	public class AlphaTrueColorBlock7 : AlphaTrueColorBlock
	{
		public new const int Index = 871;
	}

	public class AlphaTrueColorBlock8 : AlphaTrueColorBlock
	{
		public new const int Index = 872;
	}

	public class AlphaTrueColorBlock9 : AlphaTrueColorBlock
	{
		public new const int Index = 873;
	}

	public class AlphaTrueColorBlock10 : AlphaTrueColorBlock
	{
		public new const int Index = 874;
	}

	public class AlphaTrueColorBlock11 : AlphaTrueColorBlock
	{
		public new const int Index = 875;
	}

	public class AlphaTrueColorBlock12 : AlphaTrueColorBlock
	{
		public new const int Index = 876;
	}

	public class AlphaTrueColorBlock13 : AlphaTrueColorBlock
	{
		public new const int Index = 877;
	}

	public class AlphaTrueColorBlock14 : AlphaTrueColorBlock
	{
		public new const int Index = 878;
	}

	public class AlphaTrueColorBlock15 : AlphaTrueColorBlock
	{
		public new const int Index = 879;
	}

	public class AlphaTrueColorBlock16 : AlphaTrueColorBlock
	{
		public new const int Index = 880;
	}

	public class AlphaTrueColorBlock17 : AlphaTrueColorBlock
	{
		public new const int Index = 881;
	}

	public class AlphaTrueColorBlock18 : AlphaTrueColorBlock
	{
		public new const int Index = 882;
	}

	public class AlphaTrueColorBlock19 : AlphaTrueColorBlock
	{
		public new const int Index = 883;
	}

	public class AlphaTrueColorBlock20 : AlphaTrueColorBlock
	{
		public new const int Index = 884;
	}

	public class AlphaTrueColorBlock21 : AlphaTrueColorBlock
	{
		public new const int Index = 885;
	}

	public class AlphaTrueColorBlock22 : AlphaTrueColorBlock
	{
		public new const int Index = 886;
	}

	public class AlphaTrueColorBlock23 : AlphaTrueColorBlock
	{
		public new const int Index = 887;
	}

	public class AlphaTrueColorBlock24 : AlphaTrueColorBlock
	{
		public new const int Index = 888;
	}

	public class AlphaTrueColorBlock25 : AlphaTrueColorBlock
	{
		public new const int Index = 889;
	}

	public class AlphaTrueColorBlock26 : AlphaTrueColorBlock
	{
		public new const int Index = 890;
	}

	public class AlphaTrueColorBlock27 : AlphaTrueColorBlock
	{
		public new const int Index = 891;
	}

	public class AlphaTrueColorBlock28 : AlphaTrueColorBlock
	{
		public new const int Index = 892;
	}

	public class AlphaTrueColorBlock29 : AlphaTrueColorBlock
	{
		public new const int Index = 893;
	}

	public class AlphaTrueColorBlock30 : AlphaTrueColorBlock
	{
		public new const int Index = 894;
	}

	public class AlphaTrueColorBlock31 : AlphaTrueColorBlock
	{
		public new const int Index = 895;
	}

	public class AlphaTrueColorBlock32 : AlphaTrueColorBlock
	{
		public new const int Index = 896;
	}

	public class AlphaTrueColorBlock33 : AlphaTrueColorBlock
	{
		public new const int Index = 897;
	}

	public class AlphaTrueColorBlock34 : AlphaTrueColorBlock
	{
		public new const int Index = 898;
	}

	public class AlphaTrueColorBlock35 : AlphaTrueColorBlock
	{
		public new const int Index = 899;
	}

	public class AlphaTrueColorBlock36 : AlphaTrueColorBlock
	{
		public new const int Index = 900;
	}

	public class AlphaTrueColorBlock37 : AlphaTrueColorBlock
	{
		public new const int Index = 901;
	}

	public class AlphaTrueColorBlock38 : AlphaTrueColorBlock
	{
		public new const int Index = 902;
	}

	public class AlphaTrueColorBlock39 : AlphaTrueColorBlock
	{
		public new const int Index = 903;
	}

	public class AlphaTrueColorBlock40 : AlphaTrueColorBlock
	{
		public new const int Index = 904;
	}

	public class AlphaTrueColorBlock41 : AlphaTrueColorBlock
	{
		public new const int Index = 905;
	}

	public class AlphaTrueColorBlock42 : AlphaTrueColorBlock
	{
		public new const int Index = 906;
	}

	public class AlphaTrueColorBlock43 : AlphaTrueColorBlock
	{
		public new const int Index = 907;
	}

	public class AlphaTrueColorBlock44 : AlphaTrueColorBlock
	{
		public new const int Index = 908;
	}

	public class AlphaTrueColorBlock45 : AlphaTrueColorBlock
	{
		public new const int Index = 909;
	}

	public class AlphaTrueColorBlock46 : AlphaTrueColorBlock
	{
		public new const int Index = 910;
	}

	public class AlphaTrueColorBlock47 : AlphaTrueColorBlock
	{
		public new const int Index = 911;
	}

	public class AlphaTrueColorBlock48 : AlphaTrueColorBlock
	{
		public new const int Index = 912;
	}

	public class AlphaTrueColorBlock49 : AlphaTrueColorBlock
	{
		public new const int Index = 913;
	}

	public class AlphaTrueColorBlock50 : AlphaTrueColorBlock
	{
		public new const int Index = 914;
	}

	public class AlphaTrueColorBlock51 : AlphaTrueColorBlock
	{
		public new const int Index = 915;
	}

	public class AlphaTrueColorBlock52 : AlphaTrueColorBlock
	{
		public new const int Index = 916;
	}

	public class AlphaTrueColorBlock53 : AlphaTrueColorBlock
	{
		public new const int Index = 917;
	}

	public class AlphaTrueColorBlock54 : AlphaTrueColorBlock
	{
		public new const int Index = 918;
	}

	public class AlphaTrueColorBlock55 : AlphaTrueColorBlock
	{
		public new const int Index = 919;
	}

	public class AlphaTrueColorBlock56 : AlphaTrueColorBlock
	{
		public new const int Index = 920;
	}

	public class AlphaTrueColorBlock57 : AlphaTrueColorBlock
	{
		public new const int Index = 921;
	}

	public class AlphaTrueColorBlock58 : AlphaTrueColorBlock
	{
		public new const int Index = 922;
	}

	public class AlphaTrueColorBlock59 : AlphaTrueColorBlock
	{
		public new const int Index = 923;
	}

	public class AlphaTrueColorBlock60 : AlphaTrueColorBlock
	{
		public new const int Index = 924;
	}

	public class AlphaTrueColorBlock61 : AlphaTrueColorBlock
	{
		public new const int Index = 925;
	}

	public class AlphaTrueColorBlock62 : AlphaTrueColorBlock
	{
		public new const int Index = 926;
	}

	public class AlphaTrueColorBlock63 : AlphaTrueColorBlock
	{
		public new const int Index = 927;
	}

	public class AlphaTrueColorBlock64 : AlphaTrueColorBlock
	{
		public new const int Index = 928;
	}
}