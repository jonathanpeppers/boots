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
		[InlineData (typeof (MacUrlResolver), ReleaseChannel.Stable, Product.XamarinMac)]
		[InlineData (typeof (MacUrlResolver), ReleaseChannel.Stable, Product.Mono)]
		[InlineData (typeof (MacUrlResolver), ReleaseChannel.Preview, Product.XamarinAndroid)]
		[InlineData (typeof (MacUrlResolver), ReleaseChannel.Preview, Product.XamariniOS)]
		[InlineData (typeof (MacUrlResolver), ReleaseChannel.Preview, Product.XamarinMac)]
		[InlineData (typeof (MacUrlResolver), ReleaseChannel.Preview, Product.Mono)]
		[InlineData (typeof (MacUrlResolver), ReleaseChannel.Alpha, Product.XamarinAndroid)]
		[InlineData (typeof (MacUrlResolver), ReleaseChannel.Alpha, Product.XamariniOS)]
		[InlineData (typeof (MacUrlResolver), ReleaseChannel.Alpha, Product.XamarinMac)]
		[InlineData (typeof (MacUrlResolver), ReleaseChannel.Alpha, Product.Mono)]
		[InlineData (typeof (WindowsUrlResolver), ReleaseChannel.Stable, Product.XamarinAndroid)]
		[InlineData (typeof (WindowsUrlResolver), ReleaseChannel.Preview, Product.XamarinAndroid)]
		[InlineData (typeof (WindowsUrlResolver), ReleaseChannel.Alpha, Product.XamarinAndroid)]

		public async Task Resolve (Type type, ReleaseChannel channel, Product product)
		{
			var resolver = (UrlResolver) Activator.CreateInstance (type, new Bootstrapper ());
			var url = await resolver.Resolve (channel, product);
			var response = await client.GetAsync (url, HttpCompletionOption.ResponseHeadersRead);
			response.EnsureSuccessStatusCode ();
		}

		[Theory]
		[InlineData (typeof (WindowsUrlResolver), ReleaseChannel.Stable, Product.XamariniOS)]
		[InlineData (typeof (WindowsUrlResolver), ReleaseChannel.Stable, Product.XamarinMac)]
		[InlineData (typeof (WindowsUrlResolver), ReleaseChannel.Stable, Product.Mono)]
		[InlineData (typeof (WindowsUrlResolver), ReleaseChannel.Preview, Product.XamariniOS)]
		[InlineData (typeof (WindowsUrlResolver), ReleaseChannel.Preview, Product.XamarinMac)]
		[InlineData (typeof (WindowsUrlResolver), ReleaseChannel.Preview, Product.Mono)]
		[InlineData (typeof (WindowsUrlResolver), ReleaseChannel.Alpha, Product.XamariniOS)]
		[InlineData (typeof (WindowsUrlResolver), ReleaseChannel.Alpha, Product.XamarinMac)]
		[InlineData (typeof (WindowsUrlResolver), ReleaseChannel.Alpha, Product.Mono)]
		[InlineData (typeof (WindowsUrlResolver), (ReleaseChannel) 9999, Product.Mono)]
		[InlineData (typeof (WindowsUrlResolver), ReleaseChannel.Preview, (Product) 9999)]

		public async Task NotImplemented (Type type, ReleaseChannel channel, Product product)
		{
			var resolver = (UrlResolver) Activator.CreateInstance (type, new Bootstrapper ());
			await Assert.ThrowsAsync<NotImplementedException> (() => resolver.Resolve (channel, product));
		}
	}
}
