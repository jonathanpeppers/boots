using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace Boots.Tests
{
	public class MainTests : IDisposable
	{
		const string DefaultErrorMessage = "At least one of --url, --stable, or --preview must be used";
		readonly TextWriter consoleError;
		readonly StringWriter stderr;
		readonly MethodInfo main;

		public MainTests ()
		{
			consoleError = Console.Error;
			Console.SetError (stderr = new StringWriter ());

			var program = Type.GetType ("Boots.Program, Boots", throwOnError: true);
			main = program.GetMethod ("Main", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			Assert.NotNull (main);
		}

		public void Dispose ()
		{
			Console.SetError (consoleError);
		}

		[Fact]
		public async Task Empty ()
		{
			var args = Array.Empty<string> ();
			var task = (Task) main.Invoke (null, new object [] { args });
			await task;
			Assert.Contains (DefaultErrorMessage, stderr.ToString ());
		}

		[Fact]
		public async Task Stable ()
		{
			var args = new [] { "--stable", "Xamarin.Android" };
			var task = (Task) main.Invoke (null, new object [] { args });
			await task;
			Assert.DoesNotContain (DefaultErrorMessage, stderr.ToString ());
		}
	}
}
