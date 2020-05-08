using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Boots.Core
{
	class AsyncProcess : IDisposable
	{
		readonly Bootstrapper boots;

		public string Command { get; set; }
		public string Arguments { get; set; }
		public bool Elevate { get; set; } = false;

		Process process;

		public AsyncProcess (Bootstrapper boots)
		{
			this.boots = boots;
		}

		public AsyncProcess (Bootstrapper boots, string cmd, params string [] argumentList)
			: this (boots)
		{
			Command = cmd;
			Arguments = string.Join (" ", argumentList);
		}

		Process CreateProcess ()
		{
			if (!Helpers.IsWindows && Elevate) {
				Arguments = $"{Command} {Arguments}";
				Command = "/usr/bin/sudo";
			}
			return new Process {
				StartInfo = new ProcessStartInfo {
					FileName = Command,
					Arguments = Arguments,
					UseShellExecute = false,
					RedirectStandardError = true,
					RedirectStandardOutput = true,
				}
			};
		}

		Task StartAndWait (Process process, CancellationToken token)
		{
			process.Start ();
			process.BeginErrorReadLine ();
			process.BeginOutputReadLine ();
			return Task.Run (process.WaitForExit, token);
		}

		async Task<int> Run (CancellationToken token = new CancellationToken ())
		{
			process = CreateProcess ();
			process.ErrorDataReceived += (sender, e) => {
				if (e.Data != null)
					boots.Logger.WriteLine (e.Data);
			};
			process.OutputDataReceived += (sender, e) => {
				if (e.Data != null)
					boots.Logger.WriteLine (e.Data);
			};

			await StartAndWait (process, token);
			return process.ExitCode;
		}

		public async Task<int> RunAsync (CancellationToken token = new CancellationToken ())
		{
			int exitCode = await Run (token);
			if (exitCode != 0)
				throw new Exception ($"'{Command}' with arguments '{Arguments}' exited with code {process.ExitCode}");
			return exitCode;
		}

		public async Task<string> RunWithOutputAsync (CancellationToken token = new CancellationToken ())
		{
			var builder = new StringBuilder ();
			process = CreateProcess ();
			process.ErrorDataReceived += (sender, e) => {
				if (e.Data != null)
					builder.AppendLine (e.Data);
			};
			process.OutputDataReceived += (sender, e) => {
				if (e.Data != null)
					builder.AppendLine (e.Data);
			};

			await StartAndWait (process, token);
			if (process.ExitCode != 0)
				throw new Exception ($"'{Command}' with arguments '{Arguments}' exited with code {process.ExitCode}");
			return builder.ToString ();
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
