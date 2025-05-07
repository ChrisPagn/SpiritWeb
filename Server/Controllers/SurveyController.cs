using Microsoft.AspNetCore.Mvc;
using SpiritWeb.Shared.Models;
using SpiritWeb.Server.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json;
using System.Net;

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
        private readonly ILogger<SurveyController> _logger;

        /// <summary>
        /// Initialise une nouvelle instance de la classe <see cref="SurveyController"/>.
        /// </summary>
        /// <param name="surveyService">Service de gestion des enquêtes dans Firestore.</param>
        public SurveyController(FirestoreSurveyService surveyService, ILogger<SurveyController> logger)
        {
            _surveyService = surveyService;
            _logger = logger;
        }

        /// <summary>
        /// Enregistre une nouvelle enquête/suggestion dans Firestore
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("save")]
        public async Task<IActionResult> SubmitSurvey([FromBody] SurveyModel model)
        {
            try
            {
                // 1. Validation de base des champs obligatoires
                if (model == null ||
                    string.IsNullOrEmpty(model.UserId) ||
                    model.SatisfactionRating == 0 ||
                    model.PlayFrequency == 0 ||
                    string.IsNullOrEmpty(model.FavoriteFeature) ||
                    string.IsNullOrEmpty(model.OptimizationSuggestion))
                {
                    return BadRequest("Tous les champs obligatoires doivent être remplis");
                }

                // 2. Validation avancée de la suggestion
                if (model.OptimizationSuggestion.Length < 10 || model.OptimizationSuggestion.Length > 300)
                {
                    return BadRequest("La suggestion doit contenir entre 10 et 300 caractères");
                }

                // 3. Protection contre les injections/XSS
                if (model.OptimizationSuggestion.Contains("<") || model.OptimizationSuggestion.Contains(">"))
                {
                    return BadRequest("Les balises HTML ne sont pas autorisées");
                }

                // 4. Détection de spam (exemple basique)
                var spamKeywords = new[] { "http://", "https://", "www.", "acheter", "viagra" };
                if (spamKeywords.Any(k => model.OptimizationSuggestion.Contains(k, StringComparison.OrdinalIgnoreCase)))
                {
                    _logger.LogWarning($"Spam détecté de l'utilisateur {model.UserId}");
                    return BadRequest("Le contenu semble contenir du spam");
                }

                // 5. Journalisation avant sauvegarde
                _logger.LogInformation($"Nouvelle soumission de {model.UserId} - Note: {model.SatisfactionRating}");

                // 6. Sauvegarde sécurisée (avec encodage HTML)
                var sanitizedSuggestion = WebUtility.HtmlEncode(model.OptimizationSuggestion);
                model.OptimizationSuggestion = sanitizedSuggestion;

                string id = await _surveyService.SaveSurveyAsync(model);

                return Ok(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la soumission du survey");
                return StatusCode(500, "Une erreur interne est survenue");
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
                Console.WriteLine($"Erreur détaillée : {ex.ToString()}"); 
                return StatusCode(500, $"Erreur interne: {ex.Message}");
            }
        }

    }
}