using SpiritWeb.Shared.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Text.Json;

namespace SpiritWeb.Client.Services
{
    /// <summary>
    /// Service de gestion des actualités concernant le jeu Spirit
    /// </summary>
    public class NewsService
    {
        private readonly HttpClient _httpClient;
        private readonly AuthService _authService;

        /// <summary>
        /// Initialise une nouvelle instance de la classe <see cref="NewsService"/>.
        /// </summary>
        /// <param name="httpClient">Client HTTP pour les requêtes réseau.</param>
        /// <param name="authService">Service d'authentification.</param>
        public NewsService(HttpClient httpClient, AuthService authService)
        {
            _httpClient = httpClient;
            _authService = authService;
        }

        /// <summary>
        /// Récupère toutes les actualités
        /// </summary>
        /// <returns>Liste des actualités triées par date de publication</returns>
        public async Task<List<NewsModel>> GetAllNewsAsync()
        {
            try
            {
                var news = await _httpClient.GetFromJsonAsync<List<NewsModel>>("api/News/all");
                return news ?? new List<NewsModel>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la récupération des actualités : {ex.Message}");
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
                return await _httpClient.GetFromJsonAsync<NewsModel>($"api/News/{newsId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la récupération de l'actualité : {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Ajoute une nouvelle actualité (réservé aux administrateurs)
        /// </summary>
        /// <param name="newsModel">Modèle de l'actualité à ajouter</param>
        /// <returns>L'identifiant de l'actualité créée</returns>
        public async Task<string> AddNewsAsync(NewsModel newsModel)
        {
            if (!_authService.IsAuthenticated || _authService.UserRole != "admin")
            {
                throw new UnauthorizedAccessException("Seuls les administrateurs peuvent ajouter des actualités");
            }

            try
            {
                // Ajouter les informations de l'administrateur
                newsModel.AdminId = _authService.UserId;
                newsModel.AdminDisplayName = _authService.DisplayName;
                newsModel.PublishDate = DateTime.UtcNow;
                newsModel.LastModified = DateTime.UtcNow;

                var response = await _httpClient.PostAsJsonAsync("api/News/add", newsModel);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                return content; // Retourne l'ID généré
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de l'ajout de l'actualité : {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Met à jour une actualité existante (réservé aux administrateurs)
        /// </summary>
        /// <param name="newsModel">Modèle de l'actualité avec les modifications</param>
        /// <returns>True si la mise à jour a réussi</returns>
        public async Task<bool> UpdateNewsAsync(NewsModel newsModel)
        {
            if (!_authService.IsAuthenticated || _authService.UserRole != "admin")
            {
                throw new UnauthorizedAccessException("Seuls les administrateurs peuvent modifier des actualités");
            }

            try
            {
                // Mettre à jour les informations de l'administrateur
                newsModel.AdminId = _authService.UserId;
                newsModel.AdminDisplayName = _authService.DisplayName;
                newsModel.LastModified = DateTime.UtcNow;

                var response = await _httpClient.PutAsJsonAsync($"api/News/update/{newsModel.Id}", newsModel);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la mise à jour de l'actualité : {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Supprime une actualité (réservé aux administrateurs)
        /// </summary>
        /// <param name="newsId">Identifiant de l'actualité à supprimer</param>
        /// <returns>True si la suppression a réussi</returns>
        public async Task<bool> DeleteNewsAsync(string newsId)
        {
            if (!_authService.IsAuthenticated || _authService.UserRole != "admin")
            {
                throw new UnauthorizedAccessException("Seuls les administrateurs peuvent supprimer des actualités");
            }

            try
            {
                var response = await _httpClient.DeleteAsync($"api/News/delete/{newsId}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la suppression de l'actualité : {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Change le statut épinglé d'une actualité (réservé aux administrateurs)
        /// </summary>
        /// <param name="newsId">Identifiant de l'actualité</param>
        /// <param name="isPinned">Nouveau statut d'épinglage</param>
        /// <returns>True si la mise à jour a réussi</returns>
        public async Task<bool> TogglePinNewsAsync(string newsId, bool isPinned)
        {
            if (!_authService.IsAuthenticated || _authService.UserRole != "admin")
            {
                throw new UnauthorizedAccessException("Seuls les administrateurs peuvent épingler des actualités");
            }

            try
            {
                var response = await _httpClient.PutAsync($"api/News/pin/{newsId}/{isPinned}", null);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors du changement du statut d'épinglage : {ex.Message}");
                throw;
            }
        }
    }
}