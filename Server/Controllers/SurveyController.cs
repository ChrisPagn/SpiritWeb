using Microsoft.AspNetCore.Mvc;
using SpiritWeb.Shared.Models;
using SpiritWeb.Server.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpiritWeb.Server.Controllers
{
    /// <summary>
    /// Contrôleur API pour gérer les enquêtes et suggestions d'optimisation
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class SurveyController : ControllerBase
    {
        private readonly FirestoreSurveyService _surveyService;

        /// <summary>
        /// Initialise une nouvelle instance de la classe <see cref="SurveyController"/>.
        /// </summary>
        /// <param name="surveyService">Service de gestion des enquêtes dans Firestore.</param>
        public SurveyController(FirestoreSurveyService surveyService)
        {
            _surveyService = surveyService;
        }

        /// <summary>
        /// Enregistre une nouvelle enquête/suggestion dans Firestore
        /// </summary>
        /// <param name="model">Le modèle d'enquête à enregistrer</param>
        /// <returns>L'identifiant de l'enquête créée</returns>
        [HttpPost("submit")]
        public async Task<IActionResult> SubmitSurvey([FromBody] SurveyModel model)
        {
            try
            {
                if (model == null || string.IsNullOrEmpty(model.UserId))
                {
                    return BadRequest("Données invalides");
                }

                // Enregistrer l'enquête et récupérer l'ID généré
                string id = await _surveyService.SaveSurveyAsync(model);
                return Ok(id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur détaillée : {ex.ToString()}"); // Ajoutez cette ligne
                return StatusCode(500, $"Erreur interne: {ex.Message}");
            }
        }

        /// <summary>
        /// Récupère toutes les suggestions d'optimisation
        /// </summary>
        /// <returns>Liste des suggestions d'optimisation</returns>
        [HttpGet("suggestions")]
        public async Task<IActionResult> GetAllSuggestions()
        {
            try
            {
                var suggestions = await _surveyService.GetAllSuggestionsAsync();
                return Ok(suggestions);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur détaillée : {ex.ToString()}"); // Ajoutez cette ligne
                return StatusCode(500, $"Erreur interne: {ex.Message}");
            }
        }

        /// <summary>
        /// Vote pour une suggestion spécifique
        /// </summary>
        /// <param name="suggestionId">Identifiant de la suggestion</param>
        /// <param name="userId">Identifiant de l'utilisateur qui vote</param>
        /// <returns>True si le vote a été enregistré avec succès</returns>
        [HttpPost("vote/{suggestionId}/{userId}")]
        public async Task<IActionResult> VoteForSuggestion(string suggestionId, string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(suggestionId) || string.IsNullOrEmpty(userId))
                {
                    return BadRequest("Paramètres invalides");
                }

                // Vérifier si l'utilisateur a déjà voté
                bool hasVoted = await _surveyService.HasUserVotedAsync(suggestionId, userId);
                if (hasVoted)
                {
                    return BadRequest("Vous avez déjà voté pour cette suggestion");
                }

                // Enregistrer le vote
                await _surveyService.AddVoteAsync(suggestionId, userId);
                return Ok(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur détaillée : {ex.ToString()}"); // Ajoutez cette ligne
                return StatusCode(500, $"Erreur interne: {ex.Message}");
            }
        }

        /// <summary>
        /// Vérifie si un utilisateur a déjà voté pour une suggestion
        /// </summary>
        /// <param name="suggestionId">Identifiant de la suggestion</param>
        /// <param name="userId">Identifiant de l'utilisateur</param>
        /// <returns>True si l'utilisateur a déjà voté</returns>
        [HttpGet("hasVoted/{suggestionId}/{userId}")]
        public async Task<IActionResult> HasUserVoted(string suggestionId, string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(suggestionId) || string.IsNullOrEmpty(userId))
                {
                    return BadRequest("Paramètres invalides");
                }

                bool hasVoted = await _surveyService.HasUserVotedAsync(suggestionId, userId);
                return Ok(hasVoted);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur détaillée : {ex.ToString()}"); // Ajoutez cette ligne
                return StatusCode(500, $"Erreur interne: {ex.Message}");
            }
        }
    }
}