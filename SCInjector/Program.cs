#region Using directives
using System;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using Mono.Cecil;
using Mono.Collections.Generic;
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
			return method.IsFamily || method.IsPublic && !s.StartsWith("add_") && !s.StartsWith("remove_");
		}
		static bool isGet(MethodDefinition t)
		{
			return t.Parameters.Count == 2;
		}
		static bool isTarget(TypeDefinition t)
		{
			if (t.Namespace.Length != 4) //if (t.Namespace != "Game")
			{
				return false;
			}
			const string subsystem = "Subsystem";
			string s = t.Name;
			return s != subsystem + "Names" &&
				s != subsystem + "EditableItemBehavior`1" &&
				(s.StartsWith(subsystem) ||
				s.StartsWith("Component") ||
				s.StartsWith("Settings") ||
				s.StartsWith("World") ||
				s.EndsWith("ElectricElement") ||
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
				return;
			}
			Stream stream = File.OpenRead(args[0]);
			AssemblyDefinition asmDef = AssemblyDefinition.ReadAssembly(stream);
			stream.Close();
			Collection<TypeDefinition> types;
			using (stream = File.OpenRead("mscorlib.dll"))
			{
				types = AssemblyDefinition.ReadAssembly(stream).MainModule.Types;
			}
			PluginPatch p;
			using (stream = File.OpenRead("System.Core.dll"))
			{
				p = new PluginPatch(asmDef.MainModule,
									types.Concat(AssemblyDefinition.ReadAssembly(stream).MainModule.Types));
			}
			types = asmDef.MainModule.Types;
			foreach (var i in types.Where(isTarget))
			{
				p.Apply(i.Methods.Where(isTarget));
			}
			var typenames = ("Block,BlocksManager,CharacterSkinsManager,CraftingRecipesManager,DatabaseManager,"+
				"DialogsManager,ExternalContentManager,LightingManager,GameManager,MotdManager,MusicManager,PlantsManager,"+
				"PlayerData,SimplexNoise,StringsManager,TerrainUpdater,TerrainContentsGenerator,InventorySlotWidget"
				).Split(new char[]{','});
			for (int i = 0; i < typenames.Length; i++)
			{
				p.Apply(types.FindType(typenames[i]).Methods);
			}
			TypeReference type;
			p.Apply(new MethodDefinition[]
			{
				types.FindType("ContentManager").Methods.First(isGet),
				types.FindMethod("AudioManager", "PlaySound", out type),
				types.FindMethod("BlocksTexturesManager", "ValidateBlocksTexture", out type),
				types.FindMethod("FurnitureDesign", "CreateGeometry", out type),
				types.FindMethod("FurnitureDesign", "Resize", out type),
				types.FindMethod("FurnitureDesign", "SetValues", out type),
				types.FindMethod("PerformanceManager", "Draw", out type)
			});
			File.WriteAllText("methods.txt", p.Lst.ToString());
			using (stream = File.OpenWrite("output_" + dllname))
			{
				asmDef.Write(stream);
			}
			Console.WriteLine("Done.");
		}
	}
}