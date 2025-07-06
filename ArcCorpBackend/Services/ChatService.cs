using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using ArcCorpBackend.Core.Users;
using ArcCorpBackend.Models;

namespace ArcCorpBackend.Services
{
    public static class ChatService
    {
        private static readonly ConcurrentDictionary<string, SynapTronChatService> _chatSessions = new();

        public static void InitiateChat(User user, string chatId)
        {
            var synaptron = new SynapTronChatService(user, chatId);
            _chatSessions[chatId] = synaptron;
            
        }

        public static async Task<UniversalIntentResponseModel> Query(string chatId, string userMessage)
        {
            if (!_chatSessions.TryGetValue(chatId, out var session))
            {
                return new UniversalIntentResponseModel
                {
                    Success = false,
                    Message = "Chat session not found. Please initiate the chat first."
                };
            }

            var synaptronResponse = await session.CategorizeIntent(userMessage);

            var responseModel = new UniversalIntentResponseModel
            {
                Success = synaptronResponse.Success,
                Category = synaptronResponse.Category,
                HasCards = synaptronResponse.HasCards,
                Message = synaptronResponse.Message,
                MissingContext = synaptronResponse.MissingContext,
                ReadyForAction = synaptronResponse.ReadyForAction,
                Cards = synaptronResponse.Cards.Select(card => new CardModel
                {
                    Title = card.Title,
                    Image = card.Image,
                    Price = card.Price,
                    Rating = card.Rating,
                    Location = card.Location,
                    Details = card.Details,
                    Action = new ActionModel
                    {
                        Type = card.Action.Type,
                        Url = card.Action.Url
                    }
                }).ToList()
            };

            return responseModel;
        }
    }
}
