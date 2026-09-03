using AutoTask.Api.Extensions;
using Xunit;

namespace AutoTask.Api.Test.Unit;

/// <summary>Unit tests for <see cref="DateExtensions"/>. No AutoTask credentials required.</summary>
public class DateExtensionsTests
{
	/// <summary>
	/// Verifies that a UTC instant is rendered in the AutoTask date format, shifted to Eastern Standard Time.
	/// A January instant is used because the Windows ("Eastern Standard Time") and Linux ("EST") zones
	/// agree on the offset outside daylight saving.
	/// </summary>
	[Fact]
	public void ToEstString_ShiftsUtcToEasternStandardTime()
	{
		var dateTimeOffset = new DateTimeOffset(2024, 1, 15, 12, 0, 0, TimeSpan.Zero);

		Assert.Equal("2024-01-15T07:00:00.000", dateTimeOffset.ToEstString());
	}

	/// <summary>Verifies that milliseconds are preserved and always rendered to three digits.</summary>
	[Fact]
	public void ToEstString_PreservesMilliseconds()
	{
		var dateTimeOffset = new DateTimeOffset(2024, 1, 15, 12, 0, 0, 7, TimeSpan.Zero);

		Assert.Equal("2024-01-15T07:00:00.007", dateTimeOffset.ToEstString());
	}
}
