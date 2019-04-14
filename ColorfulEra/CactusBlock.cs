using System;
using System.Collections.Generic;
using System.Text;
using Engine;
using Engine.Graphics;
using TemplatesDatabase;
namespace Game
{
	public class CactusBlock : PaintedMeshBlock
	{
		public const int Index = 127;

		public override void Initialize()
		{
			Model model = ContentManager.Get<Model>("Models/Cactus");
			Matrix boneAbsoluteTransform = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("Cactus", true).ParentBone);
			BlockMesh blockMesh = m_blockMesh;
			ReadOnlyList<ModelMeshPart> meshParts = model.FindMesh("Cactus", true).MeshParts;
			blockMesh.AppendModelMeshPart(meshParts[0], boneAbsoluteTransform * Matrix.CreateTranslation(0.5f, 0f, 0.5f), false, false, false, false, Color.White);
			BlockMesh standaloneBlockMesh = m_standaloneBlockMesh;
			meshParts = model.FindMesh("Cactus", true).MeshParts;
			standaloneBlockMesh.AppendModelMeshPart(meshParts[0], boneAbsoluteTransform * Matrix.CreateTranslation(0f, -0.5f, 0f), false, false, false, false, Color.White);
			base.Initialize();
		}
		public override CraftingRecipe GetAdHocCraftingRecipe(SubsystemTerrain terrain, string[] ingredients, float heatLevel)
		{
			if (heatLevel != 0f)
				return null;
			int i = 0, count = 0, index = 0;
			for (; i < ingredients.Length; i++)
			{
				if (!string.IsNullOrEmpty(ingredients[i]))
				{
					index = i;
					count++;
				}
			}
			if (count != 1)
				return null;
			string craftingId;
			int? data;
			CraftingRecipesManager.DecodeIngredient(ingredients[i], out craftingId, out data);
			count = data.HasValue ? data.Value : 0;
			return new CraftingRecipe
			{
				ResultValue = Terrain.ReplaceData(Index, SetMaxHeight(count, GetMaxHeight(count) + 1)),
				ResultCount = 1,
				Description = "Make higher cactus",
				Ingredients = (string[])ingredients.Clone()
			};
		}
		public override string GetDisplayName(SubsystemTerrain subsystemTerrain, int value)
		{
			int data = Terrain.ExtractData(value);
			return SubsystemPalette.GetName(subsystemTerrain, GetColor(data), DefaultDisplayName + GetMaxHeight(data));
		}
		public override bool ShouldAvoid(int value)
		{
			return true;
		}
		public static int GetMaxHeight(int data)
		{
			return (Terrain.ExtractData(data) >> 5 & 63) + 1;
		}
		public static int SetMaxHeight(int data, int height)
		{
			return data & -2017 | ((height < 1 ? 0 : height - 1) & 63) << 5;
		}
	}
	public abstract class PaintedGunpowderKegBlock : GunpowderKegBlock
	{
		protected PaintedGunpowderKegBlock(string modelName, bool isIncendiary) : base(modelName, isIncendiary)
		{
		}
	}
	public class SubsystemHCactusBlockBehavior : SubsystemCactusBlockBehavior
	{
		public override void OnPoll(int value, int x, int y, int z, int pollPass)
		{
			if (m_subsystemGameInfo.WorldSettings.EnvironmentBehaviorMode == EnvironmentBehaviorMode.Living)
			{
				var terrain = SubsystemTerrain.Terrain;
				int content = Terrain.ExtractContents(value), cellValue = terrain.GetCellValue(x, y + 1, z);
				if (Terrain.ExtractContents(cellValue) == 0 && Terrain.ExtractLight(cellValue) >= 12)
				{
					int data = Terrain.ExtractData(value), maxHeight = CactusBlock.GetMaxHeight(data) - 1;
					if (maxHeight < 1)
						return;
					if (m_random.UniformFloat(0f, 1f) < 0.25f)
						m_toUpdate[new Point3(x, y + 1, z)] = Terrain.MakeBlockValue(127, 0, CactusBlock.SetMaxHeight(data, maxHeight));
				}
			}
		}
	}
}
