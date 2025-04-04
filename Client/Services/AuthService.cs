using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
//using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System;
using SpiritWeb.Shared.Models;
using System.Text.Json;
using Microsoft.JSInterop;
using FirebaseAdmin.Auth;
using System.Text.Json.Serialization;

namespace SpiritWeb.Client.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private readonly IJSRuntime _jsRuntime;

        public string UserId { get; private set; }
        public string UserEmail { get; private set; }
        public string DisplayName { get; private set; }
        public bool IsAuthenticated { get; private set; }
        public Task AuthInitialized { get; private set; }

        public event Action? OnAuthStateChanged;

        public void SetAuthenticationState(bool isAuthenticated)
        {
            IsAuthenticated = isAuthenticated;
            NotifyAuthenticationStateChanged();
        }

        private void NotifyAuthenticationStateChanged()
        {
            OnAuthStateChanged?.Invoke();
        }
        public AuthService(HttpClient httpClient, IJSRuntime jsRuntime)
        {
            _httpClient = httpClient;
            _jsRuntime = jsRuntime;
            AuthInitialized = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            await LoadAuthState();
        }

        // Chargement de l'état d'authentification depuis le stockage local
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

        // Enregistrement d'un nouvel utilisateur
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

        // Connexion d'un utilisateur existant
        //public async Task<FirebaseAuthResult> SignInWithEmailAndPasswordAsync(string email, string password)
        //{
        //    try
        //    {
        //        var response = await _httpClient.PostAsync(
        //            $"api/Auth/login?email={Uri.EscapeDataString(email)}&password={Uri.EscapeDataString(password)}",
        //            null);

        //        response.EnsureSuccessStatusCode();
        //        var content = await response.Content.ReadAsStringAsync();
        //        var authResult = JsonSerializer.Deserialize<FirebaseAuthResult>(content);

        //        if (authResult != null)
        //        {
        //            await SetAuthData(authResult.Token, authResult.UserId ?? authResult.User?.LocalId, email, null);
        //            return authResult;
        //        }
        //        return null;
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Erreur de connexion : {ex.Message}");
        //        throw;
        //    }
        //}

        public async Task<FirebaseAuthResult> SignInWithEmailAndPasswordAsync(string email, string password)
        {
            try
            {
                var response = await _httpClient.PostAsync(
                    $"api/Auth/login?email={Uri.EscapeDataString(email)}&password={Uri.EscapeDataString(password)}",
                    null);

                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();

                // Ajoutez des options de désérialisation
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

        // Déconnexion
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

        // Stockage des données d'authentification
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
    }

    // Classe pour le résultat de l'authentification Firebase
    public class FirebaseAuthResult
    {
        [JsonPropertyName("token")]
        public string Token { get; set; }
        
        [JsonPropertyName("localId")]
        public string UserId { get; set; }  // Doit correspondre à LocalId dans la réponse Firebase

        [JsonPropertyName("user")]
        public FirebaseUser User { get; set; }  // Ajoutez cette propriété si nécessaire
    }

    public class FirebaseUser
    {
        public string LocalId { get; set; }
    }
}