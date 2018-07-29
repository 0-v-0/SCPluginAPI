#region Using directives
using System;
using System.Linq;
using System.Text;
using System.IO;
using SR = System.Reflection;
using System.Runtime.CompilerServices;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;
using System.Collections.Generic;
#endregion

[assembly: SR.AssemblyTitle("SCAPIInjector")]
[assembly: SR.AssemblyVersion("1.0.0.0")]
//[assembly: SR.AssemblyCopyright("Copyright 2018")]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]

namespace SCInjector
{
	public class PluginPatch
	{
		public readonly TypeReference[] FuncTypes = new TypeReference[18];
		public readonly MethodReference[] Invokes = new MethodReference[18];
		public ModuleDefinition Module;
		//int start;
		public HashSet<MethodDefinition> AppliedMethods = new HashSet<MethodDefinition>();
		public StringBuilder Lst = new StringBuilder();
		//public static string TrimChars;
		public static string Rename(MemberReference member, bool rename = true)
		{
			string name = member.Name.TrimStart(".<>0123456789".ToCharArray()).Replace('>','_').PadLeft(1, '_');
			if (rename)
				member.Name = name;
			return name;
		}
		public static string Rename(string name)
		{
			return char.ToLower(name[0]) + name.Substring(1);
		}
		public PluginPatch(ModuleDefinition src, IEnumerable<TypeDefinition> types)
		{
			//TrimChars = ".<>";
			Module = src;
			TypeReference type;
			Invokes[0] = types.FindMethod("Action", "Invoke", out type);
			FuncTypes[0] = Module.ImportReference(type);
			int i = 1;
			for (; i < 7; i++)
				Invokes[i] = types.FindMethod("Action`" + i.ToString(), "Invoke", out FuncTypes[i]);
			for (; i < 15; i++)
				Invokes[i] = types.FindMethod("Func`" + (i - 6).ToString(), "Invoke", out FuncTypes[i]);
			Invokes[17] = types.FindMethod("Func`11", "Invoke", out FuncTypes[17]);
		}
		public void Apply(IEnumerable<MethodDefinition> methods)
		{
			if (!methods.Any()) return;
			var d = new Dictionary<string, char>();
			for (var i = methods.GetEnumerator(); i.MoveNext();)
			{
				int count;
				Instruction start;
				var method = i.Current;
				MethodBody body;
				if (!AppliedMethods.Add(method) || (body = method.Body) == null)
					continue;
				string name;
				if ((name = Rename(method, false)).Length == 5 && method.IsRuntimeSpecialName)
					continue;
				char n;
				if (d.TryGetValue(name, out n))
					d[name] = (char)(d[name] + '\x01');
				else
				{
					d.Add(name, '2');
					n = '1';
				}
				Collection<ParameterDefinition> paras;
				count = (paras = method.Parameters).Count;
				bool flag = method.IsStatic;
				int index = flag ? count : count + 1;
				TypeReference rettype;
				if ((rettype = method.ReturnType).Name != "Void")
				{
					if (count == 10 || count < 9) index += 7;
					else continue;
				}
				else if (count > 6)
					continue;
				MethodReference invoke = Module.ImportReference(Invokes[index]);
				Lst.Append(method.FullName + "\t" + (name += char.ToString(n)) + Environment.NewLine);
				//TypeReference type;
				FieldDefinition field;
				if (index != 0)
				{
					var type = new GenericInstanceType(Module.ImportReference(FuncTypes[index]));
					if (!method.IsStatic)
						type.GenericArguments.Add(method.DeclaringType);
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
				Instruction pop;
				(processor = body.GetILProcessor()).InsertBefore(start = method.IsRuntimeSpecialName ? body.Instructions.Last() : body.Instructions[0], Instruction.Create(OpCodes.Ldsfld, field));
				processor.InsertBefore(start, Instruction.Create(OpCodes.Dup));
				processor.InsertBefore(start, Instruction.Create(OpCodes.Brfalse_S, pop = Instruction.Create(OpCodes.Pop)));
				body.MaxStackSize = Math.Max(body.MaxStackSize, count + 1);
				if (method.IsRuntimeSpecialName)
					processor.InsertBefore(start, Instruction.Create(OpCodes.Dup));
				if (!flag || flag && count != 0)
					processor.InsertBefore(start, Instruction.Create(OpCodes.Ldarg_0));
				if (count != 0)
					switch (flag ? count - 1 : count)
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
							for (index = method.IsStatic ? 4 : 3; index < count; index++)
							{
								processor.InsertBefore(start, Instruction.Create(OpCodes.Ldarg_S, paras[index]));
							}
					break;
					}
				processor.InsertBefore(start, Instruction.Create(OpCodes.Callvirt, invoke));
				if (!method.IsRuntimeSpecialName)
					processor.InsertBefore(start, Instruction.Create(OpCodes.Ret));
				processor.InsertBefore(start, pop);
			}
		}
		public static void Optimize(IEnumerable<TypeDefinition> types)
		{
			for (var i = types.GetEnumerator(); i.MoveNext();)
			{
				Collection<CustomAttribute> attrs;
				MethodDefinition method;
				Collection<MethodDefinition>.Enumerator enumerator;
				TypeDefinition type;
				for (int j = 0, count = (type = i.Current).NestedTypes.Count; j < count; j++)
				{
					var nestedType = type.NestedTypes[j];
					Rename(nestedType);
					nestedType.IsNestedPublic = true;
					attrs = nestedType.CustomAttributes;
					if (attrs.Count == 1)
						attrs.Clear();
					Optimize(nestedType.Fields, !nestedType.HasGenericParameters);
					enumerator = nestedType.Methods.GetEnumerator();
					try
					{
						while (enumerator.MoveNext())
						{
							method = enumerator.Current;
							if (method.IsRuntimeSpecialName)
								continue;
							method.IsPublic = true;
							if (!nestedType.HasGenericParameters)
								Rename(method);
						}
					}
					finally
					{
						enumerator.Dispose();
					}
				}
				Optimize(type.Fields, !type.HasGenericParameters);
				enumerator = type.Methods.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						method = enumerator.Current;
						if (method.IsRuntimeSpecialName)
							continue;
						method.IsPublic = true;
						attrs = method.CustomAttributes;
						if (attrs.Count == 1)
							attrs.Clear();
						Rename(method);
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
				finally
				{
					enumerator.Dispose();
				}
			}
		}
		public static void Optimize(IEnumerable<FieldDefinition> fields, bool rename = false)
		{
			for (var enumerator = fields.GetEnumerator(); enumerator.MoveNext();) {
				var field = enumerator.Current;
				var name = field.Name;
				var attrs = field.CustomAttributes;
				if (field.IsPrivate && char.IsUpper(name, 0) && attrs.Count == 1)
					field.Name = Rename(name);
				if (name.Contains(">k_"))
				{
					field.CustomAttributes.Clear();
					if (rename)
						field.Name = Rename(name).Replace("k__BackingField", "");
				}
				if (rename)
				{
					//TrimChars = "m_";
					//Rename(field);
					//TrimChars = ".<>";
					Rename(field);
				}
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
		static string TargetName;
		internal static TypeDefinition FindType(this IEnumerable<TypeDefinition> types, string typeName)
		{
			TargetName = typeName;
			return types.FirstOrDefault(isT);
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
			TargetName = methodName;
			return result.Methods.FirstOrDefault(isT);
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
			return m.Name == TargetName;
		}
		static bool isNotT(MemberReference m)
		{
			return m.Name != "DecodeIngredient";
		}
		static bool isTarget(TypeDefinition t)
		{
			if (t.Namespace.Length != 4) //if (t.Namespace != "Game")
				return false;
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
			var asmDef = AssemblyDefinition.ReadAssembly(args[0]);
			Collection<TypeDefinition> types;
			if (Path.GetFileNameWithoutExtension(args[0]) == Path.GetFileNameWithoutExtension(dllname))
			{
				types = AssemblyDefinition.ReadAssembly("mscorlib.dll").MainModule.Types;
				var p = new PluginPatch(asmDef.MainModule, types.Concat(AssemblyDefinition.ReadAssembly("System.Core.dll").MainModule.Types));
				types = asmDef.MainModule.Types;
				TypeDefinition type;
				var enumerator = types.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						(type = enumerator.Current).IsPublic = true;
						if (isTarget(type))
							p.Apply(type.Methods);
					}
				}
				finally
				{
					enumerator.Dispose();
				}
				var typenames = ("BlocksManager,CharacterSkinsManager,DatabaseManager,DialogsManager,ExternalContentManager,"+
					"LightingManager,GameManager,MotdManager,MusicManager,PlantsManager,PlayerData,SettingsManager,StringsManager"
					).Split(',');
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
				p.Apply(types.FindType("CraftingRecipesManager").Methods.Where(isNotT));
				File.WriteAllText("methods.txt", p.Lst.ToString());
			}
			PluginPatch.Optimize(asmDef.MainModule.Types);
			asmDef.Write("output_" + Path.GetFileName(args[0]));
		}
	}
}