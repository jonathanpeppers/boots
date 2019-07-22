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

			if (Url.EndsWith (".vsix", StringComparison.OrdinalIgnoreCase)) {
				var vsixInstaller = new VsixInstaller (this);
				await vsixInstaller.Install (token);
				return;
			}
		}
	}
}
