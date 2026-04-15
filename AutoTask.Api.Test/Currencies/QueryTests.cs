using AutoTask.Api.Filters;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

namespace AutoTask.Api.Test.Currencies;


/// <summary>Query tests for the Currency entity.</summary>
public class QueryTests : TestWithOutput
{
	/// <summary>Initializes a new instance of <see cref="QueryTests"/>.</summary>
	public QueryTests(ITestOutputHelper iTestOutputHelper) : base(iTestOutputHelper)
	{
	}

	/// <summary>Verifies that a currency query with a specific ID returns an empty result.</summary>
	[Fact]
	public async System.Threading.Tasks.Task AutoTaskClientCurrency_Query()
	{
		var result = await AutoTaskClient.GetAsync<Currency>(
			new Filter
			{
				Items = new List<FilterItem>
				{
					new FilterItem{Field = "id", Operator = Operator.Equals, Value = "5" }, // Resolved
				}
			}
			);
		Assert.NotNull(result);
		Assert.Empty(result);
	}
}
