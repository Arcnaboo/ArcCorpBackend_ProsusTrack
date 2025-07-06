using ArcCorpBackend.Core.Messages;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArcCorpBackend.Domain.Interfaces
{
    public interface IChatsRepository
    {
        /// <summary>
        /// Retrieves all chats for the specified user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>List of chats belonging to the user, including their messages.</returns>
        Task<List<Chat>> GetChatsForUserAsync(Guid userId);

        /// <summary>
        /// Adds a new chat to the specified user.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="chat">The chat to add.</param>
        Task AddChatToUserAsync(Guid userId, Chat chat);

        /// <summary>
        /// Deletes a chat from the specified user.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <param name="chatId">The chat ID to delete.</param>
        Task DeleteChatFromUserAsync(Guid userId, Guid chatId);

        /// <summary>
        /// Persists changes to the pseudo-database.
        /// </summary>
        Task SaveChangesAsync();
    }
}
