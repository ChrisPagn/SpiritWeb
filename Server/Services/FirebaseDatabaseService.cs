using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
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

        // Dans FirebaseDatabaseService.cs - méthode constructeur modifiée
        public FirebaseDatabaseService(IConfiguration configuration)
        {
            var projectId = configuration["Firebase:ProjectId"];
            var credentialPath = Path.Combine(Directory.GetCurrentDirectory(),
                                            configuration["Firebase:CredentialPath"]);

            // Solution robuste avec vérifications
            if (!File.Exists(credentialPath))
            {
                throw new FileNotFoundException($"Fichier Firebase credentials introuvable: {credentialPath}");
            }

            FirebaseApp.Create(new AppOptions()
            {
                Credential = GoogleCredential.FromFile(credentialPath),
                ProjectId = projectId
            });

            _firestoreDb = FirestoreDb.Create(projectId);
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
