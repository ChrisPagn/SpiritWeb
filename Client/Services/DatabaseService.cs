using SpiritWeb.Shared.Models;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace SpiritWeb.Client.Services
{
    /// <summary>
    /// Service de gestion des interactions avec la base de données pour les opérations CRUD sur les données utilisateur.
    /// </summary>
    public class DatabaseService
    {
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Initialise une nouvelle instance de la classe <see cref="DatabaseService"/>.
        /// </summary>
        /// <param name="httpClient">Client HTTP pour les requêtes réseau.</param>
        public DatabaseService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// Sauvegarde les données utilisateur dans la base de données.
        /// </summary>
        /// <param name="saveData">Les données utilisateur à sauvegarder.</param>
        /// <exception cref="Exception">Lancée lorsqu'une erreur survient lors de la sauvegarde des données.</exception>
        public async Task SaveDataAsync(SaveData saveData)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"api/Database/save?userId={saveData.UserId}", saveData);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la sauvegarde des données : {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Charge les données utilisateur depuis la base de données.
        /// </summary>
        /// <param name="userId">Identifiant de l'utilisateur dont les données doivent être chargées.</param>
        /// <returns>Les données utilisateur chargées.</returns>
        /// <exception cref="Exception">Lancée lorsqu'une erreur survient lors du chargement des données.</exception>
        public async Task<SaveData?> LoadDataAsync(string userId)
        {
            try
            {
                var data = await _httpClient.GetFromJsonAsync<SaveData>($"api/Database/load/{userId}");
                return data ?? throw new Exception("Les données chargées utilisateur sont nulles.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors du chargement des données : {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Récupère tous les utilisateurs depuis la base de données.
        /// </summary>
        /// <returns>Une liste de tous les utilisateurs.</returns>
        /// <exception cref="Exception">Lancée lorsqu'une erreur survient lors de la récupération des utilisateurs.</exception>
        public async Task<List<SaveData>> GetAllUsersAsync()
        {
            try
            {
                var users = await _httpClient.GetFromJsonAsync<List<SaveData>>("api/Database/users");
                return users ?? throw new Exception("Les données des utilisateurs sont nulles.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la récupération des utilisateurs : {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Initialise les données pour un nouvel utilisateur.
        /// </summary>
        /// <param name="userId">Identifiant de l'utilisateur.</param>
        /// <param name="displayName">Nom d'affichage de l'utilisateur.</param>
        /// <exception cref="Exception">Lancée lorsqu'une erreur survient lors de la création des données initiales.</exception>
        public async Task CreateInitialDataAsync(string userId, string displayName)
        {
            try
            {
                var saveData = new SaveData
                {
                    UserId = userId,
                    DisplayName = displayName,
                    CoinsCount = 0,
                    LevelReached = 1,
                    LastLevelPlayed = "level01",
                    InventoryItems = new List<int>(),
                    InventoryItemsName = new List<string>(),
                    LastModified = DateTime.UtcNow
                };

                await SaveDataAsync(saveData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la création des données initiales : {ex.Message}");
                throw;
            }
        }
    }
}
