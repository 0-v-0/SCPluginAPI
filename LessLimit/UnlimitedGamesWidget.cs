using Engine;
using Engine.Graphics;

namespace Game
{
	public class UnlimitedGamesWidget : GamesWidget
	{
		public static bool ScreenLayoutChanged;

		public override void MeasureOverride(Vector2 parentAvailableSize)
		{
			base.MeasureOverride(parentAvailableSize);
			int count = Children.Count;
			var screenLayout = count == 4 ? SettingsManager.ScreenLayout4 :
				count == 2 ? SettingsManager.ScreenLayout2 :
				count == 3 ? SettingsManager.ScreenLayout3 : SettingsManager.ScreenLayout1;
			IsOverdrawRequired = screenLayout != ScreenLayout.Single;
			if (ScreenLayoutChanged)
			{
				int n = new int[] { 1, 2, 2, 2, 3, 3, 3, 3, 4, 4 }[(int)screenLayout];
				while (count-- > n)
					Children[count].IsVisible = false;
				while (--n > 0)
					Children[n].IsVisible = true;
				ScreenLayoutChanged = false;
			}
			/*if (count < 2) return;
			int i;
			var ws = Children.m_widgets;
			Widget w;
			if (Input.IsKeyDownOnce(Key.B))
			{
				w = ws[count - 1];
				for (i = count; --i > 0;)
					ws[i] = ws[i - 1];
				ws[0] = w;
			}
			else if (Input.IsKeyDownOnce(Key.N))
			{
				w = ws[0];
				for (i = 1; i < count; i++)
					ws[i - 1] = ws[i];
				ws[count - 1] = w;
			}*/
		}

		public override void ArrangeOverride()
		{
			if (Children.Count == 0) return;
			switch (Children.Count == 1 ? SettingsManager.ScreenLayout1 :
				Children.Count == 2 ? SettingsManager.ScreenLayout2 :
				Children.Count == 3 ? SettingsManager.ScreenLayout3 : SettingsManager.ScreenLayout4)
			{
				case ScreenLayout.Single:
					ArrangeChildWidgetInCell(Vector2.Zero, ActualSize, Children[0]);
					Children[0].LayoutTransform = Matrix.Identity;
					break;
				case ScreenLayout.DoubleVertical:
					{
						m_spacing = 12f;
						m_bevel = 3f;
						float x = 0f;
						float y = 0f;
						float x2 = ActualSize.X / 2f + m_spacing / 2f;
						float y2 = 0f;
						float x3 = ActualSize.X / 2f - m_spacing / 2f;
						float y3 = ActualSize.Y;
						float num = 0.5f;
						ArrangeChildWidgetInCell(new Vector2(x, y), new Vector2(x, y) + new Vector2(x3, y3) / num, Children[0]);
						Children[0].LayoutTransform = Matrix.CreateScale(num, num, 1f);
						ArrangeChildWidgetInCell(new Vector2(x2, y2), new Vector2(x2, y2) + new Vector2(x3, y3) / num, Children[1]);
						Children[1].LayoutTransform = Matrix.CreateScale(num, num, 1f);
						break;
					}

				case ScreenLayout.DoubleHorizontal:
					{
						m_spacing = 12f;
						m_bevel = 3f;
						float x4 = 0f;
						float y4 = 0f;
						float x5 = 0f;
						float y5 = ActualSize.Y / 2f + m_spacing / 2f;
						float x6 = ActualSize.X;
						float y6 = ActualSize.Y / 2f - m_spacing / 2f;
						float num2 = 0.48f;
						ArrangeChildWidgetInCell(new Vector2(x4, y4), new Vector2(x4, y4) + new Vector2(x6, y6) / num2, Children[0]);
						Children[0].LayoutTransform = Matrix.CreateScale(num2, num2, 1f);
						ArrangeChildWidgetInCell(new Vector2(x5, y5), new Vector2(x5, y5) + new Vector2(x6, y6) / num2, Children[1]);
						Children[1].LayoutTransform = Matrix.CreateScale(num2, num2, 1f);
						break;
					}

				case ScreenLayout.DoubleOpposite:
					{
						m_spacing = 20f;
						m_bevel = 4f;
						float x7 = 0f;
						float y7 = 0f;
						float x8 = ActualSize.X / 2f + m_spacing / 2f;
						float y8 = 0f;
						float x9 = ActualSize.X / 2f - m_spacing / 2f;
						float y9 = ActualSize.Y;
						float num3 = Window.Size.Y / (float)Window.Size.X;
						ArrangeChildWidgetInCell(new Vector2(x7, y7), new Vector2(x7, y7) + new Vector2(x9, y9) / num3, Children[0]);
						Children[0].LayoutTransform = new Matrix(0f, num3, 0f, 0f, 0f - num3, 0f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f);
						ArrangeChildWidgetInCell(new Vector2(x8, y8), new Vector2(x8, y8) + new Vector2(x9, y9) / num3, Children[1]);
						Children[1].LayoutTransform = new Matrix(0f, 0f - num3, 0f, 0f, num3, 0f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f);
						break;
					}

				case ScreenLayout.TripleVertical:
					{
						m_spacing = 12f;
						m_bevel = 3f;
						float x10 = 0f;
						float y10 = 0f;
						float x11 = ActualSize.X / 2f + m_spacing / 2f;
						float y11 = 0f;
						float x12 = ActualSize.X / 2f + m_spacing / 2f;
						float y12 = ActualSize.Y / 2f + m_spacing / 2f;
						float x13 = ActualSize.X / 2f - m_spacing / 2f;
						float y13 = ActualSize.Y;
						float y14 = ActualSize.Y / 2f - m_spacing / 2f;
						float num4 = 0.5f;
						ArrangeChildWidgetInCell(new Vector2(x10, y10), new Vector2(x10, y10) + new Vector2(x13, y13) / num4, Children[0]);
						Children[0].LayoutTransform = Matrix.CreateScale(num4, num4, 1f);
						ArrangeChildWidgetInCell(new Vector2(x11, y11), new Vector2(x11, y11) + new Vector2(x13, y14) / num4, Children[1]);
						Children[1].LayoutTransform = Matrix.CreateScale(num4, num4, 1f);
						ArrangeChildWidgetInCell(new Vector2(x12, y12), new Vector2(x12, y12) + new Vector2(x13, y14) / num4, Children[2]);
						Children[2].LayoutTransform = Matrix.CreateScale(num4, num4, 1f);
						break;
					}

				case ScreenLayout.TripleHorizontal:
					{
						m_spacing = 12f;
						m_bevel = 3f;
						float x14 = 0f;
						float y15 = 0f;
						float x15 = 0f;
						float y16 = ActualSize.Y / 2f + m_spacing / 2f;
						float x16 = ActualSize.X / 2f + m_spacing / 2f;
						float y17 = ActualSize.Y / 2f + m_spacing / 2f;
						float x17 = ActualSize.X;
						float x18 = ActualSize.X / 2f - m_spacing / 2f;
						float y18 = ActualSize.Y / 2f - m_spacing / 2f;
						float num5 = 0.5f;
						ArrangeChildWidgetInCell(new Vector2(x14, y15), new Vector2(x14, y15) + new Vector2(x17, y18) / num5, Children[0]);
						Children[0].LayoutTransform = Matrix.CreateScale(num5, num5, 1f);
						ArrangeChildWidgetInCell(new Vector2(x15, y16), new Vector2(x15, y16) + new Vector2(x18, y18) / num5, Children[1]);
						Children[1].LayoutTransform = Matrix.CreateScale(num5, num5, 1f);
						ArrangeChildWidgetInCell(new Vector2(x16, y17), new Vector2(x16, y17) + new Vector2(x18, y18) / num5, Children[2]);
						Children[2].LayoutTransform = Matrix.CreateScale(num5, num5, 1f);
						break;
					}

				case ScreenLayout.TripleEven:
					{
						m_spacing = 12f;
						m_bevel = 3f;
						float x19 = 0f;
						float y19 = 0f;
						float x20 = ActualSize.X / 2f + m_spacing / 2f;
						float y20 = 0f;
						float x21 = ActualSize.X / 4f + m_spacing / 4f;
						float y21 = ActualSize.Y / 2f + m_spacing / 2f;
						float x22 = ActualSize.X / 2f - m_spacing / 2f;
						float y22 = ActualSize.Y / 2f - m_spacing / 2f;
						float num6 = 0.5f;
						ArrangeChildWidgetInCell(new Vector2(x19, y19), new Vector2(x19, y19) + new Vector2(x22, y22) / num6, Children[0]);
						Children[0].LayoutTransform = Matrix.CreateScale(num6, num6, 1f);
						ArrangeChildWidgetInCell(new Vector2(x20, y20), new Vector2(x20, y20) + new Vector2(x22, y22) / num6, Children[1]);
						Children[1].LayoutTransform = Matrix.CreateScale(num6, num6, 1f);
						ArrangeChildWidgetInCell(new Vector2(x21, y21), new Vector2(x21, y21) + new Vector2(x22, y22) / num6, Children[2]);
						Children[2].LayoutTransform = Matrix.CreateScale(num6, num6, 1f);
						break;
					}

				case ScreenLayout.TripleOpposite:
					{
						m_spacing = 12f;
						m_bevel = 3f;
						float x23 = 0f;
						float y23 = 0f;
						float x24 = ActualSize.X / 2f + m_spacing / 2f;
						float y24 = 0f;
						float x25 = ActualSize.X / 2f + m_spacing / 2f;
						float y25 = ActualSize.Y / 2f + m_spacing / 2f;
						float x26 = ActualSize.X / 2f - m_spacing / 2f;
						float y26 = ActualSize.Y;
						float y27 = ActualSize.Y / 2f - m_spacing / 2f;
						float num7 = 0.5f;
						ArrangeChildWidgetInCell(new Vector2(x23, y23), new Vector2(x23, y23) + new Vector2(x26, y26) / num7, Children[0]);
						Children[0].LayoutTransform = new Matrix(0f, num7, 0f, 0f, 0f - num7, 0f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f);
						ArrangeChildWidgetInCell(new Vector2(x24, y24), new Vector2(x24, y24) + new Vector2(x26, y27) / num7, Children[1]);
						Children[1].LayoutTransform = new Matrix(0f - num7, 0f, 0f, 0f, 0f, 0f - num7, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f);
						ArrangeChildWidgetInCell(new Vector2(x25, y25), new Vector2(x25, y25) + new Vector2(x26, y27) / num7, Children[2]);
						Children[2].LayoutTransform = new Matrix(num7, 0f, 0f, 0f, 0f, num7, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f);
						break;
					}

				case ScreenLayout.Quadruple:
					{
						m_spacing = 12f;
						m_bevel = 3f;
						float x27 = 0f;
						float y28 = 0f;
						float x28 = ActualSize.X / 2f + m_spacing / 2f;
						float y29 = 0f;
						float x29 = 0f;
						float y30 = ActualSize.Y / 2f + m_spacing / 2f;
						float x30 = ActualSize.X / 2f + m_spacing / 2f;
						float y31 = ActualSize.Y / 2f + m_spacing / 2f;
						float x31 = ActualSize.X / 2f - m_spacing / 2f;
						float y32 = ActualSize.Y / 2f - m_spacing / 2f;
						float num8 = 0.5f;
						ArrangeChildWidgetInCell(new Vector2(x27, y28), new Vector2(x27, y28) + new Vector2(x31, y32) / num8, Children[0]);
						Children[0].LayoutTransform = Matrix.CreateScale(num8, num8, 1f);
						ArrangeChildWidgetInCell(new Vector2(x28, y29), new Vector2(x28, y29) + new Vector2(x31, y32) / num8, Children[1]);
						Children[1].LayoutTransform = Matrix.CreateScale(num8, num8, 1f);
						ArrangeChildWidgetInCell(new Vector2(x29, y30), new Vector2(x29, y30) + new Vector2(x31, y32) / num8, Children[2]);
						Children[2].LayoutTransform = Matrix.CreateScale(num8, num8, 1f);
						ArrangeChildWidgetInCell(new Vector2(x30, y31), new Vector2(x30, y31) + new Vector2(x31, y32) / num8, Children[3]);
						Children[3].LayoutTransform = Matrix.CreateScale(num8, num8, 1f);
						break;
					}

				case ScreenLayout.QuadrupleOpposite:
					{
						m_spacing = 12f;
						m_bevel = 3f;
						float x32 = 0f;
						float y33 = 0f;
						float x33 = ActualSize.X / 2f + m_spacing / 2f;
						float y34 = 0f;
						float x34 = 0f;
						float y35 = ActualSize.Y / 2f + m_spacing / 2f;
						float x35 = ActualSize.X / 2f + m_spacing / 2f;
						float y36 = ActualSize.Y / 2f + m_spacing / 2f;
						float x36 = ActualSize.X / 2f - m_spacing / 2f;
						float y37 = ActualSize.Y / 2f - m_spacing / 2f;
						float num9 = 0.5f;
						ArrangeChildWidgetInCell(new Vector2(x32, y33), new Vector2(x32, y33) + new Vector2(x36, y37) / num9, Children[0]);
						Children[0].LayoutTransform = new Matrix(0f - num9, 0f, 0f, 0f, 0f, 0f - num9, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f);
						ArrangeChildWidgetInCell(new Vector2(x33, y34), new Vector2(x33, y34) + new Vector2(x36, y37) / num9, Children[1]);
						Children[1].LayoutTransform = new Matrix(0f - num9, 0f, 0f, 0f, 0f, 0f - num9, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f);
						ArrangeChildWidgetInCell(new Vector2(x34, y35), new Vector2(x34, y35) + new Vector2(x36, y37) / num9, Children[2]);
						Children[2].LayoutTransform = new Matrix(num9, 0f, 0f, 0f, 0f, num9, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f);
						ArrangeChildWidgetInCell(new Vector2(x35, y36), new Vector2(x35, y36) + new Vector2(x36, y37) / num9, Children[3]);
						Children[3].LayoutTransform = new Matrix(num9, 0f, 0f, 0f, 0f, num9, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f);
						break;
					}
			}
		}

		public override void Overdraw()
		{
			Color color = new Color(181, 172, 154) * GlobalColorTransform;
			float num = 0.6f;
			float directionalLight = 0.4f;
			FlatBatch2D flatBatch2D = WidgetsManager.PrimitivesRenderer2D.FlatBatch(0, null, null, null);
			int count = flatBatch2D.TriangleVertices.Count;
			switch (Children.Count == 1 ? SettingsManager.ScreenLayout1 :
				Children.Count == 2 ? SettingsManager.ScreenLayout2 :
				Children.Count == 3 ? SettingsManager.ScreenLayout3 : SettingsManager.ScreenLayout4)
			{
				case ScreenLayout.Single: break;
				case ScreenLayout.DoubleVertical:
				case ScreenLayout.DoubleOpposite:
					BevelledRectangleWidget.QueueBevelledRectangle(null, flatBatch2D, new Vector2(ActualSize.X / 2f - m_spacing / 2f, -100f), new Vector2(ActualSize.X / 2f + m_spacing / 2f, ActualSize.Y + 100f), 0f, m_bevel, color, color, Color.Transparent, num, directionalLight, 0f);
					break;
				case ScreenLayout.DoubleHorizontal:
					BevelledRectangleWidget.QueueBevelledRectangle(null, flatBatch2D, new Vector2(-100f, ActualSize.Y / 2f - m_spacing / 2f), new Vector2(ActualSize.X + 100f, ActualSize.Y / 2f + m_spacing / 2f), 0f, m_bevel, color, color, Color.Transparent, num, directionalLight, 0f);
					break;
				case ScreenLayout.TripleVertical:
				case ScreenLayout.TripleOpposite:
					{
						float x = -100f;
						float x2 = ActualSize.X / 2f - m_spacing / 2f + m_bevel;
						float x3 = ActualSize.X / 2f + m_spacing / 2f - m_bevel;
						float x4 = ActualSize.X + 100f;
						float y = -100f;
						float y2 = ActualSize.Y / 2f - m_spacing / 2f + m_bevel;
						float y3 = ActualSize.Y / 2f + m_spacing / 2f - m_bevel;
						float y4 = ActualSize.Y + 100f;
						BevelledRectangleWidget.QueueBevelledRectangle(null, flatBatch2D, new Vector2(x, y), new Vector2(x2, y4), 0f, 0f - m_bevel, Color.Transparent, color, Color.Transparent, num, directionalLight, 0f);
						BevelledRectangleWidget.QueueBevelledRectangle(null, flatBatch2D, new Vector2(x3, y), new Vector2(x4, y2), 0f, 0f - m_bevel, Color.Transparent, color, Color.Transparent, num, directionalLight, 0f);
						BevelledRectangleWidget.QueueBevelledRectangle(null, flatBatch2D, new Vector2(x3, y3), new Vector2(x4, y4), 0f, 0f - m_bevel, Color.Transparent, color, Color.Transparent, num, directionalLight, 0f);
						Color color2 = color * new Color(num, num, num, 1f);
						flatBatch2D.QueueQuad(new Vector2(x2, y), new Vector2(x3, y4), 0f, color2);
						flatBatch2D.QueueQuad(new Vector2(x3, y2), new Vector2(x4, y3), 0f, color2);
						break;
					}

				case ScreenLayout.TripleHorizontal:
					{
						float x5 = -100f;
						float x6 = ActualSize.X / 2f - m_spacing / 2f + m_bevel;
						float x7 = ActualSize.X / 2f + m_spacing / 2f - m_bevel;
						float x8 = ActualSize.X + 100f;
						float y5 = -100f;
						float y6 = ActualSize.Y / 2f - m_spacing / 2f + m_bevel;
						float y7 = ActualSize.Y / 2f + m_spacing / 2f - m_bevel;
						float y8 = ActualSize.Y + 100f;
						BevelledRectangleWidget.QueueBevelledRectangle(null, flatBatch2D, new Vector2(x5, y5), new Vector2(x8, y6), 0f, 0f - m_bevel, Color.Transparent, color, Color.Transparent, num, directionalLight, 0f);
						BevelledRectangleWidget.QueueBevelledRectangle(null, flatBatch2D, new Vector2(x5, y7), new Vector2(x6, y8), 0f, 0f - m_bevel, Color.Transparent, color, Color.Transparent, num, directionalLight, 0f);
						BevelledRectangleWidget.QueueBevelledRectangle(null, flatBatch2D, new Vector2(x7, y7), new Vector2(x8, y8), 0f, 0f - m_bevel, Color.Transparent, color, Color.Transparent, num, directionalLight, 0f);
						Color color3 = color * new Color(num, num, num, 1f);
						flatBatch2D.QueueQuad(new Vector2(x5, y6), new Vector2(x8, y7), 0f, color3);
						flatBatch2D.QueueQuad(new Vector2(x6, y7), new Vector2(x7, y8), 0f, color3);
						break;
					}

				case ScreenLayout.TripleEven:
					{
						float x9 = -100f;
						float x10 = ActualSize.X / 2f - m_spacing / 2f + m_bevel;
						float x11 = ActualSize.X / 2f + m_spacing / 2f - m_bevel;
						float x12 = ActualSize.X + 100f;
						float x13 = ActualSize.X / 4f;
						float x14 = ActualSize.X * 3f / 4f;
						float y9 = -100f;
						float y10 = ActualSize.Y / 2f - m_spacing / 2f + m_bevel;
						float y11 = ActualSize.Y / 2f + m_spacing / 2f - m_bevel;
						float y12 = ActualSize.Y + 100f;
						BevelledRectangleWidget.QueueBevelledRectangle(null, flatBatch2D, new Vector2(x9, y9), new Vector2(x10, y10), 0f, 0f - m_bevel, Color.Transparent, color, Color.Transparent, num, directionalLight, 0f);
						BevelledRectangleWidget.QueueBevelledRectangle(null, flatBatch2D, new Vector2(x11, y9), new Vector2(x12, y10), 0f, 0f - m_bevel, Color.Transparent, color, Color.Transparent, num, directionalLight, 0f);
						BevelledRectangleWidget.QueueBevelledRectangle(null, flatBatch2D, new Vector2(x13, y11), new Vector2(x14, y12), 0f, 0f - m_bevel, Color.Transparent, color, Color.Transparent, num, directionalLight, 0f);
						Color color4 = color * new Color(num, num, num, 1f);
						flatBatch2D.QueueQuad(new Vector2(x10, y9), new Vector2(x11, y10), 0f, color4);
						flatBatch2D.QueueQuad(new Vector2(x9, y10), new Vector2(x12, y11), 0f, color4);
						flatBatch2D.QueueQuad(new Vector2(x9, y11), new Vector2(x13, y12), 0f, color4);
						flatBatch2D.QueueQuad(new Vector2(x14, y11), new Vector2(x12, y12), 0f, color4);
						break;
					}

				default:
					{
						float x15 = -100f;
						float x16 = ActualSize.X / 2f - m_spacing / 2f + m_bevel;
						float x17 = ActualSize.X / 2f + m_spacing / 2f - m_bevel;
						float x18 = ActualSize.X + 100f;
						float y13 = -100f;
						float y14 = ActualSize.Y / 2f - m_spacing / 2f + m_bevel;
						float y15 = ActualSize.Y / 2f + m_spacing / 2f - m_bevel;
						float y16 = ActualSize.Y + 100f;
						BevelledRectangleWidget.QueueBevelledRectangle(null, flatBatch2D, new Vector2(x15, y13), new Vector2(x16, y14), 0f, 0f - m_bevel, Color.Transparent, color, Color.Transparent, num, directionalLight, 0f);
						BevelledRectangleWidget.QueueBevelledRectangle(null, flatBatch2D, new Vector2(x17, y13), new Vector2(x18, y14), 0f, 0f - m_bevel, Color.Transparent, color, Color.Transparent, num, directionalLight, 0f);
						BevelledRectangleWidget.QueueBevelledRectangle(null, flatBatch2D, new Vector2(x15, y15), new Vector2(x16, y16), 0f, 0f - m_bevel, Color.Transparent, color, Color.Transparent, num, directionalLight, 0f);
						BevelledRectangleWidget.QueueBevelledRectangle(null, flatBatch2D, new Vector2(x17, y15), new Vector2(x18, y16), 0f, 0f - m_bevel, (Children.Count == 3) ? color : Color.Transparent, color, Color.Transparent, num, directionalLight, 0f);
						Color color5 = color * new Color(num, num, num, 1f);
						flatBatch2D.QueueQuad(new Vector2(x16, y13), new Vector2(x17, y16), 0f, color5);
						flatBatch2D.QueueQuad(new Vector2(x15, y14), new Vector2(x18, y15), 0f, color5);
						break;
					}
			}
			flatBatch2D.TransformTriangles(GlobalTransform, count, -1);
		}
	}
}