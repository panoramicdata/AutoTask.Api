using AutoTask.Api.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using System.ServiceModel.Channels;
using Xunit;

namespace AutoTask.Api.Test.Unit;

/// <summary>Unit tests for <see cref="AutoTaskErrorReporter"/>.</summary>
public class AutoTaskErrorReporterTests
{
	private readonly FakeLogger _fakeLogger = new();
	private readonly AutoTaskLogger _autoTaskLogger;
	private readonly AutoTaskErrorReporter _reporter;

	/// <summary>Creates a reporter writing to a fake logger.</summary>
	public AutoTaskErrorReporterTests()
	{
		_autoTaskLogger = new AutoTaskLogger(_fakeLogger);
		_reporter = new AutoTaskErrorReporter(_fakeLogger, _autoTaskLogger);
	}

	/// <summary>Verifies the placeholders are used when no exchange has been captured.</summary>
	[Fact]
	public void Describe_WithNoExchange_UsesPlaceholders()
	{
		var description = _reporter.Describe("boom");

		Assert.Contains("Message: boom", description);
		Assert.Contains("No Request", description);
		Assert.Contains("No Response", description);
	}

	/// <summary>Verifies the last captured request is included.</summary>
	[Fact]
	public void Describe_AfterRequest_IncludesRequest()
	{
		var request = Message.CreateMessage(MessageVersion.Soap11, "urn:captured");
		_autoTaskLogger.BeforeSendRequest(ref request, null!);

		Assert.Contains("urn:captured", _reporter.Describe("boom"));
	}

	/// <summary>Verifies nothing is thrown or logged when there are no errors.</summary>
	[Fact]
	public void ThrowOnErrors_WithNoErrors_DoesNothing()
	{
		_reporter.ThrowOnErrors([], "creating", "creation", null);

		Assert.Empty(_fakeLogger.Collector.GetSnapshot());
	}

	/// <summary>Verifies errors are logged and thrown with the supplied wording.</summary>
	[Fact]
	public void ThrowOnErrors_WithErrors_LogsAndThrows()
	{
		var errors = new[] { new ATWSError { Message = "bad one" }, new ATWSError { Message = "bad two" } };

		var exception = Assert.Throws<AutoTaskApiException>(
			() => _reporter.ThrowOnErrors(errors, "updating", "update", new Account { id = 9 }));

		Assert.Contains("Errors occurred during update of the AutoTask entity: bad one;bad two", exception.Message);
		var records = _fakeLogger.Collector.GetSnapshot();
		var messages = records.Select(record => record.Message).ToList();
		Assert.Contains(messages, message => message.Contains("error updating the entity. 2 errors"));
		Assert.Contains("Error 2: bad two", messages);
		Assert.Contains(messages, message => message.StartsWith("Entity: ", StringComparison.Ordinal) && message.Contains("\"id\":9"));
		Assert.All(records, record => Assert.Equal(LogLevel.Error, record.Level));
	}
}
