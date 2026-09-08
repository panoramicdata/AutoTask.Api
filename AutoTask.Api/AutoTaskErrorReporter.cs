using AutoTask.Api.Exceptions;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Text.Json;

namespace AutoTask.Api;

/// <summary>
/// Turns the errors AutoTask reports into logged output and exceptions. Kept separate from
/// <see cref="Client"/> so that the API calls read as the sequence of requests they are.
/// </summary>
/// <remarks>Initializes a new reporter writing to the supplied loggers.</remarks>
internal sealed class AutoTaskErrorReporter(ILogger logger, AutoTaskLogger autoTaskLogger)
{
	private static readonly JsonSerializerOptions EntityLogJsonSerializerOptions = new()
	{
		// Entity graphs from the AutoTask WSDL are flat, but a reference loop must never turn
		// an error being logged into a second, more confusing exception.
		ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
	};

	/// <summary>
	/// Appends the last SOAP request and response to a message, so that a thrown exception
	/// carries the exchange that produced it.
	/// </summary>
	/// <param name="message">The message describing what went wrong.</param>
	internal string Describe(string message)
		=> $"Message: {message}\r\nLastAutoTaskRequest: {autoTaskLogger.LastRequest ?? "No Request"}\r\nLastAutoTaskResponse: {autoTaskLogger.LastResponse ?? "No Response"}";

	/// <summary>
	/// Logs and throws if a create, delete or update call reported errors.
	/// </summary>
	/// <param name="errors">The errors reported by AutoTask.</param>
	/// <param name="presentParticiple">The operation, as in "an error {creating} the entity".</param>
	/// <param name="noun">The operation, as in "errors occurred during {creation} of".</param>
	/// <param name="loggedEntity">The entity or entities to log alongside the errors.</param>
	internal void ThrowOnErrors(
		ATWSError[] errors,
		string presentParticiple,
		string noun,
		object? loggedEntity)
	{
		if (errors.Length == 0)
		{
			return;
		}

		logger.LogError($"There was an error {presentParticiple} the entity. {errors.Length} errors occurred.");
		LogEachError(errors);
		logger.LogError("Entity: " + ToJson(loggedEntity));

		throw new AutoTaskApiException(Describe(
			$"Errors occurred during {noun} of the AutoTask entity: {string.Join(";", errors.Select(e => e.Message))}"));
	}

	/// <summary>Logs each individual AutoTask error in order.</summary>
	/// <param name="errors">The errors reported by AutoTask.</param>
	private void LogEachError(ATWSError[] errors)
	{
		for (var errorNum = 0; errorNum < errors.Length; errorNum++)
		{
			logger.LogError($"Error {errorNum + 1}: {errors[errorNum].Message}");
		}
	}

	// Entities are serialised through an object-typed parameter deliberately: System.Text.Json
	// serialises according to the declared type, so a Ticket passed as the abstract Entity would
	// otherwise log only Entity's own members. Declaring the parameter as object makes
	// System.Text.Json use the runtime type instead.
	private static string ToJson(object? value)
		=> JsonSerializer.Serialize(value, EntityLogJsonSerializerOptions);
}
