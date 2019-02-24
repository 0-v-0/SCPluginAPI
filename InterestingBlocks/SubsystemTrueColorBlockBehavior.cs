using Engine;
using Engine.Media;
using System;
using System.IO;
using TemplatesDatabase;

namespace Game
{
	public class SubsystemTrueColorBlockBehavior : SubsystemBlockBehavior
	{
		protected static int px, py, pz;
		public static bool IsAlpha;
		public static int Direction;
		public static GameWidget GameWidget;
		protected SubsystemFurnitureBlockBehavior m_subsystemFurnitureBlockBehavior;

		public override int[] HandledBlocks
		{
			get
			{
				var arr = new int[128];
				for (int i = 0; i < 128; i++)
					arr[i] = i + 801;
				return arr;
			}
		}

		public override void Load(ValuesDictionary valuesDictionary)
		{
			base.Load(valuesDictionary);
			m_subsystemFurnitureBlockBehavior = Project.FindSubsystem<SubsystemFurnitureBlockBehavior>(true);
		}

		public override bool OnInteract(TerrainRaycastResult raycastResult, ComponentMiner componentMiner)
		{
			if (Terrain.ExtractContents(componentMiner.ActiveBlockValue) == TargetBlock.Index)
			{
				int contents = Terrain.ExtractContents(raycastResult.Value);
				if (contents == TrueColorBlock.Index && TrueColorBlock.GetColor(raycastResult.Value) == Color.Black)
					IsAlpha = false;
				else if (contents == AlphaTrueColorBlock.Index && AlphaTrueColorBlock.GetColor(raycastResult.Value) == 0)
					IsAlpha = true;
				else return false;
				var cellFace = raycastResult.CellFace;
				px = cellFace.X;
				py = cellFace.Y;
				pz = cellFace.Z;
				Direction = cellFace.Face;
				DialogsManager.ShowDialog(componentMiner.ComponentPlayer.View.GameWidget, new TextBoxDialog("Enter file name", null, 250, DrawPic));
				SubsystemTerrain.TerrainUpdater.DowngradeAllChunksState(TerrainChunkState.InvalidVertices1, false);
			}
			return false;
		}

		public override bool OnHitAsProjectile(CellFace? cellFace, ComponentBody componentBody, WorldItem worldItem)
		{
			if (!cellFace.HasValue) return false;
			var face = cellFace.Value;
			if (SubsystemTerrain.Terrain.GetCellContentsFast(face.X, face.Y, face.Z) != LargeIncendiaryKegBlock.Index)
				return false;
			var chunk = SubsystemTerrain.Terrain.GetChunkAtCell(face.X, face.Z);
			if (chunk.ThreadState < TerrainChunkState.InvalidVertices1) return false;
			chunk.ThreadState = TerrainChunkState.InvalidVertices1;
			chunk.Geometry.GeometryHash = 0L;
			int skyLightValue = SubsystemTerrain.TerrainUpdater.m_subsystemSky.SkyLightValue;
			SubsystemTerrain.TerrainUpdater.UpdateChunkSingleStep(chunk, skyLightValue);
			SubsystemTerrain.TerrainUpdater.UpdateChunkSingleStep(chunk, skyLightValue);
			var allSubsets = chunk.Geometry.AllSubsets;
			Color color = TrueColorBlock.GetColor(worldItem.Value);
			for (int i = 0, len = allSubsets.Length; i < len; i++)
			{
				var arr = allSubsets[i].Vertices.Array;
				for (int j = 0, count = allSubsets[i].Vertices.Count; j < count; j++)
					arr[i].Color *= color;
			}
			SubsystemTerrain.TerrainRenderer.SetupTerrainChunkGeometryVertexIndexBuffers(chunk);
			return true;
		}

		public override bool OnEditInventoryItem(IInventory inventory, int slotIndex, ComponentPlayer componentPlayer)
		{
			int value = inventory.GetSlotValue(slotIndex);
			int count = inventory.GetSlotCount(slotIndex);
			DialogsManager.ShowDialog(componentPlayer.View.GameWidget, new EditColorDialog(TrueColorBlock.GetColor(value), delegate (Color? color)
			{
				if (color.HasValue)
				{
					var newColor = color.Value;
					int num = Terrain.ExtractContents(value) < 865 ? TrueColorBlock.SetColor((int)newColor.PackedValue) : AlphaTrueColorBlock.SetColor((int)newColor.PackedValue);
					if (num != value)
					{
						inventory.RemoveSlotItems(slotIndex, count);
						inventory.AddSlotItems(slotIndex, num, count);
					}
				}
			}));
			return true;
		}

		public override bool OnEditBlock(int x, int y, int z, int value, ComponentPlayer componentPlayer)
		{
			DialogsManager.ShowDialog(componentPlayer.View.GameWidget, new EditColorDialog(TrueColorBlock.GetColor(value), delegate (Color? color)
			{
				if (color.HasValue)
				{
					var newColor = color.Value;
					int num = Terrain.ExtractContents(value) < 865 ? TrueColorBlock.SetColor((int)newColor.PackedValue) : AlphaTrueColorBlock.SetColor((int)newColor.PackedValue);
					if (num != value)
						SubsystemTerrain.ChangeCell(x, y, z, num);
				}
			}));
			return true;
		}

		public void DrawPic(string path)
		{
			try
			{
				DrawPic(SubsystemTerrain.Terrain, Image.Load(File.OpenRead(Path.Combine(ContentManager.Path, path))), px, py, pz, IsAlpha);
			}
			catch (Exception e)
			{
				Log.Warning(e);
			}
		}

		public static void DrawPic(Terrain terrain, Image image, int x, int y, int z, bool alpha = false)
		{
			int i = 0, w = image.Width, h = image.Height, j;
			if (Direction != 0)
				if (alpha)
					for (; i < w; i++)
						for (j = 0; j < h; j++)
							terrain.SetCellValueFast(x + i, y + j, z, TrueColorBlock.SetColor((int)image.GetPixel(i, j).PackedValue & 0xFFFFFF));
				else for (; i < w; i++)
						for (j = 0; j < h; j++)
							terrain.SetCellValueFast(x + i, y + j, z, AlphaTrueColorBlock.SetColor((int)image.GetPixel(i, j).PackedValue & 0xFFFFFF));
			else
				if (alpha)
				for (; i < w; i++)
					for (j = 0; j < h; j++)
						terrain.SetCellValueFast(x + i, y, z + j, TrueColorBlock.SetColor((int)image.GetPixel(i, j).PackedValue & 0xFFFFFF));
			else for (; i < w; i++)
					for (j = 0; j < h; j++)
						terrain.SetCellValueFast(x + i, y, z + j, AlphaTrueColorBlock.SetColor((int)image.GetPixel(i, j).PackedValue & 0xFFFFFF));
		}

		public void DrawPicWithFurniture(string resolution)
		{
			int.TryParse(resolution, out int r);
		}

		public static void DrawPicWithFurniture(SubsystemTerrain subsystemTerrain, Image image, int x, int y, int z, int resolution = 16)
		{
			if (resolution < 2)
				throw new ArgumentOutOfRangeException("resolution");
			int i = 0, w = image.Width, h = image.Height, j, r = resolution, r2= resolution * resolution, yr;
			var terrain = subsystemTerrain.Terrain;
			FurnitureDesign design = null;
			int[] values = null;
			if (Direction != 0)
			{
				yr = z * resolution * resolution;
				for (; i < w; i++)
				{
					if (i % r == 0)
					{
						design = new FurnitureDesign(subsystemTerrain);
						values = new int[resolution * resolution * resolution];
					}
					for (j = 0; j < h; j++)
					{
						values[i % r + j % r * r + yr] = TrueColorBlock.SetColor((int)image.GetPixel(i, j).PackedValue & 0xFFFFFF);
					}
					if (i % r == r - 1)
					{
						design.SetValues(resolution, values);
						subsystemTerrain.SubsystemFurnitureBlockBehavior.TryAddDesign(design);
						terrain.SetCellValueFast(x + i / r, y + j / r, z, FurnitureBlock.SetDesignIndex(0, design.Index, design.ShadowStrengthFactor, design.IsLightEmitter));
					}
				}
			}
			else
			{
				yr = y * resolution;
				for (; i < w; i++)
				{
					if (i % r == 0)
					{
						design = new FurnitureDesign(subsystemTerrain);
						values = new int[resolution * resolution * resolution];
					}
					for (j = 0; j < h; j++)
						values[i % r + yr + j % r * r2] = TrueColorBlock.SetColor((int)image.GetPixel(i, j).PackedValue & 0xFFFFFF);
					if (i % r == r - 1 && design != null)
					{
						design.SetValues(resolution, values);
						subsystemTerrain.SubsystemFurnitureBlockBehavior.TryAddDesign(design);
						terrain.SetCellValueFast(x + i / r, y, z + j / r, FurnitureBlock.SetDesignIndex(0, design.Index, design.ShadowStrengthFactor, design.IsLightEmitter));
					}
				}
			}
			int width = w / resolution, height = h / resolution;
			/*if (w % resolution != 0) width++;
			if (h % resolution != 0) height++;
			i = 0;
			if (IsVertical)
				for (; i < width; i++)
					for (j = 0; j < height; j++)
			else for (; i < width; i++)
					for (j = 0; j < height; j++)*/
		}

		public override bool OnUse(Vector3 start, Vector3 direction, ComponentMiner componentMiner)
		{
			if (Terrain.ExtractContents(componentMiner.ActiveBlockValue) == TrueColorBlock.Index && TrueColorBlock.GetColor(componentMiner.ActiveBlockValue) == Color.Black)
			{
				TerrainRaycastResult? raycastResult = componentMiner.PickTerrainForDigging(start, direction);
				if (raycastResult.HasValue)
				{
					var cellFace = raycastResult.Value.CellFace;
					if (SubsystemTerrain.Terrain.GetCellContents(px = cellFace.X, py = cellFace.Y, pz = cellFace.Z) == TargetBlock.Index)
					{
						Direction = cellFace.Face;
						DialogsManager.ShowDialog(componentMiner.ComponentPlayer.View.GameWidget, new TextBoxDialog("Enter file name", null, 250, DrawPic));
					}
					DialogsManager.ShowDialog(componentMiner.ComponentPlayer.View.GameWidget, new TextBoxDialog("Enter furniture resolution", null, 3, DrawPic));
					SubsystemTerrain.TerrainUpdater.DowngradeAllChunksState(TerrainChunkState.InvalidVertices1, false);
				}
			}
			return false;
		}
	}
}