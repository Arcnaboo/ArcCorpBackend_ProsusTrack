using ArcCorpBackend.Core.Messages;
using ArcCorpBackend.Core.Users;
using ArcCorpBackend.Domain.ArcContextSimulation;
using ArcCorpBackend.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArcCorpBackend.Domain.Repositories
{
    public class ChatsRepository : IChatsRepository
    {
        private readonly ArcUserContext arcUserContext = new();
        private readonly IUsersRepository usersRepository = new UsersRepository();

        public async Task<List<Chat>> GetChatsForUserAsync(Guid userId)
        {
            var user =  await usersRepository.GetUserByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException($"User with ID {userId} not found.");

            return user.Chats;
        }

        public async Task AddChatToUserAsync(Guid userId, Chat chat)
        {
            User? user = await usersRepository.GetUserByIdAsync(userId); // changed here
            if (user == null)
                throw new KeyNotFoundException($"User with ID {userId} not found.");

            user.Chats.Add(chat);
            
        }

        public async Task DeleteChatFromUserAsync(Guid userId, Guid chatId)
        {
            User? user = await usersRepository.GetUserByIdAsync(userId); // changed here
            if (user == null)
                throw new KeyNotFoundException($"User with ID {userId} not found.");

            Chat? chatToRemove = user.Chats.FirstOrDefault(c => c.ChatId == chatId);
            if (chatToRemove == null)
                throw new KeyNotFoundException($"Chat with ID {chatId} not found for user {userId}.");

            user.Chats.Remove(chatToRemove);
            
        }
        public async Task SaveChangesAsync()
        {
            await arcUserContext.SaveChangesAsync();
        }
    }
}
