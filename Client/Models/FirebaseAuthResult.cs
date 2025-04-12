using SpiritWeb.Client.Services;
using System.Text.Json.Serialization;
//using SpiritWeb.Client.Models;

namespace SpiritWeb.Client.Models
{
    /// <summary>
    /// Classe représentant le résultat de l'authentification Firebase.
    /// </summary>
    public class FirebaseAuthResult
    {
        /// <summary>
        /// Jeton d'authentification.
        /// </summary>
        [JsonPropertyName("token")]
        public string Token { get; set; }

        /// <summary>
        /// Identifiant de l'utilisateur.
        /// </summary>
        [JsonPropertyName("localId")]
        public string UserId { get; set; }

        /// <summary>
        /// Objet utilisateur Firebase.
        /// </summary>
        [JsonPropertyName("user")]
        public FirebaseUser? User { get; set; }

        /// <summary>
        /// Adresse e-mail de l'utilisateur.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Nom d'affichage de l'utilisateur.
        /// </summary>
        [JsonPropertyName("displayName")]
        public string DisplayName { get; set; }

        /// <summary>
        /// Role de l'utilisateur Firebase.
        /// </summary>
        public string? Role { get; internal set; }
    }

    /// <summary>
    /// Classe représentant un utilisateur Firebase.
    /// </summary>
    public class FirebaseUser
    {
        /// <summary>
        /// Identifiant local de l'utilisateur.
        /// </summary>
        public string LocalId { get; set; }

        public string DisplayName { get; set; }
    }

}
