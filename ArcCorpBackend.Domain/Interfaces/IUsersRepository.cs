using ArcCorpBackend.Core.Messages;
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
        Task<bool> UsersExists(string email);
        User GetUserByEmailAsync(string email);
        Task<User?> GetUserByIdAsync(Guid userId);
        Task AddUserAsync(User user);
        Task DeleteUserAsync(Guid userId);

        Task<List<Chat>> GetChatsForUserAsync(Guid userId);
        Task<Chat?> GetChatByIdAsync(Guid chatId);
        Task AddChatAsync(Chat chat);
        Task DeleteChatAsync(Guid chatId);

        // Message methods
        Task<List<Message>> GetMessagesForChatAsync(Guid chatId);
        Task<Message?> GetMessageByIdAsync(Guid messageId);
        Task AddMessageAsync(Message message);
        Task DeleteMessageAsync(Guid messageId);
        // UserData methods
        Task AddUserDataAsync(UserData userData);
        Task DeleteUserDataAsync(Guid userDataId);
        Task<UserData?> GetUserDataByIdAsync(Guid userDataId);
        Task<List<UserData>> GetUserDataForUserAsync(Guid userId);
        List<UserData> GetUserDataForUser(Guid userId);
        Task DeleteAllUserDataForUserAsync(Guid userId);
        Task<List<UserData>> GetAllUserDataAsync();

        // Knowledge methods
        /*Task<Knowledge?> GetKnowledgeByUserIdAsync(Guid userId);
        Task AddKnowledgeAsync(Knowledge knowledge);
        */
        // Persistence
        Task SaveChangesAsync();
        void ReLoad();
    }
}
