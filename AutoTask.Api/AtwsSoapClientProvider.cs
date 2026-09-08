using AutoTask.Api.Extensions;
using System;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;

namespace AutoTask.Api;

/// <summary>
/// Owns the SOAP channel to AutoTask: locating the caller's zone, building the bindings,
/// applying the credentials, and tearing the channel down again. Kept separate from
/// <see cref="Client"/>, which is concerned with the API calls made over that channel.
/// </summary>
/// <remarks>Initializes a new provider for the supplied credentials and options.</remarks>
internal sealed class AtwsSoapClientProvider(
	string username,
	string password,
	ClientOptions clientOptions,
	AutoTaskLogger autoTaskLogger) : IDisposable
{
	private ATWSSoapClient? _atwsSoapClient;

	/// <summary>
	/// Returns the SOAP client, creating and caching it on first use.
	/// </summary>
	internal async Task<ATWSSoapClient> GetAsync(CancellationToken cancellationToken)
	{
		if (_atwsSoapClient != null)
		{
			return _atwsSoapClient;
		}

		var endpointAddressUrl = await GetEndpointAddressUrlAsync(cancellationToken).ConfigureAwait(false);

		// Create the endpoint address.
		var ea = new EndpointAddress(endpointAddressUrl);

		var atwsSoapClient = new ATWSSoapClient(CreateServiceBinding(), ea);
		atwsSoapClient.Endpoint.EndpointBehaviors.Add(autoTaskLogger);
		atwsSoapClient.ClientCredentials.UserName.UserName = username;
		atwsSoapClient.ClientCredentials.UserName.Password = password;
		return _atwsSoapClient = atwsSoapClient;
	}

	/// <summary>
	/// Determines the zone-specific service URL, either from the configured server id or by
	/// asking the well-known zone information endpoint.
	/// </summary>
	private async Task<string> GetEndpointAddressUrlAsync(CancellationToken cancellationToken)
	{
		if (clientOptions.ServerId is not null)
		{
			return $"https://webservices{clientOptions.ServerId}.autotask.net/ATServices/1.6/atws.asmx";
		}

		var endpoint = new EndpointAddress("https://webservices.autotask.net/ATServices/1.6/atws.asmx");
		using var zoneInfoAutoTaskClient = new ATWSSoapClient(CreateZoneInfoBinding(), endpoint);

		var zoneInfo = await zoneInfoAutoTaskClient
			.getZoneInfoAsync(new getZoneInfoRequest(username))
			.WithCancellation(cancellationToken)
			.ConfigureAwait(false);
		zoneInfoAutoTaskClient.Close();
		return zoneInfo.getZoneInfoResult.URL;
	}

	/// <summary>Creates the binding used for the small, unauthenticated zone information call.</summary>
	private BasicHttpBinding CreateZoneInfoBinding()
		=> new()
		{
			SendTimeout = new TimeSpan(0, 0, 0, 0, clientOptions.SendTimeoutMs),
			OpenTimeout = new TimeSpan(0, 0, 0, 0, clientOptions.OpenTimeoutMs),
			MaxReceivedMessageSize = 10000,
			ReaderQuotas =
			{
				MaxStringContentLength = 10000,
				MaxDepth = 10000,
				MaxArrayLength = 10000
			},
			Security = new BasicHttpSecurity
			{
				Mode = BasicHttpSecurityMode.Transport,
				Transport = new HttpTransportSecurity
				{
					ClientCredentialType = HttpClientCredentialType.None,
					ProxyCredentialType = HttpProxyCredentialType.None,
				}
			}
		};

	/// <summary>
	/// Creates the binding used for authenticated calls.
	/// Must use BasicHttpBinding instead of WSHttpBinding, otherwise a
	/// "SOAP header Action was not understood." is thrown.
	/// The maximum received message size must be set explicitly, otherwise the default
	/// 65536 byte quota is exceeded.
	/// </summary>
	private static BasicHttpBinding CreateServiceBinding()
		=> new()
		{
			Security =
			{
				Mode = BasicHttpSecurityMode.Transport,
				Transport = { ClientCredentialType = HttpClientCredentialType.Basic }
			},
			MaxReceivedMessageSize = 2147483647
		};

	/// <summary>
	/// Closes the SOAP client, falling back to aborting it if a graceful close is not possible.
	/// </summary>
	public void Dispose()
	{
		try
		{
			_atwsSoapClient?.Close();
		}
		catch (Exception exception) when (exception is CommunicationException or TimeoutException)
		{
			_atwsSoapClient?.Abort();
		}
		catch
		{
			_atwsSoapClient?.Abort();
			throw;
		}
	}
}
