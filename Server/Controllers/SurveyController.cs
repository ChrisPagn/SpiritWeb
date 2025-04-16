using Microsoft.AspNetCore.Mvc;
using SpiritWeb.Shared.Models;
using SpiritWeb.Server.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json;

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
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("save")]
        public async Task<IActionResult> SubmitSurvey([FromBody] SurveyModel model)
        {
            try
            {
                var toto = model;
                // Validation consolidée (sans vérification des votes)
                if (model == null ||
                    string.IsNullOrEmpty(model.UserId) ||
                    model.SatisfactionRating == 0 ||
                    model.PlayFrequency == 0 ||
                    string.IsNullOrEmpty(model.FavoriteFeature) ||
                    string.IsNullOrEmpty(model.OptimizationSuggestion))
                {
                    return BadRequest("Tous les champs obligatoires doivent être remplis");
                }

                //// Force l'initialisation si null
                //model.Votes = model.Votes ?? new List<string>();

                string id = await _surveyService.SaveSurveyAsync(model);
                return Ok(id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERREUR COMPLÈTE: {ex}");
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
                Console.WriteLine($"Erreur détaillée : {ex.ToString()}"); 
                return StatusCode(500, $"Erreur interne: {ex.Message}");
            }
        }

    }
}