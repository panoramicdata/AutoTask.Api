using Microsoft.Extensions.Logging;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace AutoTask.Api
{
	// based on
	// https://stackoverflow.com/questions/12842014/logging-soap-raw-response-received-by-a-clientbase-object

	public class AutoTaskLogger : IEndpointBehavior, IClientMessageInspector
	{
		private readonly ILogger _logger;

		internal string? LastResponse { get; private set; }
		internal string? LastRequest { get; private set; }

		public AutoTaskLogger(ILogger logger)
		{
			_logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
		}

		// IEndpointBehavior
		public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters) { }

		public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
			=> clientRuntime.ClientMessageInspectors.Add(this);

		public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher) { }

		public void Validate(ServiceEndpoint endpoint) { }

		// IClientMessageInspector
		public void AfterReceiveReply(ref Message reply, object correlationState)
		{
			LastResponse = reply.ToString();
			_logger.LogTrace("AutoTask Response: " + LastResponse);
		}

		public object? BeforeSendRequest(ref Message request, IClientChannel channel)
		{
			LastRequest = request.ToString();
			// Clear the response so it's clear that any response set is the response to the request
			LastResponse = null;
			_logger.LogDebug("AutoTask Request: " + LastRequest);
			return null;
		}
	}
}
