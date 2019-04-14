using Engine;
using GameEntitySystem;
using System;

namespace Game
{
	public class SignElectricElement : ElectricElement
	{
		private bool m_isMessageAllowed = true;
		private double? m_lastMessageTime;
		public SignElectricElement(SubsystemElectricity subsystemElectricity, CellFace cellFace) : base(subsystemElectricity, cellFace)
		{
		}
		private string MoveTo(string[] Lines, string p, bool sleep)
		{
			string text = null;
			if (GameManager.Project != null)
			{
				SubsystemPlayer subsystemPlayer = GameManager.Project.FindSubsystem<SubsystemPlayer>(true);
				if (subsystemPlayer.ComponentPlayer != null)
				{
					//string[] a = ;bool.Parse()
					if (!string.IsNullOrEmpty(Lines[3]))
					subsystemPlayer.ComponentPlayer.ComponentBody.Position = new Vector3(float.Parse(Lines[1]), float.Parse(Lines[3]), float.Parse(Lines[2]));
					text = p;
					if (sleep)
						subsystemPlayer.ComponentPlayer.ComponentSleep.Sleep();
				}
			}
			return text;
		}
		private int GetShow()
		{
			Vector3 v = base.SubsystemElectricity.Project.FindSubsystem<SubsystemDrawing>(true).ViewDirection;
			if ((double)v.X > 0.68) return 3;
			if ((double)v.X < -0.68) return 1;
			if ((double)v.Z > 0.68) return 4;
			if ((double)v.Z < -0.68) return 2;
			return 0;
		}
		private string MyDrawBlock(SignData signData, string p)
		{
			if (GameManager.Project != null)
			{
				ComponentPlayer componentPlayer = GameManager.Project.FindSubsystem<SubsystemPlayer>(true).ComponentPlayer;
				if (componentPlayer != null)
				{
					Vector3 position = componentPlayer.ComponentBody.Position;
					int i = 0,
						num4 = (int)position.Y,
						num5 = 0,
						num6 = 0,
						num7 = int.Parse(signData.Lines[3]) + num4,
						num8 = 0;
					int x = (int)position.X, z = (int)position.Z;
					switch (this.GetShow())
					{
					case 1:
						num6 = x - 2;
						num5 = z - 1;
						i = num6 - int.Parse(signData.Lines[1]);
						num8 = num5 + int.Parse(signData.Lines[2]);
						break;
					case 2:
						num6 = x;
						num8 = z - 2;
						i = num6 - int.Parse(signData.Lines[1]);
						i = num8 - int.Parse(signData.Lines[2]);
						break;
					case 3:
						i = x + 1;
						num8 = z;
						num6 = i + int.Parse(signData.Lines[1]);
						num5 = num8 - int.Parse(signData.Lines[2]);
						break;
					case 4:
						i = x - 1;
						num5 = z + 1;
						num6 = i + int.Parse(signData.Lines[1]);
						num8 = num5 + int.Parse(signData.Lines[2]);
						break;
					}
					int num = int.Parse(p);
					int c = 0;
					while (i < num6)
					{
						i++;
						for (int j = num4; j < num7; )
						{
							j++;
							for (int k = num5; k < num8; c++) base.SubsystemElectricity.SubsystemTerrain.ChangeCell(i, j - 2, ++k, num);
						}
					}
					p = string.Format("Count= {0}", c);
				}
			}
			return p;
		}
		private string Drop(string[] Lines, string p)
		{
			bool b = p.EndsWith("DI");Lines = b ? Lines : p.Split(new char[]{' '});int a = int.Parse(Lines[1]);
			GameManager.Project.FindSubsystem<SubsystemPickables>(true).AddPickable(a, int.Parse(Lines[2]), b ? new Vector3((float)base.CellFaces[0].X, (float)base.CellFaces[0].Y, (float)base.CellFaces[0].Z) : new Vector3(a, int.Parse(Lines[2]), int.Parse(Lines[3])), new Vector3?(Vector3.Zero), null);
			return "Dropped " + Lines[2] + " " + BlocksManager.Blocks[a].DefaultDisplayName;
		}
		private void Explode(string[] Lines, int a, string p = null){
			SubsystemExplosions e = GameManager.Project.FindSubsystem<SubsystemExplosions>(true);
			int x = int.Parse(Lines[1]), y = int.Parse(Lines[2]), z = int.Parse(Lines[3]);
			if(string.IsNullOrEmpty(p))e.TryExplodeBlock(x, y, z, a);
			else e.AddExplosion(x, y, z, float.Parse(p), a > 1, (a & 1) == 1);
		}
		private string CmdTest(SignData signData)
		{
			SubsystemDrawing subsystemDrawing = base.SubsystemElectricity.Project.FindSubsystem<SubsystemDrawing>(true);
			SubsystemPlayer subsystemPlayer = GameManager.Project.FindSubsystem<SubsystemPlayer>(true);
			string str = string.Empty;
			if (subsystemPlayer.ComponentPlayer != null)
			//str = subsystemPlayer.ComponentPlayer.ComponentCreatureModel.EyeRotation.ToString();
			str = "ViewDirection: " + subsystemDrawing.ViewDirection.ToString()
				+ "\nViewPosition: " + subsystemDrawing.ViewPosition.ToString()
				+ "\nViewUp: " + subsystemDrawing.ViewUp.ToString();
			if (signData.Colors[0].PackedValue != 0)
				str += "The Colors:" + string.Join("\n",signData.Colors);
			return str;
		}
		public override bool Simulate()
		{
			bool flag = base.CalculateHighInputsCount() > 0;
			if (flag && this.m_isMessageAllowed && (!this.m_lastMessageTime.HasValue || base.SubsystemElectricity.SubsystemTime.GameTime - this.m_lastMessageTime.Value > 0.5))
			{
				this.m_isMessageAllowed = false;
				this.m_lastMessageTime = new double?(base.SubsystemElectricity.SubsystemTime.GameTime);
				SubsystemGui subsystemGui = base.SubsystemElectricity.Project.FindSubsystem<SubsystemGui>(true);
				SignData signData = base.SubsystemElectricity.Project.FindSubsystem<SubsystemSignBlockBehavior>(true).GetSignData(new Point3(base.CellFaces[0].X, base.CellFaces[0].Y, base.CellFaces[0].Z));
				if (signData != null)
				{
					string[] Lines = signData.Lines;
					int cellContents = base.SubsystemElectricity.SubsystemTerrain.TerrainData.GetCellContents(base.CellFaces[0].X, base.CellFaces[0].Y, base.CellFaces[0].Z);
					string text = null;
					if (cellContents == 242 || cellContents == 243)
					{
						string str = Lines[0].Substring(0, 2),
						p = Lines[0].Substring(2, text.Length - 2);
						if (str == "MT")
							text = this.MoveTo(Lines, p, true);
						else if (str == "DM")
							text = this.MyDrawBlock(signData, p);
						else if (str == "DI")
							text = this.Drop(Lines, p);
						else if (str.StartsWith("D"))
						{
							//= p.Split(new char[]{' '})[0];str[0]=='D')
							this.Explode(Lines, 1, p);
						}
						else if (str == "AE")
							this.Explode(Lines, 0, p);
						else if (str == "NE")
							this.Explode(Lines, 1, p);
						else if (str == "IE")
							this.Explode(Lines, 2, p);
						else if (str == "NI")
							this.Explode(Lines, 3, p);
						else if (str == "EB")
							this.Explode(Lines, int.Parse(p));
						else if (str == "CT")
							text = this.CmdTest(signData);
					}
					if (text == null)text = string.Join("\n", signData.Lines).Trim(new char[]{'\n'}).Replace("\\\n", "");
					subsystemGui.DisplaySmallMessage(text, true, true);
				}
			}
			if (!flag) this.m_isMessageAllowed = true;
			return false;
		}
	}
}
