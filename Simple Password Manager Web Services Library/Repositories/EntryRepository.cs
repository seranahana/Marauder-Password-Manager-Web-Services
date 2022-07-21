using SimplePM.WebAPI.Library.Models;
using SimplePM.WebAPI.Library.Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace SimplePM.WebAPI.Library.Repositories
{
    public class EntryRepository : IEntryRepository
    {
        private readonly IConnectionMultiplexer _redisCache;
        private readonly IDatabase redisDB;
        private readonly AssetsContext _db;
        private const int dbNumber = (int)DBNumbers.Entries;

        public EntryRepository(AssetsContext db, IConnectionMultiplexer redisCache)
        {
            _db = db;
            _redisCache = redisCache;
            redisDB = _redisCache.GetDatabase(dbNumber);
        }

        public async Task<RepositoryEntry> CreateAsync(RepositoryEntry entry)
        {
            await _db.Entries.AddAsync(entry);
            int affected = await _db.SaveChangesAsync();
            if (affected == 1)
            {
                return entry;
            }
            else
            {
                return null;
            }
        }

        public async Task<bool> DeleteAsync(string id)
        {
            RepositoryEntry entry = _db.Entries.Find(id);
            _db.Entries.Remove(entry);
            int affected = await _db.SaveChangesAsync();
            if (affected == 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<Dictionary<string, RepositoryEntry>> RetrieveAllAsync(string accountID)
        {
            Dictionary<string, RepositoryEntry> entries = await redisDB.GetRecordAsync<Dictionary<string, RepositoryEntry>>(accountID);
            if (entries is null)
            {
                await SetCacheAsync(accountID);
                entries = await redisDB.GetRecordAsync<Dictionary<string, RepositoryEntry>>(accountID);
            }
            return entries;
        }

        public async Task SetCacheAsync(string accountID)
        {
            await redisDB.SetRecordAsync($"{accountID}_{DateTime.Now:yyyyMMdd_hhmmss}",
                _db.Entries.Where(a => a.UserID == accountID).ToDictionary(d => d.ID),
                TimeSpan.FromMinutes(30));
        }

        public async Task<RepositoryEntry> UpdateAsync(string id, RepositoryEntry entry)
        {
            _db.Entries.Update(entry);
            int affected = await _db.SaveChangesAsync();
            if (affected == 1)
            {
                return entry;
            }
            else
            {
                return null;
            }
        }
    }
}