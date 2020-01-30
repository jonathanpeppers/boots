using System;
using System.Net.Http;
using System.Threading.Tasks;
using Boots.Core;
using Xunit;

namespace Boots.Tests
{
	public class UrlResolverTests
	{
		HttpClient client = new HttpClient ();

		[Theory]
		[InlineData (typeof (MacUrlResolver), ReleaseChannel.Stable, Product.XamarinAndroid)]
		[InlineData (typeof (MacUrlResolver), ReleaseChannel.Stable, Product.XamariniOS)]
		[InlineData (typeof (MacUrlResolver), ReleaseChannel.Stable, Product.Mono)]
		[InlineData (typeof (MacUrlResolver), ReleaseChannel.Preview, Product.XamarinAndroid)]
		[InlineData (typeof (MacUrlResolver), ReleaseChannel.Preview, Product.XamariniOS)]
		[InlineData (typeof (MacUrlResolver), ReleaseChannel.Preview, Product.Mono)]

		public async Task Resolve (Type type, ReleaseChannel channel, Product product)
		{
			var resolver = (UrlResolver) Activator.CreateInstance (type);
			var url = await resolver.Resolve (channel, product);
			var response = await client.GetAsync (url, HttpCompletionOption.ResponseHeadersRead);
			response.EnsureSuccessStatusCode ();
		}
	}
}
