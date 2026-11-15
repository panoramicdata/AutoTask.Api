using AutoTask.Api.Filters;
using Xunit;

namespace AutoTask.Api.Test.Unit;

/// <summary>Unit tests for the QueryXML operator mapping.</summary>
public class OperatorExtensionsTests
{
	/// <summary>Verifies each supported operator maps to its QueryXML token.</summary>
	[Theory]
	[InlineData(Operator.BeginsWith, "beginswith")]
	[InlineData(Operator.EndsWith, "endswith")]
	[InlineData(Operator.Like, "like")]
	[InlineData(Operator.NotLike, "notlike")]
	[InlineData(Operator.GreaterThanOrEquals, "greaterthanorequals")]
	[InlineData(Operator.LessThanOrEquals, "lessthanorequals")]
	[InlineData(Operator.GreaterThan, "greaterthan")]
	[InlineData(Operator.LessThan, "lessthan")]
	[InlineData(Operator.Equals, "equals")]
	[InlineData(Operator.NotEquals, "notequal")]
	public void ToQueryXmlOperator_MapsSupportedOperators(Operator @operator, string expected)
		=> Assert.Equal(expected, @operator.ToQueryXmlOperator());

	/// <summary>Verifies operators with no QueryXML equivalent are rejected.</summary>
	[Theory]
	[InlineData(Operator.RegexMatches)]
	[InlineData(Operator.RegexNotMatches)]
	public void ToQueryXmlOperator_WithUnsupportedOperator_Throws(Operator @operator)
	{
		var exception = Assert.Throws<NotSupportedException>(() => @operator.ToQueryXmlOperator());

		Assert.Contains(@operator.ToString(), exception.Message);
	}
}
