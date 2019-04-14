using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Engine;
using System.Text;
using GameEntitySystem;
using TemplatesDatabase;

namespace Game
{
	public static class Blocks
	{
		//public static
	}
	public static class MiragesManager
	{
		public static List<int> AllowedBlocks = new List<int>();
		static SHA512 sha512 = SHA512.Create();
		static byte[] bytes;
		static byte current = 127;
		static MiragesManager()
		{
			AllowedBlocks.AddRange(new[]{3,4,5,21,26,66,67,68,72,73});
			
		}
		public static void Initialize()
		{
			//Project.FindSubsystem<
		}
		public static int GetBlock(int x, int y, int z)
		{
			if (current++ > 64) {
				bytes = sha512.ComputeHash(Converter.GetBytes(x, y, z, GetWorldInfoBytes()));
				current = 0;
			}
			return AllowedBlocks[bytes[current] % AllowedBlocks.Count];
		}
		public static byte[] GetWorldInfoBytes()
		{
			WorldInfo worldInfo = GameManager.WorldInfo;
			return Converter.GetBytes(worldInfo.Seed.ToCharArray(), (char)worldInfo.TemperatureOffset, (char)worldInfo.HumidityOffset);
		}
		public static byte[] GetGameInfoBytes(SubsystemGameInfo gameInfo)
		{
			WorldInfo worldInfo = gameInfo.WorldInfo;
			return Converter.GetBytes(gameInfo.WorldSeed, (int)worldInfo.TemperatureOffset, (int)worldInfo.HumidityOffset);
		}
		public static List<TerrainChunk> GetVisibleChunks(SubsystemTerrain subsystemTerrain, Vector3 viewPosition, Vector3 viewDirection)
		{
			Vector3 vector = Vector3.Normalize(Vector3.Cross(viewDirection, Vector3.UnitY));
			Vector3 v = Vector3.Normalize(Vector3.Cross(viewDirection, vector));
			Vector3[] array = new[]
			{
				viewPosition,
				viewPosition + 6f * viewDirection,
				viewPosition + 6f * viewDirection - 6f * vector,
				viewPosition + 6f * viewDirection + 6f * vector,
				viewPosition + 6f * viewDirection - 2f * v,
				viewPosition + 6f * viewDirection + 2f * v
			};
			var list = new List<TerrainChunk>();
			for (int i = 0; i < 6; i++)
			{
				v = array[i];
				TerrainChunk terrainChunk = subsystemTerrain.CellToChunk((int)v.X, (int)v.Z);
				if (terrainChunk != null && terrainChunk.State == TerrainChunkState.Valid && !list.Contains(terrainChunk))
					list.Add(terrainChunk);
			}
			return list;
		}
		public static void DrawMirages(SubsystemTerrain subsystemTerrain, SubsystemDrawing subsystemDrawing)
		{
			Vector3 v = subsystemDrawing.ViewPosition;
			List<TerrainChunk> list = GetVisibleChunks(subsystemTerrain, v, subsystemDrawing.ViewDirection);
			if (list.Count > 0)
				foreach (TerrainChunk current in list)
					while (current.State == TerrainChunkState.Valid)
						GenerateChunkVertices(current, v);
		}
		public static void GenerateChunkVertices(TerrainChunk chunk, Vector3 position)
		{
			int x = (int)position.X,
				y = (int)position.Y,
				z = (int)position.Z;
			int num = SettingsManager.VisibilityRange;
			int tx = num + x, ty = num + y, tz = num + z;
			for (int i = x - num; i < tx; i++)
			{
				for (int j = y - num; j < ty; j++)
				{
					for (int k = z - num; k <= tz; k++)
					{
						int value = GetBlock(i, j, k);
						int index = TerrainData.ExtractContents(value);
						if (index != 0)
						{
							BlocksManager.Blocks[index].GenerateTerrainVertices(subsystemTerrain.BlockGeometryGenerator, chunk.Geometry, value, i, j, k);
							chunk.GeometryMinY = MathUtils.Min(chunk.GeometryMinY, (float)k);
							chunk.GeometryMaxY = MathUtils.Max(chunk.GeometryMaxY, (float)k);
						}
					}
				}
			}
		}
		public static bool IsValueAllowed(int value)
		{
			return AllowedBlocks.Contains(value);
		}
	}
	public class SubsystemMirages : Subsystem//, IDrawable, IUpdateable
	{
		protected SubsystemDrawing m_subsystemDrawing;
		protected SubsystemGameInfo m_subsystemGameInfo;
		public override void Load(ValuesDictionary valuesDictionary)
		{
			m_subsystemDrawing = Project.FindSubsystem<SubsystemDrawing>(true);
			m_subsystemGameInfo = Project.FindSubsystem<SubsystemGameInfo>(true);
		}
	}
}