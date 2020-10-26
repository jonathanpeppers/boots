using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Boots.Core
{
	abstract class Installer
	{
		protected readonly Bootstrapper Boots;

		public Installer (Bootstrapper boots)
		{
			Boots = boots;
		}

		public abstract string Extension { get; }

		public abstract Task Install (string file, CancellationToken token = new CancellationToken ());

		protected async Task PrintLogFileAndDelete (string log, CancellationToken token)
		{
			if (File.Exists (log)) {
				using (var reader = File.OpenText (log)) {
					while (!reader.EndOfStream && !token.IsCancellationRequested) {
						Boots.Logger.WriteLine (await reader.ReadLineAsync ());
					}
				}
				File.Delete (log);
			} else {
				Boots.Logger.WriteLine ($"Log file did not exist: {log}");
			}
		}
	}
}
