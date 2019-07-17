using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Build.Locator;

namespace Boots.Core
{
	class VsixInstaller : Installer
	{
		public VsixInstaller (Bootstrapper boots) : base (boots) { }

		public string FilePath { get; set; }

		public async override Task Install (CancellationToken token)
		{
			if (string.IsNullOrEmpty (FilePath))
				throw new ArgumentException (nameof (FilePath));

			var vsixInstaller = await GetVsixInstallerPath (token);

			var psi = new ProcessStartInfo {
				FileName = vsixInstaller,
				Arguments = $"/quiet \"{FilePath}\"",
				UseShellExecute = false,
				RedirectStandardError = true,
				RedirectStandardOutput = true,
			};
			using (var process = new Process ()) {
				process.StartInfo = psi;
				process.ErrorDataReceived += (sender, e) => Boots.Logger.WriteLine (e.Data);
				process.OutputDataReceived += (sender, e) => Boots.Logger.WriteLine (e.Data);
				process.BeginErrorReadLine ();
				process.BeginOutputReadLine ();
				process.Start ();

				await Task.Run (process.WaitForExit, token);

				if (process.ExitCode != 0)
					throw new Exception ($"VSIXInstaller.exe exited with code {process.ExitCode}");
			}
		}

		async Task<string> GetVsixInstallerPath (CancellationToken token)
		{
			if (vsixInstaller != null)
				return vsixInstaller;

			return await Task.Run (() => {
				var instances = MSBuildLocator.QueryVisualStudioInstances (new VisualStudioInstanceQueryOptions {
					DiscoveryTypes = DiscoveryType.VisualStudioSetup,
				});
				foreach (var instance in instances) {
					return vsixInstaller = Path.Combine (instance.VisualStudioRootPath, "VSIXInstaller.exe");
				}
				throw new Exception ("No Visual Studio instances found!");
			}, token);
		}

		string vsixInstaller;
	}
}
