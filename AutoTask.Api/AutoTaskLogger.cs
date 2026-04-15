using Microsoft.Extensions.Logging;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace AutoTask.Api;

// based on
// https://stackoverflow.com/questions/12842014/logging-soap-raw-response-received-by-a-clientbase-object

/// <summary>WCF endpoint behavior that logs AutoTask SOAP request and response messages.</summary>
public class AutoTaskLogger : IEndpointBehavior, IClientMessageInspector
{
	private readonly ILogger _logger;

	internal string? LastResponse { get; private set; }
	internal string? LastRequest { get; private set; }

	/// <summary>Initializes a new instance of <see cref="AutoTaskLogger"/> with the specified logger.</summary>
	public AutoTaskLogger(ILogger logger)
	{
		_logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
	}

	// IEndpointBehavior
	/// <summary>Not used; no binding parameters are added.</summary>
	public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
	{
		// No customization required for this scenario
	}

	/// <summary>Adds this instance as a message inspector on the client runtime.</summary>
	public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
		=> clientRuntime.ClientMessageInspectors.Add(this);

	/// <summary>Not used for client-side behavior.</summary>
	public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
	{
		// No customization required for this scenario
	}

	/// <summary>Not used; no validation is performed.</summary>
	public void Validate(ServiceEndpoint endpoint)
	{
		// No customization required for this scenario
	}

	// IClientMessageInspector
	/// <summary>Captures the raw response message after it is received.</summary>
	public void AfterReceiveReply(ref Message reply, object correlationState)
	{
		LastResponse = reply.ToString();
		_logger.LogTrace("AutoTask Response: " + LastResponse);
	}

	/// <summary>Captures the raw request message before it is sent and clears the last response.</summary>
	public object? BeforeSendRequest(ref Message request, IClientChannel channel)
	{
		LastRequest = request.ToString();
		// Clear the response so it's clear that any response set is the response to the request
		LastResponse = null;
		_logger.LogDebug("AutoTask Request: " + LastRequest);
		return null;
	}
}
