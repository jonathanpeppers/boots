using System;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.NET.Sdk.WorkloadManifestReader;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Packaging.Core;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

namespace Boots.Core
{
	class WorkloadInstaller : Installer
	{
		static readonly SourceCacheContext cache = NullSourceCacheContext.Instance;

		readonly SourceRepository source;
		readonly ILogger logger;

		public WorkloadInstaller (Bootstrapper boots) : base (boots)
		{
			var url = string.IsNullOrEmpty (boots.WorkloadSource) ? NuGetConstants.V3FeedUrl : boots.WorkloadSource;
			source = Repository.Factory.GetCoreV3 (url);
			logger = new NuGetLogger (boots);
		}

		public override string Extension => ".nupkg";

		public async override Task Install (string _, CancellationToken token = default)
		{
			var directory = GetManifestDirectory ();
			if (directory == null) {
				throw new Exception ($"Unable to find a workload named '{Boots.Workload}'.");
			}
			if (!string.IsNullOrEmpty (Boots.Version)) {
				// Download workload .nupkg file
				var textInfo = CultureInfo.InvariantCulture.TextInfo;
				var id = $"{textInfo.ToTitleCase (directory.Name)}.Manifest-{directory.Parent.Name}";
				var package = new PackageIdentity (id, new NuGetVersion (Boots.Version));
				await Download (package, directory.FullName, token);
			}
		}

		DirectoryInfo? GetManifestDirectory ()
		{
			var provider = new SdkDirectoryWorkloadManifestProvider (@"C:\Program Files\dotnet", "6.0.100-preview.5.21302.13");
			foreach (var directory in provider.GetManifestDirectories ()) {
				var dir = new DirectoryInfo (directory);
				if (dir.Name.IndexOf (Boots.Workload, StringComparison.OrdinalIgnoreCase) != -1) {
					return dir;
				}
			}
			return null;
		}

		async Task Download (PackageIdentity package, string manifestDirectory, CancellationToken token)
		{
			var byIdRes = await source.GetResourceAsync<FindPackageByIdResource> ();
			if (await byIdRes.DoesPackageExistAsync (package.Id, package.Version, cache, logger, token)) {
				Boots.Logger.WriteLine ($"Found {package}");
				var temp = Path.GetTempPath ();
				var resource = await source.GetResourceAsync<DownloadResource> (token);
				using var downloader = await byIdRes.GetPackageDownloaderAsync (package, cache, logger, token);
				var context = new PackageDownloadContext (cache, directDownloadDirectory: temp, directDownload: true);
				using var result = await resource.GetDownloadResourceResultAsync (package, context, temp, logger, token);
				using var zip = new ZipArchive (result.PackageStream);
				foreach (var entry in zip.Entries) {
					if (entry.Name.StartsWith ("WorkloadManifest.", StringComparison.OrdinalIgnoreCase)) {
						var path = Path.Combine (manifestDirectory, entry.Name);
						Boots.Logger.WriteLine ($"Updating: {path}");
						entry.ExtractToFile (path, overwrite: true);
					}
				}
			} else {
				throw new Exception ($"{package} not found!");
			}
		}

		class NuGetLogger : ILogger
		{
			readonly TextWriter logger;

			public NuGetLogger (Bootstrapper boots)
			{
				logger = boots.Logger;
			}

			public void Log (LogLevel level, string data) => logger.WriteLine (data);

			public void Log (ILogMessage message) => logger.WriteLine (message.ToString ());

			public Task LogAsync (LogLevel level, string data) => logger.WriteLineAsync (data);

			public Task LogAsync (ILogMessage message) => logger.WriteLineAsync (message.ToString ());

			public void LogDebug (string data) => logger.WriteLine (data);

			public void LogError (string data) => logger.WriteLine (data);

			public void LogInformation (string data) => logger.WriteLine (data);

			public void LogInformationSummary (string data) => logger.WriteLine (data);

			public void LogMinimal (string data) => logger.WriteLine (data);

			public void LogVerbose (string data) => logger.WriteLine (data);

			public void LogWarning (string data) => logger.WriteLine (data);
		}
	}
}
