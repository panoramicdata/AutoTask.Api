using System;

namespace AutoTask.Api.Filters;

/// <summary>Extension methods for <see cref="Operator"/>.</summary>
internal static class OperatorExtensions
{
	/// <summary>
	/// Returns the token used to represent the operator in an AutoTask QueryXML expression.
	/// </summary>
	/// <param name="operator">The operator to convert.</param>
	/// <returns>The QueryXML operator token.</returns>
	/// <exception cref="NotSupportedException">The operator has no QueryXML equivalent.</exception>
	internal static string ToQueryXmlOperator(this Operator @operator)
		=> @operator switch
		{
			Operator.BeginsWith or
			Operator.EndsWith or
			Operator.Like or
			Operator.NotLike or
			Operator.GreaterThanOrEquals or
			Operator.LessThanOrEquals or
			Operator.GreaterThan or
			Operator.LessThan or
			Operator.Equals
				=> @operator.ToString().ToLowerInvariant(),
			Operator.NotEquals => "notequal",
			_ => throw new NotSupportedException($"{@operator} not supported.")
		};
}
