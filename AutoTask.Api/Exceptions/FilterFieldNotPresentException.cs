using System;

namespace LogicMonitor.Integrator.Alerts.Exceptions;

/// <summary>Thrown when a filter references a field that does not exist on the target object.</summary>
public class FilterFieldNotPresentException : Exception
{
	/// <summary>Initializes a new instance of <see cref="FilterFieldNotPresentException"/>.</summary>
	public FilterFieldNotPresentException()
	{
	}

	/// <summary>Initializes a new instance of <see cref="FilterFieldNotPresentException"/> with the specified message.</summary>
	public FilterFieldNotPresentException(string message) : base(message)
	{
	}

	/// <summary>Initializes a new instance of <see cref="FilterFieldNotPresentException"/> with the specified message and inner exception.</summary>
	public FilterFieldNotPresentException(string message, Exception innerException) : base(message, innerException)
	{
	}
}