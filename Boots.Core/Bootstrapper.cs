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
		public ReleaseChannel? Channel { get; set; }

		public Product? Product { get; set; }

		public string Url { get; set; }

		public TextWriter Logger { get; set; } = Console.Out;

		public async Task Install (CancellationToken token = new CancellationToken ())
		{
			if (string.IsNullOrEmpty (Url)) {
				if (Channel == null)
					throw new ArgumentNullException (nameof (Channel));
				if (Product == null)
					throw new ArgumentNullException (nameof (Product));

				Logger.WriteLine ("* Automatic URL resolving is a new feature. File issues at: https://github.com/jonathanpeppers/boots/issues");

				var resolver = Helpers.IsMac ?
					(UrlResolver) new MacUrlResolver (this) :
					(UrlResolver) new WindowsUrlResolver (this);
				Url = await resolver.Resolve (Channel.Value, Product.Value);
			}

			Installer installer;
			if (Helpers.IsWindows) {
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
