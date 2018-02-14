using Engine;
using System;
using System.Collections.Generic;
using TemplatesDatabase;

namespace Game
{
	public class Cell2DBlock : MagicBlock
	{
		public new const int Index = 321;
	}

	public class Cell3DBlock : MagicBlock
	{
		public new const int Index = 322;
	}

	public struct CellState : IEquatable<CellState>
	{
		public int X;
		public int Y;
		public int Z;
		public int State;

		public CellState(int x, int y, int z, int state)
		{
			X = x;
			Y = y;
			Z = z;
			State = state;
		}

		public override int GetHashCode()
		{
			return (X << 24) ^ (Y << 12) ^ Z;
		}

		public override bool Equals(object obj)
		{
			return obj is CellState state && Equals(state);
		}

		public bool Equals(CellState other)
		{
			return other.X == X && other.Y == Y && other.Z == Z;
		}
	}

	public class SubsystemCell2DBlockBehavior : SubsystemBlockBehavior, IUpdateable
	{
		public SubsystemTime SubsystemTime;
		protected double period;

		public HashSet<CellState> Cells;
		public bool IsPaused;

		public int UpdateOrder { get { return 0; } }

		public override int[] HandledBlocks { get { return new int[0]; } }

		public double GetPeriod() => period;

		public void SetPeriod(int value)
		{
			period = MathUtils.Max(0.01, (value + 1) / 100.0);
		}

		public void Update(float dt)
		{
			if (!SubsystemTime.PeriodicGameTimeEvent(period, 0.0) || IsPaused)
				return;
			var cells = Cells;
			Cells = new HashSet<CellState>();
			for (var i = cells.GetEnumerator(); i.MoveNext();)
			{
				var current = i.Current;
				int x = current.X, y = current.Y, z = current.Z, state = current.State;
				if (state != 0)
				{
					Terrain terrain = SubsystemTerrain.Terrain;
					int oldValue = Terrain.ReplaceLight(terrain.GetCellValueFast(x, y, z), 0);
					if (state > 0)
						ChangeCellFast(terrain, x, y, z, oldValue, Terrain.ReplaceContents(oldValue, state));
					else
						ChangeCellFast(terrain, x, y, z, oldValue, 0);
				}
				Cells.Add(new CellState(x, y, z, GetNextState(x, y, z)));
			}
		}

		public void ChangeCellFast(Terrain terrain, int x, int y, int z, int oldValue, int value)
		{
			if (oldValue == value)
				return;
			terrain.SetCellValueFast(x, y, z, value);
			if (value != 0)
				OnBlockAdded(value, oldValue, x, y, z);
			else
				OnBlockRemoved(oldValue, value, x, y, z);
			TerrainChunk chunkAtCell = terrain.GetChunkAtCell(x, z);
			if (chunkAtCell != null)
				SubsystemTerrain.TerrainUpdater.DowngradeChunkNeighborhoodState(chunkAtCell.Coords, 1, TerrainChunkState.InvalidLight, false);
		}

		public virtual int GetNextState(int x, int y, int z)
		{
			Terrain terrain = SubsystemTerrain.Terrain;
			const int value = Cell2DBlock.Index;
			int count = 0;
			if (Terrain.ExtractContents(terrain.GetCellValueFast(x + 1, y, z)) == value) count++;
			if (Terrain.ExtractContents(terrain.GetCellValueFast(x - 1, y, z)) == value) count++;
			if (Terrain.ExtractContents(terrain.GetCellValueFast(x, y, z + 1)) == value) count++;
			if (Terrain.ExtractContents(terrain.GetCellValueFast(x, y, z - 1)) == value) count++;
			if (Terrain.ExtractContents(terrain.GetCellValueFast(x + 1, y, z + 1)) == value) count++;
			if (Terrain.ExtractContents(terrain.GetCellValueFast(x + 1, y, z - 1)) == value) count++;
			if (Terrain.ExtractContents(terrain.GetCellValueFast(x - 1, y, z + 1)) == value) count++;
			if (Terrain.ExtractContents(terrain.GetCellValueFast(x - 1, y, z - 1)) == value) count++;

			if (Terrain.ExtractContents(terrain.GetCellValueFast(x, y, z)) == value)
			{
				if (count < 2 || count > 3)
					return -1;
			}
			else if (count == 3)
				return value;
			return 0;
		}

		public override void OnBlockAdded(int value, int oldValue, int x, int y, int z)
		{
			if (y > 0 && y < 127)
			{
				Cells.Add(new CellState(x, y, z, 0));
				Cells.Add(new CellState(x + 1, y, z, 0));
				Cells.Add(new CellState(x - 1, y, z, 0));
				Cells.Add(new CellState(x, y, z + 1, 0));
				Cells.Add(new CellState(x, y, z - 1, 0));
				Cells.Add(new CellState(x + 1, y, z + 1, 0));
				Cells.Add(new CellState(x + 1, y, z - 1, 0));
				Cells.Add(new CellState(x - 1, y, z + 1, 0));
				Cells.Add(new CellState(x - 1, y, z - 1, 0));
			}
		}

		public override void OnBlockRemoved(int value, int newValue, int x, int y, int z)
		{
			Cells.Remove(new CellState(x, y, z, 0));
		}

		public override void OnBlockGenerated(int value, int x, int y, int z, bool isLoaded)
		{
			OnBlockAdded(value, 0, x, y, z);
		}

		public override void OnChunkDiscarding(TerrainChunk chunk)
		{
			for (var i = Cells.GetEnumerator(); i.MoveNext();)
			{
				var current = i.Current;
				if (current.X >= chunk.Origin.X && current.X < chunk.Origin.X + 16 && current.Z >= chunk.Origin.Y && current.Z < chunk.Origin.Y + 16)
					Cells.Remove(current);
			}
		}

		public override bool OnInteract(TerrainRaycastResult raycastResult, ComponentMiner componentMiner)
		{
			int index = Terrain.ExtractContents(componentMiner.ActiveBlockValue);
			if (index == Cell2DBlock.Index || index == Cell3DBlock.Index)
				return false;
			IsPaused = !IsPaused;
			return true;
		}

		public override bool OnEditInventoryItem(IInventory inventory, int slotIndex, ComponentPlayer componentPlayer)
		{
			DialogsManager.ShowDialog(componentPlayer.View.GameWidget, new EditAdjustableDelayGateDialog((int)(period * 100.0) - 1, SetPeriod));
			return true;
		}

		public override bool OnEditBlock(int x, int y, int z, int value, ComponentPlayer componentPlayer)
		{
			DialogsManager.ShowDialog(componentPlayer.View.GameWidget, new EditAdjustableDelayGateDialog((int)(period * 100.0) - 1, SetPeriod));
			return true;
		}

		public override void Load(ValuesDictionary valuesDictionary)
		{
			period = valuesDictionary.GetValue("Period", 1.0);
			Cells = new HashSet<CellState>();
			IsPaused = true;
			base.Load(valuesDictionary);
			SubsystemTime = Project.FindSubsystem<SubsystemTime>(true);
		}

		public override void Save(ValuesDictionary valuesDictionary)
		{
			base.Save(valuesDictionary);
			valuesDictionary.SetValue("Period", period);
		}
	}

	public class SubsystemCell3DBlockBehavior : SubsystemCell2DBlockBehavior
	{
		public override int GetNextState(int x, int y, int z)
		{
			Terrain terrain = SubsystemTerrain.Terrain;
			const int value = Cell3DBlock.Index;
			int count = 0;
			//6 Center
			if (Terrain.ExtractContents(terrain.GetCellValueFast(x + 1, y, z)) == value) count++;
			if (Terrain.ExtractContents(terrain.GetCellValueFast(x - 1, y, z)) == value) count++;
			if (Terrain.ExtractContents(terrain.GetCellValueFast(x, y + 1, z)) == value) count++;
			if (Terrain.ExtractContents(terrain.GetCellValueFast(x, y - 1, z)) == value) count++;
			if (Terrain.ExtractContents(terrain.GetCellValueFast(x, y, z + 1)) == value) count++;
			if (Terrain.ExtractContents(terrain.GetCellValueFast(x, y, z - 1)) == value) count++;

			//12 Edges
			//Right Side
			if (Terrain.ExtractContents(terrain.GetCellValueFast(x + 1, y, z + 1)) == value) count++;
			if (Terrain.ExtractContents(terrain.GetCellValueFast(x + 1, y, z - 1)) == value) count++;
			if (Terrain.ExtractContents(terrain.GetCellValueFast(x + 1, y + 1, z)) == value) count++;
			if (Terrain.ExtractContents(terrain.GetCellValueFast(x + 1, y - 1, z)) == value) count++;
			//Left Side	ExtractContents
			if (Terrain.ExtractContents(terrain.GetCellValueFast(x - 1, y, z + 1)) == value) count++;
			if (Terrain.ExtractContents(terrain.GetCellValueFast(x - 1, y, z - 1)) == value) count++;
			if (Terrain.ExtractContents(terrain.GetCellValueFast(x - 1, y + 1, z)) == value) count++;
			if (Terrain.ExtractContents(terrain.GetCellValueFast(x - 1, y - 1, z)) == value) count++;
			//"Middle" LExtractContents
			if (Terrain.ExtractContents(terrain.GetCellValueFast(x, y + 1, z + 1)) == value) count++;
			if (Terrain.ExtractContents(terrain.GetCellValueFast(x, y + 1, z - 1)) == value) count++;
			if (Terrain.ExtractContents(terrain.GetCellValueFast(x, y - 1, z + 1)) == value) count++;
			if (Terrain.ExtractContents(terrain.GetCellValueFast(x, y - 1, z - 1)) == value) count++;

			//8 Corners
			//Right Side
			if (Terrain.ExtractContents(terrain.GetCellValueFast(x + 1, y + 1, z + 1)) == value) count++;
			if (Terrain.ExtractContents(terrain.GetCellValueFast(x + 1, y + 1, z - 1)) == value) count++;
			if (Terrain.ExtractContents(terrain.GetCellValueFast(x + 1, y - 1, z + 1)) == value) count++;
			if (Terrain.ExtractContents(terrain.GetCellValueFast(x + 1, y - 1, z - 1)) == value) count++;
			//Left Side
			if (Terrain.ExtractContents(terrain.GetCellValueFast(x - 1, y + 1, z + 1)) == value) count++;
			if (Terrain.ExtractContents(terrain.GetCellValueFast(x - 1, y + 1, z - 1)) == value) count++;
			if (Terrain.ExtractContents(terrain.GetCellValueFast(x - 1, y - 1, z + 1)) == value) count++;
			if (Terrain.ExtractContents(terrain.GetCellValueFast(x - 1, y - 1, z - 1)) == value) count++;

			if (Terrain.ExtractContents(terrain.GetCellValueFast(x, y, z)) == value)
			{
				if (count < 4 || count > 5)
					return -1;
			}
			else if (count == 5)
				return value;
			return 0;
		}

		public override void OnBlockAdded(int value, int oldValue, int x, int y, int z)
		{
			if (y > 0 && y < 127)
			{
				Cells.Add(new CellState(x, y, z, 0));
				//6 Center
				Cells.Add(new CellState(x + 1, y, z, 0));
				Cells.Add(new CellState(x - 1, y, z, 0));
				Cells.Add(new CellState(x, y + 1, z, 0));
				Cells.Add(new CellState(x, y - 1, z, 0));
				Cells.Add(new CellState(x, y, z + 1, 0));
				Cells.Add(new CellState(x, y, z - 1, 0));
				//12 Edges
				//Right Side
				Cells.Add(new CellState(x + 1, y, z + 1, 0));
				Cells.Add(new CellState(x + 1, y, z - 1, 0));
				Cells.Add(new CellState(x + 1, y + 1, z, 0));
				Cells.Add(new CellState(x + 1, y - 1, z, 0));
				//Left Side
				Cells.Add(new CellState(x - 1, y, z + 1, 0));
				Cells.Add(new CellState(x - 1, y, z - 1, 0));
				Cells.Add(new CellState(x - 1, y + 1, z, 0));
				Cells.Add(new CellState(x - 1, y - 1, z, 0));
				//"Middle" Layer
				Cells.Add(new CellState(x, y + 1, z + 1, 0));
				Cells.Add(new CellState(x, y + 1, z - 1, 0));
				Cells.Add(new CellState(x, y - 1, z + 1, 0));
				Cells.Add(new CellState(x, y - 1, z - 1, 0));

				//8 Corners
				//Right Side
				Cells.Add(new CellState(x + 1, y + 1, z + 1, 0));
				Cells.Add(new CellState(x + 1, y + 1, z - 1, 0));
				Cells.Add(new CellState(x + 1, y - 1, z + 1, 0));
				Cells.Add(new CellState(x + 1, y - 1, z - 1, 0));
				//Left Side
				Cells.Add(new CellState(x - 1, y + 1, z + 1, 0));
				Cells.Add(new CellState(x - 1, y + 1, z - 1, 0));
				Cells.Add(new CellState(x - 1, y - 1, z + 1, 0));
				Cells.Add(new CellState(x - 1, y - 1, z - 1, 0));
			}
		}
	}
}