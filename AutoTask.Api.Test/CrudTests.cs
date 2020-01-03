using PanoramicData.SheetMagic;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace AutoTask.Api.Test
{
	public class CrudTests : TestWithOutput
	{
		public CrudTests(ITestOutputHelper iTestOutputHelper) : base(iTestOutputHelper)
		{
		}

		//[Fact(Skip = "Delete doesn't work so always creates")]
		//public async void CreateAndDelete_ValidData_WorksAsExpectedAsync()
		//{
		//	var uniqueCode = Guid.NewGuid().ToString();
		//	var ticketTitle = "Test ticket title " + uniqueCode;
		//	var ticketDescription = "Test ticket description " + uniqueCode;

		//	var ticket = new Ticket
		//	{
		//		DueDateTime = DateTimeOffset.UtcNow.AddHours(8).ToEstString(),
		//		TicketCategory = 2,
		//		TicketType = 2,
		//		Title = ticketTitle,
		//		Description = ticketDescription,
		//		Status = 1,
		//		Priority = 3,
		//		AccountID = 0
		//	};
		//	var createdTicket = await Client.CreateAsync(ticket).ConfigureAwait(false);
		//	// New ticket created

		//	List<Ticket> tickets;
		//	try
		//	{
		//		// Get the created ticket to confirm it exists
		//		tickets = await AutoTaskClient.GetAsync<Ticket>(
		//			new Filter
		//			{
		//				Items = new List<FilterItem>
		//				{
		//				new FilterItem {
		//					Field = "id",
		//					Operator = Operator.Equals,
		//					Value = createdTicket.id.ToString()
		//				}
		//				}
		//			}
		//			).ConfigureAwait(false);

		//		tickets.Should().NotBeEmpty();
		//		var existingTicket = tickets[0];
		//		existingTicket.id.Should().Be(createdTicket.id);
		//		existingTicket.Title.Should().Be(ticketTitle);
		//		existingTicket.Description.Should().Be(ticketDescription);
		//		// Ticket was found and valid
		//	}
		//	finally
		//	{
		//		// Always try and delete
		//		await Client.DeleteAsync(createdTicket).ConfigureAwait(false);
		//	}

		//	// Get the created ticket to confirm it exists
		//	tickets = await AutoTaskClient.GetAsync<Ticket>(
		//		new Filter
		//		{
		//			Items = new List<FilterItem>
		//			{
		//				new FilterItem {
		//					Field = "id",
		//					Operator = Operator.Equals,
		//					Value = createdTicket.id.ToString()
		//				}
		//			}
		//		}
		//		).ConfigureAwait(false);

		//	tickets.Should().BeEmpty();
		//	// Ticket no longer exists and was deleted
		//}

		[Fact]
		public async void QueryTest()
		{
			var _ = await Client.GetAllAsync(@"<queryxml><entity>ProjectCost</entity><query><condition operator=""and""><field>Description<expression op=""BeginsWith"">Certify:</expression></field><field>CreateDate<expression op=""greaterthan"">2019-07-22 18:49:01</expression></field></condition></query></queryxml>").ConfigureAwait(false);
		}

		[Fact]
		public async void TaskTimeEntry()
		{
			var timeEntries = (await Client.GetAllAsync(@"<queryxml><entity>TimeEntry</entity><query><condition operator=""and""><field>StartDateTime<expression op=""greaterthan"">2019-09-01 00:00:00</expression></field><field>StartDateTime<expression op=""lessthan"">2019-10-01 00:00:00</expression></field></condition></query></queryxml>").ConfigureAwait(false))
				.Cast<TimeEntry>()
				.ToList();
			var resources = (await Client.GetAllAsync(@"<queryxml><entity>Resource</entity><query><condition operator=""and""><field>id<expression op=""greaterthan"">-1</expression></field></condition></query></queryxml>").ConfigureAwait(false))
				.Cast<Resource>()
				.ToList();
			var timeEntryModels = timeEntries.Select(timeEntry => new TimeEntryModel
			{
				ResourceName = resources.SingleOrDefault(r => r.id == (int)timeEntry.ResourceID)?.UserName.ToString() ?? "Unknown",
				TicketId = timeEntry.TicketID.ToString(),
				Hours = (decimal)timeEntry.HoursWorked,
				WorkType = timeEntry.Type.ToString()
			})
				.ToList();

			var cache = new Dictionary<string, Ticket>();
			var a = 1;
			foreach (var timeEntryModel in timeEntryModels)
			{
				timeEntryModel.SubIssue = (await GetSubIssueTypeByTicketIdAsync(timeEntryModel.TicketId, cache).ConfigureAwait(false)).SubIssueType?.ToString() ?? "None";
				a++;
			}

			using var magicSpreadsheet = new MagicSpreadsheet(new FileInfo("output.xlsx"));
			magicSpreadsheet.AddSheet(timeEntryModels);
		}

		private async Task<Ticket> GetSubIssueTypeByTicketIdAsync(string ticketId, Dictionary<string, Ticket> cache)
		{
			if (cache.TryGetValue(ticketId, out var ticket))
			{
				return ticket;
			}
			cache[ticketId] = ticket = (await Client.GetAllAsync($"<queryxml><entity>Ticket</entity><query><condition operator=\"and\"><field>id<expression op=\"equals\">{ticketId}</expression></field></condition></query></queryxml>").ConfigureAwait(false))
				.Cast<Ticket>()
				.Single();
			return ticket;
		}
	}
}