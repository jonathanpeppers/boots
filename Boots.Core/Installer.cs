using System.Threading;
using System.Threading.Tasks;

namespace Boots.Core
{
	abstract class Installer
	{
		protected Bootstrapper Boots;

		public Installer (Bootstrapper boots)
		{
			Boots = boots;
		}

		public abstract Task Install (CancellationToken token);
	}
}
