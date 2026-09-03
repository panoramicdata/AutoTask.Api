using Microsoft.Extensions.Logging;
using System.Collections.Generic;
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
		var logger = new RecordingLogger();
		var autoTaskLogger = new AutoTaskLogger(logger);
		var request = CreateMessage("urn:request");

		var correlationState = autoTaskLogger.BeforeSendRequest(ref request, null!);

		Assert.Null(correlationState);
		var entry = Assert.Single(logger.Entries);
		Assert.Equal(LogLevel.Debug, entry.LogLevel);
		Assert.Contains("AutoTask Request:", entry.Message);
		Assert.Contains("urn:request", entry.Message);
	}

	/// <summary>Verifies that an incoming reply is logged at trace level.</summary>
	[Fact]
	public void AfterReceiveReply_LogsTheResponse()
	{
		var logger = new RecordingLogger();
		var autoTaskLogger = new AutoTaskLogger(logger);
		var reply = CreateMessage("urn:reply");

		autoTaskLogger.AfterReceiveReply(ref reply, null!);

		var entry = Assert.Single(logger.Entries);
		Assert.Equal(LogLevel.Trace, entry.LogLevel);
		Assert.Contains("AutoTask Response:", entry.Message);
		Assert.Contains("urn:reply", entry.Message);
	}

	/// <summary>Verifies that nothing is logged when the logger has the relevant level disabled.</summary>
	[Fact]
	public void Inspect_WhenLevelDisabled_LogsNothing()
	{
		var logger = new RecordingLogger { MinimumLevel = LogLevel.Information };
		var autoTaskLogger = new AutoTaskLogger(logger);
		var request = CreateMessage("urn:request");
		var reply = CreateMessage("urn:reply");

		autoTaskLogger.BeforeSendRequest(ref request, null!);
		autoTaskLogger.AfterReceiveReply(ref reply, null!);

		Assert.Empty(logger.Entries);
	}

	/// <summary>Verifies that the members the behaviour does not use complete without throwing.</summary>
	[Fact]
	public void NoOpBehaviorMembers_DoNothing()
	{
		var autoTaskLogger = new AutoTaskLogger(new RecordingLogger());
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

	private sealed class RecordingLogger : ILogger
	{
		public List<(LogLevel LogLevel, string Message)> Entries { get; } = [];

		public LogLevel MinimumLevel { get; init; } = LogLevel.Trace;

		public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

		public bool IsEnabled(LogLevel logLevel) => logLevel >= MinimumLevel;

		public void Log<TState>(
			LogLevel logLevel,
			EventId eventId,
			TState state,
			Exception? exception,
			Func<TState, Exception?, string> formatter)
			=> Entries.Add((logLevel, formatter(state, exception)));
	}
}
