using System.Threading.Tasks;

namespace Boots.Core
{
	abstract class UrlResolver
	{
		public abstract Task<string> Resolve (ReleaseChannel channel, Product product);
	}
}
