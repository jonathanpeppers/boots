using System;
using System.IO;
using System.Net.Http;
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

		[Theory]
		[InlineData (typeof (AlwaysTimeoutClient), typeof (TimeoutRejectedException))]
		[InlineData (typeof (AlwaysThrowsClient),  typeof (HttpRequestException))]
		[InlineData (typeof (AlwaysThrowsClient),  typeof (IOException))]
		[InlineData (typeof (AlwaysThrowsClient),  typeof (NotImplementedException), 0)]
		[InlineData (typeof (AlwaysThrowsClient),  typeof (NullReferenceException), 0)]
		public async Task TimeoutPolicy (Type clientType, Type exceptionType, int expectedRetries = 5)
		{
			var writer = new StringWriter ();
			var boots = new Bootstrapper {
				NetworkRetries = 5,
				ReadWriteTimeout = TimeSpan.FromMilliseconds (1),
				Logger = writer,
			};
			boots.UpdateActivePolicy ();

			using var client = (TestClient) Activator.CreateInstance (clientType, new object [] { boots });
			client.ExceptionType = exceptionType;
			await Assert.ThrowsAsync (exceptionType, () =>
				client.DownloadAsync (new Uri ("http://google.com"), "", CancellationToken.None));
			Assert.Equal (expectedRetries + 1, client.TimesCalled);
			for (int i = 1; i <= expectedRetries; i++) {
				Assert.Contains ($"Retry attempt {i}: {exceptionType.FullName}", writer.ToString ());
			}
		}

		class TestClient : HttpClientWithPolicy
		{
			public TestClient (Bootstrapper boots) : base (boots) { }

			public int TimesCalled { get; set; }

			public Type ExceptionType { get; set; }
		}

		class AlwaysTimeoutClient : TestClient
		{
			public AlwaysTimeoutClient (Bootstrapper boots) : base (boots) { }

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

		class AlwaysThrowsClient : TestClient
		{
			public AlwaysThrowsClient (Bootstrapper boots) : base (boots) { }

			Task<T> Throw<T> ()
			{
				TimesCalled++;
				throw (Exception) Activator.CreateInstance (ExceptionType);
			}

			protected override Task DoDownloadAsync (Uri uri, string tempFile, CancellationToken token) => Throw<object> ();

			protected override Task<T> DoGetJsonAsync<T> (Uri uri, CancellationToken token) => Throw<T> ();

			protected override Task<XmlDocument> DoGetXmlDocumentAsync (Uri uri, CancellationToken token) => Throw<XmlDocument> ();
		}
	}
}
