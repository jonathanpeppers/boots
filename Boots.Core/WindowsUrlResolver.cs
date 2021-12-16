using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Boots.Core
{
	class WindowsUrlResolver : UrlResolver
	{
		const string ReleaseUrl = "https://aka.ms/vs/17/release/channel";
		const string PreviewUrl = "https://aka.ms/vs/17/pre/channel";

		public WindowsUrlResolver (Bootstrapper boots) : base (boots) { }

		public async override Task<string> Resolve (ReleaseChannel channel, Product product, CancellationToken token = new CancellationToken ())
		{
			using var httpClient = new HttpClientWithPolicy (Boots);
			Uri uri = GetUri (channel);
			string channelId = GetChannelId (channel);
			string productId = GetProductId (product);
			string payloadManifestUrl = await GetPayloadManifestUrl (httpClient, uri, channelId, token);
			return await GetPayloadUrl (httpClient, payloadManifestUrl, productId, token);
		}

		async Task<string> GetPayloadManifestUrl (HttpClientWithPolicy httpClient, Uri uri, string channelId, CancellationToken token)
		{
			Boots.Logger.WriteLine ($"Querying {uri}");

			var manifest = await httpClient.GetJsonAsync<VSManifest> (uri, token);
			var channelItem = manifest?.channelItems?.FirstOrDefault (c => c.id == channelId);
			if (channelItem == null) {
				throw new InvalidOperationException ($"Did not find '{channelId}' at: {uri}");
			}

			var payloadManifestUrl = channelItem.payloads?.Select (p => p.url)?.FirstOrDefault ();
			if (payloadManifestUrl == null || payloadManifestUrl == "") {
				throw new InvalidOperationException ($"Did not find manifest url for '{channelId}' at: {uri}");
			}
			return payloadManifestUrl;
		}

		async Task<string> GetPayloadUrl (HttpClientWithPolicy httpClient, string payloadManifestUrl, string productId, CancellationToken token)
		{
			var uri = new Uri (payloadManifestUrl);
			Boots.Logger.WriteLine ($"Querying {uri}");

			var payload = await httpClient.GetJsonAsync<VSPayloadManifest> (uri, token);
			var url = payload?.packages?.FirstOrDefault (p => p.id == productId)?.payloads?.Select (p => p.url).FirstOrDefault ();
			if (url == null || url == "") {
				throw new InvalidOperationException ($"Did not find payload url for '{productId}' at: {uri}");
			}

			// Just let this throw if it is an invalid Uri
			new Uri (url);
			return url;
		}

		Uri GetUri (ReleaseChannel channel)
		{
			switch (channel) {
				case ReleaseChannel.Stable:
					return new Uri (ReleaseUrl);
				case ReleaseChannel.Preview:
				case ReleaseChannel.Alpha:
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
				case ReleaseChannel.Alpha:
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
			public VSPackage []? channelItems { get; set; }
		}

		class VSPayload
		{
			public string? url { get; set; }
		}

		class VSPayloadManifest
		{
			public VSPackage []? packages { get; set; }
		}

		class VSPackage
		{
			public string? id { get; set; }

			public VSPayload []? payloads { get; set; }
		}
	}
}
