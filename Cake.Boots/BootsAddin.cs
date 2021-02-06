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
		public static async Task Boots (this ICakeContext context, string url, FileType? fileType = default)
		{
			var boots = new Bootstrapper {
				Url = url,
				FileType = fileType,
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

		[CakeMethodAlias]
		public static async Task Boots (this ICakeContext context, BootsSettings settings)
		{
			var boots = new Bootstrapper {
				Logger = new CakeWriter (context)
			};

			if (settings.Timeout != null)
				boots.Timeout = settings.Timeout;
			if (settings.ReadWriteTimeout != null)
				boots.ReadWriteTimeout = settings.ReadWriteTimeout.Value;
			if (settings.NetworkRetries != null)
				boots.NetworkRetries = settings.NetworkRetries.Value;
			if (settings.Channel != null)
				boots.Channel = settings.Channel.Value;
			if (settings.Product != null)
				boots.Product = settings.Product.Value;
			if (settings.FileType != null)
				boots.FileType = settings.FileType;
			if (settings.Url != null)
				boots.Url = settings.Url;

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
				value ??= "";

				// avoid System.FormatException from string.Format
				value = value.Replace ("{", "{{").Replace ("}", "}}");

				context.Log.Write (verbosity, level, value);
			}

			public override void WriteLine (string format, params object [] args)
			{
				context.Log.Write (verbosity, level, format, args);
			}
		}
	}
}
