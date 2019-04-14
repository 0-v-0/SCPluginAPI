using System;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using Engine;
using Engine.Serialization;
using Game;

namespace Game
{
	public static class WidgetHelper
	{
		public static SliderWidget CreateSlider(float min, float max, float granularity, string text = null)
		{
			var flag = !string.IsNullOrEmpty(text);
			var widget = new SliderWidget
			{
				MinValue = min,
				MaxValue = max,
				Granularity = granularity,
				IsLabelVisible = flag
			};
			if (flag)
			{ widget.Text = text; widget.LabelWidth = text.Length * 9; }
			return widget;
		}
		public static SliderWidget CreateSlider(int min, int max, string text = null)
		{
			return CreateSlider(min, max, 1f, text);
		}
		public static string ToString(this WorldItem wi)
		{
			return string.Format("{0};{1};{2};{3},{4};{5}", wi.Value, wi.Position, wi.Velocity, wi.CreationTime, wi.Light, wi.ToRemove);
		}
		public static string ToString(this Pickable p)
		{
			return string.Format("{0};{1};{2};{3},{4}", ToString((WorldItem)p), p.Count, p.SplashGenerated, p.FlyToPosition, p.StuckMatrix.ToString());
		}
		public static string ToString(this Projectile p)
		{
			return string.Format("{0};{1};{2};{3},{4};{5}", ToString((WorldItem)p), p.AngularVelocity, p.IsIncendiary, p.IsInWater, p.LastNoiseTime, p.NoChunk, p.Owner, p.ProjectileStoppedAction);
		}
	}
	public class EditDialog<T> : Dialog
	{
		public TextBoxWidget TextBox;
		public LabelWidget TitleLabel;
		protected ButtonWidget m_okButton;
		protected ButtonWidget m_cancelButton;
		protected T Value;
		protected Action<T> m_handler;

		public EditDialog(Action<T> handler)
		{
			XElement node = ContentManager.Get<XElement>("Dialogs/EditPistonDialog");
			WidgetsManager.LoadWidgetContents(this, this, node);
			m_okButton = Children.Find<ButtonWidget>("EditPistonDialog.OK", true);
			m_cancelButton = Children.Find<ButtonWidget>("EditPistonDialog.Cancel", true);
			m_handler = handler;
			UpdateControls();
		}
		public EditDialog(T value, Action<T> handler) : this(handler)
		{
			Value = value;
		}
		public EditDialog(string title, T value, Action<T> handler) : this(value, handler)
		{
			TitleLabel.Text = title;
		}
		public override void Update()
		{
			if (changed)
				TextBox.Text = Value.ToString();
			if (m_okButton.IsClicked)
			{
				DialogsManager.HideDialog(this);
				m_handler?.Invoke(Value);
			}
			if (Input.Cancel || m_cancelButton.IsClicked)
				DialogsManager.HideDialog(this);
			UpdateControls();
		}
		public void UpdateControls()
		{
		}
	}
	/*public class EditValuesDialog<T, C> : EditDialog<T[]> where C : Widget
	{
		public readonly LabelWidget[] Labels;
		public readonly C[] Widgets;
		public EditValuesDialog(T[] values, Action<T> handler) : base(values, handler)
		{
			Widgets = new C[Count = values.Length];
			var widget = Children.Find<UniformSpacingPanelWidget>("");
			for (int i = 0; i < Count; i++)
			{
				Widgets[i] = widget.Children.Find<C>();
				Labels[i] = widget.Children.Find<LabelWidget>();
			}
		}
		public readonly int Count;
		public override void Update()
		{
			for (int i = 0; i < Count; i++)
			{
				Update(Labels[i], Widgets[i]);
			}
		}
		public virtual void Update(LabelWidget label, C widget)
		{
		}
	}*/
	public class AdjustValueDialog<T> : EditDialog<T> where T : struct
	{
		protected SliderWidget m_slider;
		public SliderWidget Slider => m_slider;

		public AdjustValueDialog(SliderWidget slider, Action<int> handler) : base(handler)
		{
			var widget = new CanvasWidget();
			WidgetsManager.LoadWidgetContents(widget, widget, ContentManager.Get<XElement>("Dialogs/TextBoxDialog"));
			TitleLabel = widget.Children.Find<LabelWidget>("TextBoxDialog.Title", true);
			m_slider = slider;
		}
		public AdjustValueDialog(string title, float min, float max, Action<int> handler) : this(title, min, max, 1f, title, handler)
		{
		}
		public AdjustValueDialog(string title, float min, float max, string text, Action<int> handler) : this(title, min, max, 1f, text, handler)
		{
		}
		public AdjustValueDialog(string title, float min, float max, float granularity, string text, Action<int> handler) : this(WidgetHelper.CreateSlider(min, max, granularity, text), handler)
		{
			TitleLabel.Text = title;
		}

		public override void Update()
		{
			if (m_slider.IsSliding)
				Value = (T)m_slider.Value;
			m_slider.Value = Convert.ToSingle(Value);
			base.Update();
		}

		public void UpdateControls()
		{
			m_slider.Text = m_slider.Value.ToString();
		}
	}
	/*public class EditFlagsDialog : EditValuesDialog<bool, CheckboxWidget>
	{
		public EditFlagsDialog(bool[] value, Action<bool[]> handler) : base(value, handler)
		{
		}
	}
	public class EditFloatDialog : AdjustValueDialog<float>
	{
		
	}*/
	public class EditSaturateDialog : AdjustValueDialog<float>
	{
		public SliderWidget HSlider;
		public SliderWidget LSlider;

		public EditSaturateDialog(Action<float> handler) : base("Edit Saturate", 0f, 0.0099f, 0.0001f, "Medium", handler)
		{
			HSlider = WidgetHelper.CreateSlider(0f, 100f, 0.01f, "High");
			LSlider = WidgetHelper.CreateSlider(0f, 0.00099f, 0.000001f, "Low");
			HSlider.Size = LSlider.Size = new Vector2(50f, 400f);
		}
		public EditSaturateDialog(float value, Action<float> handler) : this(handler)
		{
			Value = MathUtils.Saturate(value);
		}
		public override void Update()
		{
			if (HSlider.IsSliding || m_slider.IsSliding || LSlider.IsSliding)
				Value = HSlider.Value + LSlider.Value + m_slider.Value;
			base.Update();
		}

		public void UpdateControls()
		{
			HSlider.Text = HSlider.Value.ToString();
			LSlider.Text = LSlider.Value.ToString();
			base.UpdateControls();
		}
	}
	public class EditTimeDialog : AdjustValueDialog<double>
	{
		protected ButtonWidget m_resetBtn;
		protected ButtonWidget m_curTimeBtn;
		//protected ButtonWidget m_lastDeathTimeBtn;
		protected double defaultValue;
		readonly SubsystemTime m_subsystemTime;

		public EditTimeDialog(Action<int> handler) : this(GameManager.Project.FindSubsystem<SubsystemTime>(true).GameTime, handler)
		{
			m_subsystemTime = GameManager.Project.FindSubsystem<SubsystemTime>(true);
		}
		public EditTimeDialog(double defaultValue, Action<int> handler) : base("Edit Time", defaultValue, handler)
		{
		}
		public EditTimeDialog(string title, double defaultValue, Action<int> handler) : base(title, -10, 10, 0.05f, handler)
		{
			this.defaultValue = defaultValue;
			TextBox.TextChanged += t => this.m_worldSettings.Name = TextBox.Text;
		}

		public override void Update()
		{
			if (m_resetBtn.IsClicked)
				Value = defaultValue;
			if (m_curTimeBtn.IsClicked)
				Value = m_subsystemTime.GameTime;
			//if (m_lastDeathTimeBtn.IsClicked)
				//Value = GameManager.WorldInfo.DeathRecords.;
			Value += m_slider.Value;
			base.Update();
		}
	}
	public class EditPoint3Dialog : EditDialog<Point3>
	{
		protected ButtonWidget XButton;
		protected ButtonWidget YButton;
		protected ButtonWidget ZButton;
		protected ButtonWidget CurrPosBtn;
		protected double defaultValue;

		public EditPoint3Dialog(Action<Point3> handler) : this(GameTime, handler)
		{
		}
		public EditPoint3Dialog(double defaultValue, Action<Point3> handler) : this("Edit Position", defaultValue, handler)
		{
		}
		public EditPoint3Dialog(string title, double defaultValue, Action<Point3> handler) : base(title, -10f, 10f, 0.05f, handler)
		{
			this.defaultValue = defaultValue;
		}

		public override void Update()
		{
			if (XButton.IsClicked)
				DialogsManager.ShowDialog(this, new AdjustValueDialog<int>(-2147483648, 2147483647), delegate(int x)
				{
					if (x.HasValue)
						Value.X = x.Value;
				});
			if (YButton.IsClicked)
				DialogsManager.ShowDialog(this, new AdjustValueDialog<int>(-2147483648, 2147483647), delegate(int y)
				{
					if (y.HasValue)
						Value.Y = y.Value;
				});
			if (ZButton.IsClicked)
				DialogsManager.ShowDialog(this, new AdjustValueDialog<int>(-2147483648, 2147483647), delegate(int z)
				{
					if (z.HasValue)
						Value.Z = z.Value;
				});
			base.Update();
		}
	}
	public class EditExplosionDialog : EditPoint3Dialog
	{
		protected float Pressure;
		protected bool IsIncendiary;
		protected bool NoExplosionSound;
		protected CheckboxWidget IsIncendiaryCheckbox;

		public EditExplosionDialog(Point3 p, float pressure, Action<Point3> handler) : base(handler)
		{
			Value = p;
			Pressure = pressure;
			IsIncendiaryCheckbox = Children.Find<CheckboxWidget>("MagmaOcean", true);
		}

		public override void Update()
		{
			base.Update();
		}
		public void Dismiss(Point3? result)
		{
			DialogsManager.HideDialog(this);
			if (m_handler != null && result.HasValue)
			{
				Point3 p = result.Value;
				m_handler(p.X, p.Y, p.Z, Pressure, IsIncendiary, NoExplosionSound);
			}
		}
	}
	/*public class EditDelayedExplosionDialog : EditExplosionDialog
	{
		public float timeToExplosion;

		public EditDelayedExplosionDialog(Action<Point3> handler) : base(handler)
		{
		}
		public EditDelayedExplosionDialog(Point3 p, Action<Point3> handler) : base(handler)
		{
			Value = p;
		}

		public override void Update()
		{
			if ()
				DialogsManager.ShowDialog(this, new AdjustValueDialog(, delegate(float v)
				{
					if (v.HasValue)
						timeToExplosion = v;);
			}
			base.Update();
		}
		public void Dismiss(Point3? result)
		{
			DialogsManager.HideDialog(this);
			if (m_handler != null && result.HasValue)
				m_handler(result.Value, timeToExplosion);
		}
	}
	public class EditRGBAColorDialog : EditColorDialog
	{
		public EditRGBAColorDialog(Color color, Action<Color?> handler) : base(color, handler)
		{
		}
	}
	public class EditMovingBlockDialog : EditPoint3Dialog
	{
		public override void Update()
		{
			DialogsManager.ShowDialog(this, new EditPoint3Dialog(delegate(Point3 p)
				if (p.HasValue)
					Value.Offset = p;));
			DialogsManager.ShowDialog(this, new SelectBlockDialog(delegate(int value)
				if (p.HasValue)
					Value.Value = value;));
			base.Update();
		}
	}*/
	public class EditVector3Dialog : EditDialog<Vector3>
	{
		public SliderWidget XSlider;
		public SliderWidget YSlider;
		public SliderWidget ZSlider;
		public EditVector3Dialog(Vector3 v)
		{
		}

		public override void Update()
		{
			base.Update();
		}
	}
	/*public class EditEntityRefDialog : EditDialog<EntityReference>
	{
		public EditEntityRefDialog(EntityReference er)
		{
			Value = er;
		}

		public override void Update()
		{
			DialogsManager.ShowDialog(this, new delegate(object o)
			{
				(ReferenceType)o
			})
			if (changed)
				TextBox.Text = Value.ReferenceString;
			base.Update();
		}
	}*/
	public class EditMatrixDialog : EditDialog<Matrix>
	{
		public SliderWidget[] sliders = new SliderWidget[16];
		public EditMatrixDialog()
		{
			TextBox.TextChanged += t => Value = HumanReadableConverter.ConvertFromString<Matrix>(TextBox.Text);
		}
		public EditMatrixDialog(Matrix m)
		{
			var arrayMatrix = new ArrayMatrix();
			arrayMatrix.Matrix = m;
			var array = arrayMatrix.Array;
			for (int i = 0; i < 16; i++)
				sliders[i].Value = array[i];
		}
		public override void Update()
		{
			bool changed = false;
			var array = new float[16];
			string.Format();
			for (int i = 0; i < 16; i++)
			{
				if (sliders[i].IsSliding)
				{
					array[i] = slider[i].Value;
					changed = true;
				}
			}
			if (changed)
			{
				var arrayMatrix = new ArrayMatrix();
				arrayMatrix.Array = array;
				Value = arrayMatrix.Matrix;
				TextBox.Text = Value.ToString();
			}
			base.Update();
		}
	}
	/*public class EditTerrainVertexDialog : EditDialog<TerrainVertex>
	{
		public EditTerrainVertexDialog(TerrainVertex v)
		{
			Value = v;
		}

		public override void Update()
		{
			bool changed = false;
			if (TextBox.IsClicked)
				DialogsManager.ShowDialog(this, new EditColorDialog(default(Color), delegate(Color? color)
				{
					if (color.HasValue)
						Value.Color = color.Value;
				}));
			if (changed)
			{
				TextBox.Text = string.Format("{0};{1},{2};{3}", new Vector3(Value.X, Value.Y, Value.Z), Value.Tx, Value.Ty, Value.Color);
			}
			base.Update();
		}
	}
	public class EditWorldItemDialog : EditDialog<Vector3>
	{
	}
	public class EditPickableDialog : EditDialog<Pickable>
	{
		protected SliderWidget ZSlider;
		public EditPickableDialog(Pickable pickable)
		{
			Value = pickable;
		}

		public override void Update()
		{
			bool changed = false;
			if (changed)
			{
				TextBox.Text = string.Format("{0};{1};{2};{3}", Value.ToString(), Value.Count, Value.FlyToPosition, Value.StuckMatrix, Value.SplashGenerated);
			}
			base.Update();
		}
	}
	public class EditProjectileDialog : EditDialog<Projectile>
	{
		public TextBoxWidget textBox;
		public EditProjectileDialog(Projectile projectile)
		{
			Value = projectile;
		}

		public override void Update()
		{
			bool changed = false;
			
			if (changed)
			{
				TextBox.Text = string.Format("{0};{1};{2};{3}", Value.Count, Value.LastNoiseTime, Value.LastNoiseTime, Value.StuckMatrix, Value.Owner, Value.ProjectileStoppedAction, Value.NoChunk, Value.IsIncendiary);
			}
			base.Update();
		}
	}
	public class EditTouchInputDialog : EditDialog<TouchInput>
	{
		public override void Update()
		{
			
			base.Update();
		}
	}
	public class EditDeathRecordDialog : EditDialog<PlayerStats.DeathRecord>
	{
		public EditDeathRecordDialog()
		{
		}
		public EditDeathRecordDialog(PlayerStats.DeathRecord record)
		{
			Value = record;
		}

		public override void Update()
		{
			
			if (changed)
			if (changed)
				TextBox.Text = Value.Save();
			base.Update();
		}
	}
	public class EditDeathRecordsDialog : EditDialog<PlayerStats.DeathRecord[]>
	{
		public class EditDeathRecordsDialog(PlayerStats.DeathRecord[] records)
		{
			Value = records;
		}

		public override void Update()
		{
			DialogsManager.ShowDialog(arg_C5_0, new ListSelectionDialog(delegate(object c)
			{
				if (c != null)
				{
					
				}
			});
			if ()
			{
				DialogsManager.ShowDialog(new EditDeathRecordDialog(, delegate(PlayerStats.DeathRecord record){
				}));
			}
			if ()
			{
				ValuesListToString<PlayerStats.DeathRecord>(';', Value);
			}
			GameManager.WorldInfo.DeathRecords.
			base.Update();
		}
	}
	public class EditParticleDialog : EditDialog<Particle>
	{

		public override void Update()
		{
			
			if (changed)
				TextBox.Text = Value.ToString();
			base.Update();
		}
	}
	public class EditCellFaceDialog : EditDialog<CellFace>
	{

		public override void Update()
		{
			
			if (changed)
				TextBox.Text = Value.ToString();
			base.Update();
		}
	}
	public class EditGlowPointDialog : EditDialog<GlowPoint>
	{

		public override void Update()
		{
			
			base.Update();
		}
	}*/
	public class EditRectangleDialog : EditDialog<Rectangle>
	{
		public EditRectangleDialog(Rectangle rect) : base(handler)
		{
			Value = rect;
		}

		public override void Update()
		{
			
			base.Update();
		}
	}
	public class EditBoxDialog : EditDialog<Box>
	{

		public override void Update()
		{
			bool changed;
			TextBox.Text = Value.ToString();
			base.Update();
		}
	}
	public class EditPlayerStatsDialog : EditDialog<PlayerStats>
	{
		public EditPlayerStatsDialog(PlayerStats stats)
		{
			Value = stats;
		}

		public override void Update()
		{
			
			if (changed)
				TextBox.Text = Value.ToString();
			base.Update();
		}
	}
	public class EditPlayerInputDialog : EditDialog<PlayerInput>
	{
		public Vector2 Look;
		public Vector3 Move;
		public Vector3 SneakMove;
		public Vector2 CameraLook;
		public Vector3 CameraMove;
		public Vector3 CameraSneakMove;
		public bool ToggleCreativeFly;
		public bool ToggleSneak;
		public bool ToggleMount;
		public bool EditItem;
		public bool Jump;
		public int ScrollInventory;
		public bool ToggleInventory;
		public bool ToggleClothing;
		public bool TakeScreenshot;
		public bool SwitchCameraMode;
		public bool TimeOfDay;
		public bool Lighting;
		public bool KeyboardHelp;
		public bool GamepadHelp;
		public Vector2? Dig;
		public Vector2? Hit;
		public Vector2? Aim;
		public Vector2? Interact;
		public Vector2? PickBlockType;
		public bool Drop;
		public int? SelectInventorySlot;

		/*public override void Update()
		{
			
			base.Update();
		}*/
	}
	public class EditScatter2DDialog : EditDialog<Rectangle>
	{
		public short Granularity;
		public float Density;
		public Random Random;
		public ButtonWidget Label1;
		public ButtonWidget Label2;
		public LabelWidget DescriptionLabel;
		public EditScatter2DDialog()
		{
			DescriptionLabel = Children.Find<LabelWidget>("Description", true);
		}
		public EditScatter2DDialog(Rectangle rect)
		{
		}

		public override void Update()
		{
			//	IsClicked
			//Random.GlobalRandom.Int();
			//if (changed)
			base.Update();
		}
	}
	public class EditScatter3DDialog : EditBoxDialog
	{
		
	}
	public class EditBlockPlacementDataDialog : EditDialog<BlockPlacementData>
	{
		
	}
	/*public class AdjustValuesDialog<T> : EditDialog<T> where T : struct
	{
		protected List<SliderWidget> sliders = new List<SliderWidget>();
		public List<SliderWidget> Sliders => sliders;
		//public AdjustValuesDialog(IEnumerable<Tuple<float, float, float>> dialogs)
		//{
		//}
		public AdjustValuesDialog(IEnumerable<SliderWidget> sliders, Action<int> handler) : this(handler)
		{
			sliders = new List<SliderWidget>(sliders);
		}
		public override void Update()
		{
			foreach (var current in sliders)
				if (current.IsSliding)
					Value = m_slider.Value;
			m_slider.Value = (float)Value;
			base.Update();
		}
	}*/
	public class SelectBlockDialog : AdjustValueDialog<int>
	{
		protected ContainerWidget selectedItem;
		protected SliderWidget lightSlider;
		protected SliderWidget dataSlider;

		public SelectBlockDialog(Action<int> handler) : this(1, handler)
		{
			m_slider = WidgetHelper.CreateSlider(0, BlocksManager.Blocks.Length, "Block Type: ");
		}
		public SelectBlockDialog(int defaultValue, Action<int> handler) : base("Select Block", defaultValue, handler)
		{
		}
		public SelectBlockDialog(string title, int defaultValue, Action<int> handler) : base(handler)
		{
			XElement node = ContentManager.Get<XElement>("Widgets/SelectBlockItem");
			var expr_18 = (ContainerWidget)WidgetsManager.LoadWidget(null, node, null);
			expr_18.Children.Find<BlockIconWidget>("SelectBlockItem.Block", true).Contents = (int)index;
			expr_18.Children.Find<LabelWidget>("SelectBlockItem.Text", true).Text = BlocksManager.Blocks[(int)index].GetDisplayName(null, Terrain.MakeBlockValue((int)index));
			selectedItem = expr_18;
			Children.Find<SliderWidget>("SelectBlockDialog.Slider", true);
			Value = defaultValue;
			UpdateControls();
		}

		public override void Update()
		{
			base.Update();
			if (m_slider.IsSliding || lightSlider.IsSliding || dataSlider.IsSliding)
				Value = Terrain.MakeBlockValue((int)m_slider.Value, (int)lightSlider.Value, (int)dataSlider.Value);
		}
		public void UpdateControls()
		{
			m_slider.Value = Terrain.ExtractContents(Value);
			lightSlider.Value = Terrain.ExtractLight(Value);
			dataSlider.Value = Terrain.ExtractData(Value);
		}
	}
}