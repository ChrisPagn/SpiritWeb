using Microsoft.AspNetCore.Components;
using MudBlazor;
using SpiritWeb.Shared.Models;


namespace SpiritWeb.Client.Pages
{
    /// <summary>
    /// Classe représentant le profil utilisateur dans l'application.
    /// Gère le chargement, l'édition et la sauvegarde des données utilisateur.
    /// </summary>
    public partial class ProfileUser
    {
        private SaveData userData;
        private SaveData originalData;
        private bool isLoading = true;
        private bool isEditing = false;

        /// <summary>
        /// Méthode appelée lors de l'initialisation du composant.
        /// Charge les données utilisateur si l'utilisateur est authentifié.
        /// </summary>
        protected override async Task OnInitializedAsync()
        {
            await AuthService.AuthInitialized; // Attend que l'authentification soit prête

            if (!AuthService.IsAuthenticated)
            {
                NavigationManager.NavigateTo("/authentication");
                Snackbar.Add("Veuillez vous connecter avant d'afficher votre profil!", Severity.Warning);
                return;
            }

            try
            {
                userData = await DatabaseService.LoadDataAsync(AuthService.UserId);

                Console.WriteLine($"UserData loaded: {userData.UserId}, {userData.DisplayName}");
                originalData = CloneSaveData(userData);
                Console.WriteLine($"OriginalData cloned: {originalData.UserId}, {originalData.DisplayName}");
                isLoading = false;
            }
            catch (Exception ex)
            {
                Snackbar.Add("Échec du chargement du profil. Veuillez réessayer.", Severity.Error);
                isLoading = false;
            }
        }

        /// <summary>
        /// Clone les données de sauvegarde d'un utilisateur.
        /// </summary>
        /// <param name="source">Les données source à cloner.</param>
        /// <returns>Une nouvelle instance de SaveData avec les mêmes valeurs que la source.</returns>
        private static SaveData CloneSaveData(SaveData source)
        {
            if (source == null) return null;

            return new SaveData
            {
                UserId = source.UserId,
                DisplayName = source.DisplayName,
                CoinsCount = source.CoinsCount,
                LevelReached = source.LevelReached,
                LastLevelPlayed = source.LastLevelPlayed,
                InventoryItems = source.InventoryItems != null ? new List<int>(source.InventoryItems) : new List<int>(),
                InventoryItemsName = source.InventoryItemsName != null ? new List<string>(source.InventoryItemsName) : new List<string>(),
                LastModified = source.LastModified
            };
        }


        /// <summary>
        /// Démarre l'édition des données utilisateur.
        /// </summary>
        private void StartEdit()
        {
            Snackbar.Add("Vous ne pouvez modifier que votre nom d'affichage!", Severity.Info);

            isEditing = true;
        }

        /// <summary>
        /// Annule les modifications et restaure les données originales.
        /// </summary>
        private void CancelEdit()
        {
            userData = CloneSaveData(originalData);
            isEditing = false;
        }


        private async Task SaveChanges()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userData.DisplayName))
                {
                    Snackbar.Add("Le nom d'affichage ne peut pas être vide", Severity.Warning);
                    return;
                }
                
                Snackbar.Add("Sauvegarde de vos données en cours... ", Severity.Info);

                userData.LastModified = DateTime.UtcNow;
                await DatabaseService.SaveDataAsync(userData); 
                originalData = CloneSaveData(userData);
                Snackbar.Add($"Sauvegarde réussie pour {userData.DisplayName} !", Severity.Success);
                isEditing = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur complète: {ex}");
                Snackbar.Add("Échec de la sauvegarde. Veuillez réessayer.", Severity.Error);
            }
        }

        /// <summary>
        /// Déconnecte l'utilisateur et redirige vers la page d'authentification.
        /// </summary>
        private async Task SignOut()
        {
            await AuthService.SignOut();
            Snackbar.Add("Vous avez été déconnecté avec succès", Severity.Info);
            NavigationManager.NavigateTo("/authentication");
        }

        /// <summary>
        /// Récupère l'ID utilisateur abrégé pour l'affichage.
        /// </summary>
        /// <param name="fullId"></param>
        /// <returns></returns>
        private string GetShortUserId(string fullId)
        {
            if (string.IsNullOrEmpty(fullId) || fullId.Length <= 6)
                return fullId;

            return $"...{fullId.Substring(fullId.Length - 6)}";
        }
    }
}
