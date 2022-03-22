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

		string GetCodeMaidUrl()
		{
			string vs = Environment.GetEnvironmentVariable("AGENT_JOBNAME") == "vs2019" ? "2019" : "2022";
			return $"https://github.com/codecadwallader/codemaid/releases/download/v12.0/CodeMaid.VS{vs}.v12.0.300.vsix";
		}

		[SkippableFact]
		public async Task SimpleInstall ()
		{
			if (Helpers.IsWindows) {
				boots.Url = GetCodeMaidUrl();
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
		public async Task DowngradeFirst ()
		{
			Skip.If (!Helpers.IsWindows, "DowngradeFirst is only applicable on Windows");
			boots.Url = GetCodeMaidUrl();
			await boots.Install ();
			 // NOTE: this only does something for .vsix files on Windows
			boots.DowngradeFirst = true;
			await boots.Install ();
		}

		[SkippableFact]
		public async Task InstallMsi ()
		{
			Skip.If (!Helpers.IsWindows, ".msis are only supported on Windows");
			boots.FileType = FileType.msi;
			boots.Url = "https://download-installer.cdn.mozilla.net/pub/firefox/releases/82.0/win64/en-US/Firefox%20Setup%2082.0.msi";
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
