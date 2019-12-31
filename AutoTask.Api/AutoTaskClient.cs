using AutoTask.Api.Config;
using AutoTask.Api.Filters;
using LogicMonitor.Integrator.Alerts.Exceptions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;

namespace AutoTask.Api
{
	/// <summary>
	/// An AutoTask client
	/// </summary>
	public class AutoTaskClient : IDisposable
	{
		private const string UDFPrefix = "UDF ";
		private readonly AutoTaskConfiguration _configuration;
		private ATWSSoapClient _clientDoNotUseDirectly;
		private AutotaskIntegrations _autotaskIntegrations;

		public string PrimaryKey => "id";
		public string NumberField => "TicketNumber";
		public string DescriptionKey => "Description";
		public string TicketNoteTitleKey => "TicketNoteTitle";

		public AutoTaskClient(AutoTaskConfiguration config)
			=> _configuration = config;

		private async Task<ATWSSoap> GetClientAsync()
		{
			if (_clientDoNotUseDirectly != null)
			{
				return _clientDoNotUseDirectly;
			}

			var client = new ATWSSoapClient();
			var zoneInfo = await client.getZoneInfoAsync(new getZoneInfoRequest(_configuration.Username)).ConfigureAwait(false);

			// Create the binding.
			// must use BasicHttpBinding instead of WSHttpBinding
			// otherwise a "SOAP header Action was not understood." is thrown.
			var myBinding = new BasicHttpBinding();
			myBinding.Security.Mode = BasicHttpSecurityMode.Transport;
			myBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Basic;

			// Must set the size otherwise
			//The maximum message size quota for incoming messages (65536) has been exceeded. To increase the quota, use the MaxReceivedMessageSize property on the appropriate binding element.
			myBinding.MaxReceivedMessageSize = 2147483647;

			// Create the endpoint address.
			var ea = new EndpointAddress(zoneInfo.getZoneInfoResult.URL);
			client.Close();

			client = new ATWSSoapClient(myBinding, ea);
			client.ClientCredentials.UserName.UserName = _configuration.Username;
			client.ClientCredentials.UserName.Password = _configuration.Password;

			//Autotask is implementing mandatory tracking identifiers for 
			//Integration developers selling or offering integrations into the Autotask channel.

			_autotaskIntegrations = new AutotaskIntegrations { IntegrationCode = _configuration.IntegrationCode };
			return _clientDoNotUseDirectly = client;
		}

		private async Task<queryResponse> QueryAsync(string sXml)
		{
			var client = await GetClientAsync().ConfigureAwait(false);
			return await client.queryAsync(new queryRequest(_autotaskIntegrations, sXml)).ConfigureAwait(false);
		}

		public async Task<List<JObject>> GetAccountsAsync(Filter filter)
			=> (await GetAsync<Account>(filter).ConfigureAwait(false))
				.Select(account => GetFilteredObject(account, filter))
				.ToList();

		public async Task<List<JObject>> GetIssuesAsync(Filter filter)
			=> (await GetAsync<Ticket>(filter).ConfigureAwait(false))
				.Select(account => GetFilteredObject(account, filter))
			.ToList();

		private JObject GetFilteredObject(object originalObject, Filter filter)
		{
			var fields = filter?.Fields;
			if (fields == null || fields.Count == 0)
			{
				return JObject.FromObject(originalObject);
			}

			// Based on the optional filter fields - build up the resulting JObject to only contain the requested fields

			var result = new JObject();
			foreach (var field in fields)
			{
				// Find any property matching the field
				var objectProperty = originalObject.GetType().GetProperties().SingleOrDefault(property => string.Equals(property.Name, field, StringComparison.OrdinalIgnoreCase));
				if (objectProperty == null)
				{
					throw new FilterFieldNotPresentException($"The filter field {field} is not present on the source object");
				}

				var v = objectProperty.GetValue(originalObject);
				result[objectProperty.Name] = v == null ? null : JToken.FromObject(v);
			}
			return result;
		}

		public async Task<List<T>> GetAsync<T>(Filter filter)
		{
			var query = GetQueryString(filter);
			var sXml = $"<queryxml><entity>{typeof(T).Name}</entity><query>{query}</query></queryxml>";
			var queryResponse = await QueryAsync(sXml).ConfigureAwait(false);
			return queryResponse.queryResult.EntityResults.Cast<T>().ToList();
		}

		private string GetQueryString(Filter filter)
			=> filter == null || filter.Items.Count == 0
			? $"<field>id<expression op=\"{GetOperatorString(Operator.GreaterThan)}\">0</expression></field>"
			: $"<condition operator=\"and\">{string.Concat(filter.Items.Select(fi => $"<field{(fi.Field.StartsWith(UDFPrefix) ? " udf=\"true\"" : string.Empty)}>{(fi.Field.StartsWith(UDFPrefix) ? fi.Field.Substring(UDFPrefix.Length) : fi.Field)}<expression op=\"{GetOperatorString(fi.Operator)}\">{fi.Value}</expression></field>"))}</condition>";

		private static object GetOperatorString(Operator @operator)
		{
			// See page 315 of https://ww4.autotask.net/help/Content/LinkedDOCUMENTS/WSAPI/T_WebServicesAPIv1_5.pdf
			switch (@operator)
			{
				case Operator.BeginsWith:
				case Operator.EndsWith:
				case Operator.Like:
				case Operator.NotLike:
				case Operator.GreaterThanOrEquals:
				case Operator.LessThanOrEquals:
				case Operator.GreaterThan:
				case Operator.LessThan:
				case Operator.Equals:
					return @operator.ToString().ToLowerInvariant();
				case Operator.NotEquals:
					return "notequal";
				default:
					throw new NotSupportedException($"{@operator} not supported.");
			}
		}

		public async Task<List<JObject>> GetIssueNotesAsync(Filter filter)
			=> (await GetAsync<TicketNote>(filter).ConfigureAwait(false))
				.Select(account => GetFilteredObject(account, filter))
				.ToList();

		public string CiLookup(string lookupValue)
		{
			// Get the ticket
			var ci = GetAsync<InstalledProduct>(new Filter { Items = new List<FilterItem> { new FilterItem { Field = "referenceTitle", Operator = Operator.Equals, Value = lookupValue } } })
				.GetAwaiter()
				.GetResult()
				.SingleOrDefault()
				;

			return $"{ci?.id}";
		}

		#region IDisposable Support
		private bool disposedValue = false; // To detect redundant calls

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					// Not an actual dispose, but happens when we get disposed to ensure closing the client nicely
					_clientDoNotUseDirectly?.Close();
				}
				disposedValue = true;
			}
		}

		// This code added to correctly implement the disposable pattern.
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
