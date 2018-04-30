#region Using directives
using System;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using Mono.Cecil;
using System.Collections.Generic;
#endregion

[assembly: AssemblyTitle("SCPlugin")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyCopyright("Copyright 2018")]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]

namespace SCInjector
{
	static class Program
	{
		internal static TypeDefinition FindType(this IEnumerable<TypeDefinition> types, string typeName)
		{
			foreach (var type in types)
			{
				if(type.Name == typeName)
				{
					return type;
				}
			}
			return null;
		}
		internal static MethodDefinition FindMethod(this IEnumerable<TypeDefinition> types, string typeName, string methodName, out TypeReference type)
		{
			var result = types.FindType(typeName);
			type = result;
			foreach (var method in result.Methods)
			{
				if (method.Name == methodName)
				{
					return method;
				}
			}
			return null;
		}
		static bool isTarget(MethodDefinition method)
		{
			string s = method.Name;
			return method.IsFamily || method.IsPublic && method.HasBody && !s.StartsWith("add_") && !s.StartsWith("remove_");
		}
		static bool isGet(MethodDefinition t)
		{
			return t.Parameters.Count == 2;
		}
		static bool isTarget(TypeDefinition t)
		{
			if (t.Namespace.Length != 4)
			{
				return false;
			}
			const string subsystem = "Subsystem";
			string s = t.Name;
			return s != subsystem + "Names" &&
				s != subsystem + "EditableItemBehavior`1" &&
				(s.EndsWith("Block") ||
				s.StartsWith(subsystem) ||
				s.StartsWith("Component") ||
				s.StartsWith("Settings") ||
				s.StartsWith("World") ||
				s.EndsWith("ElectricElement") ||
				//s.EndsWith("Manager") ||
				(s.EndsWith("Camera") && s.Length != 6) ||
				s.EndsWith("Screen") ||
				s.EndsWith("Dialog"));
		}
		static void Main(string[] args)
		{
			const string dllname = "Survivalcraft.dll";
			if (args.Length == 0)
			{
				Console.WriteLine("Usage: SCInjector " + dllname);
			}
			else
			{
				var expr_27 = File.OpenRead(args[0]);
				AssemblyDefinition asmDef = AssemblyDefinition.ReadAssembly(expr_27);
				var module = asmDef.MainModule;
				using (var mscorlib = File.OpenRead("mscorlib.dll"))
				{
					var p = new PluginPatch(asmDef, AssemblyDefinition.ReadAssembly(mscorlib));
					var types = asmDef.MainModule.Types;
					foreach (var i in types.Where(isTarget))
					{
						p.Apply(i.Methods.Where(isTarget));
					}
					var methods = new string[]{
						"PlayerData","CharacterSkinsManager","DatabaseManager","DialogsManager","ExternalContentManager",
						"LightingManager","GameManager","MotdManager","MusicManager","PlantsManager","SimplexNoise",
						"StringsManager","TerrainUpdater","TerrainContentsGenerator","InventorySlotWidget"};
					p.Apply(types.FindType("ContentManager").Methods.Where(isGet));
					for (int i = 0; i < methods.Length; i++)
					{
						p.Apply(types.FindType(methods[i]).Methods);
					}
					TypeReference type;
					p.Apply(new List<MethodDefinition>()
					{
						types.FindMethod("FurnitureDesign", "CreateGeometry", out type),
						types.FindMethod("FurnitureDesign", "Resize", out type),
						types.FindMethod("FurnitureDesign", "SetValues", out type),
						types.FindMethod("PerformanceManager", "Draw", out type)
					});
					File.WriteAllText("methods.txt", p.Lst.ToString());
				}
				using (var stream = File.OpenWrite("output_" + dllname))
				{
					asmDef.Write(stream);
				}
				expr_27.Close();
			}
			Console.WriteLine("Done.");
		}
	}
}
