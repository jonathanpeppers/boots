using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Boots.Core
{
	public class AsyncProcess : IDisposable
	{
		public string Command { get; set; }
		public string Arguments { get; set; }
		public bool Elevate { get; set; } = false;

		Process process;

		public AsyncProcess () { }

		public AsyncProcess (string cmd, params string [] argumentList)
		{
			Command = cmd;
			Arguments = string.Join (" ", argumentList);
		}

		async Task<int> Run (CancellationToken token)
		{
			if (RuntimeInformation.IsOSPlatform (OSPlatform.OSX) && Elevate) {
				Arguments = $"{Command} {Arguments}";
				Command = "/usr/bin/sudo";
			}

			process = new Process {
				StartInfo = new ProcessStartInfo {
					FileName = Command,
					Arguments = Arguments,
					UseShellExecute = false,
					RedirectStandardError = true,
					RedirectStandardOutput = true,
				}
			};

			// TODO Fix output capture / logging
			process.ErrorDataReceived += (sender, e) => Console.WriteLine (e.Data);
			process.OutputDataReceived += (sender, e) => Console.WriteLine (e.Data);

			process.Start ();
			process.BeginErrorReadLine ();
			process.BeginOutputReadLine ();

			await Task.Run (process.WaitForExit, token);
			return process.ExitCode;
		}

		public async Task<int> RunAsync (CancellationToken token)
		{
			int exitCode = await Run (token);
			if (exitCode != 0)
				throw new Exception ($"'{Command}' with arguments '{Arguments}' exited with code {process.ExitCode}");

			return exitCode;
		}

		public async Task<int> TryRunAsync (CancellationToken token)
		{
			return await Run (token);
		}

		public void Dispose ()
		{
			process?.Dispose ();
		}
	}
}
