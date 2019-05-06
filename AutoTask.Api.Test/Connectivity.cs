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
				.ExecuteQueryAsync("<queryxml><entity>Account</entity><query><field>id<expression op=\"greaterthan\">0</expression></field></query></queryxml>")
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
	}
}
