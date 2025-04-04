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

        public FirebaseDatabaseService(IConfiguration configuration)
        {
            try
            {
                // 1. Vérification des paramètres
                string? projectId = configuration["Firebase:ProjectId"];
                string? credentialPathConfig = configuration["Firebase:CredentialPath"];
                if (string.IsNullOrEmpty(projectId))
                    throw new ArgumentNullException("Firebase:ProjectId manquant dans appsettings.json");

                if (string.IsNullOrEmpty(credentialPathConfig))
                    throw new ArgumentNullException("Firebase:CredentialPath manquant dans appsettings.json");

                string credentialPath = Path.Combine(Directory.GetCurrentDirectory(), credentialPathConfig);

                if (!File.Exists(credentialPath))
                    throw new FileNotFoundException($"Fichier credentials introuvable: {credentialPath}");

                // 2. Initialisation Firebase
                if (FirebaseApp.DefaultInstance == null)
                {
                    FirebaseApp.Create(new AppOptions()
                    {
                        Credential = GoogleCredential.FromFile(credentialPath),
                        ProjectId = projectId
                    });
                }

                // 3. Initialisation Firestore avec vérification
                _firestoreDb = FirestoreDb.Create(projectId);

                if (_firestoreDb == null)
                    throw new Exception("Échec de la création de FirestoreDb");

                Console.WriteLine($"Firestore initialisé pour le projet: {projectId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERREUR Firebase: {ex.ToString()}");
                throw; // Relance pour arrêter l'application
            }
        }

        public async Task SaveDataAsync(string userId, SaveData saveData)
        {
            DocumentReference docRef = _firestoreDb.Collection("users").Document(userId);
            await docRef.SetAsync(saveData);
        }

        public async Task<SaveData?> LoadDataAsync(string userId)
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
