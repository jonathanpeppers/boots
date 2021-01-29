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

			using HttpClient client = boots.GetHttpClient ();
			var request = new HttpRequestMessage (HttpMethod.Get, uri);
			var response = await client.SendAsync (request, HttpCompletionOption.ResponseHeadersRead, token);
			response.EnsureSuccessStatusCode ();

			using var httpStream = await response.Content.ReadAsStreamAsync ();
			token.ThrowIfCancellationRequested ();

			using var fileStream = File.Create (TempFile);
			boots.Logger.WriteLine ($"Writing to {TempFile}");
			await httpStream.CopyToAsync (fileStream, 8 * 1024, token);
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
