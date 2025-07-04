using ArcCorpBackend.Core.Messages;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArcCorpBackend.Domain.Interfaces
{
    public interface IChatsRepository
    {
        Task<List<Chat>> GetChatsAsync();
        Task<Chat?> GetChatByIdAsync(Guid chatId);
        Task AddChatAsync(Chat chat);
        Task DeleteChatAsync(Guid chatId);

        Task<List<Message>> GetMessagesForChatAsync(Guid chatId);
        Task AddMessageAsync(Message message);
        Task DeleteMessageAsync(Guid messageId);

        Task SaveChangesAsync();
    }
}

