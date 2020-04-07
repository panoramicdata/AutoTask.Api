using PanoramicData.SheetMagic;
using System;
using System.IO;
using System.Linq;
using Xunit.Abstractions;

namespace AutoTask.Api.Test.TicketCosts

{
	public class DeleteTests : TestWithOutput
	{
		public DeleteTests(ITestOutputHelper iTestOutputHelper) : base(iTestOutputHelper)
		{
		}

		public async void AutoTaskClientProjectCost_Fix_Succeeds()
		{
			var projectCosts = (await Client.GetAllAsync(@"<queryxml>
    <entity>ProjectCost</entity>
    <query>
        <field>Description<expression op=""beginswith"">Certify:</expression></field>
        <field>CreateDate<expression op=""greaterthan"">2020-03-27</expression></field>
    </query>
</queryxml>").ConfigureAwait(false)).Cast<ProjectCost>().ToList();

			var recently = DateTime.Parse("2020-03-27");
			var allCreatedRecently = projectCosts.TrueForAll(pc => (DateTime)pc.CreateDate >= recently && (pc.Description as string)?.StartsWith("Certify:") == true);
			var fileInfo = new FileInfo(Path.GetTempFileName() + ".xlsx");
			try
			{
				using var magicSpreadsheet = new MagicSpreadsheet(fileInfo);
				magicSpreadsheet.AddSheet(projectCosts);
				magicSpreadsheet.Save();

				foreach (var projectCost in projectCosts)
				{
					await Client.DeleteAsync(projectCost).ConfigureAwait(false);
				}
			}
			finally
			{
				var fileName = fileInfo.FullName;
				fileInfo.Delete();
			}
		}

		public async void AutoTaskClientTicketCost_Fix_Succeeds()
		{
			var ticketCosts = (await Client.GetAllAsync(@"<queryxml>
    <entity>TicketCost</entity>
    <query>
        <field>Description<expression op=""beginswith"">Certify:</expression></field>
        <field>CreateDate<expression op=""greaterthan"">2020-03-27</expression></field>
    </query>
</queryxml>").ConfigureAwait(false)).Cast<TicketCost>().ToList();

			var recently = DateTime.Parse("2020-03-27");
			var allCreatedRecently = ticketCosts.TrueForAll(pc => (DateTime)pc.CreateDate >= recently && (pc.Description as string)?.StartsWith("Certify:") == true);
			var fileInfo = new FileInfo(Path.GetTempFileName() + ".xlsx");
			try
			{
				using var magicSpreadsheet = new MagicSpreadsheet(fileInfo);
				magicSpreadsheet.AddSheet(ticketCosts);
				magicSpreadsheet.Save();

				var ticketCostIndex = 0;
				foreach (var ticketCost in ticketCosts)
				{
					await Client.DeleteAsync(ticketCost).ConfigureAwait(false);
					ticketCostIndex++;
				}
			}
			finally
			{
				var fileName = fileInfo.FullName;
				fileInfo.Delete();
			}
		}
	}
}
