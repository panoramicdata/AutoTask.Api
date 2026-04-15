namespace AutoTask.Api.Config;

/// <summary>Configuration for connecting to the AutoTask API.</summary>
public class AutoTaskConfiguration
{
	/// <summary>Gets or sets the AutoTask username.</summary>
	public string Username { get; set; } = null!;
	/// <summary>Gets or sets the AutoTask password.</summary>
	public string Password { get; set; } = null!;
	/// <summary>Gets or sets the AutoTask integration code.</summary>
	public string IntegrationCode { get; set; } = null!;

	internal void Validate()
	{
		if (string.IsNullOrWhiteSpace(Username))
		{
			throw new ConfigurationException($"{nameof(Username)} must be set.");
		}
		if (string.IsNullOrWhiteSpace(Password))
		{
			throw new ConfigurationException($"{nameof(Password)} must be set.");
		}
		if (string.IsNullOrWhiteSpace(IntegrationCode))
		{
			throw new ConfigurationException($"{nameof(IntegrationCode)} must be set.");
		}
	}
}
