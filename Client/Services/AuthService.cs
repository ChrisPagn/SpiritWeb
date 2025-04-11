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
using SpiritWeb.Client.Models;

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
        /// Rôle de l'utilisateur authentifié (ex: "admin", "contributor", "user" par default).
        /// </summary>
        public string UserRole { get; private set; }


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
                var role = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "userRole");

                if (!string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(userId))
                {
                    UserId = userId;
                    UserEmail = email;
                    DisplayName = displayName;
                    IsAuthenticated = true;
                    UserRole = role ?? "user"; // Récupération du rôle de l'utilisateur
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors du chargement de l'état d'authentification : {ex.Message}");
            }
        }


        /// <summary>
        /// Inscrit un nouvel utilisateur avec un email, un mot de passe et un nom d'affichage.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <param name="displayName"></param>
        /// <returns></returns>
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
                    // S'assurer que l'UserId est bien défini avant de continuer
                    string userId = authResult.UserId ?? authResult.LocalId?.LocalId;
                    if (string.IsNullOrEmpty(userId))
                    {
                        throw new Exception("L'ID utilisateur n'a pas été retourné par Firebase");
                    }

                    await SetAuthData(authResult.Token, userId, email, displayName);

                    // Attendre un moment que les données initiales soient créées
                    await Task.Delay(1000);

                    // Puis charger le rôle depuis la base de données
                    await LoadUserRoleFromDatabase();
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
                  
                    // D'abord, définir les données d'authentification de base avec "user" comme rôle par défaut
                    await SetAuthData(authResult.Token, authResult.UserId ?? authResult.LocalId?.LocalId, email, null);

                    // Ensuite, charger les données utilisateur pour obtenir le rôle réel
                    await LoadUserRoleFromDatabase();

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
            UserRole = null; 
            IsAuthenticated = false;

            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "authToken");
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "userId");
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "userEmail");
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "userRole");
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "displayName");

            NotifyAuthenticationStateChanged();
        }

        /// <summary>
        /// Stocke les données d'authentification dans le local storage.
        /// </summary>
        /// <param name="token">Jeton d'authentification.</param>
        /// <param name="userId">Identifiant de l'utilisateur.</param>
        /// <param name="email">Email de l'utilisateur.</param>
        /// <param name="displayName">Nom d'affichage de l'utilisateur.</param>
        private async Task SetAuthData(string token, string userId, string email, string displayName, string role = "user")
        {
            UserId = userId;
            UserEmail = email;
            DisplayName = displayName ?? string.Empty;
            IsAuthenticated = true;
            UserRole = role; // Stockage du rôle

            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "authToken", token);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "userId", userId);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "userEmail", email);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "displayName", displayName ?? string.Empty);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "userRole", role);

            NotifyAuthenticationStateChanged();
        }


        /// <summary>
        /// Charge le rôle utilisateur depuis la base de données et met à jour l'état d'authentification.
        /// </summary>
        public async Task LoadUserRoleFromDatabase()
        {
            if (!IsAuthenticated || string.IsNullOrEmpty(UserId))
                return;

            try
            {
                // Appel au DatabaseService pour charger les données utilisateur
                var userData = await _httpClient.GetFromJsonAsync<SaveData>($"api/Database/load/{UserId}");

                if (userData != null && !string.IsNullOrEmpty(userData.Role))
                {
                    // Mettre à jour uniquement le rôle et notifier du changement
                    UserRole = userData.Role;
                    await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "userRole", UserRole);
                    NotifyAuthenticationStateChanged();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors du chargement du rôle: {ex.Message}");
            }
        }

        public async Task LoadUserDataFromDatabase()
        {
            if (!IsAuthenticated || string.IsNullOrEmpty(UserId))
                return;

            try
            {
                var userData = await _httpClient.GetFromJsonAsync<FirebaseAuthResult>($"api/Database/load/{UserId}");

                if (userData != null)
                {
                    // Mettre à jour toutes les propriétés en une fois
                    DisplayName = userData.DisplayName ?? DisplayName; // Garde l'ancienne valeur si null
                    UserRole = userData.Role ?? "user"; // Valeur par défaut

                    // Mise à jour synchrone du localStorage
                    await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "displayName", DisplayName);
                    await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "userRole", UserRole);

                    NotifyAuthenticationStateChanged();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors du chargement des données utilisateur: {ex.Message}");
                // Optionnel : Ajouter un Snackbar pour informer l'utilisateur
            }
        }

        /// <summary>
        /// Rafraîchit le rôle de l'utilisateur depuis la base de données.
        /// À appeler après des opérations qui pourraient changer le rôle utilisateur.
        /// </summary>
        public async Task RefreshUserRole()
        {
            if (IsAuthenticated)
            {
                await LoadUserRoleFromDatabase();
            }
        }

        ///// <summary>
        ///// Charge les données utilisateur depuis la base de données.
        ///// </summary>
        ///// <param name="userId"></param>
        ///// <returns></returns>
        //private async Task<SaveData?> LoadUserDataAsync(string userId)
        //{
        //    try
        //    {
        //        return await _httpClient.GetFromJsonAsync<SaveData>($"api/Database/load/{userId}");
        //    }
        //    catch
        //    {
        //        return null;
        //    }
        //}

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

}
