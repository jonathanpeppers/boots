using System;
using System.Threading;
using System.Threading.Tasks;
using Boots.Core;
using Xunit;

namespace Boots.Tests
{
	public class AsyncProcessTests
	{
		[Fact]
		public async Task EchoShouldNotThrow ()
		{
			using (var proc = new AsyncProcess {
				Command = "echo",
				Arguments = "hello world"
			}) {
				await proc.RunAsync (new CancellationToken ());
			}
		}

		[Fact]
		public async Task NonExistingCommandShouldThrow ()
		{
			using (var proc = new AsyncProcess {
				Command = Guid.NewGuid ().ToString ()
			}) {
				await Assert.ThrowsAsync<System.ComponentModel.Win32Exception> (() =>
					 proc.RunAsync (new CancellationToken ()));
			}
		}

	}
}
