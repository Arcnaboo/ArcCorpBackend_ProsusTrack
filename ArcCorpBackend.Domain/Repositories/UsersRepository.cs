using ArcCorpBackend.Core.Messages;
using ArcCorpBackend.Core.Users;
using ArcCorpBackend.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArcCorpBackend.Domain.Repositories
{
    public class UsersRepository : IUsersRepository
    {
        private readonly ArcDbContext _dbContext;

        public UsersRepository()
        {
            _dbContext = new ArcDbContext();
            _dbContext.Database.EnsureCreated();
        }

        // Chat methods implementation
        public async Task<List<Chat>> GetChatsForUserAsync(Guid userId)
        {
            return await _dbContext.Chats
                .Where(c => c.UserId == userId)
                .ToListAsync();
        }

        public async Task<Chat?> GetChatByIdAsync(Guid chatId)
        {
            return await _dbContext.Chats.FindAsync(chatId);
        }

        public async Task AddChatAsync(Chat chat)
        {
            _dbContext.Chats.Add(chat);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteChatAsync(Guid chatId)
        {
            var chat = await _dbContext.Chats.FindAsync(chatId);
            if (chat != null)
            {
                // First delete all messages in this chat
                var messages = await _dbContext.Messages
                    .Where(m => m.ChatId == chatId)
                    .ToListAsync();

                _dbContext.Messages.RemoveRange(messages);
                _dbContext.Chats.Remove(chat);
                await _dbContext.SaveChangesAsync();
            }
        }

        // Message methods implementation
        public async Task<List<Message>> GetMessagesForChatAsync(Guid chatId)
        {
            return await _dbContext.Messages
                .Where(m => m.ChatId == chatId)
                .OrderBy(m => m.CreatedAt)
                .ToListAsync();
        }

        public async Task<Message?> GetMessageByIdAsync(Guid messageId)
        {
            return await _dbContext.Messages.FindAsync(messageId);
        }

        public async Task AddMessageAsync(Message message)
        {
            _dbContext.Messages.Add(message);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteMessageAsync(Guid messageId)
        {
            var message = await _dbContext.Messages.FindAsync(messageId);
            if (message != null)
            {
                _dbContext.Messages.Remove(message);
                await _dbContext.SaveChangesAsync();
            }
        }
        public async Task AddUserAsync(User user)
        {
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteUserAsync(Guid userId)
        {
            var user = await _dbContext.Users.FindAsync(userId);
            if (user != null)
            {
                _dbContext.Users.Remove(user);
                await _dbContext.SaveChangesAsync();
            }
        }

        public User? GetUserByEmailAsync(string email)
        {
            return _dbContext.Users.FirstOrDefault(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task<User?> GetUserByIdAsync(Guid userId)
        {
            return await _dbContext.Users.FindAsync(userId);
        }

        public async Task<List<User>> GetUsersAsync()
        {
            return await _dbContext.Users.ToListAsync();
        }

        public async Task AddUserDataAsync(UserData userData)
        {
            _dbContext.UserDatas.Add(userData);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteUserDataAsync(Guid userDataId)
        {
            var data = await _dbContext.UserDatas.FindAsync(userDataId);
            if (data != null)
            {
                _dbContext.UserDatas.Remove(data);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<UserData?> GetUserDataByIdAsync(Guid userDataId)
        {
            return await _dbContext.UserDatas.FindAsync(userDataId);
        }

        public async Task<List<UserData>> GetUserDataForUserAsync(Guid userId)
        {
            return await _dbContext.UserDatas
                .Where(ud => ud.UserId == userId)
                .ToListAsync();
        }

        public async Task DeleteAllUserDataForUserAsync(Guid userId)
        {
            var all = await _dbContext.UserDatas
                .Where(ud => ud.UserId == userId)
                .ToListAsync();

            _dbContext.UserDatas.RemoveRange(all);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<UserData>> GetAllUserDataAsync()
        {
            return await _dbContext.UserDatas.ToListAsync();
        }

        public async Task SaveChangesAsync()
        {
            int retryCount = 3; // Number of retry attempts
            bool saveFailed;

            do
            {
                saveFailed = false;
                try
                {
                    await _dbContext.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    saveFailed = true;

                    if (retryCount-- <= 0)
                        throw; // Re-throw if we've exhausted retries

                    // Refresh all affected entries
                    foreach (var entry in ex.Entries)
                    {
                        await entry.ReloadAsync();

                        // For updates, you might want to preserve some changes
                        if (entry.State == EntityState.Modified)
                        {
                            var currentValues = entry.CurrentValues;
                            var databaseValues = entry.GetDatabaseValues();

                            // Here you can implement your merge strategy
                            foreach (var property in currentValues.Properties)
                            {
                                var currentValue = currentValues[property];
                                var databaseValue = databaseValues[property];

                                // Example: Keep current value if it was modified
                                if (!Equals(currentValue, entry.OriginalValues[property]))
                                {
                                    databaseValues[property] = currentValue;
                                }
                            }

                            entry.OriginalValues.SetValues(databaseValues);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log other exceptions (DbUpdateException, etc.)
                    // Consider using a logging framework here
                    Console.WriteLine($"Save changes failed: {ex.Message}");
                    throw;
                }
            } while (saveFailed && retryCount > 0);
        }

        async Task<bool> IUsersRepository.UsersExists(string email)
        {
            return await _dbContext.Users.AnyAsync(u => u.Email.ToLower() == email.ToLower());
        }

      /*  List<UserData> IUsersRepository.GetUserDataForUser(Guid userId)
        {
            return _dbContext.UserDataSet.Where(ud => ud.User.UserId == userId).ToList();
        }

        async Task<Knowledge?> IUsersRepository.GetKnowledgeByUserIdAsync(Guid userId)
        {
            return await _dbContext.KnowledgeSet
                .FirstOrDefaultAsync(k => k.User.UserId == userId);
        }

        async Task IUsersRepository.AddKnowledgeAsync(Knowledge knowledge)
        {
            var existing = await _dbContext.KnowledgeSet
                .Where(k => k.User.UserId == knowledge.User.UserId)
                .ToListAsync();

            _dbContext.KnowledgeSet.RemoveRange(existing);
            _dbContext.KnowledgeSet.Add(knowledge);
            await _dbContext.SaveChangesAsync();
        }*/

        void IUsersRepository.ReLoad()
        {
            // No-op in EF Core. If needed, can be refactored to requery data.
        }

        List<UserData> IUsersRepository.GetUserDataForUser(Guid userId)
        {
            var datas = _dbContext.UserDatas.Where(x => x.UserId == userId).ToList();
            return datas;
        }
    }
}
