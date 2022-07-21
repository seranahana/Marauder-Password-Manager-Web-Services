using SimplePM.WebAPI.Library.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimplePM.WebAPI.Library.Repositories
{
    public interface IEntryRepository
    {
        Task<RepositoryEntry> CreateAsync(RepositoryEntry entry);
        Task<Dictionary<string, RepositoryEntry>> RetrieveAllAsync(string ownerID);
        Task<RepositoryEntry> UpdateAsync(string id, RepositoryEntry entry);
        Task SetCacheAsync(string accountID);
        Task<bool> DeleteAsync(string id);
    }
}