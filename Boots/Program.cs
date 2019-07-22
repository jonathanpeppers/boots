using System;
using System.Threading;
using System.Threading.Tasks;
using Boots.Core;

namespace Boots
{
	class Program
	{
		static async Task Main (string [] args)
		{
			var cts = new CancellationTokenSource ();
			Console.CancelKeyPress += (sender, e) => cts.Cancel ();

			var boots = new Bootstrapper ();
			if (args.Length > 0) {
				boots.Url = args [0];
			}
			await boots.Install (cts.Token);
		}
	}
}
