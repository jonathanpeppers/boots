using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace Boots.Core
{
	class MacUrlResolver : UrlResolver
	{
		const string Url = "https://software.xamarin.com/Service/Updates?v=2&pv964ebddd-1ffe-47e7-8128-5ce17ffffb05=0&pv4569c276-1397-4adb-9485-82a7696df22e=0&pvd1ec039f-f3db-468b-a508-896d7c382999=0&pv0ab364ff-c0e9-43a8-8747-3afb02dc7731=0&level=";
		static readonly Dictionary<Product, string> ProductIds = new Dictionary<Product, string> {
			{ Product.Mono,           "964ebddd-1ffe-47e7-8128-5ce17ffffb05" },
			{ Product.XamarinAndroid, "d1ec039f-f3db-468b-a508-896d7c382999" },
			{ Product.XamariniOS,     "4569c276-1397-4adb-9485-82a7696df22e" },
			{ Product.XamarinMac,     "0ab364ff-c0e9-43a8-8747-3afb02dc7731" },
		};

		public MacUrlResolver (Bootstrapper boots) : base (boots) { }

		public async override Task<string> Resolve (ReleaseChannel channel, Product product, CancellationToken token = new CancellationToken ())
		{
			using HttpClient httpClient = Boots.GetHttpClient ();
			string level = GetLevel (channel);
			string productId = GetProductId (product);

			var uri = new Uri (Url + level);
			Boots.Logger.WriteLine ($"Querying {uri}");
			var response = await httpClient.GetAsync (uri, token);
			response.EnsureSuccessStatusCode ();

			var document = new XmlDocument ();
			using (var stream = await response.Content.ReadAsStreamAsync ()) {
				token.ThrowIfCancellationRequested ();
				document.Load (stream);

				var node = document.SelectSingleNode ($"/UpdateInfo/Application[@id='{productId}']/Update/@url");
				if (node == null) {
					throw new XmlException ($"Did not find {product}, at channel {channel}");
				}

				string url = node.InnerText;
				if (string.IsNullOrEmpty (url)) {
					throw new XmlException ($"Did not find {product}, at channel {channel}");
				}

				// Just let this throw if it is an invalid Uri
				new Uri (url);
				return url;
			}
		}

		string GetLevel (ReleaseChannel channel)
		{
			switch (channel) {
				case ReleaseChannel.Stable:
					return "Stable";
				case ReleaseChannel.Preview:
					return "Beta";
				default:
					throw new NotImplementedException ($"Unexpected value for release: {channel}");
			}
		}

		string GetProductId (Product product)
		{
			if (!ProductIds.TryGetValue (product, out string id)) {
				throw new NotImplementedException ($"Unexpected value for product: {product}");
			}
			return id;
		}
	}
}
