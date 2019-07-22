using System;
using System.IO;
using System.Runtime.CompilerServices;
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
			if (Environment.OSVersion.Platform == PlatformID.Win32NT) {
				installer = new VsixInstaller (this);
			} else {
				//TODO: pkg support here
				throw new NotImplementedException ();
			}

			using (var downloader = new Downloader (this, installer.Extension)) {
				await downloader.Download (token);
				await installer.Install (downloader.TempFile, token);
			}
		}
	}
}
