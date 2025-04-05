using Microsoft.AspNetCore.Mvc;
using SpiritWeb.Server.Services;
using System.Text.Json;
using System.Threading.Tasks;

namespace SpiritWeb.Server.Controllers
{
    /// <summary>
    /// Contrôleur API pour gérer l'authentification des utilisateurs, incluant l'inscription et la connexion.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly FirebaseAuthService _authService;

        /// <summary>
        /// Initialise une nouvelle instance de la classe <see cref="AuthController"/>.
        /// </summary>
        /// <param name="authService">Service d'authentification Firebase.</param>
        public AuthController(FirebaseAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Enregistre un nouvel utilisateur avec un email, un mot de passe et un nom d'affichage.
        /// </summary>
        /// <param name="email">Email de l'utilisateur.</param>
        /// <param name="password">Mot de passe de l'utilisateur.</param>
        /// <param name="displayName">Nom d'affichage de l'utilisateur.</param>
        /// <returns>Le résultat de l'authentification Firebase.</returns>
        [HttpPost("register")]
        public async Task<IActionResult> Register(string email, string password, string displayName)
        {
            var authResult = await _authService.RegisterWithEmailAndPasswordAsync(email, password, displayName);

            // Affiche les détails dans la console
            Console.WriteLine($"Utilisateur enregistré avec l'email : {email}");
            if (authResult != null)
            {
                Console.WriteLine($"ID utilisateur : {authResult.User.LocalId}");  // Supposons que authResult contient l'ID utilisateur
            }
            else
            {
                Console.WriteLine("L'enregistrement a échoué.");
            }

            return Ok(authResult);
        }

        /// <summary>
        /// Connecte un utilisateur avec un email et un mot de passe.
        /// </summary>
        /// <param name="email">Email de l'utilisateur.</param>
        /// <param name="password">Mot de passe de l'utilisateur.</param>
        /// <returns>Le résultat de l'authentification Firebase.</returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login(string email, string password)
        {
            var authResult = await _authService.SignInWithEmailAndPasswordAsync(email, password);

            if (authResult != null)
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true
                };

                return new JsonResult(authResult, options);
            }

            return BadRequest("Login failed");
        }
    }
}
