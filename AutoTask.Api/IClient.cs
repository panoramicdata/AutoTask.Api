using System.Collections.Generic;
using System.Threading.Tasks;

namespace AutoTask.Api
{
	public interface IClient
	{
		Task<Entity> CreateAsync(Entity entity);
		System.Threading.Tasks.Task DeleteAsync(Entity entity);
		Task<IEnumerable<Entity>> GetAllAsync(string sXml);
		Task<GetFieldInfoResponse> GetFieldInfoAsync(string psObjectType);
		Task<string> GetWsdlVersion();
		Task<IEnumerable<Entity>> QueryAsync(string sXml);
		Task<Entity> UpdateAsync(Entity entity);
	}
}