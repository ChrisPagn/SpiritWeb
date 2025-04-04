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

            // Affiche les détails dans la console
            Console.WriteLine($"Utilisateur enregistré avec l'email : {email}");
            if (authResult != null)
            {
                Console.WriteLine($"ID utilisateur : {authResult.User}");  // Supposons que authResult contient l'ID utilisateur
            }
            else
            {
                Console.WriteLine("L'enregistrement a échoué.");
            }

            return Ok(authResult);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(string email, string password)
        {
            var authResult = await _authService.SignInWithEmailAndPasswordAsync(email, password);

            // Affiche les détails dans la console
            Console.WriteLine($"Tentative de connexion avec l'email : {email}");
            if (authResult != null)
            {
                Console.WriteLine($"Connexion réussie ! ID utilisateur : {authResult.User}");  // Affiche l'ID utilisateur
            }
            else
            {
                Console.WriteLine("La connexion a échoué.");
            }

            return Ok(authResult);
        }
    }
}


