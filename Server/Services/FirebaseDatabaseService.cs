using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Microsoft.Extensions.Configuration;
using SpiritWeb.Shared.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritWeb.Server.Services
{
    /// <summary>
    /// Service pour interagir avec Firestore, une base de données NoSQL de Firebase.
    /// Gère les opérations CRUD pour les données utilisateur.
    /// </summary>
    public class FirebaseDatabaseService
    {
        private readonly FirestoreDb _firestoreDb;

        /// <summary>
        /// Initialise une nouvelle instance de la classe <see cref="FirebaseDatabaseService"/>.
        /// </summary>
        /// <param name="configuration">Configuration de l'application pour accéder aux paramètres Firebase.</param>
        /// <exception cref="ArgumentNullException">Lancée si les paramètres Firebase sont manquants.</exception>
        /// <exception cref="FileNotFoundException">Lancée si le fichier de credentials est introuvable.</exception>
        /// <exception cref="Exception">Lancée si l'initialisation de Firestore échoue.</exception>
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

        /// <summary>
        /// Sauvegarde les données utilisateur dans Firestore.
        /// </summary>
        /// <param name="userId">Identifiant de l'utilisateur.</param>
        /// <param name="saveData">Les données utilisateur à sauvegarder.</param>
        /// <exception cref="Exception">Lancée si la sauvegarde des données échoue.</exception>
        public async Task SaveDataAsync(string userId, SaveData saveData)
        {
            DocumentReference docRef = _firestoreDb.Collection("users").Document(userId);
            await docRef.SetAsync(saveData);
        }

        /// <summary>
        /// Charge les données utilisateur depuis Firestore.
        /// </summary>
        /// <param name="userId">Identifiant de l'utilisateur dont les données doivent être chargées.</param>
        /// <returns>Les données utilisateur chargées, ou null si elles n'existent pas.</returns>
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

        /// <summary>
        /// Récupère tous les utilisateurs depuis Firestore.
        /// </summary>
        /// <returns>Une liste de tous les utilisateurs.</returns>
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
