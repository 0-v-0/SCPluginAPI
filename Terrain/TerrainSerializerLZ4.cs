using Engine;
using LZ4PCL;
using System;
using System.Collections.Generic;
using System.IO;

namespace Game
{
	public class AdvancedLZ4Stream : LZ4Stream
	{
		protected long len = -1;
		//protected bool lastState;

		public AdvancedLZ4Stream(Stream innerStream, CompressionMode compressionMode, bool closeOnDispose = true, bool highCompression = false, int blockSize = 1048576) : base(innerStream, compressionMode, closeOnDispose, highCompression, blockSize)
		{
		}

		public override bool CanSeek => CanRead;

		public override long Position
		{
			get => base.Position; set
			{
				if (value < 0)
					goto t;
				_innerStream.Seek(0, SeekOrigin.Begin);
				long len = Skip(value);
				if (len == value)
					return;
				t: throw new InvalidOperationException("Cannot seek outside of the stream.");
			}
		}

		public override bool CanRead => base.CanRead || _compressionMode == (CompressionMode)2;

		public override bool CanWrite => base.CanWrite || _compressionMode == (CompressionMode)2;

		public Stream BaseStream => _innerStream;

		public override long Length
		{
			get
			{
				if (len == -1)
				{
					_innerStream.Seek(0, SeekOrigin.Begin);
					len = Skip(long.MaxValue);
				}
				return len;
			}
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			if (!CanRead) return _innerStream.Seek(offset, origin);
			switch (origin)
			{
				case SeekOrigin.Begin:
					Position = offset;
					break;
				case SeekOrigin.Current:
					if (offset < 0)
						throw new NotSupportedException();
					int count = Math.Min((int)offset, _bufferLength - _bufferOffset);
					if (count > 0)
					{
						_bufferOffset += count;
					}
					Skip(offset - count);
					//Position += offset;
					break;
				case SeekOrigin.End:
					Position = Length + offset;
					break;
				default:
					throw new ArgumentException("Invalid origin.", "origin");
			}
			return -1;
		}

		/// <summary>Skip the specified number of chunks.</summary>
		/// <param name="count">Number of bytes to skip</param>
		/// <returns>
		///     <c>true</c> if next has been read, or <c>false</c> if it is legitimate end of file.
		///     Throws <see cref="EndOfStreamException" /> if end of stream was unexpected.
		/// </returns>
		public long Skip(long count)
		{
			if (count == 0) return 0;
			_buffer = null;
			long total = 0;
			do
			{
				if (!TryReadVarInt(out ulong varint))
					return total;
				var flags = (ChunkFlags)varint;
				bool isCompressed = (flags & ChunkFlags.Compressed) != 0;

				int originalLength = (int)ReadVarInt();
				int compressedLength = isCompressed ? (int)ReadVarInt() : originalLength;
				if (compressedLength > originalLength)
					throw new EndOfStreamException("Unexpected end of stream"); // corrupted

				var compressed = new byte[compressedLength];
				int chunk = ReadBlock(compressed, 0, compressedLength);

				if (chunk != compressedLength)
					throw new EndOfStreamException("Unexpected end of stream"); // corrupted

				total += originalLength;
				count -= originalLength;
			} while (count > 0);
			return total;
		}

		public void Reset(int blockSize = 1048576)
		{
			_blockSize = Math.Max(16, blockSize);
		}
	}

	public class TerrainSerializerLZ4 : IDisposable
	{
		public const int MaxChunks = 65536;

		public const int TocEntryBytesCount = 12;

		public const int TocBytesCount = 786444;

		public const int ChunkSizeX = 16;

		public const int ChunkSizeY = 128;

		public const int ChunkSizeZ = 16;

		public const int ChunkBitsX = 4;

		public const int ChunkBitsZ = 4;

		public const int ChunkBytesCount = 132112;

		public const string ChunksFileName = "Chunks32.dat";

		public Terrain m_terrain;

		public byte[] m_buffer = new byte[131072];

		public Dictionary<Point2, long> m_chunkOffsets = new Dictionary<Point2, long>();

		public Stream m_stream;

		public TerrainSerializerLZ4(Terrain terrain, string directoryName)
		{
			m_terrain = terrain;
			string path = Storage.CombinePaths(directoryName, "Chunks32.lz4");
			if (!Storage.FileExists(path))
				using (m_stream = new AdvancedLZ4Stream(Storage.OpenFile(path, OpenFileMode.Create), CompressionMode.Compress, true, false, TocBytesCount))
				{
					for (int i = 0; i < 65537; i++)
						WriteTOCEntry(m_stream, 0, 0, -1);
				}
			m_stream = new AdvancedLZ4Stream(Storage.OpenFile(path, OpenFileMode.ReadWrite), (CompressionMode)2, true, false, ChunkBytesCount);
			while (true)
			{
				ReadTOCEntry(m_stream, out int cx, out int cz, out int index);
				if (index >= 0)
				{
					m_chunkOffsets[new Point2(cx, cz)] = 786444 + 132112L * index;
					continue;
				}
				break;
			}
		}

		public bool LoadChunk(TerrainChunk chunk)
		{
			return LoadChunkBlocks(chunk);
		}

		public void SaveChunk(TerrainChunk chunk)
		{
			if (chunk.State > TerrainChunkState.InvalidContents4 && chunk.ModificationCounter > 0)
			{
				SaveChunkBlocks(chunk);
				chunk.ModificationCounter = 0;
			}
		}

		public void Dispose()
		{
			Utilities.Dispose(ref m_stream);
		}

		public static void ReadChunkHeader(Stream stream)
		{
			int num = ReadInt(stream);
			int num2 = ReadInt(stream);
			ReadInt(stream);
			ReadInt(stream);
			if (num != -559038737 || num2 != -2)
				throw new InvalidOperationException("Invalid chunk header.");
		}

		public static void WriteChunkHeader(Stream stream, int cx, int cz)
		{
			WriteInt(stream, -559038737);
			WriteInt(stream, -2);
			WriteInt(stream, cx);
			WriteInt(stream, cz);
		}

		public static void ReadTOCEntry(Stream stream, out int cx, out int cz, out int index)
		{
			cx = ReadInt(stream);
			cz = ReadInt(stream);
			index = ReadInt(stream);
		}

		public static void WriteTOCEntry(Stream stream, int cx, int cz, int index)
		{
			WriteInt(stream, cx);
			WriteInt(stream, cz);
			WriteInt(stream, index);
		}

		public unsafe bool LoadChunkBlocks(TerrainChunk chunk)
		{
			bool result = false;
			int num = chunk.Origin.X >> 4;
			int num2 = chunk.Origin.Y >> 4;
			try
			{
				if (!m_chunkOffsets.TryGetValue(new Point2(num, num2), out long value))
					return result;
				double realTime = Time.RealTime;
				m_stream.Seek(value, SeekOrigin.Begin);
				ReadChunkHeader(m_stream);
				m_stream.Read(m_buffer, 0, 131072);
				fixed (byte* ptr = &m_buffer[0])
				{
					int* ptr2 = (int*)ptr;
					for (int i = 0; i < 16; i++)
					{
						for (int j = 0; j < 16; j++)
						{
							int num3 = TerrainChunk.CalculateCellIndex(i, 0, j);
							int num4 = 0;
							while (num4 < 128)
							{
								chunk.SetCellValueFast(num3, *ptr2);
								num4++;
								num3++;
								ptr2++;
							}
						}
					}
				}
				m_stream.Read(m_buffer, 0, 1024);
				fixed (byte* ptr3 = &m_buffer[0])
				{
					int* ptr4 = (int*)ptr3;
					for (int k = 0; k < 16; k++)
					{
						for (int l = 0; l < 16; l++)
						{
							m_terrain.SetShaftValue(k + chunk.Origin.X, l + chunk.Origin.Y, *ptr4);
							ptr4++;
						}
					}
				}
				result = true;
				double realTime2 = Time.RealTime;
				return result;
			}
			catch (Exception e)
			{
				Log.Error(ExceptionManager.MakeFullErrorMessage($"Error loading data for chunk ({num},{num2}).", e));
				return result;
			}
		}

		public unsafe void SaveChunkBlocks(TerrainChunk chunk)
		{
			double realTime = Time.RealTime;
			int num = chunk.Origin.X >> 4;
			int num2 = chunk.Origin.Y >> 4;
			try
			{
				bool flag = false;
				if (m_chunkOffsets.TryGetValue(new Point2(num, num2), out long value))
					m_stream.Seek(value, SeekOrigin.Begin);
				else
				{
					flag = true;
					value = m_stream.Length;
					m_stream.Seek(value, SeekOrigin.Begin);
				}
				WriteChunkHeader(m_stream, num, num2);
				try
				{
					fixed (byte* ptr = &m_buffer[0])
					{
						int* ptr2 = (int*)ptr;
						for (int i = 0; i < 16; i++)
						{
							for (int j = 0; j < 16; j++)
							{
								int num3 = TerrainChunk.CalculateCellIndex(i, 0, j);
								int num4 = 0;
								while (num4 < 128)
								{
									*ptr2 = chunk.GetCellValueFast(num3);
									num4++;
									num3++;
									ptr2++;
								}
							}
						}
					}
				}
				finally
				{
				}
				m_stream.Write(m_buffer, 0, 131072);
				try
				{
					fixed (byte* ptr3 = &m_buffer[0])
					{
						int* ptr4 = (int*)ptr3;
						for (int k = 0; k < 16; k++)
						{
							for (int l = 0; l < 16; l++)
							{
								*ptr4 = m_terrain.GetShaftValue(k + chunk.Origin.X, l + chunk.Origin.Y);
								ptr4++;
							}
						}
					}
				}
				finally
				{
				}
				m_stream.Write(m_buffer, 0, 1024);
				if (flag)
				{
					m_stream.Flush();
					int num5 = (m_chunkOffsets.Count & 65535) * 3 * 4;
					m_stream.Seek(num5, SeekOrigin.Begin);
					WriteInt(m_stream, num);
					WriteInt(m_stream, num2);
					WriteInt(m_stream, m_chunkOffsets.Count);
					m_chunkOffsets[new Point2(num, num2)] = value;
				}
				m_stream.Flush();
			}
			catch (Exception e)
			{
				Log.Error(ExceptionManager.MakeFullErrorMessage($"Error writing data for chunk ({num},{num2}).", e));
			}
			double realTime2 = Time.RealTime;
		}

		public static int ReadInt(Stream stream)
		{
			return stream.ReadByte() | (stream.ReadByte() << 8) | (stream.ReadByte() << 16) | (stream.ReadByte() << 24);
		}

		public static void WriteInt(Stream stream, int value)
		{
			stream.WriteByte((byte)value);
			stream.WriteByte((byte)(value >> 8));
			stream.WriteByte((byte)(value >> 16));
			stream.WriteByte((byte)(value >> 24));
		}
	}
}