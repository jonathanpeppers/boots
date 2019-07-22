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

		public override string Extension => ".vsix";

		public async override Task Install (string file, CancellationToken token = new CancellationToken ())
		{
			if (string.IsNullOrEmpty (file))
				throw new ArgumentException (nameof (file));

			var vsixInstaller = await GetVsixInstallerPath (token);

			var psi = new ProcessStartInfo {
				FileName = vsixInstaller,
				Arguments = $"/quiet \"{file}\"",
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
