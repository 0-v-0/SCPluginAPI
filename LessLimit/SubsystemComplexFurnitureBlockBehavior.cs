using Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using TemplatesDatabase;

namespace Game
{
	public class BuildComplexFurnitureDialog : BuildFurnitureDialog
	{
		public BuildComplexFurnitureDialog(FurnitureDesign design, FurnitureDesign sourceDesign, Action<bool> handler) : base(design, sourceDesign, handler)
		{
			int num = 0;
			num += m_design.Geometry.SubsetOpaqueByFace.Sum(delegate (BlockMesh b)
			{
				return b == null ? 0 : b.Indices.Count / 3;
			});
			num += m_design.Geometry.SubsetAlphaTestByFace.Sum(delegate (BlockMesh b)
			{
				return b == null ? 0 : b.Indices.Count / 3;
			});
			m_isValid = num <= 65536;
			m_statusLabel.Text = string.Format("Complexity {0}/{1}{2}", num, 65536, m_isValid ? " (OK)" : " (too complex)");
		}

		public override void Update()
		{
			m_nameLabel.Text = string.IsNullOrEmpty(m_design.Name) ? m_design.GetDefaultName() : m_design.Name;
			m_designWidget2d.Mode = (FurnitureDesignWidget.ViewMode)m_axis;
			m_designWidget3d.Mode = FurnitureDesignWidget.ViewMode.Perspective;
			if (m_designWidget2d.Mode == FurnitureDesignWidget.ViewMode.Side)
				m_axisButton.Text = "Side View";
			if (m_designWidget2d.Mode == FurnitureDesignWidget.ViewMode.Top)
				m_axisButton.Text = "Top View";
			if (m_designWidget2d.Mode == FurnitureDesignWidget.ViewMode.Front)
				m_axisButton.Text = "Front View";
			m_leftButton.IsEnabled = IsShiftPossible(DirectionAxisToDelta(0, m_axis));
			m_rightButton.IsEnabled = IsShiftPossible(DirectionAxisToDelta(1, m_axis));
			m_upButton.IsEnabled = IsShiftPossible(DirectionAxisToDelta(2, m_axis));
			m_downButton.IsEnabled = IsShiftPossible(DirectionAxisToDelta(3, m_axis));
			m_decreaseResolutionButton.IsEnabled = IsDecreaseResolutionPossible();
			m_increaseResolutionButton.IsEnabled = m_design.Resolution < TerrainChunk.Height;
			m_resolutionLabel.Text = $"{m_design.Resolution}";
			m_buildButton.IsEnabled = m_isValid;
			if (m_nameButton.IsClicked)
			{
				var list = new List<Tuple<string, Action>>();
				if (m_sourceDesign != null)
				{
					list.Add(new Tuple<string, Action>("Rename Original Furniture", Update_b__23_0));
					list.Add(new Tuple<string, Action>("Rename Modified Furniture", NameFurniture));
				}
				else
					list.Add(new Tuple<string, Action>("Name Modified Furniture", NameFurniture));
				if (list.Count == 1)
					list[0].Item2();
				else
					DialogsManager.ShowDialog(ParentWidget, new ListSelectionDialog("Furniture Naming", list, 64f, (Func<object, string>)ItemToStringConverter, SelectionHandler));
			}
			if (m_axisButton.IsClicked)
				m_axis = (m_axis + 1) % 3;
			if (m_leftButton.IsClicked)
				Shift(DirectionAxisToDelta(0, m_axis));
			if (m_rightButton.IsClicked)
				Shift(DirectionAxisToDelta(1, m_axis));
			if (m_upButton.IsClicked)
				Shift(DirectionAxisToDelta(2, m_axis));
			if (m_downButton.IsClicked)
				Shift(DirectionAxisToDelta(3, m_axis));
			if (m_mirrorButton.IsClicked)
				m_design.Mirror(m_axis);
			if (m_turnRightButton.IsClicked)
				m_design.Rotate(m_axis, 1);
			if (m_decreaseResolutionButton.IsClicked)
			{
				if (IsDecreaseResolutionPossible())
				{
					int resolution = m_design.Resolution;
					Point3 zero = Point3.Zero;
					if (m_design.Box.Right >= resolution)
						zero.X = -1;
					if (m_design.Box.Bottom >= resolution)
						zero.Y = -1;
					if (m_design.Box.Far >= resolution)
						zero.Z = -1;
					m_design.Shift(zero);
					m_design.Resize(resolution - 1);
				}
			}
			if (m_increaseResolutionButton.IsClicked && m_design.Resolution < TerrainChunk.Height)
				m_design.Resize(m_design.Resolution + 1);
			if (m_buildButton.IsClicked && m_isValid)
				Dismiss(true);
			if (Input.Back || m_cancelButton.IsClicked)
				Dismiss(false);
		}

		public void NameFurniture()
		{
			DialogsManager.ShowDialog(ParentWidget, new TextBoxDialog("Name Furniture", m_design.Name, 20, Update_b__23_5));
		}

		public string ItemToStringConverter(object t)
		{
			return ((Tuple<string, Action>)t).Item1;
		}

		public void SelectionHandler(object t)
		{
			((Tuple<string, Action>)t).Item2();
		}
	}

	public class SubsystemXHammerBlockBehavior : SubsystemBlockBehavior
	{
		public SubsystemFurnitureBlockBehavior m_subsystemFurnitureBlockBehavior;
		public ComponentPlayer ComponentPlayer;
		protected Dictionary<Point3, int> valuesDictionary;
		public FurnitureDesign FurnitureDesign;
		public Point3 Point;
		public int StartValue;

		public override int[] HandledBlocks
		{ get { return new int[0]; } }

		public override bool OnUse(Vector3 start, Vector3 direction, ComponentMiner componentMiner)
		{
			TerrainRaycastResult? nullable = componentMiner.PickTerrainForDigging(start, direction);
			if (nullable.HasValue)
			{
				var face = nullable.Value.CellFace;
				FurnitureDesign design = null;
				FurnitureDesign furnitureDesign = null;
				valuesDictionary = new Dictionary<Point3, int>();
				Point3 point = face.Point;
				Point3 point2 = face.Point;
				Point = point;
				int startValue = SubsystemTerrain.Terrain.GetCellValue(face.Point.X, face.Point.Y, face.Point.Z);
				StartValue = startValue;
				var componentPlayer = componentMiner.ComponentPlayer;
				if (Terrain.ExtractContents(startValue) == FurnitureBlock.Index)
				{
					furnitureDesign = m_subsystemFurnitureBlockBehavior.GetDesign(FurnitureBlock.GetDesignIndex(Terrain.ExtractData(startValue)));
					if (furnitureDesign == null)
					{
						if (componentPlayer != null)
							componentPlayer.ComponentGui.DisplaySmallMessage("Unsuitable block found", true, false);
						return true;
					}
					design = furnitureDesign.Clone();
					design.LinkedDesign = null;
					design.InteractionMode = FurnitureInteractionMode.None;
					valuesDictionary.Add(face.Point, startValue);
				}
				else
				{
					var stack = new Stack<Point3>();
					stack.Push(face.Point);
					while (stack.Count > 0)
					{
						Point3 point3 = stack.Pop();
						if (!valuesDictionary.ContainsKey(point3))
						{
							int cellValue = SubsystemTerrain.Terrain.GetCellValue(point3.X, point3.Y, point3.Z);
							if (SubsystemFurnitureBlockBehavior.IsValueDisallowed(cellValue))
							{
								componentPlayer = componentMiner.ComponentPlayer;
								if (componentPlayer != null)
									componentPlayer.ComponentGui.DisplaySmallMessage("Unsuitable block found", true, false);
								return true;
							}
							if (Terrain.ExtractContents(cellValue) != 0)
							{
								if (point3.X < point.X)
									point.X = point3.X;
								if (point3.Y < point.Y)
									point.Y = point3.Y;
								if (point3.Z < point.Z)
									point.Z = point3.Z;
								if (point3.X > point2.X)
									point2.X = point3.X;
								if (point3.Y > point2.Y)
									point2.Y = point3.Y;
								if (point3.Z > point2.Z)
									point2.Z = point3.Z;
								if (MathUtils.Abs(point.X - point2.X) >= TerrainChunk.Height || MathUtils.Abs(point.Y - point2.Y) >= TerrainChunk.Height || MathUtils.Abs(point.Z - point2.Z) >= TerrainChunk.Height)
								{
									if (componentPlayer != null)
										componentPlayer.ComponentGui.DisplaySmallMessage("Furniture design is too large", true, false);
									return true;
								}
								valuesDictionary[point3] = cellValue;
								stack.Push(new Point3(point3.X - 1, point3.Y, point3.Z));
								stack.Push(new Point3(point3.X + 1, point3.Y, point3.Z));
								stack.Push(new Point3(point3.X, point3.Y - 1, point3.Z));
								stack.Push(new Point3(point3.X, point3.Y + 1, point3.Z));
								stack.Push(new Point3(point3.X, point3.Y, point3.Z - 1));
								stack.Push(new Point3(point3.X, point3.Y, point3.Z + 1));
							}
						}
					}
					if (valuesDictionary.Count == 0)
					{
						if (componentPlayer != null)
							componentPlayer.ComponentGui.DisplaySmallMessage("No suitable blocks found", true, false);
						return true;
					}
					design = new FurnitureDesign(SubsystemTerrain);
					Point3 point4 = point2 - point;
					int num = MathUtils.Max(MathUtils.Max(point4.X, point4.Y, point4.Z) + 1, 2);
					var array = new int[num * num * num];
					foreach (KeyValuePair<Point3, int> item in valuesDictionary)
					{
						Point3 point5 = item.Key - point;
						array[point5.X + point5.Y * num + point5.Z * num * num] = item.Value;
					}
					design.SetValues(num, array);
					int steps = (face.Face > 3) ? CellFace.Vector3ToFace(direction, 3) : CellFace.OppositeFace(face.Face);
					design.Rotate(1, steps);
					Box box = design.Box;
					Point3 location = box.Location;
					var p = new Point3(design.Resolution);
					box = design.Box;
					Point3 location2 = box.Location;
					box = design.Box;
					Point3 point6 = p - (location2 + box.Size);
					var delta = new Point3((point6.X - location.X) / 2, -location.Y, (point6.Z - location.Z) / 2);
					design.Shift(delta);
				}
				FurnitureDesign = design;
				ComponentPlayer = componentPlayer;
				var dialog = new BuildComplexFurnitureDialog(design, furnitureDesign, Handler);
				if (componentPlayer != null)
					DialogsManager.ShowDialog(componentPlayer.View.GameWidget, dialog);
				return true;
			}
			return false;
		}

		public override void Load(ValuesDictionary valuesDictionary)
		{
			base.Load(valuesDictionary);
			m_subsystemFurnitureBlockBehavior = Project.FindSubsystem<SubsystemFurnitureBlockBehavior>(true);
		}

		public void Handler(bool result)
		{
			if (result)
			{
				var design = m_subsystemFurnitureBlockBehavior.TryAddDesign(FurnitureDesign);
				if (design == null)
				{
					if (ComponentPlayer != null)
						ComponentPlayer.ComponentGui.DisplaySmallMessage("Too many different furniture designs", true, false);
				}
				else
				{
					var componentMiner = ComponentPlayer.ComponentMiner;
					if (m_subsystemFurnitureBlockBehavior.m_subsystemGameInfo.WorldSettings.GameMode != 0)
					{
						foreach (var item2 in valuesDictionary)
						{
							SubsystemTerrain.DestroyCell(0, item2.Key.X, item2.Key.Y, item2.Key.Z, 0, true, true);
						}
					}
					int value = Terrain.MakeBlockValue(227, 0, FurnitureBlock.SetDesignIndex(0, design.Index, design.ShadowStrengthFactor, design.IsLightEmitter));
					int i = design.Resolution;
					Matrix matrix = componentMiner.ComponentCreature.ComponentBody.Matrix;
					Vector3 position = matrix.Translation + 1f * matrix.Forward + 1f * Vector3.UnitY;
					m_subsystemFurnitureBlockBehavior.m_subsystemPickables.AddPickable(value, i * i * (i - 1), position, null, null);
					componentMiner.DamageActiveTool(1);
					componentMiner.Poke(false);
					for (i = 0; i < 3; i++)
					{
						Time.QueueTimeDelayedExecution(Time.FrameStartTime + i * 0.25, delegate
						{
							m_subsystemFurnitureBlockBehavior.m_subsystemSoundMaterials.PlayImpactSound(StartValue, new Vector3(Point), 1f);
						});
					}
					if (componentMiner.ComponentCreature.PlayerStats != null)
						componentMiner.ComponentCreature.PlayerStats.FurnitureItemsMade += i;
				}
			}
		}
	}
}