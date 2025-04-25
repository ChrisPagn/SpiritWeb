using Microsoft.AspNetCore.Mvc;
using SpiritWeb.Shared.Models;
using SpiritWeb.Server.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpiritWeb.Server.Controllers
{
    /// <summary>
    /// Contrôleur API pour gérer les actualités du jeu Spirit
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class NewsController : ControllerBase
    {
        private readonly FirestoreNewsService _newsService;

        /// <summary>
        /// Initialise une nouvelle instance de la classe <see cref="NewsController"/>.
        /// </summary>
        /// <param name="newsService">Service de gestion des actualités dans Firestore.</param>
        public NewsController(FirestoreNewsService newsService)
        {
            _newsService = newsService;
        }

        /// <summary>
        /// Récupère toutes les actualités
        /// </summary>
        /// <returns>Liste des actualités triées par date de publication</returns>
        //[HttpGet("all")]
        //public async Task<IActionResult> GetAllNews()
        //{
        //    try
        //    {
        //        var newsList = await _newsService.GetAllNewsAsync();
        //        return Ok(newsList);
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Erreur détaillée : {ex.ToString()}");
        //        return StatusCode(500, $"Erreur interne: {ex.Message}");
        //    }
        //}
        [HttpGet("all")]
        public IActionResult GetAllNews()
        {
            var news = _newsService.GetAllNewsAsync(); // Retourne une liste (vide ou non)
            return Ok(news); // Toujours 200, même si la liste est vide
        }

        /// <summary>
        /// Récupère une actualité par son identifiant
        /// </summary>
        /// <param name="id">Identifiant de l'actualité</param>
        /// <returns>L'actualité demandée</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetNewsById(string id)
        {
            try
            {
                var news = await _newsService.GetNewsByIdAsync(id);
                if (news == null)
                {
                    return NotFound($"Actualité avec l'ID {id} non trouvée");
                }
                return Ok(news);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur détaillée : {ex.ToString()}");
                return StatusCode(500, $"Erreur interne: {ex.Message}");
            }
        }

        /// <summary>
        /// Ajoute une nouvelle actualité
        /// </summary>
        /// <param name="newsModel">Modèle de l'actualité à ajouter</param>
        /// <returns>L'identifiant de l'actualité créée</returns>
        [HttpPost("add")]
        public async Task<IActionResult> AddNews([FromBody] NewsModel newsModel)
        {
            try
            {
                if (newsModel == null || string.IsNullOrEmpty(newsModel.Title) || string.IsNullOrEmpty(newsModel.Content))
                {
                    return BadRequest("Le titre et le contenu de l'actualité sont obligatoires");
                }

                string newsId = await _newsService.AddNewsAsync(newsModel);
                return Ok(newsId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur détaillée : {ex.ToString()}");
                return StatusCode(500, $"Erreur interne: {ex.Message}");
            }
        }

        /// <summary>
        /// Met à jour une actualité existante
        /// </summary>
        /// <param name="id">Identifiant de l'actualité</param>
        /// <param name="newsModel">Modèle de l'actualité avec les modifications</param>
        /// <returns>200 OK si la mise à jour a réussi</returns>
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateNews(string id, [FromBody] NewsModel newsModel)
        {
            try
            {
                if (newsModel == null || string.IsNullOrEmpty(newsModel.Title) || string.IsNullOrEmpty(newsModel.Content))
                {
                    return BadRequest("Le titre et le contenu de l'actualité sont obligatoires");
                }

                bool success = await _newsService.UpdateNewsAsync(id, newsModel);
                if (success)
                {
                    return Ok();
                }
                return NotFound($"Actualité avec l'ID {id} non trouvée");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur détaillée : {ex.ToString()}");
                return StatusCode(500, $"Erreur interne: {ex.Message}");
            }
        }

        /// <summary>
        /// Supprime une actualité
        /// </summary>
        /// <param name="id">Identifiant de l'actualité à supprimer</param>
        /// <returns>200 OK si la suppression a réussi</returns>
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteNews(string id)
        {
            try
            {
                bool success = await _newsService.DeleteNewsAsync(id);
                if (success)
                {
                    return Ok();
                }
                return NotFound($"Actualité avec l'ID {id} non trouvée");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur détaillée : {ex.ToString()}");
                return StatusCode(500, $"Erreur interne: {ex.Message}");
            }
        }

        /// <summary>
        /// Change le statut épinglé d'une actualité
        /// </summary>
        /// <param name="id">Identifiant de l'actualité</param>
        /// <param name="isPinned">Nouveau statut d'épinglage</param>
        /// <returns>200 OK si la mise à jour a réussi</returns>
        [HttpPut("pin/{id}/{isPinned}")]
        public async Task<IActionResult> TogglePinNews(string id, bool isPinned)
        {
            try
            {
                bool success = await _newsService.TogglePinNewsAsync(id, isPinned);
                if (success)
                {
                    return Ok();
                }
                return NotFound($"Actualité avec l'ID {id} non trouvée");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur détaillée : {ex.ToString()}");
                return StatusCode(500, $"Erreur interne: {ex.Message}");
            }
        }
    }
}