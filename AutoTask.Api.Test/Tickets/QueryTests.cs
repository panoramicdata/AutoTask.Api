using AutoTask.Api.Filters;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

namespace AutoTask.Api.Test.Tickets;

/// <summary>Query tests for the Ticket entity.</summary>
[Trait("Category", "Integration")]
public class QueryTests : TestWithOutput
{
	/// <summary>Initializes a new instance of <see cref="QueryTests"/>.</summary>
	public QueryTests(ITestOutputHelper iTestOutputHelper) : base(iTestOutputHelper)
	{
	}

	/// <summary>Verifies that the AutoTask client can execute a filtered ticket query.</summary>
	[Fact]
	public async System.Threading.Tasks.Task AutoTaskClient_Query()
	{
		var result = await AutoTaskClient.GetAsync<Ticket>(
			new Filter
			{
				Items = new List<FilterItem>
				{
					new FilterItem
					{
						Field = "UDF Problem Signature",
						Operator = Operator.Equals,
						Value = "LMD15169"
					},
					new FilterItem
					{
						Field = "ticketCategory",
						Operator = Operator.Equals,
						Value = "2"
					},
					new FilterItem
					{
						Field = "ticketType",
						Operator = Operator.Equals,
						Value = "2"
					},
					// new FilterItem{Field = "Status", Operator = Operator.NotEquals, Value = "5" }, // Complete
					new FilterItem{Field = "Status", Operator = Operator.NotEquals, Value = "29" }, // Resolved
					new FilterItem{Field = "Status", Operator = Operator.NotEquals, Value = "34" }, // Cancelled
					new FilterItem{Field = "Status", Operator = Operator.NotEquals, Value = "66" }, // SD/NOC Responded
				}
			}
			);
		Assert.NotNull(result);
	}

	/// <summary>Verifies that the API client handles a bad XML query gracefully.</summary>
	[Fact]
	public async System.Threading.Tasks.Task Client_Query()
	{
		var result = await Client
			.GetAllAsync("<badsxml />");
		Assert.NotNull(result);
	}
}
