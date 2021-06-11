using System;
using System.IO;
using System.Linq;
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

		readonly string sourceUrl;
		readonly SourceRepository source;
		readonly ILogger logger;

		public WorkloadInstaller (Bootstrapper boots) : base (boots)
		{
			sourceUrl = string.IsNullOrEmpty (boots.WorkloadSource) ? NuGetConstants.V3FeedUrl : boots.WorkloadSource;
			source = Repository.Factory.GetCoreV3 (sourceUrl);
			logger = new NuGetLogger (boots);
		}

		public override string Extension => ".nupkg";

		public async override Task Install (string _, CancellationToken token = default)
		{
			var provider = new SdkDirectoryWorkloadManifestProvider ("", "");
			var workload = GetWorkloadDefinition (provider);
			if (workload == null) {
				throw new Exception ($"Unable to find a workload named '{Boots.Workload}'.");
			}

			var resolver = WorkloadResolver.Create (provider, "", "");

			if (!string.IsNullOrEmpty (Boots.Version)) {
				// Download workload .nupkg file
				var package = new PackageIdentity (Boots.Workload, new NuGetVersion (Boots.Version));
				await Download (package, Path.GetTempPath (), token);
			}
		}

		WorkloadDefinition? GetWorkloadDefinition (SdkDirectoryWorkloadManifestProvider provider)
		{
			var id = new WorkloadDefinitionId (Boots.Workload);
			foreach (var manifestFile in provider.GetManifests ()) {
				var manifest = WorkloadManifestReader.ReadWorkloadManifest (manifestFile.manifestId, manifestFile.manifestStream);
				if (manifest.Workloads.TryGetValue (id, out var workload))
					return workload;
			}
			return null;
		}

		async Task Download (PackageIdentity package, string destination, CancellationToken token)
		{
			var byIdRes = await source.GetResourceAsync<FindPackageByIdResource> ();
			if (await byIdRes.DoesPackageExistAsync (package.Id, package.Version, cache, logger, token)) {
				Boots.Logger.WriteLine ($"Found {package}");

				var resource = await source.GetResourceAsync<DownloadResource> (token);
				using var downloader = await byIdRes.GetPackageDownloaderAsync (package, cache, logger, token);
				var context = new PackageDownloadContext (cache, directDownloadDirectory: destination, directDownload: true);
				using var result = await resource.GetDownloadResourceResultAsync (package, context, destination, logger, token);
				string path = Path.Combine (destination, $"{package}.nupkg");
				using (var stream = File.Create (path)) {
					await result.PackageStream.CopyToAsync (stream);
				}

				Boots.Logger.WriteLine ($"Downloaded {path}");
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
