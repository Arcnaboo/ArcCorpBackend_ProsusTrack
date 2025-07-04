using ArcCorpBackend.Core.Users;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArcCorpBackend.Domain.Interfaces
{
    public interface IUsersRepository
    {
        // User methods
        Task<List<User>> GetUsersAsync();
        Task<User?> GetUserByEmailAsync(string email);
        Task<User?> GetUserByIdAsync(Guid userId);
        Task AddUserAsync(User user);
        Task DeleteUserAsync(Guid userId);

        // UserData methods
        Task AddUserDataAsync(UserData userData);
        Task DeleteUserDataAsync(Guid userDataId);
        Task<UserData?> GetUserDataByIdAsync(Guid userDataId);
        Task<List<UserData>> GetUserDataForUserAsync(Guid userId);
        Task DeleteAllUserDataForUserAsync(Guid userId);
        Task<List<UserData>> GetAllUserDataAsync();

        // Persistence
        Task SaveChangesAsync();
    }
}
