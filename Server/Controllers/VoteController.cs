using Microsoft.AspNetCore.Mvc;
using SpiritWeb.Server.Services;
using System;
using System.Threading.Tasks;

namespace SpiritWeb.Server.Controllers
{
    /// <summary>
    /// Contrôleur API pour gérer les votes des suggestions d'optimisation
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class VoteController : ControllerBase
    {
        private readonly FirestoreVoteService _voteService;

        /// <summary>
        /// Initialise une nouvelle instance de la classe <see cref="VoteController"/>.
        /// </summary>
        /// <param name="voteService">Service de gestion des votes dans Firestore.</param>
        public VoteController(FirestoreVoteService voteService)
        {
            _voteService = voteService;
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
                bool hasVoted = await _voteService.HasUserVotedAsync(suggestionId, userId);
                if (hasVoted)
                {
                    return BadRequest("Vous avez déjà voté pour cette suggestion");
                }

                // Enregistrer le vote
                await _voteService.AddVoteAsync(suggestionId, userId);
                return Ok(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur détaillée : {ex.ToString()}");
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

                bool hasVoted = await _voteService.HasUserVotedAsync(suggestionId, userId);
                return Ok(hasVoted);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur détaillée : {ex.ToString()}");
                return StatusCode(500, $"Erreur interne: {ex.Message}");
            }
        }
    }
}