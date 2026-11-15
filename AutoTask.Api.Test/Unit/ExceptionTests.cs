using AutoTask.Api.Config;
using AutoTask.Api.Exceptions;
using LogicMonitor.Integrator.Alerts.Exceptions;
using Xunit;

namespace AutoTask.Api.Test.Unit;

/// <summary>Unit tests for the exception types thrown by the library.</summary>
public class ExceptionTests
{
	/// <summary>Verifies each exception supports the standard constructors.</summary>
	[Theory]
	[InlineData(typeof(AutoTaskApiException))]
	[InlineData(typeof(ConfigurationException))]
	[InlineData(typeof(FilterFieldNotPresentException))]
	public void Constructors_CarryMessageAndInnerException(Type exceptionType)
	{
		var inner = new InvalidOperationException("inner");

		var withMessage = (Exception)Activator.CreateInstance(exceptionType, "message")!;
		var withInner = (Exception)Activator.CreateInstance(exceptionType, "message", inner)!;

		Assert.NotNull(Activator.CreateInstance(exceptionType));
		Assert.Equal("message", withMessage.Message);
		Assert.Null(withMessage.InnerException);
		Assert.Same(inner, withInner.InnerException);
	}

	/// <summary>Verifies the response constructor joins the error messages and keeps the response.</summary>
	[Fact]
	public void AutoTaskApiException_FromResponse_JoinsErrorMessages()
	{
		var response = new ATWSResponse
		{
			Errors = [new ATWSError { Message = "first" }, new ATWSError { Message = "second" }]
		};

		var exception = new AutoTaskApiException(response);

		Assert.Equal("first, second", exception.Message);
		Assert.Same(response, exception.Response);
		Assert.Null(new AutoTaskApiException("message").Response);
	}
}
