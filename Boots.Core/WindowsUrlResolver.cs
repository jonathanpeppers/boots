using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Boots.Core
{
	class WindowsUrlResolver : UrlResolver
	{
		const string ReleaseUrl = "https://aka.ms/vs/16/release/channel";
		const string PreviewUrl = "https://aka.ms/vs/16/pre/channel";

		readonly HttpClient httpClient = new HttpClient ();

		public WindowsUrlResolver (Bootstrapper boots) : base (boots) { }

		public async override Task<string> Resolve (ReleaseChannel channel, Product product, CancellationToken token = new CancellationToken ())
		{
			Uri uri = GetUri (channel);
			string channelId = GetChannelId (channel);
			string productId = GetProductId (product);

			Boots.Logger.WriteLine ($"Querying {uri}");
			var response = await httpClient.GetAsync (uri, token);
			response.EnsureSuccessStatusCode ();

			string payloadManifestUrl;
			using (var stream = await response.Content.ReadAsStreamAsync ()) {
				token.ThrowIfCancellationRequested ();
				var manifest = await JsonSerializer.DeserializeAsync<VSManifest> (stream, cancellationToken: token);
				var channelItem = manifest.channelItems?.FirstOrDefault (c => c.id == channelId);
				if (channelItem == null) {
					throw new InvalidOperationException ($"Did not find '{channelId}' at: {uri}");
				}
				payloadManifestUrl = channelItem.payloads?.Select (p => p.url)?.FirstOrDefault ();
				if (string.IsNullOrEmpty (payloadManifestUrl)) {
					throw new InvalidOperationException ($"Did not find manifest url for '{channelId}' at: {uri}");
				}
			}

			uri = new Uri (payloadManifestUrl);
			Boots.Logger.WriteLine ($"Querying {uri}");
			response = await httpClient.GetAsync (uri, token);
			response.EnsureSuccessStatusCode ();

			using (var stream = await response.Content.ReadAsStreamAsync ()) {
				token.ThrowIfCancellationRequested ();
				var payload = await JsonSerializer.DeserializeAsync<VSPayloadManifest> (stream, cancellationToken: token);
				var url = payload.packages?.FirstOrDefault (p => p.id == productId)?.payloads?.Select (p => p.url).FirstOrDefault ();
				if (string.IsNullOrEmpty (url)) {
					throw new InvalidOperationException ($"Did not find payload url for '{productId}' at: {uri}");
				}

				// Just let this throw if it is an invalid Uri
				new Uri (url);
				return url;
			}
		}

		Uri GetUri (ReleaseChannel channel)
		{
			switch (channel) {
				case ReleaseChannel.Stable:
					return new Uri (ReleaseUrl);
				case ReleaseChannel.Preview:
					return new Uri (PreviewUrl);
				default:
					throw new NotImplementedException ($"Unexpected value for {nameof (ReleaseChannel)}: {channel}"); ;
			}
		}

		string GetChannelId (ReleaseChannel channel)
		{
			switch (channel) {
				case ReleaseChannel.Stable:
					return "Microsoft.VisualStudio.Manifests.VisualStudio";
				case ReleaseChannel.Preview:
					return "Microsoft.VisualStudio.Manifests.VisualStudioPreview";
				default:
					throw new NotImplementedException ($"Unexpected value for release: {channel}"); ;
			}
		}

		string GetProductId (Product product)
		{
			switch (product) {
				case Product.XamarinAndroid:
					return "Xamarin.Android.Sdk";
				case Product.Mono:
				case Product.XamariniOS:
				case Product.XamarinMac:
					throw new NotImplementedException ($"Value for product not implemented on Windows: {product}");
				default:
					throw new NotImplementedException ($"Unexpected value for product: {product}");
			}
		}

		class VSManifest
		{
			public VSPackage [] channelItems { get; set; }
		}

		class VSPayload
		{
			public string url { get; set; }
		}

		class VSPayloadManifest
		{
			public VSPackage [] packages { get; set; }
		}

		class VSPackage
		{
			public string id { get; set; }

			public VSPayload [] payloads { get; set; }
		}
	}
}
