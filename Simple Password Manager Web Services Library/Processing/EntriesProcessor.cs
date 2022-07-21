using SimplePM.WebAPI.Library.Models;
using SimplePM.WebAPI.Library.Repositories;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace SimplePM.WebAPI.Library.Processing
{
    public class EntriesProcessor : IEntriesProcessor
    {
        private readonly IEntryRepository _repository;
        private Dictionary<string, RepositoryEntry> entries;
        public EntriesProcessor(IEntryRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<Entry>> GetChecklistAsync(string accountID, [CallerArgumentExpression("accountID")] string paramName = null)
        {
            entries = await _repository.RetrieveAllAsync(accountID);
            if (entries is null)
            {
                throw new ArgumentException("", paramName);
            }
            List<Entry> checklist = new();
            foreach (var entry in entries.Values)
            {
                Entry checklistEntry = new(entry.ID, entry.Version);
                checklist.Add(checklistEntry);
            }
            return checklist;
        }

        public List<Entry> GetUpdatelistAsync(string accountID, string[] idList, [CallerArgumentExpression("accountID")] string paramName = null)
        {
            List<Entry> updatelist = new();
            foreach (string id in idList)
            {
                entries.TryGetValue(id, out RepositoryEntry repositoryEntry);
                if (repositoryEntry.UserID != accountID)
                {
                    throw new ArgumentException("", paramName);
                }
                if (repositoryEntry is null)
                {
                    continue;
                }
                Entry entry = new(repositoryEntry);
                updatelist.Add(entry);
            }
            return updatelist;
        }

        public async Task<bool> TryCommitChangesAsync(string accountID, List<Entry> updatesList)
        {
            if (updatesList.Count == 0)
            {
                return true;
            }
            bool isFullyComplete = true;
            foreach (var update in updatesList)
            {
                RepositoryEntry repositoryEntry = new(accountID, update);
                switch (update.SyncOperation)
                {
                    case EntrySyncOperation.Create:
                        RepositoryEntry createdEntry = await _repository.CreateAsync(repositoryEntry);
                        if (createdEntry is null)
                        {
                            isFullyComplete = false;
                        }
                        break;
                    case EntrySyncOperation.Update:
                        RepositoryEntry updatedEntry = await _repository.UpdateAsync(repositoryEntry.ID, repositoryEntry);
                        if (updatedEntry is null)
                        {
                            isFullyComplete = false;
                        }
                        break;
                    case EntrySyncOperation.Delete:
                        bool isDeletedSuccessfull = await _repository.DeleteAsync(repositoryEntry.ID);
                        if (isDeletedSuccessfull is false)
                        {
                            isFullyComplete = false;
                        }
                        break;
                }
            }
            await _repository.SetCacheAsync(accountID);
            return isFullyComplete;
        }
    }
}
