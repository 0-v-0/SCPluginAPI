using Engine;
using Engine.Graphics;
using Engine.Serialization;
using Game;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Game
{
	public static class BlocksManager
	{
		private static Block[] m_blocks;
		private static FluidBlock[] m_fluidBlocks;
		private static readonly List<string> m_categories = new List<string>();
		private static readonly DrawBlockEnvironmentData m_defaultEnvironmentData = new DrawBlockEnvironmentData();
		private static readonly Vector4[] m_slotTexCoords = new Vector4[256];

		public static Block[] Blocks
		{
			get
			{
				return BlocksManager.m_blocks;
			}
		}

		public static FluidBlock[] FluidBlocks
		{
			get
			{
				return BlocksManager.m_fluidBlocks;
			}
		}

		public static ReadOnlyList<string> Categories
		{
			get
			{
				return new ReadOnlyList<string>(m_categories);
			}
		}

		public static void Initialize()
		{
			CalculateSlotTexCoordTables();
			var x1 = 0;
            var dictionary = new Dictionary<int, Block>();
			foreach (var definedType in GetBlockTypes())
				if (definedType.IsSubclassOf(typeof(Block)) && !definedType.IsAbstract)
				{
					var fieldInfo = definedType.AsType().GetRuntimeFields().FirstOrDefault(fi => {
						if (fi.Name == "Index" && fi.IsPublic)
							return fi.IsStatic;
						return false;
					});
					if (fieldInfo != null && fieldInfo.FieldType == typeof(int))
					{
						var num = (int) fieldInfo.GetValue(null);
						// Removed 'Index of block type \"{0}\" conflicts with another block' error
						var instance = (Block) Activator.CreateInstance(definedType.AsType());
						dictionary[instance.BlockIndex = num] = instance;
						if (num > x1)
							x1 = num;
					}
					else
					{
						throw new InvalidOperationException(string.Format(
							"Block type \"{0}\" does not have static field Index of type int.",
							new object[1] {definedType.FullName}));
					}
				}

			m_blocks = new Block[x1 + 1];
			m_fluidBlocks = new FluidBlock[x1 + 1];
			foreach (var keyValuePair in dictionary)
			{
				m_blocks[keyValuePair.Key] = keyValuePair.Value;
				m_fluidBlocks[keyValuePair.Key] = keyValuePair.Value as FluidBlock;
			}
			for (var index = 0; index < m_blocks.Length; ++index)
				if (m_blocks[index] == null)
					m_blocks[index] = Blocks[0];
			var data = ContentManager.Get<string>("BlocksData");
			ContentManager.Dispose("BlocksData");
			LoadBlocksData(data);
			var enumerator = (new ReadOnlyList<FileEntry>(ModsManager.GetEntries(".csv"))).GetEnumerator();
			while (enumerator.MoveNext())
			{
				var reader = new StreamReader(enumerator.Current.Stream);
				try
				{
					LoadBlocksData(reader.ReadToEnd());
				}
				catch (Exception e)
				{
					Log.Warning(string.Format("\"{0}\": {1}", enumerator.Current.Filename, e));
				}
				finally
				{
					reader.Dispose();
				}
			}
			for (int i = 0, length = Blocks.Length; i < length; i++) {
				Blocks[i].Initialize();
			}
			m_categories.Add("Terrain");
			m_categories.Add("Plants");
			m_categories.Add("Construction");
			m_categories.Add("Items");
			m_categories.Add("Tools");
			m_categories.Add("Clothes");
			m_categories.Add("Electrics");
			m_categories.Add("Food");
			m_categories.Add("Spawner Eggs");
			m_categories.Add("Painted");
			m_categories.Add("Dyed");
			m_categories.Add("Fireworks");
			foreach (var block in Blocks)
			foreach (var creativeValue in block.GetCreativeValues())
			{
				var category = block.GetCategory(creativeValue);
				if (!m_categories.Contains(category))
					m_categories.Add(category);
			}
		}
		public static IEnumerable<TypeInfo> GetBlockTypes()
		{
			var list = new List<TypeInfo>();
			list.AddRange(typeof(BlocksManager).GetTypeInfo().Assembly.DefinedTypes);
			var enumerator = ModsManager.LoadedAssemblies.GetEnumerator();
			while (enumerator.MoveNext())
			{
				list.AddRange(enumerator.Current.DefinedTypes);
			}
			return list;
		}

		public static Block FindBlockByTypeName(string typeName, bool throwIfNotFound)
		{
			var block = Blocks.FirstOrDefault(b => b.GetType().Name == typeName);
			if ((block == null) & throwIfNotFound)
				throw new InvalidOperationException(string.Format("Block with type {0} not found.",
					new object[1] {typeName}));
			return block;
		}

		public static Block[] FindBlocksByCraftingId(string craftingId)
		{
			return Blocks.Where(b => b.CraftingId == craftingId).ToArray();
		}

		public static void DrawCubeBlock(PrimitivesRenderer3D primitivesRenderer, int value, Vector3 size,
			ref Matrix matrix, Color color, Color topColor, DrawBlockEnvironmentData environmentData)
		{
			environmentData = environmentData ?? m_defaultEnvironmentData;
			var texture = environmentData.SubsystemTerrain != null
				? environmentData.SubsystemTerrain.SubsystemAnimatedTextures.AnimatedBlocksTexture
				: BlocksTexturesManager.DefaultBlocksTexture;
			var texturedBatch3D = primitivesRenderer.TexturedBatch(texture, true, 0, null,
				RasterizerState.CullCounterClockwiseScissor, null, SamplerState.PointClamp);
			var s = LightingManager.LightIntensityByLightValue[environmentData.Light];
			color = Color.MultiplyColorOnly(color, s);
			topColor = Color.MultiplyColorOnly(topColor, s);
			var translation = matrix.Translation;
			var vector3_1 = matrix.Right * size.X;
			var vector3_2 = matrix.Up * size.Y;
			var vector3_3 = matrix.Forward * size.Z;
			var result1 = translation + 0.5f * (-vector3_1 - vector3_2 - vector3_3);
			var result2 = translation + 0.5f * (vector3_1 - vector3_2 - vector3_3);
			var result3 = translation + 0.5f * (-vector3_1 + vector3_2 - vector3_3);
			var result4 = translation + 0.5f * (vector3_1 + vector3_2 - vector3_3);
			var result5 = translation + 0.5f * (-vector3_1 - vector3_2 + vector3_3);
			var result6 = translation + 0.5f * (vector3_1 - vector3_2 + vector3_3);
			var result7 = translation + 0.5f * (-vector3_1 + vector3_2 + vector3_3);
			var result8 = translation + 0.5f * (vector3_1 + vector3_2 + vector3_3);
			if (environmentData.ViewProjectionMatrix.HasValue)
			{
				var m = environmentData.ViewProjectionMatrix.Value;
				Vector3.Transform(ref result1, ref m, out result1);
				Vector3.Transform(ref result2, ref m, out result2);
				Vector3.Transform(ref result3, ref m, out result3);
				Vector3.Transform(ref result4, ref m, out result4);
				Vector3.Transform(ref result5, ref m, out result5);
				Vector3.Transform(ref result6, ref m, out result6);
				Vector3.Transform(ref result7, ref m, out result7);
				Vector3.Transform(ref result8, ref m, out result8);
			}

			var block = Blocks[Terrain.ExtractContents(value)];
			var slotTexCoord1 = m_slotTexCoords[block.GetFaceTextureSlot(0, value)];
			var color1 = Color.MultiplyColorOnly(color, LightingManager.CalculateLighting(-matrix.Forward));
			texturedBatch3D.QueueQuad(result1, result3, result4, result2, new Vector2(slotTexCoord1.X, slotTexCoord1.W),
				new Vector2(slotTexCoord1.X, slotTexCoord1.Y), new Vector2(slotTexCoord1.Z, slotTexCoord1.Y),
				new Vector2(slotTexCoord1.Z, slotTexCoord1.W), color1);
			var slotTexCoord2 = m_slotTexCoords[block.GetFaceTextureSlot(2, value)];
			var color2 = Color.MultiplyColorOnly(color, LightingManager.CalculateLighting(matrix.Forward));
			texturedBatch3D.QueueQuad(result5, result6, result8, result7, new Vector2(slotTexCoord2.Z, slotTexCoord2.W),
				new Vector2(slotTexCoord2.X, slotTexCoord2.W), new Vector2(slotTexCoord2.X, slotTexCoord2.Y),
				new Vector2(slotTexCoord2.Z, slotTexCoord2.Y), color2);
			var slotTexCoord3 = m_slotTexCoords[block.GetFaceTextureSlot(5, value)];
			var color3 = Color.MultiplyColorOnly(color, LightingManager.CalculateLighting(-matrix.Up));
			texturedBatch3D.QueueQuad(result1, result2, result6, result5, new Vector2(slotTexCoord3.X, slotTexCoord3.Y),
				new Vector2(slotTexCoord3.Z, slotTexCoord3.Y), new Vector2(slotTexCoord3.Z, slotTexCoord3.W),
				new Vector2(slotTexCoord3.X, slotTexCoord3.W), color3);
			var slotTexCoord4 = m_slotTexCoords[block.GetFaceTextureSlot(4, value)];
			var color4 = Color.MultiplyColorOnly(topColor, LightingManager.CalculateLighting(matrix.Up));
			texturedBatch3D.QueueQuad(result3, result7, result8, result4, new Vector2(slotTexCoord4.X, slotTexCoord4.W),
				new Vector2(slotTexCoord4.X, slotTexCoord4.Y), new Vector2(slotTexCoord4.Z, slotTexCoord4.Y),
				new Vector2(slotTexCoord4.Z, slotTexCoord4.W), color4);
			var slotTexCoord5 = m_slotTexCoords[block.GetFaceTextureSlot(1, value)];
			var color5 = Color.MultiplyColorOnly(color, LightingManager.CalculateLighting(-matrix.Right));
			texturedBatch3D.QueueQuad(result1, result5, result7, result3, new Vector2(slotTexCoord5.Z, slotTexCoord5.W),
				new Vector2(slotTexCoord5.X, slotTexCoord5.W), new Vector2(slotTexCoord5.X, slotTexCoord5.Y),
				new Vector2(slotTexCoord5.Z, slotTexCoord5.Y), color5);
			var slotTexCoord6 = m_slotTexCoords[block.GetFaceTextureSlot(3, value)];
			var color6 = Color.MultiplyColorOnly(color, LightingManager.CalculateLighting(matrix.Right));
			texturedBatch3D.QueueQuad(result2, result4, result8, result6, new Vector2(slotTexCoord6.X, slotTexCoord6.W),
				new Vector2(slotTexCoord6.X, slotTexCoord6.Y), new Vector2(slotTexCoord6.Z, slotTexCoord6.Y),
				new Vector2(slotTexCoord6.Z, slotTexCoord6.W), color6);
		}

		public static void DrawFlatBlock(PrimitivesRenderer3D primitivesRenderer, int value, float size,
			ref Matrix matrix, Texture2D texture, Color color, bool isEmissive,
			DrawBlockEnvironmentData environmentData)
		{
			environmentData = environmentData ?? m_defaultEnvironmentData;
			if (!isEmissive)
			{
				var s = LightingManager.LightIntensityByLightValue[environmentData.Light];
				color = Color.MultiplyColorOnly(color, s);
			}

			var translation = matrix.Translation;
			Vector3 v2;
			Vector3 vector3;
			if (environmentData.BillboardDirection.HasValue)
			{
				v2 = Vector3.Normalize(Vector3.Cross(environmentData.BillboardDirection.Value, Vector3.UnitY));
				vector3 = -Vector3.Normalize(Vector3.Cross(environmentData.BillboardDirection.Value, v2));
			}
			else
			{
				v2 = matrix.Right;
				vector3 = matrix.Up;
			}

			var result1 = translation + 0.85f * size * (-v2 - vector3);
			var result2 = translation + 0.85f * size * (v2 - vector3);
			var result3 = translation + 0.85f * size * (-v2 + vector3);
			var result4 = translation + 0.85f * size * (v2 + vector3);
			if (environmentData.ViewProjectionMatrix.HasValue)
			{
				var m = environmentData.ViewProjectionMatrix.Value;
				Vector3.Transform(ref result1, ref m, out result1);
				Vector3.Transform(ref result2, ref m, out result2);
				Vector3.Transform(ref result3, ref m, out result3);
				Vector3.Transform(ref result4, ref m, out result4);
			}

			var block = Blocks[Terrain.ExtractContents(value)];
			Vector4 vector4;
			if (texture == null)
			{
				texture = environmentData.SubsystemTerrain != null
					? environmentData.SubsystemTerrain.SubsystemAnimatedTextures.AnimatedBlocksTexture
					: BlocksTexturesManager.DefaultBlocksTexture;
				vector4 = m_slotTexCoords[block.GetFaceTextureSlot(-1, value)];
			}
			else
			{
				vector4 = new Vector4(0.0f, 0.0f, 1f, 1f);
			}

			var texturedBatch3D = primitivesRenderer.TexturedBatch(texture, true, 0, null,
				RasterizerState.CullCounterClockwiseScissor, null, SamplerState.PointClamp);
			texturedBatch3D.QueueQuad(result1, result3, result4, result2, new Vector2(vector4.X, vector4.W),
				new Vector2(vector4.X, vector4.Y), new Vector2(vector4.Z, vector4.Y), new Vector2(vector4.Z, vector4.W),
				color);
			if (environmentData.BillboardDirection.HasValue)
				return;
			texturedBatch3D.QueueQuad(result1, result2, result4, result3, new Vector2(vector4.X, vector4.W),
				new Vector2(vector4.Z, vector4.W), new Vector2(vector4.Z, vector4.Y), new Vector2(vector4.X, vector4.Y),
				color);
		}

		public static void DrawMeshBlock(PrimitivesRenderer3D primitivesRenderer, BlockMesh blockMesh, float size,
			ref Matrix matrix, DrawBlockEnvironmentData environmentData)
		{
			environmentData = environmentData ?? m_defaultEnvironmentData;
			var texture = environmentData.SubsystemTerrain != null
				? environmentData.SubsystemTerrain.SubsystemAnimatedTextures.AnimatedBlocksTexture
				: BlocksTexturesManager.DefaultBlocksTexture;
			DrawMeshBlock(primitivesRenderer, blockMesh, texture, Color.White, size, ref matrix, environmentData);
		}

		public static void DrawMeshBlock(PrimitivesRenderer3D primitivesRenderer, BlockMesh blockMesh, Color color,
			float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
		{
			environmentData = environmentData ?? m_defaultEnvironmentData;
			var texture = environmentData.SubsystemTerrain != null
				? environmentData.SubsystemTerrain.SubsystemAnimatedTextures.AnimatedBlocksTexture
				: BlocksTexturesManager.DefaultBlocksTexture;
			DrawMeshBlock(primitivesRenderer, blockMesh, texture, color, size, ref matrix, environmentData);
		}

		public static void DrawMeshBlock(PrimitivesRenderer3D primitivesRenderer, BlockMesh blockMesh,
			Texture2D texture, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
		{
			environmentData = environmentData ?? m_defaultEnvironmentData;
			var num1 = LightingManager.LightIntensityByLightValue[environmentData.Light];
			var vector4 = new Vector4(color);
			vector4.X *= num1;
			vector4.Y *= num1;
			vector4.Z *= num1;
			var flag1 = vector4 == Vector4.One;
			var texturedBatch3D = primitivesRenderer.TexturedBatch(texture, true, 0, null,
				RasterizerState.CullCounterClockwiseScissor, null, SamplerState.PointClamp);
			var flag2 = false;
			var m = !environmentData.ViewProjectionMatrix.HasValue
				? matrix
				: matrix * environmentData.ViewProjectionMatrix.Value;
			if (size != 1.0)
				m = Matrix.CreateScale(size) * m;
			flag2 |= m.M14 != 0.0 || m.M24 != 0.0 || m.M34 != 0.0 || m.M44 != 1.0;
			var count1 = blockMesh.Vertices.Count;
			var array1 = blockMesh.Vertices.Array;
			var count2 = blockMesh.Indices.Count;
			var array2 = blockMesh.Indices.Array;
			var triangleVertices = texturedBatch3D.TriangleVertices;
			var count3 = triangleVertices.Count;
			var count4 = triangleVertices.Count;
			triangleVertices.Count += count1;
			for (var index = 0; index < count1; ++index)
			{
				var blockMeshVertex = array1[index];
				if (flag2)
				{
					var result = new Vector4(blockMeshVertex.Position, 1f);
					Vector4.Transform(ref result, ref m, out result);
					var num2 = 1f / result.W;
					blockMeshVertex.Position = new Vector3(result.X * num2, result.Y * num2, result.Z * num2);
				}
				else
				{
					Vector3.Transform(ref blockMeshVertex.Position, ref m, out blockMeshVertex.Position);
				}

				if (flag1 || blockMeshVertex.IsEmissive)
				{
					triangleVertices.Array[count4++] = new VertexPositionColorTexture(blockMeshVertex.Position,
						blockMeshVertex.Color, blockMeshVertex.TextureCoordinates);
				}
				else
				{
					var color1 = new Color((byte) (blockMeshVertex.Color.R * (double) vector4.X),
						(byte) (blockMeshVertex.Color.G * (double) vector4.Y),
						(byte) (blockMeshVertex.Color.B * (double) vector4.Z),
						(byte) (blockMeshVertex.Color.A * (double) vector4.W));
					triangleVertices.Array[count4++] = new VertexPositionColorTexture(blockMeshVertex.Position, color1,
						blockMeshVertex.TextureCoordinates);
				}
			}

			var triangleIndices = texturedBatch3D.TriangleIndices;
			var count5 = triangleIndices.Count;
			triangleIndices.Count += count2;
			for (var index = 0; index < count2; ++index)
				triangleIndices.Array[count5++] = (ushort) ((uint) count3 + array2[index]);
		}

		public static int DamageItem(int value, int damageCount)
		{
			var block = Blocks[Terrain.ExtractContents(value)];
			if (block.Durability < 0)
				return value;
			var damage = block.GetDamage(value) + damageCount;
			if (damage <= block.Durability)
				return block.SetDamage(value, damage);
			return block.GetDamageDestructionValue(value);
		}

		public static void LoadBlocksData(string data)
		{
			data = data.Replace("\r", string.Empty);
			var strArray1 = data.Split(new char[1] {'\n'}, StringSplitOptions.RemoveEmptyEntries);
			var strArray2 = (string[]) null;
			for (var index1 = 0; index1 < strArray1.Length; ++index1)
			{
				var strArray3 = strArray1[index1].Split(';');
				if (index1 == 0)
				{
					strArray2 = new string[strArray3.Length - 1];
					Array.Copy(strArray3, 1, strArray2, 0, strArray3.Length - 1);
				}
				else
				{
					if (strArray3.Length != strArray2.Length + 1)
						throw new InvalidOperationException(string.Format("Not enough field values for block \"{0}\".",
							new object[1] {strArray3.Length != 0 ? strArray3[0] : "unknown"}));
					var typeName = strArray3[0];
					if (!string.IsNullOrEmpty(typeName))
					{
						var key1 = Blocks.FirstOrDefault(v => v.GetType().Name == typeName);
						if (key1 == null)
							throw new InvalidOperationException(string.Format(
								"Block \"{0}\" not found when loading block data.", new object[1] {typeName}));
						var dictionary2 = new Dictionary<string, FieldInfo>();
						foreach (var runtimeField in key1.GetType().GetRuntimeFields())
							if (runtimeField.IsPublic && !runtimeField.IsStatic)
								dictionary2.Add(runtimeField.Name, runtimeField);
						for (var index2 = 1; index2 < strArray3.Length; ++index2)
						{
							var key2 = strArray2[index2 - 1];
							var data1 = strArray3[index2];
							if (!string.IsNullOrEmpty(data1))
							{
								FieldInfo fieldInfo;
								if (!dictionary2.TryGetValue(key2, out fieldInfo))
									throw new InvalidOperationException(string.Format(
										"Field \"{0}\" not found or not accessible when loading block data.",
										new object[1] {key2}));
								object obj;
								if (data1.StartsWith("#"))
								{
									var refTypeName = data1.Substring(1);
									if (string.IsNullOrEmpty(refTypeName))
									{
										obj = key1.BlockIndex;
									}
									else
									{
										var block = Blocks.FirstOrDefault(v => v.GetType().Name == refTypeName);
										if (block == null)
											throw new InvalidOperationException(
												string.Format(
													"Reference block \"{0}\" not found when loading block data.",
													new object[1] {refTypeName}));
										obj = block.BlockIndex;
									}
								}
								else
								{
									obj = HumanReadableConverter.ConvertFromString(fieldInfo.FieldType, data1);
								}

								fieldInfo.SetValue(key1, obj);
							}
						}
					}
				}
			}
		}

		private static void CalculateSlotTexCoordTables()
		{
			for (var slot = 0; slot < 256; ++slot)
				m_slotTexCoords[slot] = TextureSlotToTextureCoords(slot);
		}

		public static Vector4 TextureSlotToTextureCoords(int slot)
		{
			var num1 = slot % 16;
			var num2 = slot / 16;
			return new Vector4((float) ((num1 + 1.0 / 1000.0) / 16.0), (float) ((num2 + 1.0 / 1000.0) / 16.0),
				(float) ((num1 + 1 - 1.0 / 1000.0) / 16.0), (float) ((num2 + 1 - 1.0 / 1000.0) / 16.0));
		}
	}
}