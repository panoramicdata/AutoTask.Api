using AutoTask.Api.Exceptions;
using Xunit;

namespace AutoTask.Api.Test.Unit;

/// <summary>Unit tests for the response handling in <see cref="Client"/> that needs no SOAP call.</summary>
public class ClientInternalsTests
{
	private static Client NewClient()
		=> new("fake-user", "fake-password", "fake-code");

	private static object? InvokeClient(Client client, string methodName, params object?[] arguments)
		=> PrivateAccess.Invoke(typeof(Client), client, methodName, arguments);

	/// <summary>Verifies a successful query response yields its entities.</summary>
	[Fact]
	public void GetEntityResults_WhenReturnCodeIsOne_ReturnsEntities()
	{
		using var client = NewClient();
		var entities = new Entity[] { new Account { id = 1 } };
		var response = new queryResponse(new ATWSResponse { ReturnCode = 1, EntityResults = entities });

		Assert.Same(entities, InvokeClient(client, "GetEntityResults", response));
	}

	/// <summary>Verifies a failed query response is reported with every error message.</summary>
	[Fact]
	public void GetEntityResults_WhenReturnCodeIsNotOne_ThrowsWithErrors()
	{
		using var client = NewClient();
		var response = new queryResponse(new ATWSResponse
		{
			ReturnCode = -1,
			Errors = [new ATWSError { Message = "one" }, new ATWSError { Message = "two" }]
		});

		var exception = Assert.Throws<AutoTaskApiException>(() => InvokeClient(client, "GetEntityResults", response));

		Assert.Contains("'one' and 'two'", exception.Message);
	}

	/// <summary>Verifies the update response is accepted when the counts match.</summary>
	[Fact]
	public void GetUpdatedEntities_WhenCountMatches_ReturnsEntities()
	{
		using var client = NewClient();
		var sent = new Entity[] { new Account { id = 1 } };
		var returned = new Entity[] { new Account { id = 1 } };

		var result = InvokeClient(client, "GetUpdatedEntities", new updateResponse(new ATWSResponse { EntityResults = returned }), sent);

		Assert.Same(returned, result);
	}

	/// <summary>Verifies a missing or mismatched update result is rejected.</summary>
	[Theory]
	[InlineData(false, "Did not get a result back after updating")]
	[InlineData(true, "expected 1, received 2")]
	public void GetUpdatedEntities_WithBadResponse_Throws(bool hasResults, string expectedMessage)
	{
		using var client = NewClient();
		var results = hasResults ? new Entity[] { new Account(), new Account() } : null;
		var response = new updateResponse(new ATWSResponse { EntityResults = results! });

		var exception = Assert.Throws<AutoTaskApiException>(
			() => InvokeClient(client, "GetUpdatedEntities", response, new Entity[] { new Account() }));

		Assert.Contains(expectedMessage, exception.Message);
	}

	/// <summary>Verifies the next page query is built from the last entity id, or stops when there is none.</summary>
	[Fact]
	public void TryBuildNextPageQuery_BuildsFromLastEntityOrStops()
	{
		const string query = "<queryxml><query></query></queryxml>";
		var withResults = new queryResponse(new ATWSResponse { EntityResults = [new Account { id = 3 }, new Account { id = 7 }] });
		var withoutResults = new queryResponse(new ATWSResponse { EntityResults = [] });

		var more = new object?[] { query, withResults, null };
		var none = new object?[] { query, withoutResults, null };

		Assert.Equal(true, PrivateAccess.Invoke(typeof(Client), null, "TryBuildNextPageQuery", more));
		Assert.Contains(">7</expression>", (string)more[2]!);
		Assert.Equal(false, PrivateAccess.Invoke(typeof(Client), null, "TryBuildNextPageQuery", none));
		Assert.Equal(query, (string)none[2]!);
	}
}
