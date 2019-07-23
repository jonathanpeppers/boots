using System;
using System.IO;
using System.Threading.Tasks;
using Boots.Core;
using Xunit;

namespace Boots.Tests
{
	public class InstallerTests
	{
		[Theory]
		[InlineData (typeof (VsixInstaller))]
		[InlineData (typeof (PkgInstaller))]

		public async Task NoFilePath (Type type)
		{
			var installer = (Installer) Activator.CreateInstance (type, new Bootstrapper ());
			await Assert.ThrowsAsync<ArgumentException> (() => installer.Install (null));
		}

		[Theory]
		[InlineData (typeof (VsixInstaller))]
		[InlineData (typeof (PkgInstaller))]
		public async Task FileDoesNotExist (Type type)
		{
			var installer = (Installer) Activator.CreateInstance (type, new Bootstrapper ());
			await Assert.ThrowsAsync<FileNotFoundException> (() => installer.Install ("asdf"));
		}
	}
}
