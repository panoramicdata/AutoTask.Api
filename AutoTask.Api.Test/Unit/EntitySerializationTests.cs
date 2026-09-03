using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using Xunit;

namespace AutoTask.Api.Test.Unit;

/// <summary>
/// Unit tests for the System.Text.Json serialisation the client relies on when projecting entities
/// and when logging them on error. No AutoTask credentials required.
/// </summary>
public class EntitySerializationTests
{
	/// <summary>
	/// Verifies that every entity in the generated AutoTask surface can be serialised.
	/// System.Text.Json throws when two members map to the same JSON property name, which would
	/// only ever surface at runtime for the affected entity, so all of them are checked here.
	/// </summary>
	[Fact]
	public void AllEntities_Serialise()
	{
		var failures = new List<string>();

		foreach (var type in GetEntityTypes())
		{
			var entity = Activator.CreateInstance(type);
			try
			{
				Assert.NotNull(JsonSerializer.SerializeToNode(entity, type));
			}
			catch (Exception exception)
			{
				failures.Add($"{type.Name}: {exception.Message}");
			}
		}

		Assert.NotEmpty(GetEntityTypes());
		Assert.Empty(failures);
	}

	/// <summary>
	/// Verifies that an entity is serialised by its runtime type, including the members declared
	/// by the concrete entity rather than only those on <see cref="Entity"/>.
	/// </summary>
	[Fact]
	public void Entity_SerialisesRuntimeTypeMembers()
	{
		var ticket = new Ticket
		{
			id = 42,
			Title = "A ticket"
		};

		var node = JsonSerializer.SerializeToNode(ticket, ticket.GetType());

		var jsonObject = Assert.IsType<JsonObject>(node);
		Assert.Equal(42, (long?)jsonObject["id"]);
		Assert.Equal("A ticket", (string?)jsonObject["Title"]);
	}

	private static List<Type> GetEntityTypes()
		=> [.. typeof(Entity).Assembly
			.GetExportedTypes()
			.Where(type => typeof(Entity).IsAssignableFrom(type)
				&& !type.IsAbstract
				&& type.GetConstructor(BindingFlags.Public | BindingFlags.Instance, Type.EmptyTypes) is not null)];
}
