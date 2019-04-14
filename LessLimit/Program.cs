using System;
using System.IO;
using System.Collections.Generic;
using Engine;
using Engine.Graphics;
using Engine.Content;
using Engine.Media;
using TemplatesDatabase;
using Engine.Serialization;

namespace Game
{
	public class Program
	{
		public static void Main(string[] args)
		{
			Console.Write(Converter.GetBytes(Random.GlobalRandom));
			Console.Write("Press any key to continue . . . ");
			Console.ReadKey(true);
		}
		public static bool ScaleModel(string name, string target = null, Vector3 scale = default(Vector3)){
			var path = target ?? @"E:\assets\result\";
			var s = File.OpenRead(name);//@"E:\assets\Content\Models\" +
			var n = File.Create(path + name);
			bool keepSourceVertexDataInTags = new BinaryReader(s).ReadBoolean();
			new BinaryWriter(n).Write(keepSourceVertexDataInTags);
			var m = ModelDataContentReader.ReadModelData(s);
			var w = new ModelDataContentWriter();
			w.ModelData = name;
			if (scale == default(Vector3)) scale = new Vector3(2f, 2f, 2f);
			ModelDataContentWriter.WriteModelData(n, m, scale);//w.Write(path,n);
			return true;
		}
		public static void Print3Array(int x, int y, int z){
			var c = new byte[]{3,4,5,21,26,66,67,68,72,73};
			Console.WriteLine("new byte[,,]{");
			for (int i = 0; i < x; i++) {
				Console.WriteLine("{");
				for (int j = 0; j < y; j++) {
					Console.Write("{");
					for (int k = 0; k < z; k++) {
						if(j==0||j+1==y)Console.Write(c[(i ^ j ^ k) % 10] + "{0}", k < z - 1 ? "," : "");
						else Console.Write("0,");
					}
					Console.WriteLine("},");
				}
				Console.WriteLine("},");
			}
			Console.WriteLine("}");
		}
		public static T[] ToArray<T>(T[,,] a){
			int b = 0, x = a.GetLength(0), y = a.GetLength(1), z = a.GetLength(2);
			var r = new T[a.Length];
			for (int i = 0; i < x; i++)
				for (int j = 0; j < y; j++)
					for (int k = 0; k < z; k++)
						r[b++] = a[i,j,k];
			return r;
		}
		public static T[] Merge<T>(T[] a, T[] b){
			var x = new T[a.Length + b.Length];
			a.CopyTo(x, 0);b.CopyTo(x, a.Length);
			return x;
		}
	}
	public static class ModelConverter
	{
		public static void FromDae(string path, Stream output, Vector3 scale = default(Vector3), bool keepSourceVertexDataInTags = true){
			new BinaryWriter(output).Write(keepSourceVertexDataInTags);
			File.WriteAllText(path, File.ReadAllText(path).Replace("\"<3DSRoot>\"","3DSRoot"));
			var data = Collada.Load(File.Open(path, FileMode.Open, FileAccess.Read));
			ModelDataContentWriter.WriteModelData(output, data, scale == default(Vector3) ? Vector3.One : scale);
		}
		public static void FromDae(string path, string output, Vector3 scale = default(Vector3), bool keepSourceVertexDataInTags = true){
			FromDae(path, File.OpenWrite(output), scale, keepSourceVertexDataInTags);
		}
	}
}