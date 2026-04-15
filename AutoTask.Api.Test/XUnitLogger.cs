using Microsoft.Extensions.Logging;
using System;
using Xunit.Abstractions;

namespace AutoTask.Api.Test;

/// <summary>An <see cref="ILogger"/> implementation that writes to xUnit test output.</summary>
public class XunitLogger : ILogger, IDisposable
{
	private readonly ITestOutputHelper _output;

	/// <summary>Initializes a new instance of <see cref="XunitLogger"/> with the supplied xUnit output helper.</summary>
	public XunitLogger(ITestOutputHelper output)
	{
		_output = output;
	}

	/// <inheritdoc/>
	public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
		=> _output.WriteLine(state?.ToString() ?? throw new ArgumentNullException(nameof(state)));

	/// <inheritdoc/>
	public bool IsEnabled(LogLevel logLevel)
		=> true;

	/// <inheritdoc/>
	public IDisposable BeginScope<TState>(TState state) where TState : notnull
		=> this;

	/// <inheritdoc/>
	public void Dispose()
	{
	}
}
