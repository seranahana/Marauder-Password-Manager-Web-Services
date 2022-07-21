using Microsoft.EntityFrameworkCore.ChangeTracking;
using SimplePM.WebAPI.Library.Models;
using SimplePM.WebAPI.Library.Repositories.Models;
using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimplePM.WebAPI.Library.Repositories
{
    public class AccountRepository : IAccountRepository
    {
        private readonly IConnectionMultiplexer _redisCache;
        private readonly IDatabase redisDB;
        private readonly AssetsContext _db;
        private const int dbNumber = (int)DBNumbers.Accounts;

        public AccountRepository(AssetsContext db, IConnectionMultiplexer redisCache)
        {
            _db = db;
            _redisCache = redisCache;
            redisDB = redisCache.GetDatabase(dbNumber);
        }

        public async Task<UserData> CreateAsync(UserData userData)
        {
            EntityEntry<UserData> added = await _db.DistributionData.AddAsync(userData);
            int affected = await _db.SaveChangesAsync();
            if (affected == 1)
            {
                return userData;
            }
            else
            {
                throw new DBOperationException(nameof(_db.DistributionData));
            }
        }

        public async Task<UserData> RetrieveAsync(string login)
        {
            UserData userData = await redisDB.GetRecordAsync<UserData>(login);
            if (userData is null)
            {
                await SetCacheAsync(login);
                userData = await redisDB.GetRecordAsync<UserData>(login);
            }
            return userData;
        }

        private async Task SetCacheAsync(string login)
        {
            await redisDB.SetRecordAsync($"{login}_{DateTime.Now:yyyyMMdd_hhmmss}",
                _db.DistributionData.Find(login),
                TimeSpan.FromMinutes(1));
        }

        public async Task<UserData> UpdateAsync(string id, UserData userData)
        {
            _db.DistributionData.Update(userData);
            int affected = await _db.SaveChangesAsync();
            if (affected == 1)
            {
                return userData;
            }
            else
            {
                throw new DBOperationException(nameof(_db.DistributionData));
            }
        }

        public async Task<bool> DeleteAsync(string id)
        {
            UserData userData = _db.DistributionData.Find(id);
            _db.DistributionData.Remove(userData);
            int affected = await _db.SaveChangesAsync();
            if (affected == 1)
            {
                return true;
            }
            else
            {
                throw new DBOperationException(nameof(_db.DistributionData));
            }
        }
    }
}