using System;
using System.Collections.Generic;
using Engine;

namespace Game
{
	#region base
	public struct PathData
	{
		public Point3[] Points;
		public byte[] Types;
	}
	#endregion
	public class City
	{
		public class Style
		{
		}
		public abstract class Cell
		{
			public Point3 Position;
			public Style Style;

			//protected abstract void Draw();
		}
		public class Road : Cell
		{
			public Point3 Start;
			public Point3 End;


			protected void Draw()
			{

			}
		}
		public class RoadGrid : Cell
		{
			public Road[] Roads;
			public RoadGrid(int spacing, Road[] r){
				Roads=r;
			}
			public RoadGrid(params Road[] r){
				Roads=r;
			}

			protected void Draw()
			{
				for (int i = 0, l = r.Length; i < l; i++);
					//r[i].Draw();
			}
		}
		public class RoadCell : Cell
		{/*
			GreenBelts;
			Cline;*/

		}
		public class Cross : Road
		{

		}
		public class Ramp : Road
		{

		}
		public class LeftRamp : Road
		{

		}
		public Path Path;

		protected void Draw()
		{

		}
	}
	public class Building : City.Cell
	{
		public Point3 Position;
		public Point3 Size;
		public Box Box;
		public int Height
		{
			get
			{
				return Size.Y;
			}
			set
			{
				Size.Y = value;
			}
		}
		public int Storey_Height = 3;

		public Building(Building building){
			//this = building;
		}
		public Building(Point3 position, Point3 size){
			Position = position;
			Size = size;
		}
		protected void Draw()//Factory
		{

		}
		public void DrawFrame()
		{
			Path.AddLine();
		}
		public void DrawWindow()
		{
			Path.AddLine();
		}
		public void Fill()
		{
			for (int i = 0, height = Height; i < height; i++)
				Path.FillRect(Position, Size);
		}
		public static void DrawFrame(Point3 position, Point3 size)
		{
			Path.AddLine();
		}
		public static void DrawFrame(int x1, int y1, int z1, int x2, int y2, int z2)
		{
			int x = x1, y = y1, z = z1;
			x1 = MathUtils.Min(x1,x2);
			y1 = MathUtils.Min(y1,y2);
			z1 = MathUtils.Min(z1,z2);
			x = MathUtils.Max(x,x2) - x1;
			y = MathUtils.Max(y,y2) - y1;
			z = MathUtils.Max(z,z2) - z1;
			Path.Line(x, y, z);
			Path.SetPosition(x, y, z);
		}
	}
	public class CityConstructor
	{
	}
	public sealed class Path : IEquatable<Path>
	{
		public Point3 Current;
		public Point3 Position;
		public PathData Data;
		int[] Default;
		public struct Line
		{//Segment
			Point3 Start;
			Point3 End;
			public static Line Clamp(Line a, Line b){
				return new Line(Point3.Max(a.Start, b.Start), Point3.Min(a.End, b.End));
			}
		}
		public void AddLine(int x1, int z1, int x2, int z2, int[] array = null)
		{
			int y = Current.Y;
			AddLine(x1, y, z1, x2, y, z2);
		}
		public void AddLine(int x1, int y1, int z1, int x2, int y2, int z2, int[] array = null)
		{
			array = array ?? Default;
			for (int i = 0; i < array.Length; i++)
				Set(start, end, array[i]);
		}
		public void AddLine(Line line, int[] array = null)
		{
			AddLine(line.Start, line.End, array);
		}
		public void AddLine(Point3 start, Point3 end, int[] array = null)
		{

		}
		public void AddLine(Vector3 start, Vector3 end, int[] array = null)
		{
			AddLine(PointConverter.ToPoint3(start), PointConverter.ToPoint3(end), array);
		}
		public void AddLine(Vector4 start, Vector4 end, int[] array = null)
		{
			AddLine(PointConverter.ToPoint3(start), PointConverter.ToPoint3(end), array);
		}
		public void AddLine(Quaternion start, Quaternion end, int[] array = null)
		{
			AddLine(PointConverter.ToPoint3(start), PointConverter.ToPoint3(end), array);
		}
		public void AddLines(params Line[] lines)
		{
			for (int i = 0; i < lines.Length; i++)
				AddLine(lines[i]);
		}
		public void LineTo(Point3 end, int[] array = null)
		{
			LineTo(Current, end, array);
		}
		public void AddArc(Point3 start, Point3 end, int[] array = null)
		{

		}
		public void AddArc(Vector3 start, Vector3 end, int[] array = null)
		{
			AddArc(PointConverter.ToPoint3(start), PointConverter.ToPoint3(end), array);
		}
		public void AddArc(Vector4 start, Vector4 end, int[] array = null)
		{
			AddArc(PointConverter.ToPoint3(start), PointConverter.ToPoint3(end), array);
		}
		public void AddArc(Quaternion start, Quaternion end, int[] array = null)
		{
			AddArc(PointConverter.ToPoint3(start), PointConverter.ToPoint3(end), array);
		}
		public void AddRect(Vector3 start, Vector3 end, int[] array = null)
		{
			Plane(PointConverter.ToPoint3(start), PointConverter.ToPoint3(end), array);
		}
		public void FillRect(Point3 start, Point3 end, int[] array = null)
		{
			for (int i = 0; i < array.Length; i++)
				AddLine(start, end, array);
		}
		public void FillRect(Vector3 start, Vector3 end, int[] array = null)
		{
			FillRect(PointConverter.ToPoint3(start), PointConverter.ToPoint3(end), array);
		}
		public void FillRect(Vector4 start, Vector4 end, int[] array = null)
		{
			FillRect(PointConverter.ToPoint3(start), PointConverter.ToPoint3(end), array);
		}
		public void FillRect(Quaternion start, Quaternion end, int[] array = null)
		{
			FillRect(PointConverter.ToPoint3(start), PointConverter.ToPoint3(end), array);
		}
		public void RectFrame(Vector3 start, Vector3 end, int[] array = null)
		{
			Plane(PointConverter.ToPoint3(start), PointConverter.ToPoint3(end), array);
		}
		public Path Reverse()
		{
			return this;
		}
		public static Path Reverse(Path path)
		{
			return path.Reverse();
		}
		public Path Begin(Point3 position)
		{
			Current = position;
			return this;
		}
		public void Set(Point3 position, int value)
		{
			Data.Points[position] = value;
		}
		public void Set(int x, int y, int z, int value)
		{
			Data.Points[position] = value;
		}
		public override bool Equals(object other)
		{
			return other is Path && Equals((Path)other);
		}
		public bool Equals(Path other)
		{
			return Position == other.Position;
		}
		public bool IsPointInPath(Point3 p)
		{
			Point3[] pts = Points;
			for (int i = 0; i < pts.Length; i++)
				if (pts[i] == p) return true;
			return false;
		}
		public static bool IsPointInPath(Path path, Point3 p)
		{
			Point3[] pts = path.Points;
			for (int i = 0; i < pts.Length; i++)
				if (pts[i] == p) return true;
			return false;
		}
		public bool IsPointInPath(Vector3 p)
		{
			return IsPointInPath(PointConverter.ToPoint3(p));
		}
		public override string ToString()
		{
			return "";
		}
		public static bool operator ==(Path p1, Path p2)
		{
			return p1.Equals(p2);
		}
		public static bool operator !=(Path p1, Path p2)
		{
			return !p1.Equals(p2);
		}
	}
}