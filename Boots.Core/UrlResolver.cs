using System;
using System.Threading.Tasks;

namespace Boots.Core
{
	public abstract class UrlResolver
	{
		public abstract Task<string> Resolve (ReleaseChannel channel, Product product);
	}
}
