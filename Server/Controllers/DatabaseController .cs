using Microsoft.AspNetCore.Mvc;
using SpiritWeb.Shared.Models;
using SpiritWeb.Server.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SpiritWeb.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DatabaseController : ControllerBase
    {
        private readonly FirebaseDatabaseService _databaseService;

        public DatabaseController(FirebaseDatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        [HttpPost("save")]
        public async Task<IActionResult> SaveData(string userId, [FromBody] SaveData saveData)
        {
            await _databaseService.SaveDataAsync(userId, saveData);
            return Ok();
        }

        [HttpGet("load/{userId}")]
        public async Task<IActionResult> LoadData(string userId)
        {
            var data = await _databaseService.LoadDataAsync(userId);
            return Ok(data);
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _databaseService.GetAllUsersAsync();
            return Ok(users);
        }
    }
}
