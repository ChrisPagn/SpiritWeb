using SpiritWeb.Shared.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Text.Json;
using Google.Cloud.Firestore;

namespace SpiritWeb.Client.Services
{
    /// <summary>
    /// Service de gestion des enquêtes et suggestions d'optimisation
    /// </summary>
    public class SurveyService
    {
        private readonly HttpClient _httpClient;
        private readonly AuthService _authService;

        /// <summary>
        /// Initialise une nouvelle instance de la classe <see cref="SurveyService"/>.
        /// </summary>
        /// <param name="httpClient">Client HTTP pour les requêtes réseau.</param>
        /// <param name="authService">Service d'authentification.</param>
        public SurveyService(HttpClient httpClient, AuthService authService)
        {
            _httpClient = httpClient;
            _authService = authService;
        }

        /// <summary>
        /// Enregistre une nouvelle enquête/suggestion dans Firestore
        /// </summary>
        /// <param name="surveyModel">Le modèle d'enquête à enregistrer</param>
        /// <returns>L'identifiant de l'enquête créée</returns>
        public async Task<string> SaveSurveyAsync(SurveyModel surveyModel)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"api/Survey/save", surveyModel);
                Console.WriteLine(JsonSerializer.Serialize(surveyModel));

                response.EnsureSuccessStatusCode();

                // Récupérer l'ID généré par Firestore
                var content = await response.Content.ReadAsStringAsync();
                return content;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de l'enregistrement de l'enquête : {ex.Message}");
                throw;
            }
        }




        /// <summary>
        /// Récupère toutes les suggestions d'optimisation
        /// </summary>
        /// <returns>Liste des suggestions d'optimisation</returns>
        public async Task<List<SurveyModel>> GetAllSuggestionsAsync()
        {
            try
            {
                var suggestions = await _httpClient.GetFromJsonAsync<List<SurveyModel>>("api/Survey/suggestions");
                return suggestions ?? new List<SurveyModel>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la récupération des suggestions : {ex.Message}");
                throw;
            }
        }


       
    }
}