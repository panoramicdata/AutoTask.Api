using System.Collections.Generic;
using System.Linq;

namespace AutoTask.Api.Filters;

/// <summary>Represents a query filter consisting of filter items and optional field projections.</summary>
public class Filter
{
	private readonly string? _itemString;
	private readonly string? _fieldString;

	/// <summary>Initializes a new empty <see cref="Filter"/>.</summary>
	public Filter()
	{
	}

	/// <summary>Initializes a new <see cref="Filter"/> from filter and field expression strings.</summary>
	public Filter(
		string? filterString = null,
		string? fieldString = null
		)
	{
		_itemString = filterString;
		_fieldString = fieldString;
		Items = filterString is null
			? new List<FilterItem>()
			: filterString.Split(',').Select(text => new FilterItem(text)).ToList();
		Fields = fieldString is null
			? new List<string>()
			: fieldString.Split(',').ToList();
	}

	/// <summary>Gets or sets the filter items used to build the query condition.</summary>
	public List<FilterItem> Items { get; set; } = new List<FilterItem>();

	/// <summary>Gets or sets the list of fields to include in the result. Empty means all fields.</summary>
	public List<string> Fields { get; set; } = new List<string>();

	/// <inheritdoc/>
	public override string ToString()
		=> $"Items={_itemString};Fields={_fieldString}";
}
