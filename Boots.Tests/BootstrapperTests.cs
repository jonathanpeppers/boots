using System.Threading.Tasks;
using Boots.Core;
using Xunit;
using Xunit.Abstractions;

namespace Boots.Tests
{
	public class BootstrapperTests
	{
		readonly Bootstrapper boots = new Bootstrapper ();

		public BootstrapperTests (ITestOutputHelper output)
		{
			boots.Logger = new TestWriter (output);
		}

		[SkippableFact]
		public async Task ProjectSystemToolsVsix ()
		{
			Skip.IfNot (Helpers.IsWindows);
			boots.Url = "https://marketplace.visualstudio.com/_apis/public/gallery/publishers/VisualStudioProductTeam/vsextensions/ProjectSystemTools/1.0.1.1927902/vspackage";
			await boots.Install ();
		}
	}
}
