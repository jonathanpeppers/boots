using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Boots.Core
{
	class MsiInstaller : Installer
	{
		public MsiInstaller (Bootstrapper boots) : base (boots) { }

		public override string Extension => ".msi";

		public async override Task Install (string file, CancellationToken token = default)
		{
			if (string.IsNullOrEmpty (file))
				throw new ArgumentException (nameof (file));
			if (!File.Exists (file))
				throw new FileNotFoundException ($"{Extension} file did not exist: {file}", file);

			var log = Path.GetTempFileName ();
			try {
				using (var proc = new AsyncProcess (Boots) {
					Command = "msiexec",
					Arguments = $"/i \"{file}\" /qn /L*V \"{log}\"",
					Elevate = true,
				}) {
					await proc.RunAsync (token);
				}
			} finally {
				await PrintLogFileAndDelete (log, token);
			}
		}
	}
}
