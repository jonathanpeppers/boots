using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Boots.Core
{
	class ExeInstaller : Installer
	{
		public ExeInstaller (Bootstrapper boots) : base (boots) { }

		public override string Extension => ".exe";

		public async override Task Install (string file, CancellationToken token = default)
		{
			if (string.IsNullOrEmpty (file))
				throw new ArgumentException (nameof (file));
			if (!File.Exists (file))
				throw new FileNotFoundException ($"{Extension} file did not exist: {file}", file);

			var log = Path.GetTempFileName ();
			try {
				using var proc = new AsyncProcess (Boots) {
					Command = file,
					Arguments = $"/install /quiet /norestart /log \"{log}\"",
					Elevate = true,
				};
				await proc.RunAsync (token);
			} finally {
				await PrintLogFileAndDelete (log, token);
			}
		}
	}
}
