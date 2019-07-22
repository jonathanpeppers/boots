using System.Threading;
using System.Threading.Tasks;

namespace Boots.Core
{
	abstract class Installer
	{
		protected readonly Bootstrapper Boots;

		public Installer (Bootstrapper boots)
		{
			Boots = boots;
		}

		public abstract string Extension { get; }

		public abstract Task Install (string file, CancellationToken token);
	}
}
