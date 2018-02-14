using System;
using Engine;
using System.Runtime.CompilerServices;

namespace Game
{
	public struct Rect : IEquatable<Rect>
	{
		public int X;
		public int Y;
		public int Width;
		public int Height;
		public Point2 Location
		{
			get
			{
				return new Point2(this.X, this.Y);
			}
			set
			{
				this.X = value.X;
				this.Y = value.Y;
			}
		}
		public Point2 Size
		{
			get
			{
				return new Point2(this.Width, this.Height);
			}
			set
			{
				this.Width = value.Width;
				this.Height = value.Height;
			}
		}
		public Rect(int x, int y, int length)
		{
			X = x;
			Y = y;
			Width = length;
			Height = length;
		}
		public Rect(int x, int y, int width, int height)
		{
			X = x;
			Y = y;
			Width = width;
			Height = height;
		}
		public Rect(Point2 location, Point2 size)
		{
			X = location.X;
			Y = location.Y;
			Width = size.X;
			Height = size.Y;
		}
		public static Rectangle Create()
		{

		}
		public static Rect FromLTRB(int left, int top, int right, int bottom)
		{
			return new Rect(left, top, right - left, bottom - top);
		}
		public static Rect Truncate(Quaternion value)
		{
			return new Rect((int)value.X, (int)value.Y, (int)value.Z, (int)value.W);
		}
		public override bool Equals(object obj)
		{
			return obj is Rect && this.Equals((Rect)obj);
		}
		public bool Equals(Rect other)
		{
			return other.Width == this.Width && other.Height == this.Height;
		}
		public static Rect Add(Rect a, Rect b)
		{
			return new Rect(a.X, a.Y, a.Width + b.Width, a.Height + b.Height);
		}
		public static Rect Subtract(Rect a, Rect b)
		{
			return new Rect(a.X, a.Y, a.Width - b.Width, a.Height - b.Height);
		}
		public static Rect operator +(Rect a, Rect b)
		{
			return Rect.Add(a, b);
		}
		public static Rect operator -(Rect a)
		{
			return new Rect(-a.X, -a.Y, a.Size);
		}
		public static Rect operator -(Rect a, Rect b)
		{
			return Rect.Subtract(a, b);
		}
		public bool Contains(int x, int y)
		{
			return this.X <= x && x < this.X + this.Width && this.Y <= y && y < this.Y + this.Height;
		}
		public bool Contains(Point2 p)
		{
			return Contains(p.X, p.Y);
		}
		public bool Contains(Point3 p)
		{
			return Contains(p.X, p.Z);
		}
		public bool Contains(Rect rect)
		{
			return this.X <= rect.X && rect.X + rect.Width <= this.X + this.Width && this.Y <= rect.Y && rect.Y + rect.Height <= this.Y + this.Height;
		}
		public override int GetHashCode()
		{
			return this.X ^ (this.Y << 13 | (int)((uint)this.Y >> 19)) ^ (this.Width << 26 | (int)((uint)this.Width >> 6)) ^ (this.Height << 7 | (int)((uint)this.Height >> 25));
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Rect FromPoint3(Point3 p)
		{
			return new Rect(p.X, p.Y, p.Z);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Rect FromVector3(Vector3 v)
		{
			return new Rect((int)v.X, (int)v.Y, (int)v.Z);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Rect FromVector4(Vector4 v)
		{
			return new Rect((int)v.X, (int)v.Y, (int)v.Z, (int)v.W);
		}
	}
}