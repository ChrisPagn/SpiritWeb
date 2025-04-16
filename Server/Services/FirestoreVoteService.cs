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
        public async Task<bool> AddVoteAsync(string suggestionId, string userId)
        {
            try
            {
                DocumentReference voteRef = _firestore.Collection("votes").Document(suggestionId);

                if (await HasUserVotedAsync(suggestionId, userId))
                {
                    return false;
                }

                return await _firestore.RunTransactionAsync(async transaction =>
                {
                    DocumentSnapshot snapshot = await transaction.GetSnapshotAsync(voteRef);

                    if (snapshot.Exists)
                    {
                        // Correction ici: utilisation correcte de TryGetValue avec 'out'
                        snapshot.TryGetValue("UserIds", out List<string> userIds);
                        snapshot.TryGetValue("VotesCount", out int votesCount);

                        userIds ??= new List<string>();
                        userIds.Add(userId);
                        votesCount = userIds.Count;

                        transaction.Update(voteRef, new Dictionary<string, object>
                        {
                            ["UserIds"] = userIds,
                            ["VotesCount"] = votesCount,
                            ["LastUpdated"] = DateTime.Now
                        });
                    }
                    else
                    {
                        var newVote = new Dictionary<string, object>
                        {
                            ["SuggestionId"] = suggestionId,
                            ["UserIds"] = new List<string> { userId },
                            ["VotesCount"] = 1,
                            ["LastUpdated"] = DateTime.Now
                        };
                        transaction.Set(voteRef, newVote);
                    }

                    DocumentReference suggestionRef = _firestore.Collection("suggestions").Document(suggestionId);
                    DocumentSnapshot suggestionSnapshot = await transaction.GetSnapshotAsync(suggestionRef);

                    if (suggestionSnapshot.Exists)
                    {
                        // Correction ici aussi
                        if (suggestionSnapshot.TryGetValue("VotesCount", out int currentVotes))
                        {
                            transaction.Update(suggestionRef, "VotesCount", currentVotes + 1);
                        }
                        else
                        {
                            transaction.Update(suggestionRef, "VotesCount", 1);
                        }
                    }

                    return true;
                });
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