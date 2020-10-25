using System.IO;
using System.Text;
using System.Threading.Tasks;
using Boots.Core;
using Cake.Core;
using Cake.Core.Annotations;
using Cake.Core.Diagnostics;

namespace Cake.Boots
{
	[CakeAliasCategory("Boots")]
	public static class BootsAddin
	{
		[CakeMethodAlias]
		public static async Task Boots (this ICakeContext context, string url)
		{
			var boots = new Bootstrapper {
				Url = url,
				Logger = new CakeWriter (context)
			};

			await boots.Install ();
		}

		[CakeMethodAlias]
		public static async Task Boots (this ICakeContext context, Product product, ReleaseChannel channel = ReleaseChannel.Stable)
		{
			var boots = new Bootstrapper {
				Channel = channel,
				Product = product,
				Logger = new CakeWriter (context)
			};

			await boots.Install ();
		}

		class CakeWriter : TextWriter
		{
			const Verbosity verbosity = Verbosity.Normal;
			const LogLevel level = LogLevel.Information;
			readonly ICakeContext context;

			public CakeWriter (ICakeContext context)
			{
				this.context = context;
			}

			public override Encoding Encoding => Encoding.Default;

			public override void WriteLine (string value)
			{
				context.Log.Write (verbosity, level, value ?? "");
			}

			public override void WriteLine (string format, params object [] args)
			{
				context.Log.Write (verbosity, level, format, args);
			}
		}
	}
}
