using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo ("Boots.Tests")]

namespace Boots.Core
{
	public class Bootstrapper
	{
		public string Url { get; set; }

		public TextWriter Logger { get; set; } = Console.Out;

		public async Task Install (CancellationToken token = new CancellationToken ())
		{
			if (string.IsNullOrEmpty (Url))
				throw new ArgumentNullException (nameof (Uri));

			Installer installer = null;
			if (RuntimeInformation.IsOSPlatform (OSPlatform.Windows)) {
				installer = new VsixInstaller (this);
			} else {
				installer = new PkgInstaller (this);
			}

			using (var downloader = new Downloader (this, installer.Extension)) {
				await downloader.Download (token);
				await installer.Install (downloader.TempFile, token);
			}
		}
	}
}
