using ArcCorpBackend.Core.Messages;
using ArcCorpBackend.Core.Users;
using ArcCorpBackend.Domain.Interfaces;
using ArcCorpBackend.Domain.Repositories;
using ArcCorpBackend.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArcCorpBackend.Services
{
    public class UserService
    {
        private static readonly ConcurrentDictionary<string, string> VerificationCodes = new();
        private static readonly IUsersRepository UsersRepository = new UsersRepository();
        private UserService() { }

        /// <summary>
        /// Generates a 4-digit verification code for the given email,
        /// stores it in memory, and returns it as string.
        /// </summary>
        public static string NewUser(string email)
        {
            var random = new Random();
            var code = random.Next(1000, 10000).ToString();
            VerificationCodes[email] = code;
            return code;
        }

        /// <summary>
        /// Validates the given code against stored code for the email.
        /// </summary>
        public static async Task<bool> ValidateCode(string email, string code)
        {
            return await Task.Run(() =>
            {
                if (VerificationCodes.TryGetValue(email, out var storedCode))
                {
                    return storedCode == code;
                }
                return false;
            });
        }

        public static async Task AddUser(string email)
        {
            var user = new User(email);
            await UsersRepository.AddUserAsync(user);
            await UsersRepository.SaveChangesAsync();
        }

        public static async Task<string> FixUsers()
        {
            var sb = new StringBuilder();
            var users = await UsersRepository.GetUsersAsync();
            foreach (var user in users)
            {
                sb.AppendLine(user.Email + " chats: " + user.Chats.Count);
            }
            return sb.ToString();
        }

        public static string GetExistingCode(string email)
        {
            if (VerificationCodes.TryGetValue(email, out var existingCode))
            {
                return existingCode;
            }
            return null;
        }

        public static bool IsExistingUser(string email, out string code)
        {
            if (UsersRepository.UsersExists(email).Result)
            {
                code = NewUser(email);
                return true;
            }

            code = email;
            return false;
        }

        /// <summary>
        /// Returns UserModel for given userId (as string) with nested chats/messages.
        /// </summary>
        public static async Task<UserModel> GetUserModelById(string userId)
        {
            if (!Guid.TryParse(userId, out Guid parsedUserId))
            {
                throw new ArgumentException("Invalid userId format");
            }

            var user = await UsersRepository.GetUserByIdAsync(parsedUserId);
            if (user == null)
            {
                return null;
            }

            return new UserModel(user);
        }

        /// <summary>
        /// Returns List of ChatModel for given userId (as string).
        /// </summary>
        public static async Task<List<ChatModel>> GetChatsForUser(string userId)
        {
            if (!Guid.TryParse(userId, out Guid parsedUserId))
            {
                throw new ArgumentException("Invalid userId format");
            }

            var user = await UsersRepository.GetUserByIdAsync(parsedUserId);
            if (user == null)
            {
                return null;
            }

            var chatModels = new List<ChatModel>();
            if (user.Chats != null)
            {
                var x = 1;
                foreach (var chat in user.Chats)
                {
                    var name = "Chat " + x.ToString();
                    chatModels.Add(new ChatModel(chat, name));
                    x++;
                }
            }
            return chatModels;
        }

        /// <summary>
        /// Returns List of MessageModel for given chatId (as string).
        /// </summary>
        public static async Task<List<MessageModel>> GetMessagesForChat(string chatId)
        {
            if (!Guid.TryParse(chatId, out Guid parsedChatId))
            {
                throw new ArgumentException("Invalid chatId format");
            }

            var users = await UsersRepository.GetUsersAsync();
            foreach (var user in users)
            {
                var chat = user.Chats.FirstOrDefault(c => c.ChatId == parsedChatId);
                if (chat != null)
                {
                    var messageModels = new List<MessageModel>();
                    foreach (var message in chat.Messages)
                    {
                        messageModels.Add(new MessageModel(message));
                    }
                    return messageModels;
                }
            }

            return null; // chat not found
        }


        /// <summary>
        /// Creates a new Chat for the given userId, saves changes, and returns the ChatModel.
        /// </summary>
        public static async Task<ChatModel> New_Chat(string userId)
        {
            if (!Guid.TryParse(userId, out Guid parsedUserId))
            {
                throw new ArgumentException("Invalid userId format");
            }

            var user = await UsersRepository.GetUserByIdAsync(parsedUserId);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {userId} not found.");
            }

            var newChat = new Chat(user);
            user.Chats.Add(newChat);

            await UsersRepository.SaveChangesAsync();
            ChatService.InitiateChat(user, newChat.ChatId.ToString());
            return new ChatModel(newChat);
        }

    }
}
