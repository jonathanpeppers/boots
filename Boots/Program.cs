using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Threading;
using System.Threading.Tasks;
using Boots.Core;

namespace Boots
{
	class Program
	{
		static async Task Main (string [] args)
		{
			if (args.Length == 1 && IsUrl (args[0])) {
				await Run (args [0]);
				return;
			}

			const string options = "Options include: Xamarin.Android, Xamarin.iOS, Xamarin.Mac, and Mono.";
			var rootCommand = new RootCommand
			{
				new Option(
					"--url",
					"A URL to a pkg or vsix file to install")
				{
					Argument = new Argument<string>()
				},
				new Option(
					"--stable",
					$"Install the latest *stable* version of a product from VS manifests. {options}")
				{
					Argument = new Argument<string>("product")
				},
				new Option(
					"--preview",
					$"Install the latest *preview* version of a product from VS manifests. {options}")
				{
					Argument = new Argument<string>("product")
				},
				new Option(
					"--file-type",
					$"Specifies the type of file to be installed such as vsix, pkg, or msi. Defaults to vsix on Windows and pkg on macOS.")
				{
					Argument = new Argument<FileType>("file-type")
				},
			};
			rootCommand.Name = "boots";
			rootCommand.AddValidator (Validator);
			rootCommand.Description = $"boots {Version} File issues at: https://github.com/jonathanpeppers/boots/issues";
			rootCommand.Handler = CommandHandler.Create <string, string, string, FileType?> (Run);
			await rootCommand.InvokeAsync (args);
		}

		static bool IsUrl (string value)
		{
			try {
				new Uri (value);
				return true;
			} catch (UriFormatException) {
				return false;
			}
		}

		static string Validator (CommandResult result)
		{
			if (result.GetArgumentValueOrDefault ("--url") == null &&
				result.GetArgumentValueOrDefault ("--stable") == null &&
				result.GetArgumentValueOrDefault ("--preview") == null) {
				return "At least one of --url, --stable, or --preview must be used";
			}
			return "";
		}

		static async Task Run (string url, string stable = "", string preview = "", FileType? fileType = null)
		{
			var cts = new CancellationTokenSource ();
			Console.CancelKeyPress += (sender, e) => cts.Cancel ();

			var boots = new Bootstrapper {
				Url = url,
				FileType = fileType,
			};
			SetChannelAndProduct (boots, preview, ReleaseChannel.Preview);
			SetChannelAndProduct (boots, stable,  ReleaseChannel.Stable);
			await boots.Install (cts.Token);
		}

		static void SetChannelAndProduct (Bootstrapper boots, string product, ReleaseChannel channel)
		{
			if (!string.IsNullOrEmpty (product)) {
				boots.Channel = channel;
				boots.Product = Enum.Parse<Product> (product.Replace (".", ""));
			}
		}

		static string? Version => typeof (Program).Assembly.GetName ().Version?.ToString ();
	}
}
