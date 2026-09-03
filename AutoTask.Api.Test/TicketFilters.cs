using AutoTask.Api.Filters;
using System.Collections.Generic;
using System.Linq;

namespace AutoTask.Api.Test;

/// <summary>
/// Builds the ticket filters shared by the ticket query tests, so the filter definition
/// lives in one place rather than being copied into each test.
/// </summary>
internal static class TicketFilters
{
	/// <summary>
	/// Returns a filter matching tickets with the supplied problem signature, excluding the supplied status IDs.
	/// </summary>
	/// <param name="problemSignature">The value of the "Problem Signature" user-defined field.</param>
	/// <param name="excludedStatusIds">The status IDs to exclude from the results.</param>
	internal static Filter WithProblemSignature(
		string problemSignature,
		params string[] excludedStatusIds)
	{
		var items = new List<FilterItem>
		{
			new() { Field = "UDF Problem Signature", Operator = Operator.Equals, Value = problemSignature },
			new() { Field = "ticketCategory", Operator = Operator.Equals, Value = "2" },
			new() { Field = "ticketType", Operator = Operator.Equals, Value = "2" }
		};

		items.AddRange(excludedStatusIds
			.Select(statusId => new FilterItem { Field = "Status", Operator = Operator.NotEquals, Value = statusId }));

		return new Filter { Items = items };
	}
}
