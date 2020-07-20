using System;
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
				boots.Url = "https://github.com/codecadwallader/codemaid/releases/download/v11.0/CodeMaid.v11.0.183.vsix";
			} else if (Helpers.IsMac) {
				boots.Url = "https://aka.ms/objective-sharpie";
			} else {
				Skip.If (true, "Not supported on Linux yet");
			}
			await boots.Install ();
			// Two installs back-to-back should be fine
			await boots.Install ();
		}

		[SkippableFact]
		public async Task InvalidInstallerFile ()
		{
			Skip.If (!Helpers.IsMac && !Helpers.IsWindows, "Not supported on Linux yet");
			boots.Url = "https://i.kym-cdn.com/entries/icons/mobile/000/018/012/this_is_fine.jpg";
			await Assert.ThrowsAsync<Exception> (() => boots.Install ());
		}
	}
}
