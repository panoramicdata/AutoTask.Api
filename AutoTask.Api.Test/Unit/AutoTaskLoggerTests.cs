using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using Xunit;

namespace AutoTask.Api.Test.Unit;

/// <summary>Unit tests for <see cref="AutoTaskLogger"/>. No AutoTask credentials required.</summary>
public class AutoTaskLoggerTests
{
	/// <summary>Verifies that a null logger is rejected.</summary>
	[Fact]
	public void Construct_WithNullLogger_Throws()
		=> Assert.Throws<ArgumentNullException>(() => new AutoTaskLogger(null!));

	/// <summary>Verifies that an outgoing request is logged at debug level.</summary>
	[Fact]
	public void BeforeSendRequest_LogsTheRequest()
	{
		var fakeLogger = new FakeLogger();
		var autoTaskLogger = new AutoTaskLogger(fakeLogger);
		var request = CreateMessage("urn:request");

		var correlationState = autoTaskLogger.BeforeSendRequest(ref request, null!);

		Assert.Null(correlationState);
		var record = Assert.Single(fakeLogger.Collector.GetSnapshot());
		Assert.Equal(LogLevel.Debug, record.Level);
		Assert.Contains("AutoTask Request:", record.Message);
		Assert.Contains("urn:request", record.Message);
	}

	/// <summary>Verifies that an incoming reply is logged at trace level.</summary>
	[Fact]
	public void AfterReceiveReply_LogsTheResponse()
	{
		var fakeLogger = new FakeLogger();
		var autoTaskLogger = new AutoTaskLogger(fakeLogger);
		var reply = CreateMessage("urn:reply");

		autoTaskLogger.AfterReceiveReply(ref reply, null!);

		var record = Assert.Single(fakeLogger.Collector.GetSnapshot());
		Assert.Equal(LogLevel.Trace, record.Level);
		Assert.Contains("AutoTask Response:", record.Message);
		Assert.Contains("urn:reply", record.Message);
	}

	/// <summary>Verifies that nothing is logged when the logger has the relevant level disabled.</summary>
	[Fact]
	public void Inspect_WhenLevelDisabled_LogsNothing()
	{
		var fakeLogger = new FakeLogger();
		fakeLogger.ControlLevel(LogLevel.Debug, false);
		fakeLogger.ControlLevel(LogLevel.Trace, false);
		var autoTaskLogger = new AutoTaskLogger(fakeLogger);
		var request = CreateMessage("urn:request");
		var reply = CreateMessage("urn:reply");

		autoTaskLogger.BeforeSendRequest(ref request, null!);
		autoTaskLogger.AfterReceiveReply(ref reply, null!);

		Assert.Empty(fakeLogger.Collector.GetSnapshot());
	}

	/// <summary>Verifies that the members the behaviour does not use complete without throwing.</summary>
	[Fact]
	public void NoOpBehaviorMembers_DoNothing()
	{
		var autoTaskLogger = new AutoTaskLogger(new FakeLogger());
		var endpoint = CreateEndpoint();
		var bindingParameters = new BindingParameterCollection();

		autoTaskLogger.AddBindingParameters(endpoint, bindingParameters);
		autoTaskLogger.ApplyDispatchBehavior(endpoint, null!);
		autoTaskLogger.Validate(endpoint);

		Assert.Empty(bindingParameters);
	}

	private static ServiceEndpoint CreateEndpoint()
		=> new(
			ContractDescription.GetContract(typeof(ITestContract)),
			new BasicHttpBinding(),
			new EndpointAddress("http://localhost/test"));

	private static Message CreateMessage(string action)
		=> Message.CreateMessage(MessageVersion.Soap11, action);

	[ServiceContract]
	private interface ITestContract
	{
		[OperationContract]
		void Operation();
	}
}
