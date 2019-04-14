#define Translucent

using Engine.Graphics;
using System.Collections.Generic;

namespace Game
{
	public class SubsystemTranslucentModelsRenderer : SubsystemModelsRenderer, IDrawable
	{
		public new void Draw(Camera camera, int drawOrder)
		{
			if (drawOrder == m_drawOrders[0])
			{
				ModelsDrawn = 0;
				List<ModelData>[] modelsToDraw = m_modelsToDraw;
				for (int i = 0; i < modelsToDraw.Length; i++)
				{
					modelsToDraw[i].Clear();
				}
				m_modelsToPrepare.Clear();
				foreach (ModelData value in m_componentModels.Values)
				{
					if (value.ComponentModel.Model != null)
					{
						value.ComponentModel.CalculateIsVisible(camera);
						if (value.ComponentModel.IsVisibleForCamera)
							m_modelsToPrepare.Add(value);
					}
				}
				m_modelsToPrepare.Sort();
				foreach (ModelData item in m_modelsToPrepare)
				{
					PrepareModel(item, camera);
					m_modelsToDraw[(int)item.ComponentModel.RenderingMode].Add(item);
				}
			}
			else if (!DisableDrawingModels)
			{
				if (drawOrder == m_drawOrders[1])
				{
					Display.DepthStencilState = DepthStencilState.Default;
					Display.RasterizerState = RasterizerState.CullCounterClockwiseScissor;
#if Translucent
					Display.BlendState = BlendState.Additive;
					DrawModels(camera, m_modelsToDraw[0], null);
					Display.RasterizerState = RasterizerState.CullNoneScissor;
					DrawModels(camera, m_modelsToDraw[1], 0f);
#else
					Display.BlendState = BlendState.AlphaBlend;
					DrawModels(camera, m_modelsToDraw[0], 0.1f);
					Display.RasterizerState = RasterizerState.CullNoneScissor;
					DrawModels(camera, m_modelsToDraw[1], 0.1f);
#endif
					Display.RasterizerState = RasterizerState.CullCounterClockwiseScissor;
					m_primitivesRenderer.Flush(camera.ProjectionMatrix);
				}
				else if (drawOrder == m_drawOrders[2])
				{
					Display.DepthStencilState = DepthStencilState.Default;
					Display.RasterizerState = RasterizerState.CullNoneScissor;
					Display.BlendState = BlendState.AlphaBlend;
					DrawModels(camera, m_modelsToDraw[2], null);
				}
				else if (drawOrder == m_drawOrders[3])
				{
					Display.DepthStencilState = DepthStencilState.Default;
					Display.RasterizerState = RasterizerState.CullNoneScissor;
					Display.BlendState = BlendState.AlphaBlend;
					DrawModels(camera, m_modelsToDraw[3], null);
					m_primitivesRenderer.Flush(camera.ProjectionMatrix);
				}
			}
			else
			{
				m_primitivesRenderer.Clear();
			}
		}
	}
}