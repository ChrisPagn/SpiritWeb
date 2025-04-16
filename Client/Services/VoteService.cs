﻿using System.Net.Http;
using System.Net.Http.Json;

namespace SpiritWeb.Client.Services
{
    /// <summary>
    /// Service de gestion des votes pour les suggestions d'optimisation
    /// </summary>
    public class VoteService
    {
       
        private readonly HttpClient _httpClient;
        private readonly AuthService _authService;
        /// <summary>
        /// Vote pour une suggestion spécifique
        /// </summary>
        /// <param name="suggestionId">Identifiant de la suggestion</param>
        /// <returns>True si le vote a été enregistré avec succès</returns>
        public async Task<bool> VoteForSuggestionAsync(string suggestionId)
        {
            if (!_authService.IsAuthenticated)
                return false;

            try
            {
                var response = await _httpClient.PostAsync($"api/Vote/save/{suggestionId}/{_authService.UserId}", null);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors du vote pour la suggestion : {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Vérifie si l'utilisateur actuel a déjà voté pour une suggestion
        /// </summary>
        /// <param name="suggestionId">Identifiant de la suggestion</param>
        /// <returns>True si l'utilisateur a déjà voté</returns>
        public async Task<bool> HasUserVotedAsync(string suggestionId)
        {
            if (!_authService.IsAuthenticated)
                return false;

            try
            {
                return await _httpClient.GetFromJsonAsync<bool>($"api/Survey/hasVoted/{suggestionId}/{_authService.UserId}");
            }
            catch
            {
                return false;
            }
        }
    }
}
