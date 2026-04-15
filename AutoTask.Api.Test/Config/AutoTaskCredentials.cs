namespace AutoTask.Api.Test.Config;

/// <summary>AutoTask credentials loaded from test configuration.</summary>
public class AutoTaskCredentials
{
	/// <summary>Gets or sets the AutoTask username.</summary>
	public string Username { get; set; } = null!;
	/// <summary>Gets or sets the AutoTask password.</summary>
	public string Password { get; set; } = null!;
	/// <summary>Gets or sets the AutoTask integration code.</summary>
	public string IntegrationCode { get; set; } = null!;
	/// <summary>Gets or sets the optional AutoTask server ID.</summary>
	public int? ServerId { get; set; }
}