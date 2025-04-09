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
        public FirebaseUser? LocalId { get; set; }
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
    }

}
