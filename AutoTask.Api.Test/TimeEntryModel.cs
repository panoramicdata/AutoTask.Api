namespace AutoTask.Api.Test
{
	internal class TimeEntryModel
	{
		public string? ResourceName { get; set; }
		public decimal Hours { get; set; }
		public string? WorkType { get; set; }
		public string? SubIssue { get; set; }
		public string? TicketId { get; set; }
	}
}