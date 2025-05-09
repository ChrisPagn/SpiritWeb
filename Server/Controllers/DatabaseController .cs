﻿using Microsoft.AspNetCore.Mvc;
using SpiritWeb.Shared.Models;
using SpiritWeb.Server.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json;

namespace SpiritWeb.Server.Controllers
{
    /// <summary>
    /// Contrôleur API pour gérer les opérations CRUD sur les données utilisateur dans la base de données.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class DatabaseController : ControllerBase
    {
        private readonly FirebaseDatabaseService _databaseService;

        /// <summary>
        /// Initialise une nouvelle instance de la classe <see cref="DatabaseController"/>.
        /// </summary>
        /// <param name="databaseService">Service de gestion de la base de données Firebase.</param>
        public DatabaseController(FirebaseDatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        /// <summary>
        /// /// <summary>
        /// Sauvegarde les données utilisateur dans la base de données Firestore.
        /// Ce endpoint accepte un objet SaveData complet et le persiste dans la collection "users"
        /// en utilisant le UserId comme identifiant de document.
        /// </summary>
        /// <param name="saveData"></param>
        /// <returns></returns>        
        [HttpPost("save")] 
        public async Task<IActionResult> SaveData([FromBody] SaveData saveData)
        {
            try
            {
                if (saveData == null || string.IsNullOrEmpty(saveData.UserId))
                {
                    return BadRequest("Données invalides");
                }

                Console.WriteLine($"Données reçues: {JsonSerializer.Serialize(saveData)}");

                // Passez directement saveData.UserId
                await _databaseService.SaveDataAsync(saveData.UserId, saveData);
                return Ok();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERREUR Serveur: {ex}");
                return StatusCode(500, $"Erreur interne: {ex.Message}");
            }
        }

        /// <summary>
        /// Charge les données utilisateur depuis la base de données.
        /// </summary>
        /// <param name="userId">Identifiant de l'utilisateur dont les données doivent être chargées.</param>
        /// <returns>Les données utilisateur chargées.</returns>
        [HttpGet("load/{userId}")]
        public async Task<IActionResult> LoadData(string userId)
        {
            var data = await _databaseService.LoadDataAsync(userId);
            return Ok(data);
        }

        /// <summary>
        /// Récupère tous les utilisateurs depuis la base de données.
        /// </summary>
        /// <returns>Une liste de tous les utilisateurs.</returns>
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _databaseService.GetAllUsersAsync();
            return Ok(users);
        }
    }
}
