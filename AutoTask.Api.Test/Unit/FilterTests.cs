using System.Collections.Generic;
using AutoTask.Api.Filters;
using Xunit;

namespace AutoTask.Api.Test.Unit;

/// <summary>Unit tests for <see cref="Filter"/>. No AutoTask credentials required.</summary>
public class FilterTests
{
	/// <summary>Verifies that a default filter has no items and no field projections.</summary>
	[Fact]
	public void Default_HasNoItemsOrFields()
	{
		var filter = new Filter();

		Assert.Empty(filter.Items);
		Assert.Empty(filter.Fields);
	}

	/// <summary>Verifies that a comma-delimited filter string produces one item per expression.</summary>
	[Fact]
	public void FromFilterString_ParsesEachItem()
	{
		var filter = new Filter("id>0,status!:5");

		Assert.Equal(2, filter.Items.Count);
		Assert.Equal("id", filter.Items[0].Field);
		Assert.Equal(Operator.GreaterThan, filter.Items[0].Operator);
		Assert.Equal("status", filter.Items[1].Field);
		Assert.Equal(Operator.NotEquals, filter.Items[1].Operator);
		Assert.Empty(filter.Fields);
	}

	/// <summary>Verifies that a comma-delimited field string produces one projected field per name.</summary>
	[Fact]
	public void FromFieldString_ParsesEachField()
	{
		var filter = new Filter("id>0", "id,Title");

		Assert.Equal(new List<string> { "id", "Title" }, filter.Fields);
	}

	/// <summary>Verifies that null filter and field strings produce empty collections rather than nulls.</summary>
	[Fact]
	public void FromNullStrings_ProducesEmptyCollections()
	{
		var filter = new Filter(null, null);

		Assert.Empty(filter.Items);
		Assert.Empty(filter.Fields);
	}

	/// <summary>Verifies that <see cref="Filter.ToString"/> reports the original filter and field strings.</summary>
	[Fact]
	public void ToString_ReportsOriginalStrings()
	{
		var filter = new Filter("id>0", "id,Title");

		Assert.Equal("Items=id>0;Fields=id,Title", filter.ToString());
	}
}
