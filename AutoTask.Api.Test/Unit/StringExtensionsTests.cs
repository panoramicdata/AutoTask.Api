using AutoTask.Api.Extensions;
using Xunit;

namespace AutoTask.Api.Test.Unit;

/// <summary>Unit tests for the human readable list formatting.</summary>
public class StringExtensionsTests
{
	/// <summary>Verifies the list is quoted, comma separated and joined with the final conjunction.</summary>
	[Theory]
	[InlineData("", "")]
	[InlineData("a", "'a'")]
	[InlineData("a,b", "'a' or 'b'")]
	[InlineData("a,b,c", "'a', 'b' or 'c'")]
	public void ToHumanReadableString_FormatsByCount(string commaSeparated, string expected)
	{
		var items = commaSeparated.Length == 0 ? [] : commaSeparated.Split(',');

		Assert.Equal(expected, items.ToHumanReadableString());
	}

	/// <summary>Verifies that a null list produces an empty string.</summary>
	[Fact]
	public void ToHumanReadableString_WithNull_ReturnsEmpty()
		=> Assert.Equal(string.Empty, ((IEnumerable<object>)null!).ToHumanReadableString());

	/// <summary>Verifies that the quote and delimiters can be overridden.</summary>
	[Fact]
	public void ToHumanReadableString_UsesSuppliedDelimiters()
		=> Assert.Equal("\"a\"; \"b\" and \"c\"", new[] { "a", "b", "c" }.ToHumanReadableString("\"", "; ", " and "));
}
