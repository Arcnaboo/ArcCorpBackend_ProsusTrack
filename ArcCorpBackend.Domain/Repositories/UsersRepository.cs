using ArcCorpBackend.Core.Users;
using ArcCorpBackend.Domain.ArcContextSimulation;
using ArcCorpBackend.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArcCorpBackend.Domain.Repositories
{
    public class UsersRepository : IUsersRepository
    {
        private readonly ArcUserContext _userContext = new();

        public UsersRepository() { }

        public async Task AddUserAsync(User user)
        {
            await Task.Run(() => _userContext.AddUser(user));
        }

        public async Task DeleteUserAsync(Guid userId)
        {
            await Task.Run(() => _userContext.RemoveUser(userId));
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await Task.Run(() =>
                _userContext.Users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase))
            );
        }

        public async Task<User?> GetUserByIdAsync(Guid userId)
        {
            return await Task.Run(() =>
                _userContext.Users.FirstOrDefault(u => u.UserId == userId)
            );
        }

        public async Task<List<User>> GetUsersAsync()
        {
            return await Task.Run(() => _userContext.Users.ToList());
        }

        public async Task AddUserDataAsync(UserData userData)
        {
            await Task.Run(() => _userContext.AddUserData(userData));
        }

        public async Task DeleteUserDataAsync(Guid userDataId)
        {
            await Task.Run(() => _userContext.RemoveUserData(userDataId));
        }

        public async Task<UserData?> GetUserDataByIdAsync(Guid userDataId)
        {
            return await Task.Run(() =>
                _userContext.UserDataSet.FirstOrDefault(ud => ud.Id == userDataId)
            );
        }

        public async Task<List<UserData>> GetUserDataForUserAsync(Guid userId)
        {
            return await Task.Run(() =>
                _userContext.UserDataSet.Where(ud => ud.User.UserId == userId).ToList()
            );
        }

        public async Task DeleteAllUserDataForUserAsync(Guid userId)
        {
            await Task.Run(() =>
                _userContext.UserDataSet.RemoveWhere(ud => ud.User.UserId == userId)
            );
        }

        public async Task<List<UserData>> GetAllUserDataAsync()
        {
            return await Task.Run(() => _userContext.UserDataSet.ToList());
        }

        public async Task SaveChangesAsync()
        {
            await _userContext.SaveChangesAsync();
        }
    }
}
