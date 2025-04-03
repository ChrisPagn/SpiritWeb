using Google.Cloud.Firestore;
using Microsoft.Extensions.Configuration;
using SpiritWeb.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritWeb.Server.Services
{
    public class FirebaseDatabaseService
    {
        private readonly FirestoreDb _firestoreDb;

        public FirebaseDatabaseService(IConfiguration configuration)
        {
            string projectId = configuration["Firebase:ProjectId"];
            if (string.IsNullOrEmpty(projectId))
            {
                throw new Exception("Firebase ProjectId is missing in appsettings.json");
            }

            _firestoreDb = FirestoreDb.Create(projectId);
            Console.WriteLine($"Connected to Firestore project: {projectId}");
        }

        public async Task SaveDataAsync(string userId, SaveData saveData)
        {
            DocumentReference docRef = _firestoreDb.Collection("users").Document(userId);
            await docRef.SetAsync(saveData);
        }

        public async Task<SaveData> LoadDataAsync(string userId)
        {
            DocumentReference docRef = _firestoreDb.Collection("users").Document(userId);
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

            if (snapshot.Exists)
            {
                return snapshot.ConvertTo<SaveData>();
            }

            return null;
        }

        public async Task<List<SaveData>> GetAllUsersAsync()
        {
            CollectionReference usersRef = _firestoreDb.Collection("users");
            QuerySnapshot snapshot = await usersRef.GetSnapshotAsync();

            return snapshot.Documents.Select(doc =>
            {
                var data = doc.ConvertTo<SaveData>();
                data.UserId = doc.Id;
                return data;
            }).ToList();
        }
    }
}
