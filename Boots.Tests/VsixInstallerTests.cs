using System;
using System.Threading.Tasks;
using Boots.Core;
using Xunit;

namespace Boots.Tests
{
	public class VsixInstallerTests
	{
		[Fact]
		public async Task NoFilePath ()
		{
			var installer = new VsixInstaller (new Bootstrapper ());
			await Assert.ThrowsAsync<ArgumentException> (() => installer.Install (null));
		}
	}
}
