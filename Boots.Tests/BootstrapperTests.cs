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

		[SkippableFact]
		public async Task InvalidInstallerFile ()
		{
			Skip.If (!Helpers.IsMac && !Helpers.IsWindows, "Not supported on Linux yet");
			boots.Url = "https://i.kym-cdn.com/entries/icons/mobile/000/018/012/this_is_fine.jpg";
			await Assert.ThrowsAsync<Exception> (() => boots.Install ());
		}

		[Fact]
		public void InvalidTimeout ()
		{
			var boots = new Bootstrapper {
				Timeout = TimeSpan.FromSeconds (-1),
			};
			Assert.Throws<ArgumentOutOfRangeException> (() => boots.GetHttpClient ());
		}

		[Fact]
		public void DefaultTimeout ()
		{
			// Mainly validates the 100-second default:
			// https://docs.microsoft.com/dotnet/api/system.net.http.httpclient.timeout#remarks
			var boots = new Bootstrapper ();
			Assert.Equal (TimeSpan.FromSeconds (100), boots.GetHttpClient ().Timeout);
		}
	}
}
