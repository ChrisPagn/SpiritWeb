using Google.Cloud.Firestore;
using SpiritWeb.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritWeb.Server.Services
{
    /// <summary>
    /// Service pour gérer les enquêtes et suggestions dans Firestore
    /// </summary>
    public class FirestoreSurveyService
    {
        private readonly FirestoreDb _firestoreDb;
        private const string COLLECTION_NAME = "suggestions";

        /// <summary>
        /// Initialise une nouvelle instance de la classe <see cref="FirestoreSurveyService"/>.
        /// </summary>
        /// <param name="firestoreDb">Base de données Firestore.</param>
        public FirestoreSurveyService(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }

        /// <summary>
        /// Enregistre une nouvelle enquête/suggestion dans Firestore
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<string> SaveSurveyAsync(SurveyModel model)
        {
            //// Normalisation des données
            //model.Votes = model.Votes ?? new List<string>(); // Garantit que Votes n'est jamais null

            // Si le DisplayName est null, essayez de le récupérer
            if (string.IsNullOrEmpty(model.UserDisplayName) && !string.IsNullOrEmpty(model.UserId))
            {
                DocumentReference userRef = _firestoreDb.Collection("users").Document(model.UserId);
                DocumentSnapshot userSnapshot = await userRef.GetSnapshotAsync();

                if (userSnapshot.Exists)
                {
                    var userData = userSnapshot.ToDictionary();
                    if (userData.ContainsKey("DisplayName"))
                    {
                        model.UserDisplayName = userData["DisplayName"].ToString();
                    }
                }
            }

            CollectionReference suggestionsCollection = _firestoreDb.Collection(COLLECTION_NAME);
            DocumentReference docRef = await suggestionsCollection.AddAsync(model);
            return docRef.Id;
        }

        /// <summary>
        /// Récupère toutes les suggestions d'optimisation
        /// </summary>
        /// <returns>Liste des suggestions d'optimisation</returns>
        public async Task<List<SurveyModel>> GetAllSuggestionsAsync()
        {
            CollectionReference suggestionsCollection = _firestoreDb.Collection(COLLECTION_NAME);
            QuerySnapshot snapshot = await suggestionsCollection.GetSnapshotAsync();
            return snapshot.Documents.Select(doc => doc.ConvertTo<SurveyModel>()).ToList();
        }

    }
}
