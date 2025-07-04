using ArcCorpBackend.Core.Messages;
using MessagePack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ArcCorpBackend.Domain.ArcContextSimulation
{
    public class ArcChatContext
    {
        private const string ChatsFileName = "ArcChatContext.dat";
        private const string MessagesFileName = "ArcMessageContext.dat";

        public HashSet<Chat> Chats { get; private set; }
        public HashSet<Message> Messages { get; private set; }

        public ArcChatContext()
        {
            Chats = new HashSet<Chat>();
            Messages = new HashSet<Message>();

            Task.Run(async () =>
            {
                // Load Chats
                if (File.Exists(ChatsFileName))
                {
                    try
                    {
                        byte[] chatBytes = await File.ReadAllBytesAsync(ChatsFileName);
                        var loadedChats = MessagePackSerializer.Deserialize<HashSet<Chat>>(chatBytes);
                        Chats = loadedChats ?? new HashSet<Chat>();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error loading {ChatsFileName}: {ex.Message}");
                        Chats = new HashSet<Chat>();
                    }
                }

                // Load Messages
                if (File.Exists(MessagesFileName))
                {
                    try
                    {
                        byte[] messageBytes = await File.ReadAllBytesAsync(MessagesFileName);
                        var loadedMessages = MessagePackSerializer.Deserialize<HashSet<Message>>(messageBytes);
                        Messages = loadedMessages ?? new HashSet<Message>();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error loading {MessagesFileName}: {ex.Message}");
                        Messages = new HashSet<Message>();
                    }
                }
            }).GetAwaiter().GetResult();
        }

        public void AddChat(Chat chat)
        {
            if (!Chats.Add(chat))
                throw new InvalidOperationException("Chat already exists in the context.");
        }

        public void AddMessage(Message message)
        {
            if (!Messages.Add(message))
                throw new InvalidOperationException("Message already exists in the context.");
        }

        public void RemoveChat(Guid chatId)
        {
            Chats.RemoveWhere(c => c.ChatId == chatId);
        }

        public void RemoveMessage(Guid messageId)
        {
            Messages.RemoveWhere(m => m.MessageId == messageId);
        }

        public async Task SaveChangesAsync()
        {
            try
            {
                byte[] chatBytes = MessagePackSerializer.Serialize(Chats);
                await File.WriteAllBytesAsync(ChatsFileName, chatBytes);

                byte[] messageBytes = MessagePackSerializer.Serialize(Messages);
                await File.WriteAllBytesAsync(MessagesFileName, messageBytes);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving chats or messages: {ex.Message}");
            }
        }
    }
}
