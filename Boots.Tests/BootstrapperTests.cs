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
				boots.Url = "http://vsixgallery.com/extensions/1209461d-57f8-46a4-814a-dbe5fecef941/EditorConfig%20Language%20Service%20v1.17.271.vsix";
			} else if (Helpers.IsMac) {
				boots.Url = "https://aka.ms/objective-sharpie";
			} else {
				Skip.If (true, "Not supported on Linux yet");
			}
			await boots.Install ();
		}
	}
}
