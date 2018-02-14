using Game;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security;
using System.Security.Permissions;

[assembly: AssemblyTrademark("")]
//[assembly: TargetFramework(".NETFramework,Version=v4.0,Profile=Client", FrameworkDisplayName = ".NET Framework 4 Client Profile")]
[assembly: AssemblyTitle("Collapsing")]
[assembly: SecurityRules(SecurityRuleSet.Level1, SkipVerificationInFullTrust = true)]
[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: ComVisible(false)]
//[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
[assembly: AssemblyVersion("0.0.0.0")]
//[module: UnverifiableCode]
[PluginLoader("Collapsing Leaves", "Enjoy cutting trees", 0u)]
public class A
{
	private static void Initialize()
	{
		List<int> list = new List<int>(SubsystemCollapsingBlockBehavior.m_handledBlocks);
		list.Add(12);
		list.Add(13);
		list.Add(14);
		list.Add(225);
		SubsystemCollapsingBlockBehavior.m_handledBlocks = list.ToArray();
	}
}
