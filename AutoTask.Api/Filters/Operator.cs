namespace AutoTask.Api.Filters;

/// <summary>Operators available for AutoTask query filter expressions.</summary>
public enum Operator
{
	/// <summary>Greater than or equals (>=).</summary>
	GreaterThanOrEquals,
	/// <summary>Less than or equals (&lt;=).</summary>
	LessThanOrEquals,
	/// <summary>Greater than (&gt;).</summary>
	GreaterThan,
	/// <summary>Less than (&lt;).</summary>
	LessThan,
	/// <summary>Field value matches the regex.</summary>
	RegexMatches,
	/// <summary>Field value does not match the regex.</summary>
	RegexNotMatches,
	/// <summary>Not equal to.</summary>
	NotEquals,
	/// <summary>Equal to.</summary>
	Equals,
	/// <summary>Field value begins with the supplied string.</summary>
	BeginsWith,
	/// <summary>Field value ends with the supplied string.</summary>
	EndsWith,
	/// <summary>Field value is like the supplied string.</summary>
	Like,
	/// <summary>Field value is not like the supplied string.</summary>
	NotLike
}
