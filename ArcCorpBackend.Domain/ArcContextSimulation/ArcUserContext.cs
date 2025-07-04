using ArcCorpBackend.Core.Users;
using MessagePack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ArcCorpBackend.Domain.ArcContextSimulation
{
    public class ArcUserContext
    {
        private const string UsersFileName = "ArcUserContext.dat";
        private const string UserDataFileName = "ArcUserDataContext.dat";

        public HashSet<User> Users { get; private set; }
        public HashSet<UserData> UserDataSet { get; private set; }

        public ArcUserContext()
        {
            Users = new HashSet<User>();
            UserDataSet = new HashSet<UserData>();

            Task.Run(async () =>
            {
                // Load Users
                if (File.Exists(UsersFileName))
                {
                    try
                    {
                        byte[] userBytes = await File.ReadAllBytesAsync(UsersFileName);
                        var loadedUsers = MessagePackSerializer.Deserialize<HashSet<User>>(userBytes);
                        Users = loadedUsers ?? new HashSet<User>();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error loading {UsersFileName}: {ex.Message}");
                        Users = new HashSet<User>();
                    }
                }

                // Load UserData
                if (File.Exists(UserDataFileName))
                {
                    try
                    {
                        byte[] userDataBytes = await File.ReadAllBytesAsync(UserDataFileName);
                        var loadedUserData = MessagePackSerializer.Deserialize<HashSet<UserData>>(userDataBytes);
                        UserDataSet = loadedUserData ?? new HashSet<UserData>();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error loading {UserDataFileName}: {ex.Message}");
                        UserDataSet = new HashSet<UserData>();
                    }
                }
            }).GetAwaiter().GetResult();
        }

        public void AddUser(User user)
        {
            if (!Users.Add(user))
                throw new InvalidOperationException("User with this email already exists in the context.");
        }

        public void RemoveUser(Guid userId)
        {
            Users.RemoveWhere(u => u.UserId == userId);
        }

        public void AddUserData(UserData userData)
        {
            if (!UserDataSet.Add(userData))
                throw new InvalidOperationException("This UserData already exists in the context.");
        }

        public void RemoveUserData(Guid userDataId)
        {
            UserDataSet.RemoveWhere(ud => ud.Id == userDataId);
        }

        public async Task SaveChangesAsync()
        {
            try
            {
                byte[] userBytes = MessagePackSerializer.Serialize(Users);
                await File.WriteAllBytesAsync(UsersFileName, userBytes);

                byte[] userDataBytes = MessagePackSerializer.Serialize(UserDataSet);
                await File.WriteAllBytesAsync(UserDataFileName, userDataBytes);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving users or user data: {ex.Message}");
            }
        }
    }
}
