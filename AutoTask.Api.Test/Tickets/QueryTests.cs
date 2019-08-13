using AutoTask.Api.Filters;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

namespace AutoTask.Api.Test.Tickets
{
	public class QueryTests : TestWithOutput
	{
		public QueryTests(ITestOutputHelper iTestOutputHelper) : base(iTestOutputHelper)
		{
		}

		[Fact]
		public async void AutoTaskClient_Query()
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
				).ConfigureAwait(false);
			Assert.NotNull(result);
		}
	}
}
