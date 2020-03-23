using AutoTask.Api.Filters;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

namespace AutoTask.Api.Test.Currencies

{
	public class QueryTests : TestWithOutput
	{
		public QueryTests(ITestOutputHelper iTestOutputHelper) : base(iTestOutputHelper)
		{
		}

		[Fact]
		public async void AutoTaskClientCurrency_Query()
		{
			// TODO - Detect the system has multi-currency enabled
			var result = await AutoTaskClient.GetAsync<Currency>(
				new Filter
				{
					Items = new List<FilterItem>
					{
						new FilterItem{Field = "id", Operator = Operator.Equals, Value = "5" }, // Resolved
					}
				}
				).ConfigureAwait(false);
			Assert.NotNull(result);
			Assert.Empty(result);
		}
	}
}
