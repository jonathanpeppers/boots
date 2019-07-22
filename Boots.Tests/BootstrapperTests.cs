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

		[SkippableFact (Skip = "On AzDO it wants to close `vstest.console.exe`, skip for now.")]
		public async Task ProjectSystemToolsVsix ()
		{
			Skip.IfNot (Helpers.IsWindows);
			boots.Url = "https://marketplace.visualstudio.com/_apis/public/gallery/publishers/VisualStudioProductTeam/vsextensions/ProjectSystemTools/1.0.1.1927902/vspackage";
			await boots.Install ();
		}

		[SkippableFact]
		public async Task ObjectiveSharpiePkg ()
		{
			Skip.IfNot (Helpers.IsMac);
			boots.Url = "https://aka.ms/objective-sharpie";
			await boots.Install ();
		}
	}
}
