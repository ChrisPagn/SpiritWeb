using SpiritWeb.Shared.Models;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace SpiritWeb.Client.Services
{
    public class DatabaseService
    {
        private readonly HttpClient _httpClient;

        public DatabaseService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // Sauvegarde des données utilisateur
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

        // Chargement des données utilisateur
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

        // Récupération de tous les utilisateurs
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

        // Initialisation des données pour un nouvel utilisateur
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