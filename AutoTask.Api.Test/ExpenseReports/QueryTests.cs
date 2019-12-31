using AutoTask.Api.Filters;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
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
						new FilterItem{Field = "SubmitterId", Operator = Operator.Equals, Value = "4" },
						//new FilterItem{Field = "id", Operator = Operator.Equals, Value = "773" },
					}
				}
				).ConfigureAwait(false);
			Assert.NotNull(result);
		}

		// Can't delete ExpenseReports so only run this if there's an issue

		//[Fact]
		//public async void CreateExpenseReport_Succeeds()
		//{
		//	var resources = await AutoTaskClient.GetAsync<Resource>(
		//			new Filter
		//			{
		//				Items = new List<FilterItem>
		//				{
		//					//new FilterItem{Field = "id", Operator = Operator.Equals, Value = "5" }, // Resolved
		//				}
		//			}
		//		).ConfigureAwait(false);
		//	Assert.NotNull(resources);
		//	Assert.NotEmpty(resources);
		//	var resource = resources.FirstOrDefault();// (r => (string)r.FirstName == "David" && (string)r.LastName == "Bond");
		//															// We have a user

		//	var expenseReport = new ExpenseReport
		//	{
		//		WeekEnding = "2019-08-31",
		//		Name = "Test Expense Report ABC",
		//		SubmitterID = resource.id
		//	};

		//	await Client
		//		.CreateAsync(expenseReport)
		//		.ConfigureAwait(false);
		//}
	}
}
