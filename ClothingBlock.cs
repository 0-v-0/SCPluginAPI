using Engine;
using Engine.Graphics;
using System.Collections.Generic;
using System.Xml.Linq;
using XmlUtilities;

namespace Game
{
	public class ClothingBlock : Block
	{
		static ClothingData[] m_clothingData;
		BlockMesh m_innerMesh;
		BlockMesh m_outerMesh;
		// Replace ClothingBlock.Initialize
		public override void Initialize()
		{
			var num = 0;
			var dictionary = new Dictionary<int, ClothingData>();
			for (var i = ContentManager.CombineXml(ContentManager.Get<XElement>("Clothes"), ModsManager.GetEntries(".clo"), "Index", "DisplayName", "Clothes").Elements().GetEnumerator(); i.MoveNext();)
			{
				var element = i.Current;
				var clothingData = new ClothingData
				{
					Index = XmlUtils.GetAttributeValue<int>(element, "Index"),
					DisplayIndex = num++,
					DisplayName = XmlUtils.GetAttributeValue<string>(element, "DisplayName"),
					Slot = XmlUtils.GetAttributeValue<ClothingSlot>(element, "Slot"),
					ArmorProtection = XmlUtils.GetAttributeValue<float>(element, "ArmorProtection"),
					Sturdiness = XmlUtils.GetAttributeValue<float>(element, "Sturdiness"),
					Insulation = XmlUtils.GetAttributeValue<float>(element, "Insulation"),
					MovementSpeedFactor = XmlUtils.GetAttributeValue<float>(element, "MovementSpeedFactor"),
					SteedMovementSpeedFactor = XmlUtils.GetAttributeValue<float>(element, "SteedMovementSpeedFactor"),
					DensityModifier = XmlUtils.GetAttributeValue<float>(element, "DensityModifier"),
					IsOuter = XmlUtils.GetAttributeValue<bool>(element, "IsOuter"),
					CanBeDyed = XmlUtils.GetAttributeValue<bool>(element, "CanBeDyed"),
					Layer = XmlUtils.GetAttributeValue<int>(element, "Layer"),
					PlayerLevelRequired = XmlUtils.GetAttributeValue<int>(element, "PlayerLevelRequired"),
					Texture = ContentManager.Get<Texture2D>(XmlUtils.GetAttributeValue<string>(element, "TextureName")),
					ImpactSoundsFolder = XmlUtils.GetAttributeValue<string>(element, "ImpactSoundsFolder"),
					Description = XmlUtils.GetAttributeValue<string>(element, "Description")
				};
				dictionary[clothingData.Index] = clothingData;
			}

			m_clothingData = new ClothingData[dictionary.Count];
			int index;
			for (index = 0; index < dictionary.Count; ++index)
				m_clothingData[index] = dictionary[index];
			var playerModel = CharacterSkinsManager.GetPlayerModel(PlayerClass.Male);
			var absoluteTransforms1 = new Matrix[playerModel.Bones.Count];
			playerModel.CopyAbsoluteBoneTransformsTo(absoluteTransforms1);
			index = playerModel.FindBone("Hand1", true).Index;
			int index2 = playerModel.FindBone("Hand2", true).Index;
			absoluteTransforms1[index] = Matrix.CreateRotationY(0.1f) * absoluteTransforms1[index];
			absoluteTransforms1[index2] = Matrix.CreateRotationY(-0.1f) * absoluteTransforms1[index2];
			m_innerMesh = new BlockMesh();
			foreach (var mesh in playerModel.Meshes)
			{
				var matrix = absoluteTransforms1[mesh.ParentBone.Index];
				foreach (var meshPart in mesh.MeshParts)
				{
					var color = Color.White * 0.8f;
					color.A = byte.MaxValue;
					m_innerMesh.AppendModelMeshPart(meshPart, matrix, false, false, false, false, Color.White);
					m_innerMesh.AppendModelMeshPart(meshPart, matrix, false, true, false, true, color);
				}
			}

			var outerClothingModel = CharacterSkinsManager.GetOuterClothingModel(PlayerClass.Male);
			var absoluteTransforms2 = new Matrix[outerClothingModel.Bones.Count];
			outerClothingModel.CopyAbsoluteBoneTransformsTo(absoluteTransforms2);
			var index3 = outerClothingModel.FindBone("Leg1", true).Index;
			var index4 = outerClothingModel.FindBone("Leg2", true).Index;
			absoluteTransforms2[index3] = Matrix.CreateTranslation(-0.02f, 0.0f, 0.0f) * absoluteTransforms2[index3];
			absoluteTransforms2[index4] = Matrix.CreateTranslation(0.02f, 0.0f, 0.0f) * absoluteTransforms2[index4];
			m_outerMesh = new BlockMesh();
			foreach (var mesh in outerClothingModel.Meshes)
			{
				var matrix = absoluteTransforms2[mesh.ParentBone.Index];
				foreach (var meshPart in mesh.MeshParts)
				{
					var color = Color.White * 0.8f;
					color.A = byte.MaxValue;
					m_outerMesh.AppendModelMeshPart(meshPart, matrix, false, false, false, false, Color.White);
					m_outerMesh.AppendModelMeshPart(meshPart, matrix, false, true, false, true, color);
				}
			}

			base.Initialize();
		}

		public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometrySubsets geometry, int value, int x, int y, int z)
		{
		}

		public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
		{
		}
	}
}