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
		public async Task SimpleInstall ()
		{
			if (Helpers.IsWindows) {
				boots.Url = "https://marketplace.visualstudio.com/_apis/public/gallery/publishers/VisualStudioProductTeam/vsextensions/ProjectSystemTools/1.0.1.1927902/vspackage";
			} else if (Helpers.IsMac) {
				boots.Url = "https://aka.ms/objective-sharpie";
			} else {
				Skip.If (true, "Not supported on Linux yet");
			}
			await boots.Install ();
		}
	}
}
