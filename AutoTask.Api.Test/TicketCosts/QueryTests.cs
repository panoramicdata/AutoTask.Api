using AutoTask.Api.Filters;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace AutoTask.Api.Test.TicketCosts

{
	public class QueryTests : TestWithOutput
	{
		public QueryTests(ITestOutputHelper iTestOutputHelper, ILogger logger) : base(iTestOutputHelper, logger)
		{
		}

		[Fact]
		public async void AutoTaskClientTicketCost_Crud_Succeeds()
		{
			var resources = await AutoTaskClient.GetAsync<Ticket>(
				new Filter
				{
					Items = new List<FilterItem>
					{
						new FilterItem{Field = nameof(Ticket.TicketNumber), Operator = Operator.Equals, Value = "T20190626.0001" },
					}
				}
				).ConfigureAwait(false);
			Assert.Single(resources);
			var ticket = resources.Single();
			// We have a ticket

			var allocationCodes = await AutoTaskClient.GetAsync<AllocationCode>(
				new Filter
				{
					Items = new List<FilterItem>
					{
						new FilterItem{Field = nameof(AllocationCode.Name), Operator = Operator.Equals, Value = "Certify Expense" },
					}
				}
				).ConfigureAwait(false);
			Assert.Single(allocationCodes);
			var allocationCode = allocationCodes.Single();
			// We have an AllocationCode

			// Create a TicketCost
			var ticketCost = new TicketCost
			{
				Name = "Test Cost",
				Description = "Created by AutoTask.Api Unit Test",
				TicketID = ticket.id,
				BillableAmount = 100,
				Status = 7, // Delivered/Shipped Full
				AllocationCodeID = allocationCode.id,
				UnitQuantity = 1,
				DatePurchased = DateTime.UtcNow,
				CostType = 1 // Operational (2 is Capitalised)
			};
			var createdTicketCost = await Client
				.CreateAsync(ticketCost)
				.ConfigureAwait(false);

			// Assert that there is a ticketcost assigned
			var result = await AutoTaskClient.GetAsync<TicketCost>(
				new Filter
				{
					Items = new List<FilterItem>
					{
						new FilterItem{Field = "id", Operator = Operator.Equals, Value = createdTicketCost.id.ToString() },
					}
				}
				).ConfigureAwait(false);
			Assert.NotNull(result);
			Assert.NotEmpty(result);

			// Delete the ticket cost
			await Client
				.DeleteAsync(createdTicketCost)
				.ConfigureAwait(false);

			// Assert that there is now NO ticketcost assigned
			var ticketCostMissingResult = await AutoTaskClient.GetAsync<TicketCost>(
				new Filter
				{
					Items = new List<FilterItem>
					{
						new FilterItem{Field = "id", Operator = Operator.Equals, Value = createdTicketCost.id.ToString() },
					}
				}
				).ConfigureAwait(false);
			Assert.NotNull(ticketCostMissingResult);
			Assert.Empty(ticketCostMissingResult);
		}

		[Fact]
		public async void AutoTaskClientTicketCost_GetAllAsync()
		{
			var result = await Client.GetAllAsync("<queryxml><entity>TicketCost</entity><query><condition operator=\"and\"><field>CreateDate<expression op=\"greaterthan\">2000-01-01</expression></field></condition></query></queryxml>").ConfigureAwait(false);
			Assert.NotNull(result);
			Assert.NotEmpty(result);
		}
	}
}
