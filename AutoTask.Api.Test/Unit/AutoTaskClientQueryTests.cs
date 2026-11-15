using AutoTask.Api.Config;
using AutoTask.Api.Filters;
using LogicMonitor.Integrator.Alerts.Exceptions;
using System.Text.Json.Nodes;
using Xunit;

namespace AutoTask.Api.Test.Unit;

/// <summary>Unit tests for the query building and result shaping in <see cref="AutoTaskClient"/>.</summary>
public class AutoTaskClientQueryTests
{
	private static object? InvokeStatic(string methodName, params object?[] arguments)
		=> PrivateAccess.Invoke(typeof(AutoTaskClient), null, methodName, arguments);

	/// <summary>Verifies the query XML for no filter, a plain filter and a user defined field filter.</summary>
	[Theory]
	[InlineData(null, "<field>id<expression op=\"greaterthan\">0</expression></field>")]
	[InlineData("id:5", "<condition operator=\"and\"><field>id<expression op=\"equals\">5</expression></field></condition>")]
	[InlineData("UDF Colour:red", "<condition operator=\"and\"><field udf=\"true\">Colour<expression op=\"equals\">red</expression></field></condition>")]
	public void GetQueryString_BuildsExpectedXml(string? filterText, string expected)
	{
		var filter = filterText is null ? null : new Filter(filterText);

		Assert.Equal(expected, InvokeStatic("GetQueryString", filter));
	}

	/// <summary>Verifies all properties are returned when no fields are requested.</summary>
	[Fact]
	public void GetFilteredObject_WithoutFields_ReturnsWholeEntity()
	{
		var result = (JsonObject)InvokeStatic("GetFilteredObject", new Account { AccountName = "Acme" }, new Filter())!;

		Assert.Equal("Acme", (string?)result["AccountName"]);
	}

	/// <summary>Verifies only the requested fields are returned, matched case-insensitively.</summary>
	[Fact]
	public void GetFilteredObject_WithFields_ReturnsOnlyThose()
	{
		var filter = new Filter { Fields = ["accountname"] };

		var result = (JsonObject)InvokeStatic("GetFilteredObject", new Account { AccountName = "Acme", id = 4 }, filter)!;

		var property = Assert.Single(result);
		Assert.Equal("AccountName", property.Key);
		Assert.Equal("Acme", (string?)property.Value);
	}

	/// <summary>Verifies a requested field that does not exist is rejected.</summary>
	[Fact]
	public void GetFilteredObject_WithUnknownField_Throws()
	{
		var filter = new Filter { Fields = ["NoSuchField"] };

		Assert.Throws<FilterFieldNotPresentException>(() => InvokeStatic("GetFilteredObject", new Account(), filter));
	}

	/// <summary>Verifies the fixed field names and that disposing an unused client is safe.</summary>
	[Fact]
	public void Client_ExposesFieldNamesAndDisposes()
	{
		var client = new AutoTaskClient(new AutoTaskConfiguration { Username = "u", Password = "p", IntegrationCode = "c" });

		Assert.Equal("id", client.PrimaryKey);
		Assert.Equal("TicketNumber", client.NumberField);
		Assert.Equal("Description", client.DescriptionKey);
		Assert.Equal("TicketNoteTitle", client.TicketNoteTitleKey);
		client.Dispose();
		client.Dispose();
	}
}
