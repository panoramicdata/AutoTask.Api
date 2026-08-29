using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AutoTask.Api;

/// <summary>Defines the contract for an AutoTask API client.</summary>
public interface IClient
{
	/// <summary>Creates a new entity in AutoTask.</summary>
	/// <param name="entity">The entity to create.</param>
	/// <returns>The created entity.</returns>
	Task<Entity> CreateAsync(Entity entity);

	/// <summary>Creates a new entity in AutoTask.</summary>
	/// <param name="entity">The entity to create.</param>
	/// <param name="cancellationToken">A token to cancel the operation.</param>
	/// <returns>The created entity.</returns>
	Task<Entity> CreateAsync(Entity entity, CancellationToken cancellationToken);

	/// <summary>Deletes an entity from AutoTask.</summary>
	/// <param name="entity">The entity to delete.</param>
	System.Threading.Tasks.Task DeleteAsync(Entity entity);

	/// <summary>Deletes an entity from AutoTask.</summary>
	/// <param name="entity">The entity to delete.</param>
	/// <param name="cancellationToken">A token to cancel the operation.</param>
	System.Threading.Tasks.Task DeleteAsync(Entity entity, CancellationToken cancellationToken);

	/// <summary>Returns all entities matching the supplied query XML, auto-paging beyond the 500-record limit.</summary>
	/// <param name="sXml">The query XML.</param>
	/// <returns>The matching entities.</returns>
	Task<IEnumerable<Entity>> GetAllAsync(string sXml);

	/// <summary>Returns all entities matching the supplied query XML, auto-paging beyond the 500-record limit.</summary>
	/// <param name="sXml">The query XML.</param>
	/// <param name="cancellationToken">A token to cancel the operation.</param>
	/// <returns>The matching entities.</returns>
	Task<IEnumerable<Entity>> GetAllAsync(string sXml, CancellationToken cancellationToken);

	/// <summary>Returns field information for the specified AutoTask object type.</summary>
	/// <param name="psObjectType">The AutoTask object type name.</param>
	/// <returns>The field metadata.</returns>
	Task<GetFieldInfoResponse> GetFieldInfoAsync(string psObjectType);

	/// <summary>Returns field information for the specified AutoTask object type.</summary>
	/// <param name="psObjectType">The AutoTask object type name.</param>
	/// <param name="cancellationToken">A token to cancel the operation.</param>
	/// <returns>The field metadata.</returns>
	Task<GetFieldInfoResponse> GetFieldInfoAsync(string psObjectType, CancellationToken cancellationToken);

	/// <summary>Returns the WSDL version of the AutoTask web service.</summary>
	/// <returns>The WSDL version.</returns>
	Task<string> GetWsdlVersion();

	/// <summary>Returns the WSDL version of the AutoTask web service.</summary>
	/// <param name="cancellationToken">A token to cancel the operation.</param>
	/// <returns>The WSDL version.</returns>
	Task<string> GetWsdlVersion(CancellationToken cancellationToken);

	/// <summary>Executes a query against AutoTask and returns matching entities (up to 500 per page).</summary>
	/// <param name="sXml">The query XML.</param>
	/// <returns>The matching entities.</returns>
	Task<IEnumerable<Entity>> QueryAsync(string sXml);

	/// <summary>Executes a query against AutoTask and returns matching entities (up to 500 per page).</summary>
	/// <param name="sXml">The query XML.</param>
	/// <param name="cancellationToken">A token to cancel the operation.</param>
	/// <returns>The matching entities.</returns>
	Task<IEnumerable<Entity>> QueryAsync(string sXml, CancellationToken cancellationToken);

	/// <summary>Updates an existing entity in AutoTask.</summary>
	/// <param name="entity">The entity to update.</param>
	/// <returns>The updated entity.</returns>
	Task<Entity> UpdateAsync(Entity entity);

	/// <summary>Updates an existing entity in AutoTask.</summary>
	/// <param name="entity">The entity to update.</param>
	/// <param name="cancellationToken">A token to cancel the operation.</param>
	/// <returns>The updated entity.</returns>
	Task<Entity> UpdateAsync(Entity entity, CancellationToken cancellationToken);
}
