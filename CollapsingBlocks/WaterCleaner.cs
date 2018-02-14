\using Game;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Security;
using System.Security.Permissions;

[assembly: TargetFramework(".NETFramework,Version=v4.0,Profile=Client", FrameworkDisplayName = ".NET Framework 4 Client Profile")]
[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
[assembly: AssemblyVersion("0.0.0.0")]
[module: UnverifiableCode]
[PluginLoader("Water Cleaner", "", 0u)]
public class B
{
	private static void Initialize()
	{
		List<int> list = new List<int>(SubsystemCollapsingBlockBehavior.m_handledBlocks);
		list.Add(18);
		SubsystemCollapsingBlockBehavior.m_handledBlocks = list.ToArray();
	}
}
