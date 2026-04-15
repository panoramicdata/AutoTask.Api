namespace AutoTask.Api;

/// <summary>Options for configuring an AutoTask API client.</summary>
public class ClientOptions
{
	/// <summary>Gets or sets the connection open timeout in milliseconds. Default is 10000.</summary>
	public int OpenTimeoutMs { get; set; } = 10000;

	/// <summary>Gets or sets the send timeout in milliseconds. Default is 30000.</summary>
	public int SendTimeoutMs { get; set; } = 30000;

	/// <summary>Gets or sets the AutoTask server ID, or <see langword="null"/> to auto-detect via zone lookup.</summary>
	public int? ServerId { get; set; }
}