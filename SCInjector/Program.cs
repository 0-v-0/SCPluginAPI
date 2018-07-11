#region Using directives
using System;
using System.Linq;
using System.Text;
using System.IO;
using SR = System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;
using System.Collections.Generic;
#endregion

[assembly: SR.AssemblyTitle("SCPluginLoaderInjector")]
[assembly: SR.AssemblyVersion("1.0.0.0")]
//[assembly: SR.AssemblyCopyright("Copyright 2018")]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]

namespace SCInjector
{
	public class PluginPatch
	{
		public readonly TypeReference[] FuncTypes = new TypeReference[18];
		public readonly MethodReference[] Invokes = new MethodReference[18];
		protected ModuleDefinition module;
		//int start;
		public HashSet<MethodDefinition> AppliedMethods = new HashSet<MethodDefinition>();
		public StringBuilder Lst = new StringBuilder();
		public PluginPatch(ModuleDefinition src, IEnumerable<TypeDefinition> types)
		{
			module = src;
			TypeReference type;
			Invokes[0] = types.FindMethod("Action", "Invoke", out type);
			FuncTypes[0] = module.ImportReference(type);
			int i = 1;
			for (; i < 7; i++)
				Invokes[i] = types.FindMethod("Action`" + i.ToString(), "Invoke", out FuncTypes[i]);
			for (; i < 15; i++)
				Invokes[i] = types.FindMethod("Func`" + (i - 6).ToString(), "Invoke", out FuncTypes[i]);
			Invokes[17] = types.FindMethod("Func`11", "Invoke", out FuncTypes[17]);
			//attribute = types.FindType("CompilerGeneratedAttribute");
		}
		public void Apply(IEnumerable<MethodDefinition> methods)
		{
			int count = methods.Count();
			if (count == 0) return;
			var d = new Dictionary<string, char>();
			foreach (var method in methods)
			{
				MethodBody body;
				if (!AppliedMethods.Add(method) || (body = method.Body) == null)
					continue;
				string name;
				bool flag = method.IsRuntimeSpecialName;
				if ((name = method.Name.TrimStart('.', '<').Replace('>','_')).Length == 5 && flag)
					continue;
				char n;
				if (d.TryGetValue(name, out n)) d[name]++;
				else
				{
					d.Add(name, '2');
					n = '1';
				}
				Collection<ParameterDefinition> paras;
				int index = count = (paras = method.Parameters).Count;
				TypeReference rettype;
				if ((rettype = method.ReturnType).Name != "Void")
				{
					if (count == 10 || count < 9) index = count + 7;
					else continue;
				}
				else if (count > 6)
					continue;
				MethodReference invoke = module.ImportReference(Invokes[index]);
				Lst.Append(method.FullName + "\t" + (name += char.ToString(n)) + Environment.NewLine);
				//TypeReference type;
				FieldDefinition field;
				if (index != 0)
				{
					var type = new GenericInstanceType(module.ImportReference(FuncTypes[index]));
					for (index = 0; index < count; index++)
						type.GenericArguments.Add(paras[index].ParameterType);
					if (rettype.Name != "Void")
						type.GenericArguments.Add(rettype);
					field = new FieldDefinition(name, FieldAttributes.Public | FieldAttributes.Static, type);
					invoke.DeclaringType = type;
				}
				else
					field = new FieldDefinition(name, FieldAttributes.Public | FieldAttributes.Static, FuncTypes[0]);
				method.DeclaringType.Fields.Add(field);
				ILProcessor processor;
				/*var ins = body.Instructions;*/
				Instruction start, pop;
				(processor = body.GetILProcessor()).InsertBefore(start = flag ? body.Instructions.Last() : body.Instructions[0], Instruction.Create(OpCodes.Ldsfld, field));
				processor.InsertBefore(start, Instruction.Create(OpCodes.Dup));
				processor.InsertBefore(start, Instruction.Create(OpCodes.Brfalse_S, pop = Instruction.Create(OpCodes.Pop)));
				body.MaxStackSize = Math.Max(body.MaxStackSize, count + (flag ? 2 : 1));
				if(flag)
					processor.InsertBefore(start, Instruction.Create(OpCodes.Dup));
				/*if (flag = method.IsStatic && count != 0)
					processor.InsertBefore(start, Instruction.Create(OpCodes.Ldarg_0));
				switch (count - (flag ? 1 : 0))
				{
				case 0: break;
				case 1: processor.InsertBefore(start, Instruction.Create(OpCodes.Ldarg_1));
				break;
				case 2: processor.InsertBefore(start, Instruction.Create(OpCodes.Ldarg_1));
						processor.InsertBefore(start, Instruction.Create(OpCodes.Ldarg_2));
				break;
				default:processor.InsertBefore(start, Instruction.Create(OpCodes.Ldarg_1));
						processor.InsertBefore(start, Instruction.Create(OpCodes.Ldarg_2));
						processor.InsertBefore(start, Instruction.Create(OpCodes.Ldarg_3));
						for (index = flag ? 4 : 3; index < count; index++)
						{
							processor.InsertBefore(start, Instruction.Create(OpCodes.Ldarg_S, paras[index]));
						}
				break;
				}*/
				for (index = 0; index < count; index++)
				{
					processor.InsertBefore(start, Instruction.Create(OpCodes.Ldarg_S, paras[index]));
				}
				processor.InsertBefore(start, Instruction.Create(OpCodes.Callvirt, invoke));
				if(!flag) //name != "ctor"
				{
					processor.InsertBefore(start, Instruction.Create(OpCodes.Ret));
				}
				processor.InsertBefore(start, pop);
			}
		}
		public static void Optimize(IEnumerable<TypeDefinition> types)
		{
			foreach (var type in types)
			{
				Collection<CustomAttribute> attrs;
				foreach (var nestedType in type.NestedTypes)
				{
					//if (nestedType.IsNestedPrivate)
						//nestedType.Name = "m" + i++.ToString("x");
					attrs = nestedType.CustomAttributes;
					if (attrs.Count == 1)
					{
						attrs.Clear();
					}
					Optimize(nestedType.Fields);
					using (var enumerator = nestedType.Methods.GetEnumerator())
						while (enumerator.MoveNext())
							enumerator.Current.IsPublic = true;
					//		if (!enumerator.Current.IsPublic)
					//			enumerator.Current.Name = "m" + i++.ToString("x");
				}
				Optimize(type.Fields);
				foreach (var method in type.Methods)
				{
					if (method.IsRuntimeSpecialName) continue;
					if (method.IsPrivate)
					{
						// if (char.IsUpper(method.Name, 0))
							method.IsPublic = true;
						//	method.Name = "m" + i++.ToString("x");
					}
					attrs = method.CustomAttributes;
					if (attrs.Count == 1) attrs.Clear();
					/*MethodBody body;
					if ((body = method.Body) == null) continue;
					Collection<VariableDefinition> v;
					int lc;
					if ((lc = (v = body.Variables).Count) != 0)
					{
						Optimize(body, OpCodes.Ldloc_0, OpCodes.Stloc_0);
						if (lc > 1)
						{
							Optimize(body, OpCodes.Ldloc_1, OpCodes.Stloc_1);
							if (lc > 2)
							{
								Optimize(body, OpCodes.Ldloc_2, OpCodes.Stloc_2);
								if (lc > 3)
								{
									Optimize(body, OpCodes.Ldloc_3, OpCodes.Stloc_3);
									while (lc-- != 0)
									{
										Optimize(body, OpCodes.Ldloc_S, OpCodes.Stloc_S, v[lc]);
									}
								}
							}
						}
					}*/
				}
			}
		}
		public static void Optimize(IEnumerable<FieldDefinition> fields)
		{
			foreach (var field in fields)
			{
				var name = field.Name;
				//if (name[0] == 'm' && name[1] == '_')
				//	field.Name = name.Substring(2);
				if (name.IndexOf(">k_") == -1 && (field.IsPrivate || field.IsAssembly) && field.CustomAttributes.Count == 1)
					field.CustomAttributes.Clear();
				field.IsPublic = true;
			}
		}
		/*void Insert(this Collection<Instruction> ins, OpCode opcode)
		{
			ins.Insert(start, Instruction.Create(opcode));
		}
		static void Optimize(MethodBody body, OpCode ldloc, OpCode stloc, VariableDefinition variable = null)
		{
			var ins = body.Instructions.ToArray();
			int i = 0, len = ins.Length;
			while ((ins[i].OpCode != stloc || ins[i++].Operand as VariableDefinition != variable) && i < len) {
			}
			int dc = i;
			while (ins[dc].OpCode == ldloc && ins[dc].Operand as VariableDefinition == variable && i < len)
				dc++;
			for (int j = dc + 1; j < len; j++)
			{
				if (ins[j].OpCode == ldloc || ins[j].OpCode == stloc || ins[j].Operand as VariableDefinition == variable) return;
			}
			body.Instructions.RemoveAt(i - 1);
			body.Instructions.RemoveAt(i);
			while (i < dc)
				body.Instructions[i++] = Instruction.Create(OpCodes.Dup);
			body.Variables.Remove(variable);
		}*/
	}
	static class Program
	{
		internal static TypeDefinition FindType(this IEnumerable<TypeDefinition> types, string typeName)
		{
			using (var enumerator = types.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if(enumerator.Current.Name == typeName)
						return enumerator.Current;
				}
			}
			return null;
		}
		internal static MethodDefinition FindMethod(this IEnumerable<TypeDefinition> types, string typeName, string methodName)
		{
			TypeReference type;
			return types.FindMethod(typeName, methodName, out type);
		}
		internal static MethodDefinition FindMethod(this IEnumerable<TypeDefinition> types, string typeName, string methodName, out TypeReference type)
		{
			var result = types.FindType(typeName);
			type = result;
			using (var enumerator = result.Methods.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if(enumerator.Current.Name == methodName)
						return enumerator.Current;
				}
			}
			return null;
		}
		static bool isTarget(MethodDefinition method)
		{
			var s = method.Name;
			return method.IsFamily || method.IsPublic && s != "Update" && !s.StartsWith("Draw") && !s.StartsWith("add_") && !s.StartsWith("remove_");
		}
		static bool isGet(IMethodSignature t)
		{
			return t.Parameters.Count == 2;
		}
		static bool isT(MemberReference m)
		{
			return m.Name != "DecodeIngredient";
		}
		static bool isTarget(TypeDefinition t)
		{
			if (t.Namespace.Length != 4) //if (t.Namespace != "Game")
			{
				return false;
			}
			var s = t.Name;
			return s.StartsWith("World") || (s.EndsWith("Camera") && !t.IsAbstract);//|| s.EndsWith("ElectricElement");
		}
		static void Main(string[] args)
		{
			const string dllname = "Survivalcraft.dll";
			if (args.Length == 0)
			{
				Console.WriteLine("Usage: SCInjector " + dllname);
				return;
			}
			var stream = File.OpenRead(args[0]);
			var asmDef = AssemblyDefinition.ReadAssembly(stream);
			stream.Close();
			Collection<TypeDefinition> types;
			if (Path.GetFileNameWithoutExtension(args[0]) == Path.GetFileNameWithoutExtension(dllname))
			{
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
				TypeDefinition type;
				using (var enumerator = types.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						(type = enumerator.Current).IsPublic = true;
						if (isTarget(type))
							p.Apply(type.Methods);
					}
				}
				var typenames = ("BlocksManager,CharacterSkinsManager,DatabaseManager,DialogsManager,"+
					"ExternalContentManager,LightingManager,GameManager,MotdManager,MusicManager,PlantsManager,"+
					"PlayerData,SettingsManager,StringsManager"
					).Split(new []{','});
				for (int i = 0; i < typenames.Length; i++)
				{
					/*int index;
					if ((index = typenames[i].IndexOf('.') + 1) != 0)
					{
						p.Apply(new []
						{
							types.FindMethod(typenames[i].Substring(0, index), typenames[i].Substring(index))
						});
					}
					else*/
					p.Apply(types.FindType(typenames[i]).Methods.Where(isTarget));
				}
				p.Apply(new []
				{
					types.FindMethod("AudioManager", "PlaySound"),
					types.FindMethod("BlocksTexturesManager", "ValidateBlocksTexture"),
					types.FindType("ContentManager").Methods.First(isGet),
					types.FindMethod("FurnitureDesign", "CreateGeometry"),
					types.FindMethod("FurnitureDesign", "Resize"),
					types.FindMethod("FurnitureDesign", "SetValues"),
					types.FindMethod("InventorySlotWidget", ".ctor"),
					types.FindMethod("PerformanceManager", "Draw"),
					types.FindMethod("ScreensManager", "Initialize"),
					types.FindMethod("TerrainUpdater","GenerateChunkVertices")
				});
				p.Apply(types.FindType("CraftingRecipesManager").Methods.Where(isT));
				File.WriteAllText("methods.txt", p.Lst.ToString());
			}
			PluginPatch.Optimize(asmDef.MainModule.Types);
			using (stream = File.OpenWrite("output_" + Path.GetFileName(args[0])))
			{
				asmDef.Write(stream);
			}
		}
	}
}