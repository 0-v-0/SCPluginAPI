using Engine;
using Engine.Graphics;
using Game;
using System;
using System.Collections.Generic;
using System.Linq;
using TemplatesDatabase;
namespace G
{
	public class EggBlock : Game.EggBlock
	{
		public new const int Index = 118;
		public override void Initialize()
		{
			var dictionary = new Dictionary<int, EggType>();
			DatabaseObjectType parameterSetType = DatabaseManager.GameDatabase.ParameterSetType;
			int max = 0;
			//Guid eggParameterSetGuid = new Guid("300ff557-775f-4c7c-a88a-26655369f00b");
			foreach (DatabaseObject item in from o in DatabaseManager.GameDatabase.Database.Root.GetExplicitNestingChildren(parameterSetType, false)
											where o.Name == "CreatureEggData"
											select o)
			{
				int nestedValue = item.GetNestedValue<int>("EggTypeIndex");
				if (nestedValue >= 0)
				{
					if (dictionary.ContainsKey(nestedValue))
						throw new InvalidOperationException($"Duplicate creature egg data EggTypeIndex ({nestedValue}).");
					if (nestedValue > max) max = nestedValue;
					dictionary.Add(nestedValue, new EggType
					{
						EggTypeIndex = nestedValue,
						ShowEgg = true,//item.GetNestedValue<bool>("ShowEgg"),
						DisplayName = item.GetNestedValue<string>("DisplayName"),
						TemplateName = item.NestingParent.Name,
						NutritionalValue = item.GetNestedValue<float>("NutritionalValue"),
						Color = item.GetNestedValue<Color>("Color"),
						ScaleUV = item.GetNestedValue<Vector2>("ScaleUV"),
						SwapUV = item.GetNestedValue<bool>("SwapUV"),
						Scale = item.GetNestedValue<float>("Scale"),
						TextureSlot = item.GetNestedValue<int>("TextureSlot")
					});
				}
			}
			var eggs = new EggType[max + 1];
			var e = dictionary.GetEnumerator();
			while (e.MoveNext())
			{
				eggs[e.Current.Key] = e.Current.Value;
			}
			m_eggTypes = new List<EggType>(eggs);
			Model model = ContentManager.Get<Model>("Models/Egg");
			Matrix boneAbsoluteTransform = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("Egg", true).ParentBone);
			foreach (EggType eggType in m_eggTypes)
			{
				eggType.BlockMesh = new BlockMesh();
				eggType.BlockMesh.AppendModelMeshPart(model.FindMesh("Egg", true).MeshParts[0], boneAbsoluteTransform, false, false, false, false, eggType.Color);
				Matrix matrix = Matrix.Identity;
				if (eggType.SwapUV)
				{
					matrix.M11 = 0f;
					matrix.M12 = 1f;
					matrix.M21 = 1f;
					matrix.M22 = 0f;
				}
				matrix *= Matrix.CreateScale(0.0625f * eggType.ScaleUV.X, 0.0625f * eggType.ScaleUV.Y, 1f);
				matrix *= Matrix.CreateTranslation(eggType.TextureSlot % 16 / 16f, eggType.TextureSlot / 16 / 16f, 0f);
				eggType.BlockMesh.TransformTextureCoordinates(matrix, -1);
			}
			base.Initialize();
		}
	}
}