using AutoTask.Api.Exceptions;
using AutoTask.Api.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;

namespace AutoTask.Api
{
	public class Client : IDisposable, IClient
	{
		/// <summary>
		///	This API call executes a query against Autotask and returns an array of matching entities.
		///	The queries are built using the QueryXML format and will return a maximum of 500 records at once,
		///	sorted by their id value.
		///	To query for additional records over the 500 maximum for a given set of search criteria, repeat
		///	the query and filter by id value > the previous maximum id value retrieved.
		/// </summary>
		private const int AutoTaskPageSize = 500;

		private ATWSSoapClient? _autoTaskClient;
		private bool disposed; // To detect redundant calls

		internal AutoTaskLogger AutoTaskLogger { get; }

		private readonly AutotaskIntegrations _autotaskIntegrations;
		private readonly ClientOptions _clientOptions;
		private readonly ILogger _logger;
		private readonly string _username;
		private readonly string _password;

		public Client(
			string username,
			string password,
			string integrationCode,
			ILogger? logger = default,
			ClientOptions? clientOptions = default)
		{
			_logger = logger ?? new NullLogger<Client>();
			AutoTaskLogger = new AutoTaskLogger(_logger);
			_autotaskIntegrations = new AutotaskIntegrations { IntegrationCode = integrationCode };
			_clientOptions = clientOptions ?? new ClientOptions();
			_username = username;
			_password = password;
		}

		private async Task<ATWSSoapClient> GetATWSSoapClientAsync(CancellationToken cancellationToken)
		{
			if (_autoTaskClient != null)
			{
				return _autoTaskClient;
			}

			var binding = new BasicHttpBinding
			{
				SendTimeout = new TimeSpan(0, 0, 0, 0, _clientOptions.SendTimeoutMs),
				OpenTimeout = new TimeSpan(0, 0, 0, 0, _clientOptions.OpenTimeoutMs),
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

			string endpointAddressUrl;
			if (_clientOptions.ServerId is null)
			{
				var endpoint = new EndpointAddress("https://webservices.autotask.net/ATServices/1.6/atws.asmx");
				using var zoneInfoAutoTaskClient = new ATWSSoapClient(binding, endpoint);

				var zoneInfo = await zoneInfoAutoTaskClient
					.getZoneInfoAsync(new getZoneInfoRequest(_username))
					.WithCancellation(cancellationToken)
					.ConfigureAwait(false);
				zoneInfoAutoTaskClient.Close();
				endpointAddressUrl = zoneInfo.getZoneInfoResult.URL;
			}
			else
			{
				endpointAddressUrl = $"https://webservices{_clientOptions.ServerId}.autotask.net/ATServices/1.6/atws.asmx";
			}

			// Create the binding.
			// must use BasicHttpBinding instead of WSHttpBinding
			// otherwise a "SOAP header Action was not understood." is thrown.
			var myBinding = new BasicHttpBinding
			{
				Security =
				{
					Mode = BasicHttpSecurityMode.Transport,
					Transport = { ClientCredentialType = HttpClientCredentialType.Basic }
				},
				MaxReceivedMessageSize = 2147483647
			};

			// Must set the size otherwise
			//The maximum message size quota for incoming messages (65536) has been exceeded. To increase the quota, use the MaxReceivedMessageSize property on the appropriate binding element.

			// Create the endpoint address.
			var ea = new EndpointAddress(endpointAddressUrl);

			var autoTaskClient = new ATWSSoapClient(myBinding, ea);
			autoTaskClient.Endpoint.EndpointBehaviors.Add(AutoTaskLogger);
			autoTaskClient.ClientCredentials.UserName.UserName = _username;
			autoTaskClient.ClientCredentials.UserName.Password = _password;
			return _autoTaskClient = autoTaskClient;
		}

		public async Task<GetFieldInfoResponse> GetFieldInfoAsync(string psObjectType)
			=> await GetFieldInfoAsync(psObjectType, CancellationToken.None);

		public async Task<GetFieldInfoResponse> GetFieldInfoAsync(string psObjectType, CancellationToken cancellationToken)
			=> await (await GetATWSSoapClientAsync(cancellationToken).ConfigureAwait(false))
				.GetFieldInfoAsync(new GetFieldInfoRequest(_autotaskIntegrations, psObjectType))
				.WithCancellation(cancellationToken)
				.ConfigureAwait(false);

		/// <summary>
		/// Use GetAllAsync if you want to auto-page more than 500 results
		/// </summary>
		/// <param name="sXml"></param>
		/// <returns></returns>

		public async Task<IEnumerable<Entity>> QueryAsync(string sXml)
			=> await QueryAsync(sXml, CancellationToken.None);

		public async Task<IEnumerable<Entity>> QueryAsync(string sXml, CancellationToken cancellationToken)
		{
			// this example will not handle the 500 results limitation.
			// AutoTask only returns up to 500 results in a response. if there are more you must query again for the next 500.
			// See https://www.powershellgallery.com/packages/Autotask/0.2.1.4/Content/Public%5CGet-AtwsData.ps1 for one approach:
			// - Get the highest id in a resultset and keep asking for items when the ID > the last observed value
			// This post: https://github.com/opendns/autotask-php/issues/9 hints that you CAN rely on using the ID as a paging method
			// as it is not possible to set the order by in the query and that the results always come back in id asc order.
			var atwsResponse = await (await GetATWSSoapClientAsync(cancellationToken).ConfigureAwait(false))
				.queryAsync(new queryRequest(_autotaskIntegrations, sXml))
				.WithCancellation(cancellationToken)
				.ConfigureAwait(false);
			if (atwsResponse.queryResult.ReturnCode != 1)
			{
				var message = atwsResponse.queryResult.Errors.Select(e => e.Message).ToHumanReadableString(delimitLastWith: " and ");

				throw new AutoTaskApiException(BuildExceptionMessage(message));
			}
			// Executed fine

			return atwsResponse.queryResult.EntityResults;
		}

		public async Task<IEnumerable<Entity>> GetAllAsync(string sXml)
			=> await GetAllAsync(sXml, CancellationToken.None);

		public async Task<IEnumerable<Entity>> GetAllAsync(string sXml, CancellationToken cancellationToken)
		{
			var list = new List<Entity>();

			var amendedSxml = sXml;
			queryResponse atwsResponse;
			do
			{
				atwsResponse = await (await GetATWSSoapClientAsync(cancellationToken).ConfigureAwait(false))
					.queryAsync(new queryRequest(_autotaskIntegrations, amendedSxml))
					.WithCancellation(cancellationToken)
					.ConfigureAwait(false);
				if (atwsResponse.queryResult.ReturnCode != 1)
				{
					var message = atwsResponse.queryResult.Errors.Select(e => e.Message).ToHumanReadableString(delimitLastWith: " and ");

					throw new AutoTaskApiException(BuildExceptionMessage(message));
				}
				// Executed fine

				list.AddRange(atwsResponse.queryResult.EntityResults);

				// We MAY have more data
				// Determine the max id from the last page
				var last = atwsResponse.queryResult.EntityResults.LastOrDefault();
				if (last == null)
				{
					break;
				}
				var lastId = last.id;
				// Amend the sXml
				amendedSxml = sXml.Replace("</query>", $"<condition operator=\"and\"><field>id<expression op=\"GreaterThan\">{lastId}</expression></field></condition></query>");
			} while (atwsResponse.queryResult.EntityResults.Length == AutoTaskPageSize);

			return list;
		}

		private string BuildExceptionMessage(string message)
			=> $"Message: {message}\r\nLastAutoTaskRequest: {AutoTaskLogger.LastRequest ?? "No Request"}\r\nLastAutoTaskResponse: {AutoTaskLogger.LastResponse ?? "No Response"}";

		public async Task<Entity> CreateAsync(Entity entity)
			=> await CreateAsync(entity, CancellationToken.None);

		public async Task<Entity> CreateAsync(Entity entity, CancellationToken cancellationToken)
		{
			var createRequest = new createRequest(_autotaskIntegrations, new[] { entity });
			var createResponse = await (await GetATWSSoapClientAsync(cancellationToken).ConfigureAwait(false))
				.createAsync(createRequest)
				.WithCancellation(cancellationToken)
				.ConfigureAwait(false);
			var errorCount = createResponse.createResult.Errors.Length;
			if (errorCount > 0)
			{
				_logger.LogError($"There was an error creating the entity. {errorCount} errors occurred.");
				for (var errorNum = 0; errorNum < errorCount; errorNum++)
				{
					_logger.LogError($"Error {errorNum + 1}: {createResponse.createResult.Errors[errorNum].Message}");
				}
				_logger.LogError("Entity: " + JsonConvert.SerializeObject(entity));
				var message = $"Errors occurred during creation of the AutoTask entity: {string.Join(";", createResponse.createResult.Errors.Select(e => e.Message))}";
				throw new AutoTaskApiException(BuildExceptionMessage(message));
			}

			var createdEntity = createResponse?.createResult?.EntityResults?.FirstOrDefault();
			_logger.LogDebug($"Successfully created entity with Id: {createdEntity?.id.ToString() ?? "UNKNOWN!"}");
			if (createdEntity == null)
			{
				throw new AutoTaskApiException(BuildExceptionMessage("Did not get a result back after creating the AutoTask entity."));
			}
			return createdEntity;
		}

		public async System.Threading.Tasks.Task DeleteAsync(Entity entity)
			=> await DeleteAsync(entity, CancellationToken.None);

		public async System.Threading.Tasks.Task DeleteAsync(Entity entity, CancellationToken cancellationToken)
		{
			var deleteRequest = new deleteRequest(_autotaskIntegrations, new[] { entity });
			var deleteResponse = await (await GetATWSSoapClientAsync(cancellationToken).ConfigureAwait(false))
				.deleteAsync(deleteRequest)
				.WithCancellation(cancellationToken)
				.ConfigureAwait(false);
			var errorCount = deleteResponse.deleteResult.Errors.Length;
			if (errorCount > 0)
			{
				_logger.LogError($"There was an error deleting the entity. {errorCount} errors occurred.");
				for (var errorNum = 0; errorNum < errorCount; errorNum++)
				{
					_logger.LogError($"Error {errorNum + 1}: {deleteResponse.deleteResult.Errors[errorNum].Message}");
				}
				_logger.LogError("Entity: " + JsonConvert.SerializeObject(entity));
				throw new AutoTaskApiException(BuildExceptionMessage($"Errors occurred during deletion of the AutoTask entity: {string.Join(";", deleteResponse.deleteResult.Errors.Select(e => e.Message))}"));
			}

			if (deleteResponse.deleteResult.Errors.Length != 0)
			{
				throw new AutoTaskApiException(BuildExceptionMessage($"Received the following error(s) while deleting: {string.Join("; ", deleteResponse.deleteResult.Errors.Select(e => e.Message))}."));
			}
			_logger.LogDebug($"Successfully deleted entity with Id: {entity?.id.ToString() ?? "UNKNOWN!"}");
		}

		public async Task<Entity> UpdateAsync(Entity entity)
			=> await UpdateAsync(entity, CancellationToken.None);

		public async Task<Entity> UpdateAsync(Entity entity, CancellationToken cancellationToken)
		=> (await UpdateAsync(new[] { entity }).ConfigureAwait(false)).Single();

		public async Task<Entity[]> UpdateAsync(Entity[] entityArray)
			=> await UpdateAsync(entityArray, CancellationToken.None);

		public async Task<Entity[]> UpdateAsync(Entity[] entityArray, CancellationToken cancellationToken)
		{
			var updateRequest = new updateRequest(_autotaskIntegrations, entityArray);
			var updateResponse = await (await GetATWSSoapClientAsync(cancellationToken).ConfigureAwait(false))
				.updateAsync(updateRequest)
				.WithCancellation(cancellationToken)
				.ConfigureAwait(false);
			var errorCount = updateResponse.updateResult.Errors.Length;
			if (errorCount > 0)
			{
				_logger.LogError($"There was an error updating the entity. {errorCount} errors occurred.");
				for (var errorNum = 0; errorNum < errorCount; errorNum++)
				{
					_logger.LogError($"Error {errorNum + 1}: {updateResponse.updateResult.Errors[errorNum].Message}");
				}
				_logger.LogError("Entity: " + JsonConvert.SerializeObject(entityArray));
				throw new AutoTaskApiException(BuildExceptionMessage($"Errors occurred during update of the AutoTask entity: {string.Join(";", updateResponse.updateResult.Errors.Select(e => e.Message))}"));
			}

			var updatedEntities = updateResponse?.updateResult?.EntityResults;
			if (updatedEntities is null)
			{
				throw new AutoTaskApiException(BuildExceptionMessage("Did not get a result back after updating the AutoTask entities."));
			}
			if (entityArray.Length != updatedEntities.Length)
			{
				throw new AutoTaskApiException(BuildExceptionMessage($"Did not receive the expected update entity count (expected {entityArray.Length}, received {updatedEntities.Length})."));
			}
			_logger.LogDebug($"Updated {updatedEntities.Length} {(updatedEntities.Length == 1 ? "entity" : "entities")}: ({string.Join(", ", updatedEntities.Select(e => e.id.ToString() ?? "?"))})");
			return updatedEntities;
		}

		public async Task<string> GetWdslVersion()
			=> await GetWsdlVersion(CancellationToken.None);

		public async Task<string> GetWsdlVersion(CancellationToken cancellationToken)
		{
			var getWsdlVersionResponse = await (await GetATWSSoapClientAsync(cancellationToken).ConfigureAwait(false))
				.GetWsdlVersionAsync(new GetWsdlVersionRequest(_autotaskIntegrations))
				.WithCancellation(cancellationToken)
				.ConfigureAwait(false);
			return getWsdlVersionResponse.GetWsdlVersionResult;
		}

		#region IDisposable Support

		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					try
					{
						_autoTaskClient?.Close();
					}
					catch (CommunicationException)
					{
						_autoTaskClient?.Abort();
					}
					catch (TimeoutException)
					{
						_autoTaskClient?.Abort();
					}
					catch
					{
						_autoTaskClient?.Abort();
						throw;
					}
				}

				disposed = true;
			}
		}

		public void Dispose() => Dispose(true);
		#endregion
	}
}
