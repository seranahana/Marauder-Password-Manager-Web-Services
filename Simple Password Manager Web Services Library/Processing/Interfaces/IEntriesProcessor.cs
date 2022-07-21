using SimplePM.WebAPI.Library.Models;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace SimplePM.WebAPI.Library.Processing
{
    public interface IEntriesProcessor
    {
        Task<List<Entry>> GetChecklistAsync(string accountID, [CallerArgumentExpression("accountID")] string paramName = null);
        List<Entry> GetUpdatelistAsync(string accountID, string[] idList, [CallerArgumentExpression("accountID")] string paramName = null);
        Task<bool> TryCommitChangesAsync(string ownerID, List<Entry> updatesList);
    }
}