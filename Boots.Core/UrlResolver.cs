using System.Threading;
using System.Threading.Tasks;

namespace Boots.Core
{
	abstract class UrlResolver
	{
		protected readonly Bootstrapper Boots;

		public UrlResolver (Bootstrapper boots)
		{
			Boots = boots;
		}

		public abstract Task<string> Resolve (ReleaseChannel channel, Product product, CancellationToken token = new CancellationToken ());
	}
}
