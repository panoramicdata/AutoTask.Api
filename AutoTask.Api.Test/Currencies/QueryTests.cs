﻿using AutoTask.Api.Filters;
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
}
