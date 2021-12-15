public enum ReleaseChannel
{
	/// <summary>
	/// The "stable" channel, or https://aka.ms/vs/17/release/channel on Windows, and "Stable" on Mac.
	/// </summary>
	Stable,
	/// <summary>
	/// The "preview" channel, or https://aka.ms/vs/17/pre/channel on Windows, and "Beta" on Mac.
	/// </summary>
	Preview,
	/// <summary>
	/// This channel is only valid for Visual Studio for Mac, it resolves to Preview on Windows.
	/// </summary>
	Alpha,
}
