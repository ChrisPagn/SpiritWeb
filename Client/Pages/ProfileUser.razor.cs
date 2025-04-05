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
                return;
            }

            try
            {
                userData = await DatabaseService.LoadDataAsync(AuthService.UserId);
                originalData = CloneSaveData(userData);
                isLoading = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors du chargement des données: {ex.Message}");
                isLoading = false;
            }
        }

        /// <summary>
        /// Clone les données de sauvegarde d'un utilisateur.
        /// </summary>
        /// <param name="source">Les données source à cloner.</param>
        /// <returns>Une nouvelle instance de SaveData avec les mêmes valeurs que la source.</returns>
        private SaveData CloneSaveData(SaveData source)
        {
            return new SaveData
            {
                UserId = source.UserId,
                DisplayName = source.DisplayName,
                CoinsCount = source.CoinsCount,
                LevelReached = source.LevelReached,
                LastLevelPlayed = source.LastLevelPlayed,
                InventoryItems = new List<int>(source.InventoryItems),
                InventoryItemsName = new List<string>(source.InventoryItemsName),
                LastModified = source.LastModified
            };
        }

        /// <summary>
        /// Démarre l'édition des données utilisateur.
        /// </summary>
        private void StartEdit()
        {
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

        /// <summary>
        /// Sauvegarde les modifications apportées aux données utilisateur.
        /// </summary>
        private async Task SaveChanges()
        {
            try
            {
                userData.LastModified = DateTime.UtcNow;
                await DatabaseService.SaveDataAsync(userData);
                originalData = CloneSaveData(userData);
                isEditing = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de la sauvegarde des modifications: {ex.Message}");
            }
        }

        /// <summary>
        /// Déconnecte l'utilisateur et redirige vers la page d'authentification.
        /// </summary>
        private async Task SignOut()
        {
            await AuthService.SignOut();
            NavigationManager.NavigateTo("/authentication");
        }
    }
}
