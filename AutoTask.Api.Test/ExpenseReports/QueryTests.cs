using AutoTask.Api.Filters;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace AutoTask.Api.Test.ExpenseReports

{
	public class QueryTests : TestWithOutput
	{
		public QueryTests(ITestOutputHelper iTestOutputHelper) : base(iTestOutputHelper)
		{
		}

		[Fact]
		public async void AutoTaskClient_Query()
		{
			var result = await AutoTaskClient.GetAsync<ExpenseReport>(
				new Filter
				{
					Items = new List<FilterItem>
					{
						new FilterItem{Field = "ApprovedDate", Operator = Operator.GreaterThanOrEquals, Value = "2018-07-01" }, // Resolved
					}
				}
				).ConfigureAwait(false);
			Assert.NotNull(result);
		}

		[Fact]
		public async void CreateExpenseReport_Succeeds()
		{
			var clientPortalUsers = await AutoTaskClient.GetAsync<ClientPortalUser>(
					new Filter
				{
					Items = new List<FilterItem>
					{
						//new FilterItem{Field = "id", Operator = Operator.Equals, Value = "5" }, // Resolved
					}
				}
				).ConfigureAwait(false);
			Assert.NotNull(clientPortalUsers);
			Assert.NotEmpty(clientPortalUsers);
			var user = clientPortalUsers.First();
			// We have a user

			await AutoTaskClient
				.CreateAsync(expenseReport)
				.ConfigureAwait(false);

		}
	}
}
