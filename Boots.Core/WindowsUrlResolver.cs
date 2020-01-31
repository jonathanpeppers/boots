using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Boots.Core
{
	class WindowsUrlResolver : UrlResolver
	{
		readonly HttpClient httpClient = new HttpClient ();

		public WindowsUrlResolver (Bootstrapper boots) : base (boots) { }

		public override Task<string> Resolve (ReleaseChannel channel, Product product, CancellationToken token = new CancellationToken ())
		{
			throw new NotImplementedException ();
		}
	}
}
