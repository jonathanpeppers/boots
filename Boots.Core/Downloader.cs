using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Boots.Core
{
	public class Downloader : IDisposable
	{
		readonly Bootstrapper boots;
		readonly Uri uri;

		public Downloader (Bootstrapper boots, string extension = "")
		{
			this.boots = boots;
			uri = new Uri (boots.Url);

			TempFile = Path.Combine (Path.GetTempPath (), Path.GetRandomFileName () + extension);
		}

		public string TempFile { get; private set; }

		public async Task Download (CancellationToken token = new CancellationToken ())
		{
			boots.Logger.WriteLine ($"Downloading {uri}");
			using var client = new HttpClientWithPolicy (boots);
			await client.DownloadAsync (uri, TempFile, token);
		}

		public void Dispose ()
		{
			if (File.Exists (TempFile)) {
				boots.Logger.WriteLine ($"Deleting {TempFile}");
				File.Delete (TempFile);
			}
		}
	}
}
