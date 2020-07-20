using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Boots.Core;
using Xunit;
using Xunit.Abstractions;

namespace Boots.Tests
{
	public class AsyncProcessTests
	{
		readonly Bootstrapper boots = new Bootstrapper ();

		public AsyncProcessTests (ITestOutputHelper output)
		{
			boots.Logger = new TestWriter (output);
		}

		[SkippableFact]
		public async Task EchoShouldNotThrow ()
		{
			using (var proc = new AsyncProcess (boots) {
				Command = Helpers.IsWindows ? "cmd" : "echo",
				Arguments = Helpers.IsWindows ? "/C echo test" : "test"
			}) {
				await proc.RunAsync (new CancellationToken ());
			}
		}

		[Fact]
		public async Task NonExistingCommandShouldThrow ()
		{
			using (var proc = new AsyncProcess (boots) {
				Command = Guid.NewGuid ().ToString ()
			}) {
				await Assert.ThrowsAsync<Win32Exception> (() => proc.RunAsync (new CancellationToken ()));
			}
		}

		[Fact]
		public async Task RunWithOutput ()
		{
			using (var proc = new AsyncProcess (boots) {
				Command = Helpers.IsWindows ? "cmd" : "echo",
				Arguments = Helpers.IsWindows ? "/C echo test" : "test"
			}) {
				var text = await proc.RunWithOutputAsync (new CancellationToken ());
				Assert.Equal ("test", text.Trim ());
			}
		}
	}
}
