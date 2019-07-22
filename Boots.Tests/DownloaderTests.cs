using System.IO;
using System.Threading.Tasks;
using Boots.Core;
using Xunit;

namespace Boots.Tests
{
	public class DownloaderTests
	{
		[Fact]
		public async Task Get ()
		{
			string tempFile;
			using (var downloader = new Downloader (new Bootstrapper {
				Url = "http://httpbin.org/json",
			})) {
				tempFile = downloader.TempFile;
				Assert.False (File.Exists (tempFile), $"{tempFile} should *not* exist!");
				await downloader.Download ();
				Assert.True (File.Exists (tempFile), $"{tempFile} should exist!");
			}
			Assert.False (File.Exists (tempFile), $"{tempFile} should *not* exist!");
		}

		[Fact]
		public void VsixFilePath ()
		{
			var downloader = new Downloader (new Bootstrapper {
				Url = "https://marketplace.visualstudio.com/_apis/public/gallery/publishers/VisualStudioProductTeam/vsextensions/ProjectSystemTools/1.0.1.1927902/vspackage"
			}, ".vsix");
			Assert.Equal (".vsix", Path.GetExtension (downloader.TempFile));
		}

		[Fact]
		public void PkgFilePath ()
		{
			var downloader = new Downloader (new Bootstrapper {
				Url = "https://aka.ms/objective-sharpie"
			}, ".pkg");
			Assert.Equal (".pkg", Path.GetExtension (downloader.TempFile));
		}
	}
}
