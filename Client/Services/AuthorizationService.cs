using System.Linq;

namespace SpiritWeb.Client.Services
{
    /// <summary>
    /// Service de gestion des autorisations d'accès aux pages de l'application.
    /// </summary>
    public class AuthorizationService
    {
        private readonly AuthService _authService;

        /// <summary>
        ///  Initialise une nouvelle instance de la classe <see cref="AuthorizationService"/>.
        /// </summary>
        /// <param name="authService"></param>
        public AuthorizationService(AuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Vérifie si l'utilisateur a accès à une page spécifique.
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        public bool HasAccessTo(string page)
        {
            if (!_authService.IsAuthenticated) return false;

            return page switch
            {
                "home" => true,
                "userprofile" => true,
                "userslist" => true,
                "news" => true,
                "suggestion" => _authService.UserRole is "contributor" or "admin",
                "admin" => _authService.UserRole == "admin",
                "settings" => _authService.UserRole == "admin", // Accès à la gestion des actualités uniquement pour les admins
                _ => false
            };
        }
    }
}