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
		public readonly TypeReference[] FuncTypes = new TypeReference[18];
		public readonly MethodReference[] Invokes = new MethodReference[18];
		protected ModuleDefinition module;
		public Dictionary<MethodDefinition, bool> AppliedMethods = new Dictionary<MethodDefinition, bool>();
		public StringBuilder Lst = new StringBuilder();
		public PluginPatch(ModuleDefinition src, IEnumerable<TypeDefinition> types)
		{
			module = src;
			TypeReference FuncType;
			Invokes[0] = types.FindMethod("Action", "Invoke", out FuncType);
			FuncTypes[0] = module.ImportReference(FuncType);
			int i = 1;
			for (; i < 7; i++)
			{
				Invokes[i] = types.FindMethod("Action`" + i.ToString(), "Invoke", out FuncTypes[i]);
			}
			for (; i < 15; i++)
			{
				Invokes[i] = types.FindMethod("Func`" + (i - 6).ToString(), "Invoke", out FuncTypes[i]);
			}
			Invokes[17] = types.FindMethod("Func`11", "Invoke", out FuncTypes[17]);
		}
		public void Apply(IEnumerable<MethodDefinition> methods)
		{
			int count = methods.Count();
			if (count == 0)
			{
				return;
			}
			var d = new Dictionary<string, char>();
			foreach (var method in methods)
			{
				bool flag;
				if (AppliedMethods.TryGetValue(method, out flag))
				{
					continue;
				}
				MethodBody body;
				if ((body = method.Body) == null)
				{
					continue;
				}
				string name;
				if ((name = method.Name.TrimStart('.')) == "cctor")
					continue;
				char n;
				if (d.TryGetValue(name, out n)) d[name]++;
				else{
					d.Add(name, '2');
					n = '1';
				}
				var rettype = method.ReturnType;
				var paras = method.Parameters;
				int index = count = paras.Count;
				if (rettype.FullName != "System.Void")
				{
					if (count == 10 || count < 9) index = count + 7;
					else continue;
				}
				else if (count > 6)
				{
					continue;
				}
				MethodReference invoke = module.ImportReference(Invokes[index]);
				FieldDefinition field = new FieldDefinition(name += n.ToString(), FieldAttributes.Public | FieldAttributes.Static, FuncTypes[0]);
				AppliedMethods.Add(method, true);
				Lst.AppendFormat("{0}\t{1}\n", method.FullName, name);
				if (index != 0)
				{
					var type = new GenericInstanceType(module.ImportReference(FuncTypes[index]));
					for (index = 0; index < count; index++)
					{
						var pt = paras[index].ParameterType;
						type.GenericArguments.Add(pt);
					}
					if (rettype.FullName != "System.Void")
					{
						type.GenericArguments.Add(rettype);
					}
					field = new FieldDefinition(name, FieldAttributes.Public | FieldAttributes.Static, type);
					invoke.DeclaringType = type;
				}
				method.DeclaringType.Fields.Add(field);
				flag = name == "ctor";
				var processor = body.GetILProcessor();
				Instruction start = flag ? body.Instructions.Last() : body.Instructions[0], pop = Instruction.Create(OpCodes.Pop);
				processor.InsertBefore(start, Instruction.Create(OpCodes.Ldsfld, field));
				processor.InsertBefore(start, Instruction.Create(OpCodes.Dup));
				processor.InsertBefore(start, Instruction.Create(OpCodes.Brfalse_S, pop));
				body.MaxStackSize = Math.Max(body.MaxStackSize, count + (flag ? 2 : 1));
				if(flag)
				{
					processor.InsertBefore(start, Instruction.Create(OpCodes.Dup));
				}
				if (flag = method.IsStatic && count != 0)
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
						for (index = flag ? 4 : 3; index < count; index++)
						{
							processor.InsertBefore(start, Instruction.Create(OpCodes.Ldarg_S, paras[index]));
						}
				break;
				}
				processor.InsertBefore(start, Instruction.Create(OpCodes.Callvirt, invoke));
				if(name != "ctor")
				{
					processor.InsertBefore(start, Instruction.Create(OpCodes.Ret));
				}
				processor.InsertBefore(start, pop);
			}
		}
	}
}