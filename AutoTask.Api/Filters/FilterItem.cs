using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoTask.Api.Filters
{
	public class FilterItem
	{
		private static readonly Dictionary<string, Operator> Operators = new Dictionary<string, Operator>
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

		public FilterItem()
		{
		}

		public FilterItem(string text)
		{
			var key = Operators.Keys.FirstOrDefault(k => text.Contains(k));
			if (key == null)
			{
				throw new ArgumentException("No operator present.");
			}
			var items = text.Split(new string[] { key }, StringSplitOptions.None);
			Field = items[0];
			Value = items[1];
			Operator = Operators[key];
			if (string.IsNullOrWhiteSpace(Field))
			{
				throw new ArgumentException("No field present");
			}
		}

		public string Field { get; set; } = null!;
		public Operator Operator { get; set; }
		public string Value { get; set; } = null!;
	}
}
