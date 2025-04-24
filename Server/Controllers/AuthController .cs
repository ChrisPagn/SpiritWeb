using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Mvc;
using SpiritWeb.Client.Models;
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
        /// Inscrit un nouvel utilisateur avec un email, un mot de passe et un nom d'affichage.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <param name="displayName"></param>
        /// <returns></returns>
        [HttpPost("register")]
        public async Task<ActionResult<FirebaseAuthResult>> Register(string email, string password, string displayName)
        {
            try
            {
                // Créer un nouvel utilisateur avec l'email et le mot de passe fournis
                UserRecord userRecord = await FirebaseAuth.DefaultInstance.CreateUserAsync(new UserRecordArgs
                {
                    Email = email,
                    Password = password,
                    DisplayName = displayName
                });

                // Générer un token personnalisé
                var token = await FirebaseAuth.DefaultInstance.CreateCustomTokenAsync(userRecord.Uid);

                // Créer un lien d'authentification Firebase
                return new FirebaseAuthResult
                {
                    Token = token,
                    UserId = userRecord.Uid,
                    Email = email,
                    DisplayName = displayName
                };
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
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
