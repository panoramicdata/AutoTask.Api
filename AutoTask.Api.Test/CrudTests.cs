using AutoTask.Api.Extensions;
using AutoTask.Api.Filters;
using FluentAssertions;
using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

namespace AutoTask.Api.Test
{
	public class CrudTests : TestWithOutput
	{
		public CrudTests(ITestOutputHelper iTestOutputHelper) : base(iTestOutputHelper)
		{
		}

		[Fact(Skip = "Delete doesn't work so always creates")]
		public async void CreateAndDelete_ValidData_WorksAsExpectedAsync()
		{
			var uniqueCode = Guid.NewGuid().ToString();
			var ticketTitle = "Test ticket title " + uniqueCode;
			var ticketDescription = "Test ticket description " + uniqueCode;

			var ticket = new Ticket
			{
				DueDateTime = DateTimeOffset.UtcNow.AddHours(8).ToEstString(),
				TicketCategory = 2,
				TicketType = 2,
				Title = ticketTitle,
				Description = ticketDescription,
				Status = 1,
				Priority = 3,
				AccountID = 0
			};
			var createdTicket = await Client.CreateAsync(ticket).ConfigureAwait(false);
			// New ticket created

			List<Ticket> tickets;
			try
			{
				// Get the created ticket to confirm it exists
				tickets = await AutoTaskClient.GetAsync<Ticket>(
					new Filter
					{
						Items = new List<FilterItem>
						{
						new FilterItem {
							Field = "id",
							Operator = Operator.Equals,
							Value = createdTicket.id.ToString()
						}
						}
					}
					).ConfigureAwait(false);

				tickets.Should().NotBeEmpty();
				var existingTicket = tickets[0];
				existingTicket.id.Should().Be(createdTicket.id);
				existingTicket.Title.Should().Be(ticketTitle);
				existingTicket.Description.Should().Be(ticketDescription);
				// Ticket was found and valid
			}
			finally
			{
				// Always try and delete
				await Client.DeleteAsync(createdTicket).ConfigureAwait(false);
			}

			// Get the created ticket to confirm it exists
			tickets = await AutoTaskClient.GetAsync<Ticket>(
				new Filter
				{
					Items = new List<FilterItem>
					{
						new FilterItem {
							Field = "id",
							Operator = Operator.Equals,
							Value = createdTicket.id.ToString()
						}
					}
				}
				).ConfigureAwait(false);

			tickets.Should().BeEmpty();
			// Ticket no longer exists and was deleted
		}
	}
}