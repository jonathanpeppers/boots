using System.Threading.Tasks;
using Boots.Core;
using Xunit;
using Xunit.Abstractions;

namespace Boots.Tests
{
	public class WorkloadInstallerTests
	{
		readonly TestWriter logger;

		public WorkloadInstallerTests (ITestOutputHelper output)
		{
			logger = new TestWriter (output);
		}

		[Fact]
		public async Task Test ()
		{
			var boots = new Bootstrapper {
				Workload = "Microsoft.NET.Sdk.Android.Manifest-6.0.100",
				WorkloadSource = "https://aka.ms/maui-preview/index.json",
				Version = "30.0.100-preview.5.28",
				Logger = logger,
			};
			await boots.Install ();
		}
	}
}
