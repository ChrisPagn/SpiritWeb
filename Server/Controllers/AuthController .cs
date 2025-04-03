using Microsoft.AspNetCore.Mvc;
using SpiritWeb.Server.Services;
using System.Threading.Tasks;

namespace SpiritWeb.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly FirebaseAuthService _authService;

        public AuthController(FirebaseAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(string email, string password, string displayName)
        {
            var authResult = await _authService.RegisterWithEmailAndPasswordAsync(email, password, displayName);
            return Ok(authResult);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(string email, string password)
        {
            var authResult = await _authService.SignInWithEmailAndPasswordAsync(email, password);
            return Ok(authResult);
        }
    }
}
