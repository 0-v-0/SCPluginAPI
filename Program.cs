using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Text;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Engine;
using Engine.Media;
using TemplatesDatabase;
using Engine.Serialization;
namespace Game
{
	public class Program
	{
		static Vector3 ViewDirection = Vector3.UnitX;
		private SubsystemTerrain m_subsystemTerrain;
		private SubsystemGameInfo m_subsystemGameInfo;
		public static void Main(string[] args)
		{
			//byte[] b = GetMD5(Console.ReadLine());
			//byte[] l = GetMD5(new byte[10]);
			while(true){
				//for (int i = 0; i < 50; i++){
				//Print(b,l);b=GetMD5(l=b);
				//MD5_256.ToString(b,l);b=GetMD5(l=b);
				//}
				Console.WriteLine();
				Console.ReadKey(true);
			}
		}
		public static void G(string[] args)
		{
			string[] a = Blocks;
			var m = new StringBuilder();
			var r = new BinaryReader(st);
			for (int i = 0, n, w = 1, h = 1, j,k; i < 10; i++) {
				n=r.ReadByte();
				if (n>=238) continue;
				m.Append("<Recipe Result=\""+a[n]+" ResultCount=\""+n.ToString()+"\" RequiredHeatLevel=\"0\" ");
				for (n = r.ReadByte() % 9;n > 0; n--)
					if((n & 1) == 0 && h < 3) h++;
					else if(w < 3) w++;
				n=h*w;
				for (j = 0; j < n; j++)
					m.Append((j+10).ToString("x")+"=\""+a[r.ReadByte()%238]+"\" ");
				m.Append("Description=\"#\">");
				for (j = 0; j < h; j++) {
					m.Append("\n\t\"");
					for (k = 0; k < w; k++)
						m.Append((r.ReadByte()%n+10).ToString("x"));
					m.Append("\"");
				}
				m.Append("</Recipe>\n");
			}
			File.WriteAllText(@"C:\r.xml",m.ToString());
			//byte[,,] m = new byte[10,10,10];
			//Print3Array(9, 8, 15);
		}
		static void Test()
		{
			/*using (FileStream f = File.OpenWrite(string.Format("C:\\rand{0}",1))) {
				Random r = new Random(568565);
				//System.Random s=new System.Random(f.GetHashCode());
				BinaryWriter w = new BinaryWriter(f);
				w.Write(f.GetHashCode());new EngineBinaryReader().ReadByte().ReadColor()
				for (int i = 1; i < 4096; i++) {
					w.Write(r.Sign()*r.Int());//s.NextDouble();
				}
			}
			Random r = new Random(Time.RealTime.GetHashCode());
			//string s = "bcdfghjklmnpqrstvwy", o="aeiou";char c;
			Point3 p=new Point3(4000,64,4000);
			for (int i = 1; i < 200; i++)
				Console.WriteLine(p+=new Point3(r.UniformInt(-50,50), r.Sign(), r.UniformInt(-50,50)));*/
		}
		public static byte[] GetMD5(byte[] s)
		{
			Random r = new Random(BitConverter.ToInt32(s,0));
			var result = new byte[256];
			for (int i = 0; i < 256; i++)
				result[i] = (byte)Random.GlobalRandom.Int();
			return result;
			//return SHA1.Create().ComputeHash(s);
		}
		public static byte[] GetMD5(string s)
		{
			return GetMD5(System.Text.Encoding.Unicode.GetBytes(s));
		}
		public static string GetMD5String(byte[] s)
		{
			return MD5_.ToString(GetMD5(s));
		}
		public static string GetMD5String(string s)
		{
			return MD5_.ToString(GetMD5(s));
		}
		public static string Print(byte[] value, byte[] last){
			StringBuilder sb = new StringBuilder();
			/*for (int i = 0, l = value.Length; i < l; i++)
			{
				for (int j = 0, k = value[i]; i < 8; i++)
					sb.Append(c((k / ~last[i]) >> j));
			}*/
			for (int i = 0, l = value.Length; i < l; i++)
			{
				int j = value[i] / ~last[i];
				sb.Append(c(j));
				sb.Append(c(j >> 1));
				sb.Append(c(j >> 2));
				sb.Append(c(j >> 3));
				sb.Append(c(j >> 4));
				sb.Append(c(j >> 5));
				sb.Append(c(j >> 6));
				sb.Append(c(j >> 7));
			}
			return sb.ToString();
		}
		public static void d(SubsystemDrawing subsystemDrawing){
			Vector3 viewPosition = subsystemDrawing.ViewPosition;
			int x = (int)viewPosition.X;
			int y = (int)viewPosition.Y;
			int z = (int)viewPosition.Z;
			if (y < 255 || y > 210000)
				return;
		}
		public static char c(int value){
			bool flag = (value & 1) == 1;
			Console.BackgroundColor = (ConsoleColor)(flag ? 15 : 0);
			Console.Write(' ');
			return flag ? '█' : '　';
		}
	}
	public abstract class Encoder
	{
		public abstract byte Encode(byte a, byte b);
		public abstract byte Decode(byte a, byte b);
		public byte[] Encode(byte[] a){
			return this.Encode(a, 0, a.Length);
		}
		public byte[] Encode(byte[] a, int len){
			return this.Encode(a, 0, len);
		}
		public byte[] Encode(byte[] a, int start, int end){
			byte last=0;
			for(int i=start;i<end;i++)
				last = a[i] = this.Encode(a[i], last);
			return a;
		}
		public static byte[] Encode(byte[] a, int start, int end, Func<byte, byte, byte> f){
			byte last=0;
			for(int i=start;i<end;i++)
				last = a[i] = f(a[i], last);
			return a;
		}
		public byte[] Decode(byte[] a){
			return this.Decode(a, 0, a.Length);
		}
		public byte[] Decode(byte[] a, int len){
			return this.Decode(a, 0, len);
		}
		public byte[] Decode(byte[] a, int start, int end){
			byte last=0;
			for(int i=start;i<end;i++)
				last = a[i] = Decode(a[i], last);
			return a;
		}
		public static byte[] Decode(byte[] a, int start, int end, Func<byte, byte, byte> f){
			byte last=0;
			for(int i=start;i<end;i++)
				last = a[i] = f(a[i], last);
			return a;
		}
	}
	public class NewXorEncoder : Encoder
	{
		public override byte Encode(byte a, byte b){
			byte l = 0;
			for (int i = 0; i < 8; i++)
				a ^= a & 1 << i ^ b;
			return (byte)(a ^ b);
		}
		public override byte Decode(byte a, byte b){
			return (byte)(a ^ b);
		}
	}
}
public abstract class MD5_ : MD5
{
	public override string ToString()
	{
		return ToString(this.HashValue);
	}
	public static string ToString(byte[] value){
		StringBuilder sb = new StringBuilder();
		for (int i = 0, l = value.Length; i < l; i++)
			for (int j = 0, k = value[i]; i < 8; i++)
				sb.Append(Game.Program.c(k >> j));
		return sb.ToString();
	}
}