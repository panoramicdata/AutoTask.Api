using Xunit;

namespace AutoTask.Api.Test.Unit;

/// <summary>Pure unit tests: a refused write throws before any SOAP call, so no credentials or network are needed.</summary>
public class ReadOnlyOptionTests
{
	private static Client NewClient(bool isReadOnly)
		=> new("fake-user", "fake-password", "fake-code", clientOptions: new ClientOptions { IsReadOnly = isReadOnly });

	/// <summary>Verifies the default.</summary>
	[Fact]
	public void IsReadOnly_DefaultsToFalse()
		=> Assert.False(new ClientOptions().IsReadOnly);

	/// <summary>Verifies the create refusal.</summary>
	[Fact]
	public async System.Threading.Tasks.Task CreateIsRefused_WhenIsReadOnly()
	{
		using var client = NewClient(true);

		await Assert.ThrowsAsync<InvalidOperationException>(() => client.CreateAsync(new Account(), TestContext.Current.CancellationToken));
	}

	/// <summary>Verifies the update refusal.</summary>
	[Fact]
	public async System.Threading.Tasks.Task UpdateIsRefused_WhenIsReadOnly()
	{
		using var client = NewClient(true);

		await Assert.ThrowsAsync<InvalidOperationException>(() => client.UpdateAsync(new Account(), TestContext.Current.CancellationToken));
	}

	/// <summary>Verifies the delete refusal.</summary>
	[Fact]
	public async System.Threading.Tasks.Task DeleteIsRefused_WhenIsReadOnly()
	{
		using var client = NewClient(true);

		await Assert.ThrowsAsync<InvalidOperationException>(() => client.DeleteAsync(new Account(), TestContext.Current.CancellationToken));
	}
}
