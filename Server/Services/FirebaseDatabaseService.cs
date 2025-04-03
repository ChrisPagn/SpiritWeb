using Firebase.Database;
using Firebase.Database.Query;
using SpiritWeb.Shared.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritWeb.Server.Services
{
    public class FirebaseDatabaseService
    {
        private readonly FirebaseClient _firebaseClient;

        public FirebaseDatabaseService(IConfiguration configuration)
        {
            string dbUrl = configuration["Firebase:DatabaseUrl"];
            _firebaseClient = new FirebaseClient(dbUrl);
        }

        public async Task SaveDataAsync(string userId, SaveData saveData)
        {
            await _firebaseClient
                .Child("users")
                .Child(userId)
                .PutAsync(saveData);
        }

        public async Task<SaveData> LoadDataAsync(string userId)
        {
            var result = await _firebaseClient
                .Child("users")
                .Child(userId)
                .OnceSingleAsync<SaveData>();

            return result;
        }

        public async Task<List<SaveData>> GetAllUsersAsync()
        {
            var result = await _firebaseClient
                .Child("users")
                .OnceAsync<SaveData>();

            return result.Select(item =>
            {
                var userData = item.Object;
                userData.UserId = item.Key;
                return userData;
            }).ToList();
        }
    }
}
