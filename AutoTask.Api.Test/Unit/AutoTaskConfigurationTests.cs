using AutoTask.Api.Config;
using Xunit;

namespace AutoTask.Api.Test.Unit;

/// <summary>Unit tests for <see cref="AutoTaskConfiguration"/> validation. No AutoTask credentials required.</summary>
public class AutoTaskConfigurationTests
{
	/// <summary>Verifies that each missing credential is reported by name when a client is constructed.</summary>
	[Theory]
	[InlineData(null, "password", "integrationCode", "Username")]
	[InlineData("username", null, "integrationCode", "Password")]
	[InlineData("username", "password", null, "IntegrationCode")]
	[InlineData("", "password", "integrationCode", "Username")]
	[InlineData("username", " ", "integrationCode", "Password")]
	public void Validate_WithMissingCredential_Throws(
		string? username,
		string? password,
		string? integrationCode,
		string expectedPropertyName)
	{
		var configuration = new AutoTaskConfiguration
		{
			Username = username!,
			Password = password!,
			IntegrationCode = integrationCode!
		};

		var exception = Assert.Throws<ConfigurationException>(() => new AutoTaskClient(configuration));

		Assert.Equal($"{expectedPropertyName} must be set.", exception.Message);
	}

	/// <summary>Verifies that a fully populated configuration constructs a client without contacting AutoTask.</summary>
	[Fact]
	public void Validate_WithCompleteConfiguration_Succeeds()
	{
		var configuration = new AutoTaskConfiguration
		{
			Username = "username",
			Password = "password",
			IntegrationCode = "integrationCode"
		};

		using var autoTaskClient = new AutoTaskClient(configuration);

		Assert.Equal("id", autoTaskClient.PrimaryKey);
	}
}
