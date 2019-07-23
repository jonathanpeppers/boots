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
			Version ();
			if (args.Length == 0 || args [0] == "--help") {
				Help ();
				return;
			}

			var cts = new CancellationTokenSource ();
			Console.CancelKeyPress += (sender, e) => cts.Cancel ();

			var boots = new Bootstrapper {
				Url = args [0],
			};
			await boots.Install (cts.Token);
		}

		static void Version ()
		{
			var name = typeof (Program).Assembly.GetName ();
			Console.WriteLine ($"boots {name.Version}");
		}

		static void Help ()
		{
			Console.WriteLine ();
			Console.WriteLine ("usage:");
			Console.WriteLine ("\tboots https://url/to/your/package");
			Console.WriteLine ();
			Console.WriteLine ("On Windows, a .vsix file is assumed and .pkg on OSX. Linux is not currently supported.");
		}
	}
}
