using AutoTask.Api.Extensions;
using Xunit;

namespace AutoTask.Api.Test.Unit;

/// <summary>Unit tests for awaiting a task with a cancellation token.</summary>
public class TaskTimeoutExtensionsTests
{
	/// <summary>Verifies the original task is returned when cancellation cannot occur or the task is done.</summary>
	[Fact]
	public void WithCancellation_WhenNoWaitingNeeded_ReturnsOriginalTask()
	{
		using var source = new CancellationTokenSource();
		var pending = new TaskCompletionSource<int>().Task;
		var completed = System.Threading.Tasks.Task.FromResult(1);

		Assert.Same(pending, pending.WithCancellation(CancellationToken.None));
		Assert.Same(completed, completed.WithCancellation(source.Token));
	}

	/// <summary>Verifies an already cancelled token produces a cancelled task for an unfinished task.</summary>
	[Fact]
	public void WithCancellation_WithCancelledToken_ReturnsCancelledTask()
	{
		using var source = new CancellationTokenSource();
		source.Cancel();

		var result = new TaskCompletionSource<int>().Task.WithCancellation(source.Token);

		Assert.True(result.IsCanceled);
	}

	/// <summary>Verifies cancelling while waiting breaks out of the await.</summary>
	[Fact]
	public async System.Threading.Tasks.Task WithCancellation_WhenCancelledWhileWaiting_Throws()
	{
		using var source = new CancellationTokenSource();
		var wrapped = new TaskCompletionSource<int>().Task.WithCancellation(source.Token);

		await source.CancelAsync();

		await Assert.ThrowsAnyAsync<OperationCanceledException>(() => wrapped);
	}

	/// <summary>Verifies the result flows through when the task finishes before cancellation.</summary>
	[Fact]
	public async System.Threading.Tasks.Task WithCancellation_WhenTaskCompletesFirst_ReturnsResult()
	{
		using var source = new CancellationTokenSource();
		var completion = new TaskCompletionSource<int>();
		var wrapped = completion.Task.WithCancellation(source.Token);

		completion.SetResult(42);

		Assert.Equal(42, await wrapped);
	}

	/// <summary>Verifies the non-generic cancelled task helper.</summary>
	[Fact]
	public void TaskFromCanceled_ReturnsCancelledTask()
	{
		using var source = new CancellationTokenSource();
		source.Cancel();

		Assert.True(TaskTimeoutExtensions.TaskFromCanceled(source.Token).IsCanceled);
	}

	/// <summary>Verifies all empty structs are equal.</summary>
	[Fact]
	public void EmptyStruct_IsEqualToItself()
	{
		var first = default(EmptyStruct);
		var second = default(EmptyStruct);

		Assert.True(first == second);
		Assert.False(first != second);
		Assert.True(first.Equals(second));
		Assert.True(first.Equals((object)second));
		Assert.False(first.Equals("other"));
		Assert.Equal(0, first.GetHashCode());
	}
}
