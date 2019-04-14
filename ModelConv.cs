using Engine;
using Engine.Content;
using Engine.Media;
using Engine.Serialization;
using System;
using System.IO;
using System.Collections.Generic;

static class Program
{
	using Game;
	internal static bool MatchExtension(string name, string ext)
	{
		return Storage.GetExtension(name).Equals("." + ext, StringComparison.OrdinalIgnoreCase);
	}
	static void Main(string[] args)
	{
		string s = args[0];
		if (MatchExtension)
		{
			if (Directory.Exists(text))
				Packer.PAK(text);
			else
			{
				Packer.unPAK(text);
			}
		}
		switch(args.Length)
		{
			case 2: s=args[0];
			case 4:
			if ((s[1].ToLower() == 'm')&&(s[0] == '-' || s[0] == '/'))
				try{
					ParseMatrix(args[2], args[1]);catch{
					Console.WriteLine("错误：-m参数格式错误");
					Console.WriteLine("Press any key to continue . . . ");
					Console.ReadKey();
					Environment.Exit(0);
				}
			}
			else
			{
				s=args[0]
			}
		}
		if (string.IsNullOrEmpty(s))
		{
			Console.WriteLine(@"pak处理:
将pak文件拖动到程序图标上以解包
将文件夹拖动到程序图标上以打包
模型转换:
将dae文件拖动到程序图标上以转换
模型处理:
命令行参数：  [-m floatArray] filename output
-m  设置模型的Matrix(4x4矩阵) 目前支持下列8种格式
1. -m scale                        = Matrix.CreateScale(scale,scale,scale)
2. -m X,Y,Z                          通过Vector3创建X,Y,Z缩放
3. -m X,Y,Z,W                        通过Quaternion(四元数)创建矩阵
4. -m X1,Y1,Z1,X2,Y2,Z2,X3,Y3,Z3     通过Matrix.CreateLookAt
5. -ma X,Y,Z,A                       通过角度创建矩阵
6. -mt X,Y,Z                         通过翻转创建矩阵
7. -mw X1,Y1,Z1,X2,Y2,Z2,X3,Y3,Z3    通过Matrix.CreateWorld创建
8. -m XScale,M12,M13,M14,M21,YScale,M23,M24,M31,M32,ZScale,M34,M41,M42,M43,M44
filename  文件名(目前只支持dae格式和sc模型)");
			goto a;
		}
		var s = args[0];
		var m = Matrix.Identity;
		if (args.Length == 3)
		{
			try
			{
				if ((char.ToLower(s[1]) == 'm') && (s[0] == '-' || s[0] == '/'))
					m = ParseMatrix(args[2], args[1].Substring(2));
			}
			catch(Exception e)
			{
				Console.WriteLine(e.ToString());
				goto a;
			}
		}
		var target = Path.GetFileNameWithoutExtension(s);
		Stream output;
		try
		{
			if (File.Exists(target))
			{
				Console.WriteLine("文件已存在，请输入新文件名(按回车键覆盖)：");
				var target2 = Console.ReadLine();
				if (!string.IsNullOrWhiteSpace(target2)) target = target2;
			}
			output = File.OpenWrite(target);
			ModelConverter.FromDae(s, output);
			return;
			//ModelData.ExtensionToFileFormat(Storage.GetExtension(source));
		}
		catch(Exception e)
		{
			Console.WriteLine("错误：" + e);
			//output.Close();
			Stream stream = File.OpenRead(s);
			var reader = new EngineBinaryReader(stream, true);
			if (reader.ReadInt32() > 1)
			{
				//throw new NotSupportedException();
				Console.WriteLine("不支持的模型格式");
				goto a;
			}
			stream.Seek(0, SeekOrigin.Begin);
			ModelConverter.FromDae(stream, target, m);
		}
		a:
		Console.WriteLine("请按任意键继续. . .");
		Console.ReadKey();
		//Thread.Sleep(800);
	}
	static Matrix ParseMatrix(string str, string m)
	{
		if (string.IsNullOrWhiteSpace(m))
			return Matrix.Identity;
		string[] s = str.Substring(2).Split(',');
		int len = s.Length;
		var a = new float[len];
		for (int i = 0; i < len; i++)
			if (!string.IsNullOrEmpty(s[i]))
				a[i] = float.Parse(s[i]);
		if (m.Length == 3)
		{
			switch(m[2].ToLower())
			{
				case 'a': return Matrix.CreateFromAxisAngle(new Vector3(a[0], a[1], a[2]), a[3]);
				case 't': return Matrix.CreateTranslation(a[0], a[1], a[2]);
				case 'w': return Matrix.CreateWorld(new Vector3(a[0], a[1], a[2]), new Vector3(a[3], a[4], a[5]), new Vector3(a[6], a[7], a[8]));
				default: goto t;
			}
		}
			switch (len)
			{
				case 1: return Matrix.CreateScale(a[0]);
				case 3: return Matrix.CreateScale(a[0], a[1], a[2]);
				case 4: return new Quaternion(a[0], a[1], a[2], a[3]).ToMatrix();
				case 9: return Matrix.CreateLookAt(new Vector3(a[0], a[1], a[2]), new Vector3(a[3], a[4], a[5]), new Vector3(a[6], a[7], a[8]));
				case 16: return new Matrix(a[0], a[1], a[2], a[3], a[4], a[5], a[6], a[7], a[8], a[9], a[10], a[11], a[12], a[13], a[14], a[15]);
				default: goto t;
			}
		t: throw new NotSupportedException();
	}
}
namespace Game
{
	public static class ModelConverter
	{
		public static void FromDae(string path, Stream output, Vector3 scale = default(Vector3), bool keepSourceVertexDataInTags = true)
		{
			new BinaryWriter(output).Write(keepSourceVertexDataInTags);
			ModelData data = Collada.Load(File.Open(path, FileMode.Open, FileAccess.Read));
			ModelDataContentWriter.WriteModelData(output, data, scale);
		}
		public static void FromDae(string path, string output, Matrix m, bool keepSourceVertexDataInTags = true)
		{
			FromDae(path, File.OpenWrite(output), m);
		}
		public static void FromDae(Stream source, string output, Matrix m, bool keepSourceVertexDataInTags = true)
		{
			FromDae(source, File.OpenWrite(output), m);
		}
		public static void FromDae(string path, Stream output, Matrix m, bool keepSourceVertexDataInTags = true)
		{
			FromDae(File.OpenRead(path), output, m);
		}
		public static void FromDae(Stream source, Stream output, Matrix m, bool keepSourceVertexDataInTags = true)
		{
			new BinaryWriter(output).Write(keepSourceVertexDataInTags);
			ModelData data = Collada.Load(source);
			ModelDataContentWriter.WriteModelData(output, ModelScaler.Scale(data, m), Vector3.One);
		}
		public static Stream FromDae(string source, Vector3 scale = default(Vector3), bool keepSourceVertexDataInTags = true)
		{
			Stream result = new MemoryStream();
			ModelDataContentWriter.WriteModelData(result, FromDae(source), scale);
			return result;
		}
		//public static Stream FromDae(string source, Vector3 scale = default(Vector3), bool keepSourceVertexDataInTags = true)
		//{
		//throw new NotSupportedException();
		//Stream result = new MemoryStream();
		//ModelDataContentWriter.WriteModelData(result, FromDae(source), scale);
		//return result;
		//}
		public static ModelData FromDae(string source, Vector3 scale = default(Vector3), bool keepSourceVertexDataInTags = true)
		{
			return FromDae(File.OpenRead(source), scale);
		}
		public static ModelData FromDae(Stream source, Vector3 scale = default(Vector3), bool keepSourceVertexDataInTags = true)
		{
			//File.WriteAllText(source, File.ReadAllText(source).Replace("\"<3DSRoot>\"", "3DSRoot"));
			return ModelScaler.Scale(Collada.Load(source), scale);
		}

		public static void FromDae(string path, string output, Vector3 scale = default(Vector3), bool keepSourceVertexDataInTags = true)
		{
			FromDae(path, File.OpenWrite(output), scale, keepSourceVertexDataInTags);
		}
	}
	public static class ModelScaler
	{
		public static ModelData Scale(string source, Vector3 scale = default(Vector3))
		{
			return Scale(File.OpenRead(source), scale);
		}
		public static ModelData Scale(Stream source, Vector3 scale = default(Vector3))
		{
			return Scale(ModelDataContentReader.ReadModelData(source), scale);
		}
		public static ModelData Scale(ModelData source, Vector3 scale = default(Vector3))
		{
			return Scale(source, Matrix.CreateScale((scale == default(Vector3)) ? Vector3.One : scale));
		}
		public static ModelData Scale(ModelData source, Matrix m)
		{
			foreach (ModelBoneData current in source.Bones)
				current.Transform = (current.ParentBoneIndex < 0) ? (current.Transform * m) : current.Transform;
			return source;
		}
		public static bool Scale(ref ModelData source, Vector3 scale = default(Vector3))
		{
			source = Scale(source, scale);
			return true;
		}
		public static bool Scale(string source, string target, Vector3 scale = default(Vector3), bool autoDispose = false)
		{
			return Scale(File.OpenRead(source), File.Open(source, FileMode.OpenOrCreate, FileAccess.Write), scale, source, autoDispose);
		}
		public static bool Scale(Stream source, Stream target = null, Vector3 scale = default(Vector3), string name = null, bool toDispose = false)
		{
			target = target ?? source;
			bool keepSourceVertexDataInTags = new BinaryReader(source).ReadBoolean();
			new BinaryWriter(target).Write(keepSourceVertexDataInTags);
			var j = ModelDataContentReader.ReadModelData(source);
			var w = new ModelDataContentWriter();
			//target.ModelData = name;
			ModelDataContentWriter.WriteModelData(target, j, (scale == default(Vector3)) ? Vector3.One : scale);
			if(toDispose){
				source.Dispose();
				target.Dispose();
			}
			return true;
		}
	}
}