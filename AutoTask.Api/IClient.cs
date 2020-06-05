using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AutoTask.Api
{
	public interface IClient
	{
		Task<Entity> CreateAsync(Entity entity, CancellationToken cancellationToken = default);
		System.Threading.Tasks.Task DeleteAsync(Entity entity, CancellationToken cancellationToken = default);
		Task<IEnumerable<Entity>> GetAllAsync(string sXml, CancellationToken cancellationToken = default);
		Task<GetFieldInfoResponse> GetFieldInfoAsync(string psObjectType, CancellationToken cancellationToken = default);
		Task<string> GetWsdlVersion(CancellationToken cancellationToken = default);
		Task<IEnumerable<Entity>> QueryAsync(string sXml, CancellationToken cancellationToken = default);
		Task<Entity> UpdateAsync(Entity entity, CancellationToken cancellationToken = default);
	}
}