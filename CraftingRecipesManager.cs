using Engine;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using XmlUtilities;

namespace Game
{
	public static class CraftingRecipesManager
	{
		static List<CraftingRecipe> m_recipes;
		public static ReadOnlyList<CraftingRecipe> Recipes
		{
			get { return new ReadOnlyList<CraftingRecipe>(m_recipes); }
		}
		public static void Initialize()
		{
			m_recipes = new List<CraftingRecipe>();
			for (var i = ContentManager.CombineXml(ContentManager.Get<XElement>("CraftingRecipes"), ModsManager.GetEntries(".cr"), "Description", "Result", "Recipes").Descendants("Recipe").GetEnumerator(); i.MoveNext();)
			{
				var descendant = i.Current;
				var recipe = new CraftingRecipe();
				var attributeValue1 = XmlUtils.GetAttributeValue<string>(descendant, "Result");
				recipe.ResultValue = DecodeResult(attributeValue1);
				recipe.ResultCount = XmlUtils.GetAttributeValue<int>(descendant, "ResultCount");
				var attributeValue2 = XmlUtils.GetAttributeValue(descendant, "Remains", string.Empty);
				if (!string.IsNullOrEmpty(attributeValue2))
				{
					recipe.RemainsValue = DecodeResult(attributeValue2);
					recipe.RemainsCount = XmlUtils.GetAttributeValue<int>(descendant, "RemainsCount");
				}

				recipe.RequiredHeatLevel = XmlUtils.GetAttributeValue<float>(descendant, "RequiredHeatLevel");
				recipe.Description = XmlUtils.GetAttributeValue<string>(descendant, "Description");
				if (recipe.ResultCount >
					BlocksManager.Blocks[Terrain.ExtractContents(recipe.ResultValue)].MaxStacking)
					throw new InvalidOperationException(string.Format(
						"In recipe for \"{0}\" ResultCount is larger than max stacking of result block.",
						new object[1] {attributeValue1}));
				if (recipe.RemainsValue != 0 && recipe.RemainsCount >
					BlocksManager.Blocks[Terrain.ExtractContents(recipe.RemainsValue)].MaxStacking)
					throw new InvalidOperationException(string.Format(
						"In Recipe for \"{0}\" RemainsCount is larger than max stacking of remains block.",
						new object[1] {attributeValue2}));
				var dictionary = new Dictionary<char, string>();
				foreach (var xattribute in descendant.Attributes().Where(a =>
				a.Name.LocalName.Length == 1 && char.IsLower(a.Name.LocalName[0])))
				{
					string craftingId;
					int? data;
					DecodeIngredient(xattribute.Value, out craftingId, out data);
					if (BlocksManager.FindBlocksByCraftingId(craftingId).Length == 0)
						throw new InvalidOperationException(string.Format("Block with craftingId \"{0}\" not found.",
							new object[1] {xattribute.Value}));
					if (data.HasValue && (data.Value < 0 || data.Value > 262143))
						throw new InvalidOperationException(string.Format(
							"Data in recipe ingredient \"{0}\" must be between 0 and 0x3FFFF.",
							new object[1] {xattribute.Value}));
					dictionary.Add(xattribute.Name.LocalName[0], xattribute.Value);
				}

				var strArray = descendant.Value.Trim().Split('\n');
				for (var index1 = 0; index1 < strArray.Length; ++index1)
				{
					var num1 = strArray[index1].IndexOf('"');
					var num2 = strArray[index1].LastIndexOf('"');
					if (num1 < 0 || num2 < 0 || num2 <= num1)
						throw new InvalidOperationException("Invalid recipe line.");
					var str1 = strArray[index1].Substring(num1 + 1, num2 - num1 - 1);
					for (var index2 = 0; index2 < str1.Length; ++index2)
					{
						var c = str1[index2];
						if (char.IsLower(c))
							recipe.Ingredients[index2 + index1 * 3] = dictionary[c];
					}
				}

				m_recipes.Add(recipe);
			}

			var blocks = BlocksManager.Blocks;
			for (int i = 0; i < blocks.Length; i++)
				m_recipes.AddRange(blocks[i].GetProceduralCraftingRecipes());
			m_recipes.Sort((r1, r2) =>
			{
				return Comparer<int>.Default.Compare(r2.Ingredients.Count(s => !string.IsNullOrEmpty(s)), r1.Ingredients.Count(s => !string.IsNullOrEmpty(s)));
			});
		}

		public static CraftingRecipe FindMatchingRecipe(SubsystemTerrain terrain, string[] ingredients, float heatLevel)
		{
			var blocks = BlocksManager.Blocks;
			int i = 0;
			CraftingRecipe recipe;
			for (; i < blocks.Length; i++)
			{
				recipe = blocks[i].GetAdHocCraftingRecipe(terrain, ingredients, heatLevel);
				if (recipe != null && heatLevel >= recipe.RequiredHeatLevel && MatchRecipe(recipe.Ingredients, ingredients))
					return recipe;
			}
			int RecipesCount = Recipes.Count;
			for (i = 0; i < RecipesCount; i++) {
				recipe = Recipes[i];
				if (heatLevel >= recipe.RequiredHeatLevel && MatchRecipe(recipe.Ingredients, ingredients))
					return recipe;
			}
			return null;
		}

		public static int DecodeResult(string result)
		{
			var strArray = result.Split(':');
			return Terrain.MakeBlockValue(BlocksManager.FindBlockByTypeName(strArray[0], true).BlockIndex, 0,
				strArray.Length >= 2 ? int.Parse(strArray[1], CultureInfo.InvariantCulture) : 0);
		}

		public static void DecodeIngredient(string ingredient, out string craftingId, out int? data)
		{
			var strArray = ingredient.Split(':');
			craftingId = strArray[0];
			data = strArray.Length >= 2 ? int.Parse(strArray[1], CultureInfo.InvariantCulture) : new int?();
		}

		static bool MatchRecipe(string[] requiredIngredients, string[] actualIngredients)
		{
			var transformedIngredients = new string[9];
			for (int index1 = 0; index1 < 2; ++index1)
			for (int shiftY = -3; shiftY <= 3; ++shiftY)
			for (int shiftX = -3; shiftX <= 3; ++shiftX)
			{
				var flip = index1 != 0;
				if (TransformRecipe(transformedIngredients, requiredIngredients, shiftX, shiftY, flip))
				{
					var flag = true;
					for (var index2 = 0; index2 < 9; ++index2)
						if (!CompareIngredients(transformedIngredients[index2], actualIngredients[index2]))
						{
							flag = false;
							break;
						}

					if (flag)
						return true;
				}
			}

			return false;
		}

		static bool TransformRecipe(string[] transformedIngredients, string[] ingredients, int shiftX,
			int shiftY, bool flip)
		{
			for (int index = 0; index < 9; ++index)
				transformedIngredients[index] = null;
			for (int index1 = 0; index1 < 3; ++index1)
			for (int index2 = 0; index2 < 3; ++index2)
			{
				int num1 = (flip ? 3 - index2 - 1 : index2) + shiftX;
				int num2 = index1 + shiftY;
				var ingredient = ingredients[index2 + index1 * 3];
				if (num1 >= 0 && num2 >= 0 && num1 < 3 && num2 < 3)
					transformedIngredients[num1 + num2 * 3] = ingredient;
				else if (!string.IsNullOrEmpty(ingredient))
					return false;
			}

			return true;
		}

		static bool CompareIngredients(string requiredIngredient, string actualIngredient)
		{
			if (requiredIngredient == null)
				return actualIngredient == null;
			if (actualIngredient == null)
				return requiredIngredient == null;
			string craftingId1;
			int? data1;
			DecodeIngredient(requiredIngredient, out craftingId1, out data1);
			string craftingId2;
			int? data2;
			DecodeIngredient(actualIngredient, out craftingId2, out data2);
			if (!data2.HasValue)
				throw new InvalidOperationException("Actual ingredient data not specified.");
			if (craftingId1 != craftingId2)
				return false;
			if (!data1.HasValue)
				return true;
			return data1.Value == data2.Value;
		}
	}
}