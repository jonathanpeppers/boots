using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace Boots.Core
{
	class HttpClientWithPolicy : IDisposable
	{
		readonly Bootstrapper boots;
		readonly HttpClient client;

		public TimeSpan Timeout => client.Timeout;

		public HttpClientWithPolicy (Bootstrapper boots)
		{
			this.boots = boots;
			client = new HttpClient ();
			if (boots.Timeout != null) {
				client.Timeout = boots.Timeout.Value;
			}
		}

		public void Dispose () => client.Dispose ();

		public Task DownloadAsync (Uri uri, string tempFile, CancellationToken token) =>
			boots.ActivePolicy.ExecuteAsync (t => DoDownloadAsync (uri, tempFile, t), token);

		protected async virtual Task DoDownloadAsync (Uri uri, string tempFile, CancellationToken token)
		{
			var request = new HttpRequestMessage (HttpMethod.Get, uri);
			var response = await client.SendAsync (request, HttpCompletionOption.ResponseHeadersRead, token);
			response.EnsureSuccessStatusCode ();
			using var httpStream = await response.Content.ReadAsStreamAsync ();
			using var fileStream = File.Create (tempFile);
			boots.Logger.WriteLine ($"Writing to {tempFile}");
			await httpStream.CopyToAsync (fileStream, 8 * 1024, token);
		}

		public virtual Task<T?> GetJsonAsync<T> (Uri uri, CancellationToken token) =>
			boots.ActivePolicy.ExecuteAsync (t => DoGetJsonAsync<T> (uri, t), token);

		protected async virtual Task<T?> DoGetJsonAsync<T> (Uri uri, CancellationToken token)
		{
			var response = await client.GetAsync (uri, token);
			response.EnsureSuccessStatusCode ();
			using var stream = await response.Content.ReadAsStreamAsync ();
			return await JsonSerializer.DeserializeAsync<T> (stream, cancellationToken: token);
		}

		public virtual Task<XmlDocument> GetXmlDocumentAsync (Uri uri, CancellationToken token) =>
			boots.ActivePolicy.ExecuteAsync (t => DoGetXmlDocumentAsync (uri, t), token);

		protected async virtual Task<XmlDocument> DoGetXmlDocumentAsync (Uri uri, CancellationToken token)
		{
			var response = await client.GetAsync (uri, token);
			response.EnsureSuccessStatusCode ();
			var document = new XmlDocument ();
			using var stream = await response.Content.ReadAsStreamAsync ();
			await Task.Factory.StartNew (() => document.Load (stream), token);
			return document;
		}
	}
}
