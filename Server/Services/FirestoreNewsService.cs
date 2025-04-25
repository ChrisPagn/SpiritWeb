using Google.Cloud.Firestore;
using SpiritWeb.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritWeb.Server.Services
{
    /// <summary>
    /// Service pour gérer les actualités dans Firestore
    /// </summary>
    public class FirestoreNewsService
    {
        private readonly FirestoreDb _firestoreDb;
        private const string COLLECTION_NAME = "news";

        /// <summary>
        /// Initialise une nouvelle instance de la classe <see cref="FirestoreNewsService"/>.
        /// </summary>
        /// <param name="firestoreDb">Base de données Firestore.</param>
        public FirestoreNewsService(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }

        /// <summary>
        /// Récupère toutes les actualités triées par date de publication (descendant)
        /// </summary>
        /// <returns>Liste triée des actualités</returns>
        public async Task<List<NewsModel>> GetAllNewsAsync()
        {
            try
            {
                CollectionReference newsCollection = _firestoreDb.Collection(COLLECTION_NAME);

                // Récupérer et trier les actualités : les épinglées d'abord, puis par date de publication
                QuerySnapshot snapshot = await newsCollection.OrderByDescending("IsPinned")
                                                            .OrderByDescending("PublishDate")
                                                            .GetSnapshotAsync();

                var newsList = new List<NewsModel>();

                foreach (DocumentSnapshot document in snapshot.Documents)
                {
                    var news = document.ConvertTo<NewsModel>();
                    news.Id = document.Id; // Assigner l'ID du document
                    newsList.Add(news);
                }

                return newsList;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la récupération des actualités: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Récupère une actualité par son identifiant
        /// </summary>
        /// <param name="newsId">Identifiant de l'actualité</param>
        /// <returns>L'actualité demandée ou null si non trouvée</returns>
        public async Task<NewsModel> GetNewsByIdAsync(string newsId)
        {
            try
            {
                DocumentReference docRef = _firestoreDb.Collection(COLLECTION_NAME).Document(newsId);
                DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

                if (snapshot.Exists)
                {
                    var news = snapshot.ConvertTo<NewsModel>();
                    news.Id = snapshot.Id;
                    return news;
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la récupération de l'actualité: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Ajoute une nouvelle actualité
        /// </summary>
        /// <param name="newsModel">Modèle de l'actualité à ajouter</param>
        /// <returns>L'identifiant de l'actualité créée</returns>
        public async Task<string> AddNewsAsync(NewsModel newsModel)
        {
            try
            {
                CollectionReference newsCollection = _firestoreDb.Collection(COLLECTION_NAME);
                DocumentReference docRef = await newsCollection.AddAsync(newsModel);
                return docRef.Id;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de l'ajout de l'actualité: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Met à jour une actualité existante
        /// </summary>
        /// <param name="newsId">Identifiant de l'actualité</param>
        /// <param name="newsModel">Modèle de l'actualité avec les modifications</param>
        /// <returns>True si la mise à jour a réussi</returns>
        public async Task<bool> UpdateNewsAsync(string newsId, NewsModel newsModel)
        {
            try
            {
                DocumentReference docRef = _firestoreDb.Collection(COLLECTION_NAME).Document(newsId);
                await docRef.SetAsync(newsModel, SetOptions.MergeAll);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la mise à jour de l'actualité: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Supprime une actualité
        /// </summary>
        /// <param name="newsId">Identifiant de l'actualité à supprimer</param>
        /// <returns>True si la suppression a réussi</returns>
        public async Task<bool> DeleteNewsAsync(string newsId)
        {
            try
            {
                DocumentReference docRef = _firestoreDb.Collection(COLLECTION_NAME).Document(newsId);
                await docRef.DeleteAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la suppression de l'actualité: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Change le statut épinglé d'une actualité
        /// </summary>
        /// <param name="newsId">Identifiant de l'actualité</param>
        /// <param name="isPinned">Nouveau statut d'épinglage</param>
        /// <returns>True si la mise à jour a réussi</returns>
        public async Task<bool> TogglePinNewsAsync(string newsId, bool isPinned)
        {
            try
            {
                DocumentReference docRef = _firestoreDb.Collection(COLLECTION_NAME).Document(newsId);
                await docRef.UpdateAsync("IsPinned", isPinned);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors du changement du statut d'épinglage: {ex.Message}");
                throw;
            }
        }
    }
}