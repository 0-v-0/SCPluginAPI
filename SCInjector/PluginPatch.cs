using System;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;
using System.Collections.Generic;

namespace SCInjector
{
	public class PluginPatch
	{
		public static readonly TypeReference[] FuncTypes = new TypeReference[15];
		public static readonly MethodReference[] Invokes = new MethodReference[15];
		protected ModuleDefinition module;
		public Dictionary<MethodDefinition, bool> AppliedMethods = new Dictionary<MethodDefinition, bool>();
		public StringBuilder Lst = new StringBuilder();
		public PluginPatch(AssemblyDefinition src, AssemblyDefinition mscorlib)
		{
			module = src.MainModule;
			var types = mscorlib.MainModule.Types;
			TypeReference FuncType;
			Invokes[0] = types.FindMethod("Action", "Invoke", out FuncType);
			FuncTypes[0] = module.ImportReference(FuncType);
			int i = 1;
			for (; i < 7; i++)
			{
				Invokes[i] = types.FindMethod("Action`" + i.ToString(), "Invoke", out FuncType);
				FuncTypes[i] = FuncType;
			}
			for (; i < 15; i++)
			{
				Invokes[i] = types.FindMethod("Func`" + (i - 6).ToString(), "Invoke", out FuncType);
				FuncTypes[i] = FuncType;
			}
		}
		public void Apply(IEnumerable<MethodDefinition> methods)
		{
			int count = methods.Count();
			if (count == 0)
			{
				return;
			}
			var fields = methods.First().DeclaringType.Fields;
			var d = new Dictionary<string, char>();
			foreach (var method in methods)
			{
				bool flag;
				if (AppliedMethods.TryGetValue(method, out flag))
				{
					continue;
				}
				AppliedMethods.Add(method, true);
				var body = method.Body;
				var processor = body.GetILProcessor();
				var start = body.Instructions[0];
				var paras = method.Parameters;
				count = paras.Count;
				int index = count;
				string name = method.Name.TrimStart('.');
				char n;
				if (d.TryGetValue(name, out n))
				{
					d[name]++;
				}
				else
				{
					d.Add(name, '1');
				}
				FieldDefinition field = new FieldDefinition(name += (n + '\u0001').ToString(), FieldAttributes.Public | FieldAttributes.Static, FuncTypes[0]);
				Lst.AppendFormat("{0}\t{1}\n", method.FullName, name);
				var rettype = method.ReturnType;
				if (rettype.FullName != "System.Void")
				{
					if (count > 8)
					{
						continue;
					}
					index = count + 7;
				}
				else if (count > 6)
				{
					continue;
				}
				MethodReference invoke = module.ImportReference(Invokes[index]);
				if (index != 0)
				{
					var type = new GenericInstanceType(module.ImportReference(FuncTypes[index]));
					for (int i = 0; i < paras.Count; i++)
					{
						var pt = paras[i].ParameterType;
						type.GenericArguments.Add(pt);
					}
					if (rettype.FullName != "System.Void")
					{
						type.GenericArguments.Add(rettype);
					}
					field = new FieldDefinition(name, FieldAttributes.Public | FieldAttributes.Static, type);
					invoke.DeclaringType = type;
				}
				fields.Add(field);
				processor.InsertBefore(start, Instruction.Create(OpCodes.Ldsfld, field));
				processor.InsertBefore(start, Instruction.Create(OpCodes.Dup));
				var pop = Instruction.Create(OpCodes.Pop);
				processor.InsertBefore(start, Instruction.Create(OpCodes.Brfalse_S, pop));
				body.MaxStackSize = Math.Max(body.MaxStackSize, count + 1);
				flag = method.IsStatic && count != 0;
				if (flag)
				{
					processor.InsertBefore(start, Instruction.Create(OpCodes.Ldarg_0));
				}
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
						for (int i = 3; i < count; i++)
						{
							processor.InsertBefore(start, Instruction.Create(OpCodes.Ldarg_S, paras[i]));
						}
				break;
				}
				processor.InsertBefore(start, Instruction.Create(OpCodes.Callvirt, invoke));
				if(method.Name != ".ctor")
				{
					processor.InsertBefore(start, Instruction.Create(OpCodes.Ret));
				}
				else
				{
					processor.InsertBefore(start, Instruction.Create(OpCodes.Br_S, start));
				}
				processor.InsertBefore(start, pop);
			}
		}
	}
}