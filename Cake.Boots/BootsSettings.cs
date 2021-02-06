using System;

public class BootsSettings
{
	public TimeSpan? Timeout { get; set; }

	public TimeSpan? ReadWriteTimeout { get; set; }

	public int? NetworkRetries { get; set; }

	public ReleaseChannel? Channel { get; set; }

	public Product? Product { get; set; }

	public FileType? FileType { get; set; }

	public string? Url { get; set; }
}
