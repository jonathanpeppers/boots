using System;
using System.IO;
using System.Text;
using Boots.Core;
using Xunit;
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
			output.WriteLine (value ?? "");
		}

		public override void WriteLine (string format, params object [] args)
		{
			output.WriteLine (format, args);
		}
	}
}
