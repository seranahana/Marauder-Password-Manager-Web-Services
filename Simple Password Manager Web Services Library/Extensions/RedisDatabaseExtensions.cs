using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimplePM.WebAPI.Library
{
    internal static class RedisDatabaseExtensions
    {
        internal static async Task<T> GetRecordAsync<T>(this IDatabase database, string recordId)
        {
            string jsonData = await database.StringGetAsync(recordId);

            if (jsonData is null)
            {
                return default;
            }

            return JsonConvert.DeserializeObject<T>(jsonData);
        }

        internal static async Task SetRecordAsync<T>(this IDatabase database, string recordId, T data, TimeSpan? expireTime)
        {
            var jsonData = JsonConvert.SerializeObject(data);
            await database.StringSetAsync(recordId, jsonData, expireTime ?? TimeSpan.FromSeconds(10));
        }
    }
}
