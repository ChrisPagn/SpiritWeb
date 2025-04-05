using Firebase.Auth;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace SpiritWeb.Server.Services
{
    /// <summary>
    /// Service d'authentification Firebase pour gérer l'inscription et la connexion des utilisateurs.
    /// </summary>
    public class FirebaseAuthService
    {
        private readonly FirebaseAuthProvider _authProvider;

        /// <summary>
        /// Initialise une nouvelle instance de la classe <see cref="FirebaseAuthService"/>.
        /// </summary>
        /// <param name="configuration">Configuration de l'application pour accéder aux paramètres Firebase.</param>
        /// <exception cref="ArgumentNullException">Lancée si la clé API Firebase est manquante dans la configuration.</exception>
        public FirebaseAuthService(IConfiguration configuration)
        {
            string? apiKey = configuration["Firebase:ApiKey"];
            if (apiKey == null)
            {
                throw new ArgumentNullException(nameof(apiKey), "Firebase API key is missing in configuration.");
            }
            _authProvider = new FirebaseAuthProvider(new FirebaseConfig(apiKey));
        }

        /// <summary>
        /// Enregistre un nouvel utilisateur avec un email, un mot de passe et un nom d'affichage.
        /// </summary>
        /// <param name="email">Email de l'utilisateur.</param>
        /// <param name="password">Mot de passe de l'utilisateur.</param>
        /// <param name="displayName">Nom d'affichage de l'utilisateur.</param>
        /// <returns>Un lien d'authentification Firebase pour le nouvel utilisateur.</returns>
        public async Task<FirebaseAuthLink> RegisterWithEmailAndPasswordAsync(string email, string password, string displayName)
        {
            return await _authProvider.CreateUserWithEmailAndPasswordAsync(email, password, displayName);
        }

        /// <summary>
        /// Connecte un utilisateur avec un email et un mot de passe.
        /// </summary>
        /// <param name="email">Email de l'utilisateur.</param>
        /// <param name="password">Mot de passe de l'utilisateur.</param>
        /// <returns>Un lien d'authentification Firebase pour l'utilisateur connecté.</returns>
        public async Task<FirebaseAuthLink> SignInWithEmailAndPasswordAsync(string email, string password)
        {
            return await _authProvider.SignInWithEmailAndPasswordAsync(email, password);
        }
    }
}
