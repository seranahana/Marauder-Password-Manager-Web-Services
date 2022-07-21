using SimplePM.WebAPI.Library.Models;
using SimplePM.WebAPI.Library.Repositories.Models;
using System.Threading.Tasks;

namespace SimplePM.WebAPI.Library.Repositories
{
    public interface IAccountRepository
    {
        Task<UserData> CreateAsync(UserData userData);
        Task<UserData> RetrieveAsync(string login);
        Task<UserData> UpdateAsync(string id, UserData userData);
        Task<bool> DeleteAsync(string id);
    }
}