using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Boots.Core
{
	class VsixInstaller : Installer
	{
		string visualStudioDirectory;

		public VsixInstaller (Bootstrapper boots) : base (boots) { }

		public override string Extension => ".vsix";

		public async override Task Install (string file, CancellationToken token = new CancellationToken ())
		{
			if (string.IsNullOrEmpty (file))
				throw new ArgumentException (nameof (file));

			var vs = await GetVisualStudioDirectory (token);
			var vsixInstaller = Path.Combine (vs, "Common7", "IDE", "VSIXInstaller.exe");
			var log = Path.GetTempFileName ();
			try {
				await RunProcess (vsixInstaller, $"/quiet /logFile:{log} \"{file}\"", token);
			} finally {
				await ReadLogFile (log, token);
			}
		}

		Task ReadLogFile (string log, CancellationToken token)
		{
			return Task.Factory.StartNew (() => {
				if (File.Exists (log)) {
					using (var reader = File.OpenText (log)) {
						while (!reader.EndOfStream) {
							Boots.Logger.WriteLine (reader.ReadLine ());
						}
					}
				} else {
					Boots.Logger.WriteLine ($"Log file did not exist: {log}");
				}
			}, token);
		}

		async Task RunProcess (string fileName, string arguments, CancellationToken token)
		{
			var psi = new ProcessStartInfo {
				FileName = fileName,
				Arguments = arguments,
				UseShellExecute = false,
				RedirectStandardError = true,
				RedirectStandardOutput = true,
			};
			using (var process = new Process ()) {
				process.StartInfo = psi;
				process.ErrorDataReceived += (sender, e) => Boots.Logger.WriteLine (e.Data);
				process.OutputDataReceived += (sender, e) => Boots.Logger.WriteLine (e.Data);
				process.Start ();
				process.BeginErrorReadLine ();
				process.BeginOutputReadLine ();

				await Task.Run (process.WaitForExit, token);

				if (process.ExitCode != 0)
					throw new Exception ($"VSIXInstaller.exe exited with code {process.ExitCode}");
			}
		}

		static async Task<string> Exec (string fileName, string arguments, CancellationToken token)
		{
			var info = new ProcessStartInfo {
				FileName = fileName,
				WorkingDirectory = Path.GetDirectoryName (fileName),
				Arguments = arguments,
				UseShellExecute = false,
				RedirectStandardOutput = true,
				CreateNoWindow = true,
				WindowStyle = ProcessWindowStyle.Hidden,
			};
			using (var process = Process.Start (info)) {
				await Task.Run (process.WaitForExit, token);
				return process.StandardOutput.ReadToEnd ().Trim ();
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
				visualStudioDirectory = await Exec (vswhere, "-latest -products * -property installationPath", token);
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
