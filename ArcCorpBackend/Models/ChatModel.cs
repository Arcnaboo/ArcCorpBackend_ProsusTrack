using ArcCorpBackend.Core.Messages;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ArcCorpBackend.Models
{
    public class ChatModel

    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("messages")]
        public List<MessageModel> Messages { get; set; }

        public ChatModel(Chat chat)
        {
            Id = chat.ChatId.ToString();
            CreatedAt = chat.CreatedAt;
            Messages = new List<MessageModel>();
            if (chat.Messages != null)
            {
                foreach (var message in chat.Messages)
                {
                    Messages.Add(new MessageModel(message));
                }
            }
        }
    }

    public class MessageModel
    {
        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("isUserMessage")]
        public bool IsUserMessage { get; set; }

        [JsonPropertyName("content")]
        public string Content { get; set; }

        public MessageModel(Message message)
        {
            CreatedAt = message.CreatedAt;
            IsUserMessage = message.IsUserMessage;
            Content = message.Content;
        }
    }
}
