using ArcCorpBackend.Core.Users;
using ArcCorpBackend.Domain.Interfaces;
using ArcCorpBackend.Domain.Repositories;
using System;
using System.Collections.Concurrent;
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
        /// <param name="email">User email</param>
        /// <returns>4-digit code as string</returns>
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
        /// <param name="email">User email</param>
        /// <param name="code">Code to validate</param>
        /// <returns>Task<bool> indicating validation result</returns>
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
            IUsersRepository usersRepository = new UsersRepository();
            var user = new User(email);
            await usersRepository.AddUserAsync(user);
        
        }

        public static string GetExistingCode(string email)
        {
            if (VerificationCodes.TryGetValue(email, out var existingCode))
            {
                return existingCode;
            }
            return null;
        }
        public static bool   IsExistingUser(string email, out string code)
        { 
            if  (UsersRepository.UsersExists(email).Result)
            {
                code = NewUser(email);
                return true;
            }

            code = email;
            return false;
        }



    }
}
