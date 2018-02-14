//num3 = design.Resolution;num3 *= num3 * num3;num3 = MathUtils.Clamp(num3, 1, 1727);
using Engine;
using System;
namespace Game
{
	public class FurnitureExtractorBlock : PaintedCubeBlock
	{
		protected FurnitureExtractorBlock() : base(40)
		{
		}
		public static bool GetIsDrawing(int data){
			return (data >> 5 & 1) == 0;
		}
		public static bool GetAvoidBlock(int data){
			return (data >> 6 & 1) == 0;
		}
	}
	public class SubsystemFurnitureExtractorBlockBehavior : SubsystemBlockBehavior
	{
		public override int[] HandledBlocks
		{
			get
			{
				return new int[]{297};
			}
		}
		public override bool OnUse(Vector3 start, Vector3 direction, ComponentMiner componentMiner)
		{
			int value = componentMiner.ActiveBlockValue;
			if (FurnitureExtractorBlock.GetIsDrawing(TerrainData.ExtractData(value)))
			{
				//DialogsManager.ShowDialog(new SphereDialog(this.m_componentPlayer));
				return true;
			}
			else
			{
				TerrainRaycastResult? terrainRaycastResult = componentMiner.PickTerrainForDigging(start, direction);
				if (terrainRaycastResult.HasValue)
					return Extract(componentMiner.ActiveBlockValue, terrainRaycastResult.Value.CellFace, base.SubsystemTerrain);
			}
			return false;
		}
		public override bool OnInteract(TerrainRaycastResult raycastResult, ComponentMiner componentMiner)
		{
			int value = componentMiner.ActiveBlockValue;
			if (TerrainData.ExtractContents(value) == 227)
				return Extract(value, raycastResult.CellFace, base.SubsystemTerrain);
			return false;
		}
		public static bool Extract(int value, CellFace cellFace, SubsystemTerrain subsystemTerrain, bool flag = false)
		{
			TerrainData terrainData = subsystemTerrain.TerrainData;
			int x = cellFace.X, y = cellFace.Y, z = cellFace.Z, count,
				cellValue = terrainData.GetCellValue(x, y, z),
				data = TerrainData.ExtractData(cellValue);
				if (value != 227)
				{
					if (cellValue != 227) return false;
					count = value;
					value = cellValue;
					cellValue = count;
				}
			FurnitureDesign design = subsystemTerrain.SubsystemFurnitureBlockBehavior.GetDesign(FurnitureBlock.GetDesignIndex(TerrainData.ExtractData(value)));
			if (design != null)
			{
				count = design.Resolution;
				flag = FurnitureExtractorBlock.GetAvoidBlock(data);
				for (int i = 0; i < count; i++)
					for (int j = 0; j < count; j++)
						for (int k = 0; k < count; k++)
							if ((value = design.GetValue(i + j * count + k * count * count)) != 0 || flag)
								terrainData.SetCellValueFast(x + i, y + j, z + k, value);
				return true;
			}
			return false;
		}
		/*public override bool OnInteract(TerrainRaycastResult raycastResult, ComponentMiner componentMiner)
		{
			int value = componentMiner.ActiveBlockValue;
			if (TerrainData.ExtractContents(value) == 227)
			{
				SubsystemTerrain subsystemTerrain = base.SubsystemTerrain;
				TerrainData terrainData = subsystemTerrain.TerrainData;
				CellFace cellFace = raycastResult.CellFace;
				int data = TerrainData.ExtractData(raycastResult.Value);
				int x = cellFace.X;
				int y = cellFace.Y;
				int z = cellFace.Z;
				terrainData.GetCellValue(x, y, z);
				bool flag = data >> 5 == 0;
				FurnitureDesign design = subsystemTerrain.SubsystemFurnitureBlockBehavior.GetDesign(FurnitureBlock.GetDesignIndex(TerrainData.ExtractData(value)));
				if (design != null)
				{
					int count = design.Resolution;
					for (int i = 0; i < count; i++)
						for (int j = 0; j < count; j++)
							for (int k = 0; k < count; k++)
								if ((value = design.GetValue(i + j * count + k * count * count)) != 0 || flag)
									terrainData.SetCellValueFast(x + i, y + j, z + k, value);
					return true;
				}
			}
			return false;
		}*/
	}
}