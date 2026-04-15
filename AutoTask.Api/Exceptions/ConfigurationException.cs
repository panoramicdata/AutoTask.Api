using System;
using System.Runtime.Serialization;

namespace AutoTask.Api.Config;

/// <summary>Thrown when the AutoTask configuration is invalid or incomplete.</summary>
[Serializable]
public class ConfigurationException : Exception
{
	/// <summary>Initializes a new instance of <see cref="ConfigurationException"/>.</summary>
	public ConfigurationException()
	{
	}

	/// <summary>Initializes a new instance of <see cref="ConfigurationException"/> with the specified message.</summary>
	public ConfigurationException(string message) : base(message)
	{
	}

	/// <summary>Initializes a new instance of <see cref="ConfigurationException"/> with the specified message and inner exception.</summary>
	public ConfigurationException(string message, Exception innerException) : base(message, innerException)
	{
	}
}