using Xunit;

namespace AutoTask.Api.Test;

/// <summary>Connectivity tests that verify basic AutoTask API operations.</summary>
/// <remarks>Initializes a new instance of <see cref="Connectivity"/>.</remarks>
[Trait("Category", "Integration")]
public class Connectivity(ITestOutputHelper iTestOutputHelper) : TestWithOutput(iTestOutputHelper)
{


	/// <summary>Verifies that a basic account query returns a non-null result.</summary>
	[Fact]
	public async System.Threading.Tasks.Task BasicTest_Connects()
	{
		var result = await Client
			.QueryAsync(
				"<queryxml><entity>Account</entity><query><field>id<expression op=\"greaterthan\">0</expression></field></query></queryxml>",
				TestContext.Current.CancellationToken);
		Assert.NotNull(result);
	}

	/// <summary>Verifies that the WSDL version can be retrieved.</summary>
	[Fact]
	public async System.Threading.Tasks.Task GetWsdlVersion_Succeeds()
	{
		var result = await Client
			.GetWsdlVersion(TestContext.Current.CancellationToken);
		Assert.NotNull(result);
	}

	/// <summary>Verifies that field info can be retrieved for the Account entity.</summary>
	[Fact]
	public async System.Threading.Tasks.Task GetFieldInfo()
	{
		var result = await Client
			.GetFieldInfoAsync(nameof(Account), TestContext.Current.CancellationToken);
		Assert.NotNull(result);
	}

	/// <summary>Verifies that the AutoTask client can execute a filtered ticket query.</summary>
	[Fact]
	public async System.Threading.Tasks.Task AutoTaskClient_Query()
	{
		// 5: Complete, 29: Resolved, 34: Cancelled, 66: SD/NOC Responded
		var result = await AutoTaskClient.GetAsync<Ticket>(
			TicketFilters.WithProblemSignature("LMD15169", "5", "29", "34", "66"));

		Assert.NotNull(result);
	}
}
