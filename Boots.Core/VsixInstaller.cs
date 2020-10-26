using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Boots.Core
{
	class VsixInstaller : Installer
	{
		/// <summary>
		/// See: https://stackoverflow.com/a/28212173
		/// </summary>
		const int AlreadyInstalledException = 1001;

		string? visualStudioDirectory;

		public VsixInstaller (Bootstrapper boots) : base (boots) { }

		public override string Extension => ".vsix";

		public async override Task Install (string file, CancellationToken token = default)
		{
			if (string.IsNullOrEmpty (file))
				throw new ArgumentException (nameof (file));
			if (!File.Exists (file))
				throw new FileNotFoundException ($"{Extension} file did not exist: {file}", file);

			var vs = await GetVisualStudioDirectory (token);
			var vsixInstaller = Path.Combine (vs, "Common7", "IDE", "VSIXInstaller.exe");
			var log = Path.GetTempFileName ();
			try {
				using (var process = new AsyncProcess (Boots) {
					Command = vsixInstaller,
					Arguments = $"/quiet /logFile:{log} \"{file}\"",
				}) {
					int exitCode = await process.RunAsync (token, throwOnError: false);
					if (exitCode == AlreadyInstalledException) {
						Boots.Logger.WriteLine ("VSIX already installed.");
					} else if (exitCode != 0) {
						process.ThrowForExitCode (exitCode);
					}
				}
			} finally {
				await PrintLogFileAndDelete (log, token);
			}
		}

		async Task<string> GetVisualStudioDirectory (CancellationToken token)
		{
			if (visualStudioDirectory != null)
				return visualStudioDirectory;

			var vsInstallDir = Environment.GetEnvironmentVariable ("VSINSTALLDIR");
			if (string.IsNullOrEmpty (vsInstallDir)) {
				var programFiles = Environment.GetFolderPath (Environment.SpecialFolder.ProgramFilesX86);
				var vswhere = Path.Combine (programFiles, "Microsoft Visual Studio", "Installer", "vswhere.exe");
				if (!File.Exists (vswhere))
					throw new FileNotFoundException ("Cannot find vswhere.exe!", vswhere);
				using (var process = new AsyncProcess (Boots) {
					Command = vswhere,
					Arguments = "-latest -products * -property installationPath",
				}) {
					visualStudioDirectory = await process.RunWithOutputAsync (token);
					visualStudioDirectory = visualStudioDirectory.Trim ();
				}
				if (!Directory.Exists (visualStudioDirectory)) {
					throw new DirectoryNotFoundException ($"vswhere.exe result returned a directory that did not exist: {visualStudioDirectory}");
				}
				Boots.Logger.WriteLine ($"Using path from vswhere: {visualStudioDirectory}");
				return visualStudioDirectory;
			} else {
				Boots.Logger.WriteLine ($"Using path from %VSINSTALLDIR%: {visualStudioDirectory}");
				return visualStudioDirectory = vsInstallDir;
			}
		}
	}
}
