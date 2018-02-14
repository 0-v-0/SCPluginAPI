using System;
using System.Collections.Generic;
using System.Reflection;
namespace Game
{
	public static class BlocksManager
	{
		public static IEnumerable<TypeInfo> GetBlockTypes()
		{
			List<TypeInfo> list = new List<TypeInfo>();
			list.AddRange(typeof(BlocksManager).GetTypeInfo().Assembly.DefinedTypes);
			foreach (Assembly current in ModsManager.LoadedAssemblies)
			{
				list.AddRange(current.DefinedTypes);
			}
			return list;
		}
	}
}