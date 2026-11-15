using Microsoft.Extensions.Logging.Abstractions;
using System.ServiceModel;
using Xunit;

namespace AutoTask.Api.Test.Unit;

/// <summary>Unit tests for <see cref="AtwsSoapClientProvider"/> that never open a connection.</summary>
public class AtwsSoapClientProviderTests
{
	private static AtwsSoapClientProvider NewProvider(ClientOptions options)
		=> new("fake-user", "fake-password", options, new AutoTaskLogger(NullLogger.Instance));

	/// <summary>Verifies a configured server id builds the zone endpoint without any lookup, and the client is cached.</summary>
	[Fact]
	public async System.Threading.Tasks.Task GetAsync_WithServerId_BuildsEndpointAndCaches()
	{
		using var provider = NewProvider(new ClientOptions { ServerId = 5 });

		var first = await provider.GetAsync(TestContext.Current.CancellationToken);
		var second = await provider.GetAsync(TestContext.Current.CancellationToken);

		Assert.Same(first, second);
		Assert.Equal("https://webservices5.autotask.net/ATServices/1.6/atws.asmx", first.Endpoint.Address.Uri.ToString());
		Assert.Equal("fake-user", first.ClientCredentials.UserName.UserName);
		Assert.Equal("fake-password", first.ClientCredentials.UserName.Password);
	}

	/// <summary>Verifies the service binding is HTTPS with basic credentials and a large message quota.</summary>
	[Fact]
	public void CreateServiceBinding_UsesTransportSecurityWithBasicCredentials()
	{
		var binding = (BasicHttpBinding)PrivateAccess.Invoke(typeof(AtwsSoapClientProvider), null, "CreateServiceBinding")!;

		Assert.Equal(BasicHttpSecurityMode.Transport, binding.Security.Mode);
		Assert.Equal(HttpClientCredentialType.Basic, binding.Security.Transport.ClientCredentialType);
		Assert.Equal(int.MaxValue, binding.MaxReceivedMessageSize);
	}

	/// <summary>Verifies the zone binding is unauthenticated and honours the configured timeouts.</summary>
	[Fact]
	public void CreateZoneInfoBinding_UsesConfiguredTimeouts()
	{
		using var provider = NewProvider(new ClientOptions { OpenTimeoutMs = 1234, SendTimeoutMs = 5678 });

		var binding = (BasicHttpBinding)PrivateAccess.Invoke(typeof(AtwsSoapClientProvider), provider, "CreateZoneInfoBinding")!;

		Assert.Equal(TimeSpan.FromMilliseconds(1234), binding.OpenTimeout);
		Assert.Equal(TimeSpan.FromMilliseconds(5678), binding.SendTimeout);
		Assert.Equal(HttpClientCredentialType.None, binding.Security.Transport.ClientCredentialType);
	}

	/// <summary>Verifies disposing a provider that never created a client is harmless.</summary>
	[Fact]
	public void Dispose_WithoutClient_DoesNotThrow()
		=> Assert.Null(Record.Exception(() => NewProvider(new ClientOptions()).Dispose()));
}
