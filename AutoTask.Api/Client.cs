using AutoTask.Api.Exceptions;
using AutoTask.Api.Extensions;
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
	public class Client : IDisposable
	{
		private readonly ATWSSoapClient _autoTaskClient;

		internal AutoTaskLogger AutoTaskLogger { get; }

		private readonly AutotaskIntegrations _autotaskIntegrations;
		private readonly ILogger _logger;
		private bool disposed; // To detect redundant calls

		/// <summary>
		///	This API call executes a query against Autotask and returns an array of matching entities.
		///	The queries are built using the QueryXML format and will return a maximum of 500 records at once,
		///	sorted by their id value.
		///	To query for additional records over the 500 maximum for a given set of search criteria, repeat
		///	the query and filter by id value > the previous maximum id value retrieved.
		/// </summary>
		private const int AutoTaskPageSize = 500;

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

			AutoTaskLogger = new AutoTaskLogger(_logger);
			_autoTaskClient.Endpoint.EndpointBehaviors.Add(AutoTaskLogger);

			_autoTaskClient.ClientCredentials.UserName.UserName = username;
			_autoTaskClient.ClientCredentials.UserName.Password = password;

			// have no clue what this does.
			_autotaskIntegrations = new AutotaskIntegrations();
		}

		public async Task<GetFieldInfoResponse> GetFieldInfoAsync(string psObjectType)
			=> await _autoTaskClient.GetFieldInfoAsync(new GetFieldInfoRequest(_autotaskIntegrations, psObjectType)).ConfigureAwait(false);

		/// <summary>
		/// Use GetAllAsync if you want to auto-page more than 500 results
		/// </summary>
		/// <param name="sXml"></param>
		/// <returns></returns>
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
				var message = atwsResponse.queryResult.Errors.Select(e => e.Message).ToHumanReadableString(delimitLastWith: " and ");

				throw new AutoTaskApiException(BuildExceptionMessage(message));
			}
			// Executed fine

			return atwsResponse.queryResult.EntityResults;
		}

		public async Task<IEnumerable<Entity>> GetAllAsync(string sXml)
		{
			List<Entity> list = new List<Entity>();

			var amendedSxml = sXml;
			queryResponse atwsResponse;
			do
			{
				atwsResponse = await _autoTaskClient.queryAsync(new queryRequest(_autotaskIntegrations, amendedSxml)).ConfigureAwait(false);
				if (atwsResponse.queryResult.ReturnCode != 1)
				{
					var message = atwsResponse.queryResult.Errors.Select(e => e.Message).ToHumanReadableString(delimitLastWith: " and ");

					throw new AutoTaskApiException(BuildExceptionMessage(message));
				}
				// Executed fine

				list.AddRange(atwsResponse.queryResult.EntityResults);

				// We MAY have more data
				// Determine the max id from the last page
				var lastId = atwsResponse.queryResult.EntityResults.Last().id;
				// Amend the sXml
				amendedSxml = sXml.Replace("</query>", $"<condition operator=\"and\"><field>id<expression op=\"GreaterThan\">{lastId}</expression></field></condition></query>");
			} while (atwsResponse.queryResult.EntityResults.Length == AutoTaskPageSize);

			return list;
		}

		private string BuildExceptionMessage(string message)
			=> $"Message: {message}\r\nLastAutoTaskRequest: {AutoTaskLogger.LastRequest ?? "No Request"}\r\nLastAutoTaskResponse: {AutoTaskLogger.LastResponse ?? "No Response"}";

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
				throw new AutoTaskApiException(BuildExceptionMessage($"Errors occurred during deletion of the AutoTask entity: {string.Join(";", deleteResponse.deleteResult.Errors.Select(e => e.Message))}"));
			}

			if (deleteResponse.deleteResult.Errors.Length != 0)
			{
				throw new AutoTaskApiException(BuildExceptionMessage($"Received the following error(s) while deleting: {string.Join("; ", deleteResponse.deleteResult.Errors.Select(e => e.Message))}."));
			}
			_logger.LogDebug($"Successfully deleted entity with Id: {entity?.id.ToString() ?? "UNKNOWN!"}");
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
				throw new AutoTaskApiException(BuildExceptionMessage($"Errors occurred during update of the AutoTask entity: {string.Join(";", updateResponse.updateResult.Errors.Select(e => e.Message))}"));
			}

			var updatedEntity = updateResponse?.updateResult?.EntityResults?.FirstOrDefault();
			_logger.LogDebug($"Successfully updated entity with Id: {updatedEntity?.id.ToString() ?? "UNKNOWN!"}");
			if (updatedEntity == null)
			{
				throw new AutoTaskApiException(BuildExceptionMessage("Did not get a result back after updating the AutoTask entity."));
			}
			return updatedEntity;
		}

		public async Task<string> GetWsdlVersion() => (await _autoTaskClient.GetWsdlVersionAsync(new GetWsdlVersionRequest(_autotaskIntegrations)).ConfigureAwait(false)).GetWsdlVersionResult;

		#region IDisposable Support

		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					try
					{
						_autoTaskClient.Close();
					}
					catch (CommunicationException e)
					{
						_autoTaskClient.Abort();
					}
					catch (TimeoutException e)
					{
						_autoTaskClient.Abort();
					}
					catch (Exception e)
					{
						_autoTaskClient.Abort();
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
