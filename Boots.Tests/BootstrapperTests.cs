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
		public async Task InstallMsi ()
		{
			Skip.If (!Helpers.IsWindows, ".msis are only supported on Windows");
			boots.FileType = FileType.msi;
			boots.Url = "https://download-installer.cdn.mozilla.net/pub/firefox/releases/82.0/win64/en-US/Firefox%20Setup%2082.0.msi";
			await boots.Install ();
			// Two installs back-to-back should be fine
			await boots.Install ();
		}

		// [SkippableFact]
		// public async Task InstallExe ()
		// {
		// 	Skip.If (!Helpers.IsWindows, ".exes are only supported on Windows");
		// 	boots.FileType = FileType.exe;
		// 	boots.Url = "https://download.visualstudio.microsoft.com/download/pr/2290b039-85d8-4d95-85f7-edbd9fcd118d/a64bef89625bc61db2a6832878610214/dotnet-sdk-6.0.100-preview.2.21155.3-win-x64.exe";
		// 	await boots.Install ();
		// 	// Two installs back-to-back should be fine
		// 	await boots.Install ();
		// }

		[SkippableFact]
		public async Task InvalidInstallerFile ()
		{
			Skip.If (!Helpers.IsMac && !Helpers.IsWindows, "Not supported on Linux yet");
			boots.Url = "https://i.kym-cdn.com/entries/icons/mobile/000/018/012/this_is_fine.jpg";
			await Assert.ThrowsAsync<Exception> (() => boots.Install ());
		}
	}
}
