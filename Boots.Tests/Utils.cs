using System;
using System.IO;
using System.Text;
using Xunit.Abstractions;

namespace Boots.Tests
{
	public class TestWriter : TextWriter
	{
		readonly ITestOutputHelper output;

		public TestWriter (ITestOutputHelper output)
		{
			this.output = output;
		}

		public override Encoding Encoding => Encoding.Default;

		public override void WriteLine (string value)
		{
			if (value != null) {
				output.WriteLine (value);
				Console.WriteLine (value);
			}
		}

		public override void WriteLine (string format, params object [] args)
		{
			output.WriteLine (format, args);
			Console.WriteLine (format, args);
		}
	}
}
