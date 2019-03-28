using System.Collections.Generic;
using System.Linq;

namespace AutoTask.Api.Extensions
{
	internal static class StringExtensions
	{
		public static string ToHumanReadableString(this IEnumerable<object> enumerable, string quoteWith = "'", string delimitWith = ", ", string delimitLastWith = " or ")
		{
			if (enumerable == null)
			{
				return string.Empty;
			}
			var list = enumerable.ToList();
			return list.Count == 0
				? string.Empty
				: list.Count == 1
					? $"{quoteWith}{list.Single()}{quoteWith}"
					: $"{string.Join(delimitWith ?? string.Empty, list.Take(list.Count - 1).Select(item => $"{quoteWith}{item}{quoteWith}"))}{delimitLastWith}{quoteWith}{list.Last()}{quoteWith}";
		}
	}
}
