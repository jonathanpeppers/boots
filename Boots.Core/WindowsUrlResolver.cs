using System;
using System.Threading.Tasks;

namespace Boots.Core
{
	class WindowsUrlResolver : UrlResolver
	{
		public override Task<string> Resolve (ReleaseChannel channel, Product product)
		{
			throw new NotImplementedException ();
		}
	}
}
