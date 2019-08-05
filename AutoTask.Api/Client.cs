using AutoTask.Api.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;

namespace AutoTask.Api
{
	public class Client
	{
		private readonly ATWSSoapClient _autoTaskClient;
		private readonly AutotaskIntegrations _autotaskIntegrations;
		private readonly ILogger _logger;

		public Client(string username, string password, ILogger logger = null)
		{
			_logger = logger ?? new NullLogger<Client>();
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

			_autoTaskClient.Close();
			_autoTaskClient = new ATWSSoapClient(myBinding, ea);

			_autoTaskClient.Endpoint.EndpointBehaviors.Add(new AutoTaskLogger(_logger));

			_autoTaskClient.ClientCredentials.UserName.UserName = username;
			_autoTaskClient.ClientCredentials.UserName.Password = password;

			// have no clue what this does.
			_autotaskIntegrations = new AutotaskIntegrations();
		}

		public async Task<GetFieldInfoResponse> GetFieldInfoAsync(string psObjectType)
			=> await _autoTaskClient.GetFieldInfoAsync(new GetFieldInfoRequest(_autotaskIntegrations, psObjectType)).ConfigureAwait(false);

		public async Task<IEnumerable<Entity>> QueryAsync(string sXml)
		{
			// this example will not handle the 500 results limitation.
			// AutoTask only returns up to 500 results in a response. if there are more you must query again for the next 500.
			// See https://www.powershellgallery.com/packages/Autotask/0.2.1.4/Content/Public%5CGet-AtwsData.ps1 for one approach:
			// - Get the highest id in a resultset and keep asking for items when the ID > the last observed value
			// This post: https://github.com/opendns/autotask-php/issues/9 hints that you CAN rely on using the ID as a paging method
			// as it is not possible to set the order by in the query and that the results always come back in id asc order.
			var atwsResponse = await _autoTaskClient.queryAsync(new queryRequest(_autotaskIntegrations, sXml)).ConfigureAwait(false);
			if (atwsResponse.queryResult.ReturnCode != 1)
			{
				throw new AutoTaskApiException(atwsResponse.queryResult);
			}
			// Executed fine

			return atwsResponse.queryResult.EntityResults;
		}

		public async Task<Entity> CreateAsync(Entity entity)
		{
			var createRequest = new createRequest(_autotaskIntegrations, new[] { entity });
			var createResponse = await _autoTaskClient.createAsync(createRequest).ConfigureAwait(false);
			var errorCount = createResponse.createResult.Errors.Length;
			if (errorCount > 0)
			{
				_logger.LogError($"There was an error creating the entity. {errorCount} errors occurred.");
				for (var errorNum = 0; errorNum < errorCount; errorNum++)
				{
					_logger.LogError($"Error {errorNum + 1}: {createResponse.createResult.Errors[errorNum].Message}");
				}
				_logger.LogError("Entity: " + JsonConvert.SerializeObject(entity));
				throw new AutoTaskApiException($"Errors occurred during creation of the AutoTask entity: {string.Join(";", createResponse.createResult.Errors.Select(e => e.Message))}");
			}

			var createdEntity = createResponse?.createResult?.EntityResults?.FirstOrDefault();
			_logger.LogDebug($"Successfully created entity with Id: {createdEntity?.id.ToString() ?? "UNKNOWN!"}");
			if (createdEntity == null)
			{
				throw new AutoTaskApiException("Did not get a result back after creating the AutoTask entity.");
			}
			return createdEntity;
		}

		public async Task<Entity> DeleteAsync(Entity entity)
		{
			var deleteRequest = new deleteRequest(_autotaskIntegrations, new[] { entity });
			var deleteResponse = await _autoTaskClient.deleteAsync(deleteRequest).ConfigureAwait(false);
			var errorCount = deleteResponse.deleteResult.Errors.Length;
			if (errorCount > 0)
			{
				_logger.LogError($"There was an error deleting the entity. {errorCount} errors occurred.");
				for (var errorNum = 0; errorNum < errorCount; errorNum++)
				{
					_logger.LogError($"Error {errorNum + 1}: {deleteResponse.deleteResult.Errors[errorNum].Message}");
				}
				_logger.LogError("Entity: " + JsonConvert.SerializeObject(entity));
				throw new AutoTaskApiException($"Errors occurred during deletion of the AutoTask entity: {string.Join(";", deleteResponse.deleteResult.Errors.Select(e => e.Message))}");
			}

			var deletedEntity = deleteResponse?.deleteResult?.EntityResults?.FirstOrDefault();
			_logger.LogDebug($"Successfully deleted entity with Id: {deletedEntity?.id.ToString() ?? "UNKNOWN!"}");
			if (deletedEntity == null)
			{
				throw new AutoTaskApiException("Did not get a result back after deleting the AutoTask entity.");
			}
			return deletedEntity;
		}

		public async Task<Entity> UpdateAsync(Entity entity)
		{
			var updateRequest = new updateRequest(_autotaskIntegrations, new[] { entity });
			var updateResponse = await _autoTaskClient.updateAsync(updateRequest).ConfigureAwait(false);
			var errorCount = updateResponse.updateResult.Errors.Length;
			if (errorCount > 0)
			{
				_logger.LogError($"There was an error updating the entity. {errorCount} errors occurred.");
				for (var errorNum = 0; errorNum < errorCount; errorNum++)
				{
					_logger.LogError($"Error {errorNum + 1}: {updateResponse.updateResult.Errors[errorNum].Message}");
				}
				_logger.LogError("Entity: " + JsonConvert.SerializeObject(entity));
				throw new AutoTaskApiException($"Errors occurred during update of the AutoTask entity: {string.Join(";", updateResponse.updateResult.Errors.Select(e => e.Message))}");
			}

			var updatedEntity = updateResponse?.updateResult?.EntityResults?.FirstOrDefault();
			_logger.LogDebug($"Successfully updated entity with Id: {updatedEntity?.id.ToString() ?? "UNKNOWN!"}");
			if (updatedEntity == null)
			{
				throw new AutoTaskApiException("Did not get a result back after updating the AutoTask entity.");
			}
			return updatedEntity;
		}

		public async Task<string> GetWsdlVersion() => (await _autoTaskClient.GetWsdlVersionAsync(new GetWsdlVersionRequest(_autotaskIntegrations)).ConfigureAwait(false)).GetWsdlVersionResult;
	}
}
