using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Security.Claims;
using System;
using SpiritWeb.Shared.Models;
using System.Text.Json;
using Microsoft.JSInterop;
using FirebaseAdmin.Auth;
using System.Text.Json.Serialization;

namespace SpiritWeb.Client.Services
{
    /// <summary>
    /// Service d'authentification gérant l'inscription, la connexion et la déconnexion des utilisateurs.
    /// </summary>
    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private readonly IJSRuntime _jsRuntime;

        /// <summary>
        /// Identifiant unique de l'utilisateur authentifié.
        /// </summary>
        public string UserId { get; private set; }

        /// <summary>
        /// Email de l'utilisateur authentifié.
        /// </summary>
        public string UserEmail { get; private set; }

        /// <summary>
        /// Nom d'affichage de l'utilisateur authentifié.
        /// </summary>
        public string DisplayName { get; private set; }

        /// <summary>
        /// Indique si un utilisateur est actuellement authentifié.
        /// </summary>
        public bool IsAuthenticated { get; private set; }

        /// <summary>
        /// Tâche d'initialisation de l'état d'authentification.
        /// </summary>
        public Task AuthInitialized { get; private set; }

        /// <summary>
        /// Événement déclenché lorsque l'état d'authentification change.
        /// </summary>
        public event Action? OnAuthStateChanged;

        /// <summary>
        /// Initialise une nouvelle instance de la classe <see cref="AuthService"/>.
        /// </summary>
        /// <param name="httpClient">Client HTTP pour les requêtes réseau.</param>
        /// <param name="jsRuntime">Runtime JavaScript pour interagir avec le local storage.</param>
        public AuthService(HttpClient httpClient, IJSRuntime jsRuntime)
        {
            _httpClient = httpClient;
            _jsRuntime = jsRuntime;
            AuthInitialized = InitializeAsync();
        }

        /// <summary>
        /// Initialise l'état d'authentification.
        /// </summary>
        private async Task InitializeAsync()
        {
            await LoadAuthState();
        }

        /// <summary>
        /// Charge l'état d'authentification depuis le stockage local.
        /// </summary>
        private async Task LoadAuthState()
        {
            try
            {
                var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken");
                var userId = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "userId");
                var email = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "userEmail");
                var displayName = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "displayName");

                if (!string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(userId))
                {
                    UserId = userId;
                    UserEmail = email;
                    DisplayName = displayName;
                    IsAuthenticated = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors du chargement de l'état d'authentification : {ex.Message}");
            }
        }

        /// <summary>
        /// Enregistre un nouvel utilisateur avec un email, un mot de passe et un nom d'affichage.
        /// </summary>
        /// <param name="email">Email de l'utilisateur.</param>
        /// <param name="password">Mot de passe de l'utilisateur.</param>
        /// <param name="displayName">Nom d'affichage de l'utilisateur.</param>
        /// <returns>True si l'enregistrement est réussi, sinon False.</returns>
        public async Task<bool> RegisterWithEmailAndPasswordAsync(string email, string password, string displayName)
        {
            try
            {
                var response = await _httpClient.PostAsync(
                    $"api/Auth/register?email={Uri.EscapeDataString(email)}&password={Uri.EscapeDataString(password)}&displayName={Uri.EscapeDataString(displayName)}",
                    null);

                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                var authResult = JsonSerializer.Deserialize<FirebaseAuthResult>(content);

                if (authResult != null)
                {
                    await SetAuthData(authResult.Token, authResult.UserId, email, displayName);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur d'enregistrement : {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Connecte un utilisateur avec un email et un mot de passe.
        /// </summary>
        /// <param name="email">Email de l'utilisateur.</param>
        /// <param name="password">Mot de passe de l'utilisateur.</param>
        /// <returns>Le résultat de l'authentification Firebase.</returns>
        public async Task<FirebaseAuthResult> SignInWithEmailAndPasswordAsync(string email, string password)
        {
            try
            {
                var response = await _httpClient.PostAsync(
                    $"api/Auth/login?email={Uri.EscapeDataString(email)}&password={Uri.EscapeDataString(password)}",
                    null);

                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var authResult = JsonSerializer.Deserialize<FirebaseAuthResult>(content, options);

                if (authResult != null)
                {
                    await SetAuthData(authResult.Token, authResult.UserId ?? authResult.User?.LocalId, email, null);
                    return authResult;
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur de connexion : {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Déconnecte l'utilisateur actuel.
        /// </summary>
        public async Task SignOut()
        {
            UserId = null;
            UserEmail = null;
            DisplayName = null;
            IsAuthenticated = false;

            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "authToken");
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "userId");
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "userEmail");
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "displayName");
        }

        /// <summary>
        /// Stocke les données d'authentification dans le local storage.
        /// </summary>
        /// <param name="token">Jeton d'authentification.</param>
        /// <param name="userId">Identifiant de l'utilisateur.</param>
        /// <param name="email">Email de l'utilisateur.</param>
        /// <param name="displayName">Nom d'affichage de l'utilisateur.</param>
        private async Task SetAuthData(string token, string userId, string email, string displayName)
        {
            UserId = userId;
            UserEmail = email;
            DisplayName = displayName ?? string.Empty;
            IsAuthenticated = true;

            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "authToken", token);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "userId", userId);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "userEmail", email);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "displayName", displayName ?? string.Empty);
        }

        /// <summary>
        /// Définit l'état d'authentification.
        /// </summary>
        /// <param name="isAuthenticated">Vrai si l'utilisateur est authentifié, sinon Faux.</param>
        public void SetAuthenticationState(bool isAuthenticated)
        {
            IsAuthenticated = isAuthenticated;
            NotifyAuthenticationStateChanged();
        }

        /// <summary>
        /// Notifie les abonnés que l'état d'authentification a changé.
        /// </summary>
        private void NotifyAuthenticationStateChanged()
        {
            OnAuthStateChanged?.Invoke();
        }
    }

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
        public FirebaseUser User { get; set; }
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
