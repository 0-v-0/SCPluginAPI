using Engine;
using System;
using System.Collections.Generic;
using System.Globalization;
using TemplatesDatabase;

namespace Game
{
	public class SubsystemAdjustableElectricity : SubsystemElectricity, IUpdateable
	{
		public float UpdatePeriod;

		public override void Load(ValuesDictionary valuesDictionary)
		{
			base.Load(valuesDictionary);
			UpdatePeriod = valuesDictionary.GetValue("UpdatePeriod", .01f);
		}

		public override void Save(ValuesDictionary valuesDictionary)
		{
			base.Save(valuesDictionary);
			valuesDictionary.SetValue("UpdatePeriod", UpdatePeriod);
		}

		public new void Update(float dt)
		{
			FrameStartCircuitStep = CircuitStep;
			SimulatedElectricElements = 0;
			m_remainingSimulationTime = MathUtils.Min(m_remainingSimulationTime + dt, 0.1f);
			float updatePeriod = UpdatePeriod;
			while (m_remainingSimulationTime >= updatePeriod)
			{
				UpdateElectricElements();
				int num = ++CircuitStep;
				m_remainingSimulationTime -= updatePeriod;
				m_nextStepSimulateList = null;
				if (m_futureSimulateLists.TryGetValue(CircuitStep, out Dictionary<ElectricElement, bool> value))
				{
					m_futureSimulateLists.Remove(CircuitStep);
					SimulatedElectricElements += value.Count;
					foreach (ElectricElement key in value.Keys)
						if (m_electricElements.ContainsKey(key))
							SimulateElectricElement(key);
					ReturnListToCache(value);
				}
			}
			if (DebugDrawElectrics)
				DebugDraw();
		}
	}

	public class SubsystemEUPAdjusterBlockBehavior : SubsystemBlockBehavior
	{
		public SubsystemAdjustableElectricity SubsystemAdjustableElectricity;

		public override int[] HandledBlocks => new int[0];

		public override void Load(ValuesDictionary valuesDictionary)
		{
			base.Load(valuesDictionary);
			SubsystemAdjustableElectricity = Project.FindSubsystem<SubsystemAdjustableElectricity>(true);
		}

		public override bool OnUse(Vector3 start, Vector3 direction, ComponentMiner componentMiner)
		{
			var player = componentMiner.ComponentPlayer;
			if (player == null || player.ComponentBody.ParentBody == null || player.ComponentBody.ParentBody.ImmersionFactor < 0.33f || player.ComponentBody.StandingOnValue.HasValue)
				return false;
			TerrainRaycastResult? terrainRaycastResult = componentMiner.PickTerrainForInteraction(start, direction);
			if (terrainRaycastResult.HasValue)
			{
				CellFace cellFace = terrainRaycastResult.Value.CellFace;
				if (cellFace.Face == 5 && SubsystemTerrain.Terrain.GetCellContents(cellFace.X, cellFace.Y, cellFace.Z) == Terrain.ExtractContents(componentMiner.ActiveBlockValue))
				{
					try
					{
						DialogsManager.ShowDialog(player.View.GameWidget, new TextBoxDialog("Set Electricity Update Period", null, 30, SetPeriod));
					}
					catch (Exception e)
					{
						Log.Warning(e);
					}
					return true;
				}
			}
			return false;
		}

		public void SetPeriod(string s)
		{
			if(!string.IsNullOrWhiteSpace(s))
				SubsystemAdjustableElectricity.UpdatePeriod = float.Parse(s.Trim(), CultureInfo.CurrentCulture);
		}
	}
}