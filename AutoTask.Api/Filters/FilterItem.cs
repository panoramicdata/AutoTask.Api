using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoTask.Api.Filters;

/// <summary>A single field-operator-value condition in a query filter.</summary>
public class FilterItem
{
	private static readonly Dictionary<string, Operator> Operators = new()
	{
		{ ">:", Operator.GreaterThanOrEquals },
		{ "<:", Operator.LessThanOrEquals },
		{ ">", Operator.GreaterThan },
		{ "<", Operator.LessThan },
		{ ":~", Operator.RegexMatches },
		{ "!~", Operator.RegexNotMatches },
		{ "^", Operator.BeginsWith },
		{ "$", Operator.EndsWith },
		{ "!%", Operator.NotLike },
		{ "%", Operator.Like },
		{ "!:", Operator.NotEquals },
		{ ":", Operator.Equals },
	};

	/// <summary>Initializes a new empty <see cref="FilterItem"/>.</summary>
	public FilterItem()
	{
	}

	/// <summary>Initializes a new <see cref="FilterItem"/> by parsing the supplied expression text.</summary>
	public FilterItem(string text)
	{
		var key = Operators.Keys.FirstOrDefault(k => text.Contains(k)) ?? throw new ArgumentException("No operator present.");
		var items = text.Split(new string[] { key }, StringSplitOptions.None);
		Field = items[0];
		Value = items[1];
		Operator = Operators[key];
		if (string.IsNullOrWhiteSpace(Field))
		{
			throw new ArgumentException("No field present");
		}
	}

	/// <summary>Gets or sets the AutoTask field name to filter on.</summary>
	public string Field { get; set; } = null!;
	/// <summary>Gets or sets the comparison operator.</summary>
	public Operator Operator { get; set; }
	/// <summary>Gets or sets the value to compare against.</summary>
	public string Value { get; set; } = null!;
}
