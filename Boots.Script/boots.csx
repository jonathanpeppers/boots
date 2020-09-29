#r "../../../lib/netstandard2.0/Boots.Core.dll"

using System.Threading.Tasks;
using Boots.Core;

public static async Task boots (string url)
{
	var boots = new Bootstrapper {
		Url = url,
	};

	await boots.Install ();
}

public static async Task boots (Product product, ReleaseChannel channel = ReleaseChannel.Stable)
{
	var boots = new Bootstrapper {
		Channel = channel,
		Product = product,
	};

	await boots.Install ();
}
