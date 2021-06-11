using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Reflection;
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
				new Option ("--timeout",
					$"Specifies a timeout for HttpClient. If omitted, uses the .NET default of 100 seconds.")
				{
					Argument = new Argument<double>("seconds")
				},
				new Option ("--read-write-timeout",
					$"Specifies a timeout for reading/writing from a HttpClient stream. If omitted, uses a default of 300 seconds.")
				{
					Argument = new Argument<double>("seconds")
				},
				new Option ("--retries",
					$"Specifies a number of retries for HttpClient failures. If omitted, uses a default of 3 retries.")
				{
					Argument = new Argument<int>("int")
				},
				new Option(
					"--workload",
					$"*Experimental* install a .NET 6 workload. 'dotnet workload search' for valid workload names.")
				{
					Argument = new Argument<string>("workload")
				},
				new Option(
					"--version",
					$"*Experimental* specify a specific version for .NET 6 workloads. If omitted, latest is used.")
				{
					Argument = new Argument<string>("version")
				},
				new Option(
					"--workload-source",
					$"*Experimental* specify a NuGet feed for .NET 6 workloads. If omitted, uses NuGet.org.")
				{
					Argument = new Argument<string>("url")
				},
			};
			rootCommand.Name = "boots";
			rootCommand.AddValidator (Validator);
			rootCommand.Description = $"boots {Version} File issues at: https://github.com/jonathanpeppers/boots/issues";
			var method = typeof (Program).GetMethod (nameof (Run), BindingFlags.Static | BindingFlags.NonPublic);
			if (method == null) {
				throw new Exception ("Unable to find Run method!");
			}
			rootCommand.Handler = CommandHandler.Create (method);

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
			if (result.OptionResult ("--url") == null &&
				result.OptionResult ("--stable") == null &&
				result.OptionResult ("--preview") == null &&
				result.OptionResult ("--workload") == null) {
				return "At least one of --url, --stable, --preview, or --workload must be used";
			}
			if (result.OptionResult ("--workload") != null && Environment.Version < new Version (6, 0)) {
				return "Must be running on .NET 6 or higher to use --workload.";
			}
			return "";
		}

		static async Task Run (
			string url,
			string stable = "",
			string preview = "",
			FileType? fileType = null,
			double? timeout = null,
			double? readWriteTimeout = null,
			int? retries = null,
			string workload = "",
			string version = "",
			string workloadSource = "")
		{
			var cts = new CancellationTokenSource ();
			Console.CancelKeyPress += (sender, e) => cts.Cancel ();

			var boots = new Bootstrapper {
				Url = url,
				FileType = fileType,
				Workload = workload,
				Version = version,
				WorkloadSource = workloadSource,
			};
			if (timeout != null) {
				boots.Timeout = TimeSpan.FromSeconds (timeout.Value);
			}
			if (readWriteTimeout != null) {
				boots.ReadWriteTimeout = TimeSpan.FromSeconds (readWriteTimeout.Value);
			}
			if (retries != null) {
				boots.NetworkRetries = retries.Value;
			}
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
