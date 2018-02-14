using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace Game
{
	/*[StructLayout(LayoutKind.Explicit)]
	public struct Converter
	{
		[FieldOffset(0)]
		public object[] Objects;
		[FieldOffset(0)]
		public sbyte[] SBytes;
		[FieldOffset(0)]
		public byte[] Bytes;
		[FieldOffset(0)]
		public int[] Int;
		[FieldOffset(0)]
		public uint[] UInt;
		[FieldOffset(0)]
		public bool[] Bool;
		[FieldOffset(0)]
		public char[] Chars;
		[FieldOffset(0)]
		public short[] Short;
		[FieldOffset(0)]
		public ushort[] UShort;
		[FieldOffset(0)]
		public long[] Long;
		[FieldOffset(0)]
		public ulong[] ULong;
		[FieldOffset(0)]
		public float[] Float;
		[FieldOffset(0)]
		public double[] Double;
		//[FieldOffset(0)]
		//public string String;
		//[FieldOffset(0)]
		//public decimal[]
		public static Converter Create(params object[] objects)
		{
			Converter converter = new Converter();
			converter.Objects = objects;
			return converter;
		}
		public static byte[] GetBytes(params object[] objects)
		{
			Converter converter = new Converter();
			converter.Objects = objects;
			return converter.Bytes;
		}
	}*/
	[StructLayout(LayoutKind.Explicit)]
	public struct Point2Converter
	{
		[FieldOffset(0)]
		public Point2 Point;
		[FieldOffset(0)]
		public BlockDropValue DropValue;
	}
	[StructLayout(LayoutKind.Explicit)]
	public struct Point3Converter
	{
		[FieldOffset(0)]
		public Point3 Point;
		[FieldOffset(0)]
		public CellFace Face;
		[FieldOffset(0)]
		public MovingBlock Block;
	}
	[StructLayout(LayoutKind.Explicit)]
	public struct Vector2Progress
	{
		[FieldOffset(0)]
		public Vector2 Vector;
		[FieldOffset(0)]
		public Progress Progress;
	}
	[StructLayout(LayoutKind.Explicit)]
	public struct Array2Matrix
	{
		[FieldOffset(0)]
		public float[] Array;
		[FieldOffset(0)]
		public Matrix Matrix;
	}
	public static class PointUtils
	{
		public static Point2 Offset(Point2 p, int v)
		{
			p.X += v;
			p.Y += v;
			return p;
		}
		public static Point3 Offset(this Point3 p, int dx, int dz)
		{
			p.X += dx;
			p.Z += dz;
			return p;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static explicit operator Point2(Point3 p)
		{
			return new Point2(p.X, p.Y);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static explicit operator Point2(Color c)
		{
			return new Point2((int)c.R, (int)c.B);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static explicit operator Point2(Vector2 v)
		{
			return new Point2((int)v.X, (int)v.Y);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static explicit operator Point2(Vector3 v)
		{
			return new Point2((int)v.X, (int)v.Y);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static explicit operator Point2(Vector4 v)
		{
			return new Point2((int)v.X, (int)v.Y);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static explicit operator Point2(Quaternion q)
		{
			return new Point2((int)q.X, (int)q.Y);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator Point3(Point2 p)
		{
			return new Point3(p.X, 0, p.Y);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static explicit operator Point3(Color c)
		{
			return new Point3((int)c.R, (int)c.G, (int)c.B);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static explicit operator Point3(Vector2 v)
		{
			return new Point3((int)v.X, 0, (int)v.Y);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static explicit operator Point3(Vector3 v)
		{
			return new Point3((int)v.X, (int)v.Y, (int)v.Z);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static explicit operator Point3(Vector4 v)
		{
			return new Point3((int)v.X, (int)v.Y, (int)v.Z);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static explicit operator Point3(Quaternion q)
		{
			return new Point3((int)q.X, (int)q.Y, (int)q.Z);
		}
	}
}