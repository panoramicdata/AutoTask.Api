using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;

namespace AutoTask.Api
{
	public class Client
	{
		private readonly ATWSSoapClient _autoTaskClient;
		private readonly AutotaskIntegrations _autotaskIntegrations;

		public Client(string username, string password)
		{
			var binding = new BasicHttpBinding
			{
				SendTimeout = new TimeSpan(0, 0, 0, 0, 100000),
				OpenTimeout = new TimeSpan(0, 0, 0, 0, 100000),
				MaxReceivedMessageSize = 10000,
				ReaderQuotas =
				{
					MaxStringContentLength = 10000,
					MaxDepth = 10000,
					MaxArrayLength = 10000
				}
				,
				Security = new BasicHttpSecurity
				{
					Mode = BasicHttpSecurityMode.Transport,
					//Message = new BasicHttpMessageSecurity
					//{
					//	AlgorithmSuite = SecurityAlgorithmSuite.Default,
					//	ClientCredentialType = BasicHttpMessageCredentialType.Certificate
					//},
					Transport = new HttpTransportSecurity
					{
						ClientCredentialType = HttpClientCredentialType.None,
						ProxyCredentialType = HttpProxyCredentialType.None,
						//Realm = ""
					}
				}
			};
			var endpoint = new EndpointAddress("https://webservices1.autotask.net/ATServices/1.5/atws.asmx");
			_autoTaskClient = new ATWSSoapClient(binding, endpoint);
			var zoneInfo = _autoTaskClient.getZoneInfoAsync(new getZoneInfoRequest(username)).GetAwaiter().GetResult();

			// Create the binding.
			// must use BasicHttpBinding instead of WSHttpBinding
			// otherwise a "SOAP header Action was not understood." is thrown.
			var myBinding = new BasicHttpBinding
			{
				Security =
				{
					Mode = BasicHttpSecurityMode.Transport,
					Transport = {ClientCredentialType = HttpClientCredentialType.Basic}
				},
				MaxReceivedMessageSize = 2147483647
			};

			// Must set the size otherwise
			//The maximum message size quota for incoming messages (65536) has been exceeded. To increase the quota, use the MaxReceivedMessageSize property on the appropriate binding element.

			// Create the endpoint address.
			var ea = new EndpointAddress(zoneInfo.getZoneInfoResult.URL);

			_autoTaskClient = new ATWSSoapClient(myBinding, ea);
			_autoTaskClient.ClientCredentials.UserName.UserName = username;
			_autoTaskClient.ClientCredentials.UserName.Password = password;

			// have no clue what this does.
			_autotaskIntegrations = new AutotaskIntegrations();
		}

		public async Task<GetFieldInfoResponse> GetFieldInfoAsync(string psObjectType)
			=> await _autoTaskClient.GetFieldInfoAsync(new GetFieldInfoRequest(_autotaskIntegrations, psObjectType)).ConfigureAwait(false);

		public async Task<IEnumerable<Entity>> ExecuteQueryAsync(string sXml)
		{
			// this example will not handle the 500 results limitation.
			// AutoTask only returns up to 500 results in a response. if there are more you must query again for the next 500.
			// See https://www.powershellgallery.com/packages/Autotask/0.2.1.4/Content/Public%5CGet-AtwsData.ps1 for one approach:
			// - Get the highest id in a resultset and keep asking for items when the ID > the last observed value
			// This post: https://github.com/opendns/autotask-php/issues/9 hints that you CAN rely on using the ID as a paging method
			// as it is not possible to set t he order by in the query and that the results always come back in id asc order.
			var atwsResponse = await _autoTaskClient.queryAsync(new queryRequest(_autotaskIntegrations, sXml)).ConfigureAwait(false);
			if (atwsResponse.queryResult.ReturnCode != 1)
			{
				throw new AutoTaskQueryException(atwsResponse.queryResult);
			}
			// Executed fine

			return atwsResponse.queryResult.EntityResults;
		}

		public async Task<string> GetWsdlVersion() => (await _autoTaskClient.GetWsdlVersionAsync(new GetWsdlVersionRequest(_autotaskIntegrations)).ConfigureAwait(false)).GetWsdlVersionResult;
	}
}
