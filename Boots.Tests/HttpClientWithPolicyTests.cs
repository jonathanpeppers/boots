using System;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Boots.Core;
using Polly.Timeout;
using Xunit;

namespace Boots.Tests
{
	public class HttpClientWithPolicyTests
	{
		[Fact]
		public void InvalidTimeout ()
		{
			var boots = new Bootstrapper {
				Timeout = TimeSpan.FromSeconds (-1),
			};
			Assert.Throws<ArgumentOutOfRangeException> (() => new HttpClientWithPolicy (boots));
		}

		[Fact]
		public void DefaultTimeout ()
		{
			// Mainly validates the 100-second default:
			// https://docs.microsoft.com/dotnet/api/system.net.http.httpclient.timeout#remarks
			var boots = new Bootstrapper ();
			using var client = new HttpClientWithPolicy (boots);
			Assert.Equal (TimeSpan.FromSeconds (100), client.Timeout);
		}

		[Fact]
		public async Task Cancelled ()
		{
			var boots = new Bootstrapper ();
			boots.UpdateActivePolicy ();

			using var client = new AlwaysTimeoutClient (boots);
			var token = new CancellationToken (canceled: true);
			await Assert.ThrowsAsync<OperationCanceledException> (() =>
				client.DownloadAsync (new Uri ("http://google.com"), "", token));
			Assert.Equal (0, client.TimesCalled);
		}

		[Fact]
		public async Task RetryPolicy ()
		{
			var boots = new Bootstrapper {
				NetworkRetries = 5,
				ReadWriteTimeout = TimeSpan.FromMilliseconds (1),
			};
			boots.UpdateActivePolicy ();

			using var client = new AlwaysTimeoutClient (boots);
			await Assert.ThrowsAsync<TimeoutRejectedException> (() =>
				client.DownloadAsync (new Uri ("http://google.com"), "", CancellationToken.None));
			Assert.Equal (boots.NetworkRetries + 1, client.TimesCalled);
		}

		class AlwaysTimeoutClient : HttpClientWithPolicy
		{
			public AlwaysTimeoutClient (Bootstrapper boots) : base (boots) { }

			public int TimesCalled { get; set; }

			async Task<T> Forever<T> (CancellationToken token)
			{
				TimesCalled++;
				await Task.Delay (System.Threading.Timeout.Infinite, token);
				return default;
			}

			protected override Task DoDownloadAsync (Uri uri, string tempFile, CancellationToken token) => Forever<object> (token);

			protected override Task<T> DoGetJsonAsync<T> (Uri uri, CancellationToken token) => Forever<T> (token);

			protected override Task<XmlDocument> DoGetXmlDocumentAsync (Uri uri, CancellationToken token) => Forever<XmlDocument> (token);
		}
	}
}
