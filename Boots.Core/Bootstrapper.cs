using System;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo ("Boots.Tests")]

namespace Boots.Core
{
	public class Bootstrapper
	{
		public TimeSpan? Timeout { get; set; }

		public ReleaseChannel? Channel { get; set; }

		public Product? Product { get; set; }

		public FileType? FileType { get; set; }

		public string Url { get; set; } = "";

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
			if (Helpers.IsMac) {
				installer = new PkgInstaller (this);
			} else if (Helpers.IsWindows) {
				if (FileType == null) {
					if (Url.EndsWith (".msi", StringComparison.OrdinalIgnoreCase)) {
						FileType = global::FileType.msi;
						Logger.WriteLine ("Inferring .msi from URL.");
					} else if (Url.EndsWith (".vsix", StringComparison.OrdinalIgnoreCase)) {
						FileType = global::FileType.vsix;
						Logger.WriteLine ("Inferring .vsix from URL.");
					}
				}
				if (FileType == global::FileType.msi) {
					installer = new MsiInstaller (this);
				} else {
					installer = new VsixInstaller (this);
				}
			} else {
				throw new NotSupportedException ("Unsupported platform, neither macOS or Windows detected.");
			}

			using var downloader = new Downloader (this, installer.Extension);
			await downloader.Download (token);
			await installer.Install (downloader.TempFile, token);
		}

		internal HttpClient GetHttpClient ()
		{
			var httpClient = new HttpClient ();
			if (Timeout != null) {
				httpClient.Timeout = Timeout.Value;
			}
			return httpClient;
		}
	}
}
