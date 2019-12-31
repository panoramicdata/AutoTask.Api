using AutoTask.Api.Filters;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

namespace AutoTask.Api.Test
{
	public class Connectivity : TestWithOutput
	{
		public Connectivity(ITestOutputHelper iTestOutputHelper) : base(iTestOutputHelper)
		{
		}

		[Fact]
		public async void BasicTest_Connects()
		{
			var result = await Client
				.QueryAsync("<queryxml><entity>Account</entity><query><field>id<expression op=\"greaterthan\">0</expression></field></query></queryxml>")
				.ConfigureAwait(false);
			Assert.NotNull(result);
		}

		[Fact]
		public async void GetWsdlVersion_Succeeds()
		{
			var result = await Client
				.GetWsdlVersion()
				.ConfigureAwait(false);
			Assert.NotNull(result);
		}

		[Fact]
		public async void GetFieldInfo()
		{
			var result = await Client
				.GetFieldInfoAsync(nameof(Account))
				.ConfigureAwait(false);
			Assert.NotNull(result);
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
						new FilterItem{Field = "Status", Operator = Operator.NotEquals, Value = "5" }, // Complete
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
