using ArcCorpBackend.Core.Messages;
using ArcCorpBackend.Domain.ArcContextSimulation;
using ArcCorpBackend.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArcCorpBackend.Domain.Repositories
{
    internal class ChatsRepository : IChatsRepository
    {
        private readonly ArcChatContext _chatContext = new();

        public ChatsRepository() { }

        public async Task AddChatAsync(Chat chat)
        {
            await Task.Run(() => _chatContext.AddChat(chat));
        }

        public async Task AddMessageAsync(Message message)
        {
            await Task.Run(() => _chatContext.AddMessage(message));
        }

        public async Task DeleteChatAsync(Guid chatId)
        {
            await Task.Run(() => _chatContext.RemoveChat(chatId));
        }

        public async Task DeleteMessageAsync(Guid messageId)
        {
            await Task.Run(() => _chatContext.RemoveMessage(messageId));
        }

        public async Task<Chat?> GetChatByIdAsync(Guid chatId)
        {
            return await Task.Run(() =>
                _chatContext.Chats.FirstOrDefault(c => c.ChatId == chatId)
            );
        }

        public async Task<List<Chat>> GetChatsAsync()
        {
            return await Task.Run(() => _chatContext.Chats.ToList());
        }

        public async Task<List<Message>> GetMessagesForChatAsync(Guid chatId)
        {
            return await Task.Run(() =>
                _chatContext.Messages.Where(m => m.Chat.ChatId == chatId).ToList()
            );
        }

        public async Task SaveChangesAsync()
        {
            await _chatContext.SaveChangesAsync();
        }
    }
}
