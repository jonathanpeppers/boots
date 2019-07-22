using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Boots.Core
{
	class PkgInstaller : Installer
	{
		public PkgInstaller (Bootstrapper boots) : base (boots) { }

		public override string Extension => ".pkg";

		public async override Task Install (string file, CancellationToken token = new CancellationToken ())
		{
			if (string.IsNullOrEmpty (file))
				throw new ArgumentException (nameof (file));

			if (!File.Exists (file))
				throw new FileNotFoundException ($"Could not find '${Extension}' installer file.", file);

			using (var proc = new AsyncProcess {
				Command = "/usr/sbin/installer",
				Arguments = $"-pkg \"{file}\" -target / -verbose",
				Elevate = true,
			}) {
				await proc.RunAsync (token);
			}
		}
	}
}
