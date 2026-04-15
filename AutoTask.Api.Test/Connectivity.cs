using AutoTask.Api.Filters;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

namespace AutoTask.Api.Test;

/// <summary>Connectivity tests that verify basic AutoTask API operations.</summary>
public class Connectivity : TestWithOutput
{
	/// <summary>Initializes a new instance of <see cref="Connectivity"/>.</summary>
	public Connectivity(ITestOutputHelper iTestOutputHelper) : base(iTestOutputHelper)
	{
	}

	/// <summary>Verifies that a basic account query returns a non-null result.</summary>
	[Fact]
	public async System.Threading.Tasks.Task BasicTest_Connects()
	{
		var result = await Client
			.QueryAsync("<queryxml><entity>Account</entity><query><field>id<expression op=\"greaterthan\">0</expression></field></query></queryxml>");
		Assert.NotNull(result);
	}

	/// <summary>Verifies that the WSDL version can be retrieved.</summary>
	[Fact]
	public async System.Threading.Tasks.Task GetWsdlVersion_Succeeds()
	{
		var result = await Client
			.GetWsdlVersion();
		Assert.NotNull(result);
	}

	/// <summary>Verifies that field info can be retrieved for the Account entity.</summary>
	[Fact]
	public async System.Threading.Tasks.Task GetFieldInfo()
	{
		var result = await Client
			.GetFieldInfoAsync(nameof(Account));
		Assert.NotNull(result);
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
					new FilterItem{Field = "Status", Operator = Operator.NotEquals, Value = "5" }, // Complete
					new FilterItem{Field = "Status", Operator = Operator.NotEquals, Value = "29" }, // Resolved
					new FilterItem{Field = "Status", Operator = Operator.NotEquals, Value = "34" }, // Cancelled
					new FilterItem{Field = "Status", Operator = Operator.NotEquals, Value = "66" }, // SD/NOC Responded
				}
			}
			);
		Assert.NotNull(result);
	}
}
