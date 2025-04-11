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
        private const string VOTES_SUBCOLLECTION = "votes";

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
        /// <param name="model">Le modèle d'enquête à enregistrer</param>
        /// <returns>L'identifiant de l'enquête créée</returns>
        //public async Task<string> SaveSurveyAsync(SurveyModel model)
        //{
        //    CollectionReference suggestionsCollection = _firestoreDb.Collection(COLLECTION_NAME);
        //    DocumentReference docRef = await suggestionsCollection.AddAsync(model);
        //    return docRef.Id;
        //}

        // Dans FirestoreSurveyService.cs
        public async Task<string> SaveSurveyAsync(SurveyModel model)
        {
            // Si le DisplayName est null, essayez de le récupérer
            if (string.IsNullOrEmpty(model.UserDisplayName) && !string.IsNullOrEmpty(model.UserId))
            {
                DocumentReference userRef = _firestoreDb.Collection("users").Document(model.UserId);
                DocumentSnapshot userSnapshot = await userRef.GetSnapshotAsync();

                if (userSnapshot.Exists)
                {
                    // Récupérer le DisplayName du document utilisateur
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

        /// <summary>
        /// Vérifie si un utilisateur a déjà voté pour une suggestion
        /// </summary>
        /// <param name="suggestionId">Identifiant de la suggestion</param>
        /// <param name="userId">Identifiant de l'utilisateur</param>
        /// <returns>True si l'utilisateur a déjà voté</returns>
        public async Task<bool> HasUserVotedAsync(string suggestionId, string userId)
        {
            DocumentReference suggestionDocRef = _firestoreDb.Collection(COLLECTION_NAME).Document(suggestionId);
            CollectionReference votesCollection = suggestionDocRef.Collection(VOTES_SUBCOLLECTION);
            QuerySnapshot snapshot = await votesCollection.WhereEqualTo("UserId", userId).GetSnapshotAsync();
            return snapshot.Documents.Any();
        }

        /// <summary>
        /// Ajoute un vote pour une suggestion spécifique
        /// </summary>
        /// <param name="suggestionId">Identifiant de la suggestion</param>
        /// <param name="userId">Identifiant de l'utilisateur qui vote</param>
        /// <returns>True si le vote a été enregistré avec succès</returns>
        public async Task AddVoteAsync(string suggestionId, string userId)
        {
            DocumentReference suggestionDocRef = _firestoreDb.Collection(COLLECTION_NAME).Document(suggestionId);
            CollectionReference votesCollection = suggestionDocRef.Collection(VOTES_SUBCOLLECTION);
            DocumentReference voteDocRef = await votesCollection.AddAsync(new { UserId = userId, Timestamp = FieldValue.ServerTimestamp });
        }
    }
}
