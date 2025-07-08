using System.Collections.Concurrent;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using ArcCorpBackend.Core.Messages;
using ArcCorpBackend.Core.Users;
using ArcCorpBackend.Domain.Interfaces;
using ArcCorpBackend.Domain.Repositories;
using ArcCorpBackend.Models;

namespace ArcCorpBackend.Services
{
    public static class ChatService
    {
        private static readonly ConcurrentDictionary<string, SynapTronChatService> _chatSessions = new();
        private static readonly IUsersRepository usersRepository = new UsersRepository();

        public static void InitiateChat(User user, string chatId)
        {
            var synaptron = new SynapTronChatService(user, chatId);
            _chatSessions[chatId] = synaptron;
            
        }

        public static async Task<UniversalIntentResponseModel> Query(User user,string chatId, string userMessage)
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
            //Lets save messages
            var chat = user.Chats.Where(x => x.ChatId.ToString().Equals(chatId)).FirstOrDefault();
            var usermessage = new Message(chat, true, userMessage);
            var assistantmsg = new Message(chat, false, synaptronResponse.Message);
            chat.Messages.Add(usermessage);
            chat.Messages.Add(assistantmsg);
            //mybe cards so do same thing as ui 
            if (responseModel.Success && responseModel.HasCards)
            {
                foreach (var card in responseModel.Cards)
                {
                    /*Console.WriteLine(JsonSerializer.Serialize(card, new JsonSerializerOptions
                    {
                        WriteIndented = true
                    }));*/

                    // You can later display these as message-style entries too:



                    var Content = $"✈️ {card.Title} | {card.Location}\n{card.Details}\n{card.Price} | {card.Action?.Url}";
                    var messag = new Message(chat, false, Content);
                    chat.Messages.Add(messag);
                }
                await usersRepository.SaveChangesAsync();
            }


            return responseModel;
        }
    }
}
