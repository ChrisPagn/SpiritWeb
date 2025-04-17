using Google.Cloud.Firestore;
using SpiritWeb.Shared.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpiritWeb.Server.Services
{
    public class FirestoreVoteService
    {
        
        private readonly FirestoreDb _firestore;

        /// <summary>
        /// Initialise une nouvelle instance de la classe <see cref="FirestoreVoteService"/>.
        /// </summary>
        /// <param name="firestore"></param>
        public FirestoreVoteService(FirestoreDb firestore)
        {
            _firestore = firestore;
        }

        /// <summary>
        /// Vérifie si un utilisateur a déjà voté pour une suggestion
        /// </summary>
        public async Task<bool> HasUserVotedAsync(string suggestionId, string userId)
        {
            try
            {
                // Récupérer le document de vote associé à la suggestion
                DocumentReference voteRef = _firestore.Collection("votes").Document(suggestionId);
                DocumentSnapshot snapshot = await voteRef.GetSnapshotAsync();

                if (snapshot.Exists)
                {
                    // Si le document existe, on vérifie si l'userId est dans la liste des votes
                    if (snapshot.TryGetValue("UserIds", out List<string> userIds))
                    {
                        return userIds.Contains(userId);
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la vérification du vote: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Ajoute un vote pour une suggestion
        /// </summary>
        /// <param name="suggestionId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<bool> AddVoteAsync(string suggestionId, string userId)
        {
            try
            {
                DocumentReference voteRef = _firestore.Collection("votes").Document(suggestionId);
                DocumentSnapshot snapshot = await voteRef.GetSnapshotAsync();

                Vote voteModel;

                if (snapshot.Exists)
                {
                    voteModel = snapshot.ConvertTo<Vote>() ?? new Vote();

                    if (voteModel.UserIds == null)
                        voteModel.UserIds = new List<string>();

                    if (voteModel.UserIds.Contains(userId))
                        return false; // déjà voté

                    voteModel.UserIds.Add(userId);
                    voteModel.VotesCount = voteModel.UserIds.Count;
                }
                else
                {
                    voteModel = new Vote
                    {
                        SuggestionId = suggestionId,
                        UserIds = new List<string> { userId },
                        VotesCount = 1
                    };
                }

                // Sauvegarde du vote
                await voteRef.SetAsync(voteModel);

                // Mise à jour du compteur de votes dans la suggestion (incrémentation atomique)
                DocumentReference suggestionRef = _firestore.Collection("suggestions").Document(suggestionId);
                await suggestionRef.UpdateAsync("VotesCount", FieldValue.Increment(1));

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de l'ajout du vote: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Charge les votes des utilisateurs pour un ensemble de suggestions
        /// </summary>
        public async Task<Dictionary<string, List<string>>> GetVotesForSuggestionsAsync(List<string> suggestionIds)
        {
            var result = new Dictionary<string, List<string>>();

            try
            {
                foreach (var suggestionId in suggestionIds)
                {
                    DocumentReference voteRef = _firestore.Collection("votes").Document(suggestionId);
                    DocumentSnapshot snapshot = await voteRef.GetSnapshotAsync();

                    if (snapshot.Exists && snapshot.TryGetValue("UserIds", out List<string> userIds))
                    {
                        result[suggestionId] = userIds;
                    }
                    else
                    {
                        result[suggestionId] = new List<string>();
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors du chargement des votes: {ex.Message}");
                return result;
            }
        }
    }
}