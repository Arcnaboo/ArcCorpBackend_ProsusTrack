﻿using ArcCorpBackend.Core.Messages;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ArcCorpBackend.Models
{
    public class ChatModel

    {
        [JsonPropertyName("chatId")]
        public string Id { get; set; }
        
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonPropertyName("messages")]
        public List<MessageModel> Messages { get; set; }

        public ChatModel(Chat chat)
        {
            Name = chat.Name;
            Id = chat.ChatId.ToString();
            CreatedAt = chat.CreatedAt;
            Messages = new List<MessageModel>();



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
