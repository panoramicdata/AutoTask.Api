using AutoTask.Api.Filters;
using Xunit;

namespace AutoTask.Api.Test.Unit;

/// <summary>Unit tests for <see cref="FilterItem"/> expression parsing. No AutoTask credentials required.</summary>
public class FilterItemTests
{
	/// <summary>Verifies that each supported operator token is parsed into the matching <see cref="Operator"/>.</summary>
	[Theory]
	[InlineData("id:5", "id", Operator.Equals, "5")]
	[InlineData("id!:5", "id", Operator.NotEquals, "5")]
	[InlineData("id>:5", "id", Operator.GreaterThanOrEquals, "5")]
	[InlineData("id<:5", "id", Operator.LessThanOrEquals, "5")]
	[InlineData("id>5", "id", Operator.GreaterThan, "5")]
	[InlineData("id<5", "id", Operator.LessThan, "5")]
	[InlineData("name:~^A", "name", Operator.RegexMatches, "^A")]
	[InlineData("name!~^A", "name", Operator.RegexNotMatches, "^A")]
	[InlineData("name^Acme", "name", Operator.BeginsWith, "Acme")]
	[InlineData("name$Ltd", "name", Operator.EndsWith, "Ltd")]
	[InlineData("name%Acme", "name", Operator.Like, "Acme")]
	[InlineData("name!%Acme", "name", Operator.NotLike, "Acme")]
	public void Parse_SetsFieldOperatorAndValue(
		string text,
		string expectedField,
		Operator expectedOperator,
		string expectedValue)
	{
		var filterItem = new FilterItem(text);

		Assert.Equal(expectedField, filterItem.Field);
		Assert.Equal(expectedOperator, filterItem.Operator);
		Assert.Equal(expectedValue, filterItem.Value);
	}

	/// <summary>Verifies that an expression without a recognised operator is rejected.</summary>
	[Fact]
	public void Parse_WithNoOperator_Throws()
	{
		var exception = Assert.Throws<ArgumentException>(() => new FilterItem("id"));

		Assert.Equal("No operator present.", exception.Message);
	}

	/// <summary>Verifies that an expression with an empty field name is rejected.</summary>
	[Fact]
	public void Parse_WithNoField_Throws()
	{
		var exception = Assert.Throws<ArgumentException>(() => new FilterItem(":5"));

		Assert.Equal("No field present", exception.Message);
	}
}
