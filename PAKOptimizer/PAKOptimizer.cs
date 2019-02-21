//#define FullScreen
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SCPAK
{
	public static class PAKOptimizer
	{
#if FullScreen
		const string C = "<Screen xmlns=\"runtime-namespace:Game\">\n" +
"\n" +
"  <PanoramaWidget />\n" +
"\n" +
"  <StackPanelWidget Direction=\"Horizontal\">\n" +
"\n" +
"    <CanvasWidget Style=\"{Widgets/TopBarContents}\" >\n" +
"      <LabelWidget Name=\"TopBar.Label\" Text=\"Adjust Graphics Settings\" />\n" +
"    </CanvasWidget>\n" +
"\n" +
"    <CanvasWidget Margin=\"10, 20\">\n" +
"      <RectangleWidget FillColor=\"0, 0, 0, 192\" OutlineColor=\"128, 128, 128, 128\" />\n" +
"      <ScrollPanelWidget Direction=\"Vertical\" HorizontalAlignment=\"Center\" VerticalAlignment=\"Center\" Margin=\"3, 3\">\n" +
"        <StackPanelWidget Direction=\"Vertical\" HorizontalAlignment=\"Center\" VerticalAlignment=\"Center\">\n" +
"\n" +
"          <UniformSpacingPanelWidget Direction=\"Horizontal\" Margin=\"0, 3\">\n" +
"            <LabelWidget Text=\"Dark Areas Brightness:\" Font=\"{Fonts/Pericles18}\" HorizontalAlignment=\"Far\" VerticalAlignment=\"Center\" Margin=\"20, 0\" />\n" +
"            <SliderWidget Name=\"BrightnessSlider\" Size=\"Infinity, 60\" VerticalAlignment=\"Center\" Margin=\"20, 0\" />\n" +
"          </UniformSpacingPanelWidget>\n" +
"\n" +
"          <UniformSpacingPanelWidget Direction=\"Horizontal\" Margin=\"0, 3\">\n" +
"            <LabelWidget Text=\"Full screen setting (F11):\" Font=\"{Fonts/Pericles18}\" HorizontalAlignment=\"Far\" VerticalAlignment=\"Center\" Margin=\"20, 0\" />\n" +
"            <CheckboxWidget Name=\"FullScreenCheckbox\" Size=\"Infinity, 60\" VerticalAlignment=\"Center\" Margin=\"20, 0\" />\n" +
"          </UniformSpacingPanelWidget>\n" +
"\n" +
"        </StackPanelWidget>\n" +
"      </ScrollPanelWidget>\n" +
"    </CanvasWidget>\n" +
"\n" +
"  </StackPanelWidget>\n" +
"\n" +
"</Screen>";
#endif
		private static void isPakFile(BinaryReader binaryReader)
		{
			var array = new byte[4];
			if (binaryReader.Read(array, 0, array.Length) != array.Length || array[0] != (byte)'P' || array[1] != (byte)'A' || array[2] != (byte)'K' || array[3] != (byte)'\0')
			{
				throw new FileLoadException("该文件不是Survivalcraft2的PAK文件！");
			}
		}
		public static void Optimize(Stream stream, Stream output)
		{
			long pos = stream.Position;
			var binaryReader = new BinaryReader(stream, Encoding.UTF8, true);
			isPakFile(binaryReader); //binaryWriter.Write(new byte[] { 80, 65, 75, 0, 0, 0, 0, 0 });
			int num = binaryReader.ReadInt32();
			int num2 = binaryReader.ReadInt32();
			int i = (int)(stream.Position - pos);
			var buf = new byte[i];
			stream.Position = pos;
			stream.Read(buf, 0, i);
			output.Write(buf, 0, i);
			var binaryWriter = new BinaryWriter(output, Encoding.UTF8, true);
			var streams = new byte[num2][];
			var arr = new long[num2];
			i = 0;
			long lastpos;
			for (; i < num2; i++)
			{
				var name = binaryReader.ReadString();
#if FullScreen
				lastpos = (long)(Path.GetFileName(name) == "SettingsGraphicsScreen" ? 1 : 0);
#endif
				binaryWriter.Write(name);
				name = binaryReader.ReadString();
				binaryWriter.Write(name);
				int position = binaryReader.ReadInt32() + num;
				int bytesCount = binaryReader.ReadInt32();
				pos = stream.Position;
				stream.Position = position;
				buf = new byte[bytesCount];
				stream.Read(buf, 0, bytesCount);
				if (string.Equals(name, "System.String") || string.Equals(name, "System.Xml.Linq.XElement"))
				{
					MemoryStream mstream;
					StringBuilder sb = null;
#if FullScreen
					if (lastpos == 0)
					{
#endif
						mstream = new MemoryStream(buf, true);
						var s = new StringReader(new BinaryReader(mstream).ReadString());
						sb = new StringBuilder(bytesCount >> 1);
						//if (string.Equals(name, "BlocksData"))
						while (true)
						{
							name = s.ReadLine();
							if (name == null) break;
							sb.Append(name.TrimEnd());
							sb.Append('\n');
						}
						mstream.Position = 0;
#if FullScreen
					}
					else mstream = new MemoryStream(C.Length);
#endif
					new BinaryWriter(mstream, new UTF8Encoding(false)).Write(
#if FullScreen
						lastpos != 0 ? C :
#endif
						sb.ToString().TrimEnd());
					mstream.SetLength(mstream.Position);
					buf = mstream.ToArray();
				}
				streams[i] = buf;
				stream.Position = pos;
				arr[i] = output.Position;
				binaryWriter.Write(0);
				binaryWriter.Write(buf.Length);
			}
			lastpos = output.Position;
			for (i = 0; i < num2; i++)
			{
				output.Position = lastpos;
				//binaryWriter.Write(new byte[] { 222, 173, 190, 239 });
				pos = output.Position;
				binaryWriter.Write(streams[i]);
				lastpos = output.Position;
				output.Position = arr[i];
				binaryWriter.Write((int)pos - num);
			}
			binaryReader.Dispose();
			binaryWriter.Dispose();
		}
	}
	class Program
	{
		static void Main(string[] args)
		{
			if (args.Length < 1 || args.Length > 2)
			{
				Console.WriteLine("pak优化器1.0\n" +
					"Usage： PAKOptimizer pakfile [output]\n" +
					"将pak文件拖动到程序图标上，pak优化器会自动对pak中的文本文件进行删除行尾空白和换行符转换为LF(\\n)的处理，若正常退出且看到output_开头的pak文件即优化成功\n");
			}
			else
			{
				string text = args[0];
				/*if (Directory.Exists(text))
				{
					try
					{
						new Pak(text);
					}
					catch (Exception ex)
					{
						Console.WriteLine($"发生了一个错误：{ex}");
						Console.WriteLine("按Enter键退出......");
						Console.ReadKey();
					}
				}
				else
				{
					try
					{
						new UnPak(text);
					}
					catch (Exception ex2)
					{
						Console.WriteLine($"发生了一个错误：{ex2}");
						Console.WriteLine("按Enter键退出......");
						Console.ReadKey();
					}
				}*/
				try
				{
					PAKOptimizer.Optimize(File.OpenRead(text),
						File.OpenWrite(args.Length > 1 ? args[1] : Path.Combine(Path.GetDirectoryName(text), "output_" + Path.GetFileName(text)))
						);
					return;
				}
				catch (Exception ex)
				{
					Console.WriteLine("发生了一个错误：" + ex);
				}
				Console.WriteLine("按Enter键退出......");
				Console.ReadKey();
			}
		}
	}
}
