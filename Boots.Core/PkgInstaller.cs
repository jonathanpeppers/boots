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

		public async override Task Install (string file, CancellationToken token = default)
		{
			if (string.IsNullOrEmpty (file))
				throw new ArgumentException (nameof (file));
			if (!File.Exists (file))
				throw new FileNotFoundException ($"{Extension} file did not exist: {file}", file);

			using (var proc = new AsyncProcess (Boots) {
				Command = "/usr/sbin/installer",
				Arguments = $"-verbose -dumplog -pkg \"{file}\" -target /",
				Elevate = true,
			}) {
				await proc.RunAsync (token);
			}
		}
	}
}
