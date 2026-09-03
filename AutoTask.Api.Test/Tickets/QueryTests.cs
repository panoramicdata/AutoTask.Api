using Xunit;

namespace AutoTask.Api.Test.Tickets;

/// <summary>Query tests for the Ticket entity.</summary>
/// <remarks>Initializes a new instance of <see cref="QueryTests"/>.</remarks>
[Trait("Category", "Integration")]
public class QueryTests(ITestOutputHelper iTestOutputHelper) : TestWithOutput(iTestOutputHelper)
{


	/// <summary>Verifies that the AutoTask client can execute a filtered ticket query.</summary>
	[Fact]
	public async System.Threading.Tasks.Task AutoTaskClient_Query()
	{
		// 29: Resolved, 34: Cancelled, 66: SD/NOC Responded
		var result = await AutoTaskClient.GetAsync<Ticket>(
			TicketFilters.WithProblemSignature("LMD15169", "29", "34", "66"));

		Assert.NotNull(result);
	}

	/// <summary>Verifies that the API client handles a bad XML query gracefully.</summary>
	[Fact]
	public async System.Threading.Tasks.Task Client_Query()
	{
		var result = await Client
			.GetAllAsync("<badsxml />", TestContext.Current.CancellationToken);
		Assert.NotNull(result);
	}
}
