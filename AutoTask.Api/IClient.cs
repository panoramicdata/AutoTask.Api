using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AutoTask.Api;

/// <summary>Defines the contract for an AutoTask API client.</summary>
public interface IClient
{
	/// <summary>Creates a new entity in AutoTask.</summary>
	Task<Entity> CreateAsync(Entity entity, CancellationToken cancellationToken = default);
	/// <summary>Deletes an entity from AutoTask.</summary>
	System.Threading.Tasks.Task DeleteAsync(Entity entity, CancellationToken cancellationToken = default);
	/// <summary>Returns all entities matching the supplied query XML, auto-paging beyond the 500-record limit.</summary>
	Task<IEnumerable<Entity>> GetAllAsync(string sXml, CancellationToken cancellationToken = default);
	/// <summary>Returns field information for the specified AutoTask object type.</summary>
	Task<GetFieldInfoResponse> GetFieldInfoAsync(string psObjectType, CancellationToken cancellationToken = default);
	/// <summary>Returns the WSDL version of the AutoTask web service.</summary>
	Task<string> GetWsdlVersion(CancellationToken cancellationToken = default);
	/// <summary>Executes a query against AutoTask and returns matching entities (up to 500 per page).</summary>
	Task<IEnumerable<Entity>> QueryAsync(string sXml, CancellationToken cancellationToken = default);
	/// <summary>Updates an existing entity in AutoTask.</summary>
	Task<Entity> UpdateAsync(Entity entity, CancellationToken cancellationToken = default);
}