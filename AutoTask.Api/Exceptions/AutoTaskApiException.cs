using System;
using System.Linq;
using System.Runtime.Serialization;

namespace AutoTask.Api.Exceptions;

/// <summary>Thrown when the AutoTask API returns an error response.</summary>
[Serializable]
public class AutoTaskApiException : Exception
{
	/// <summary>Gets the AutoTask response that caused this exception, if available.</summary>
	public ATWSResponse? Response { get; }

	/// <summary>Initializes a new instance from an <see cref="ATWSResponse"/> error response.</summary>
	public AutoTaskApiException(ATWSResponse queryResult) : base(string.Join(", ", queryResult.Errors.Select(e => e.Message)))
	{
		Response = queryResult;
	}

	/// <summary>Initializes a new instance with the specified message.</summary>
	public AutoTaskApiException(string message) : base(message)
	{
		Response = null;
	}

	/// <summary>Initializes a new instance with the specified message and inner exception.</summary>
	public AutoTaskApiException(string message, Exception innerException) : base(message, innerException)
	{
		Response = null;
	}

	/// <summary>Initializes a new instance of <see cref="AutoTaskApiException"/>.</summary>
	public AutoTaskApiException()
	{
	}
}
