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
                Console.WriteLine($"UserData loaded: {userData.UserId}, {userData.DisplayName}");
                originalData = CloneSaveData(userData);
                Console.WriteLine($"OriginalData cloned: {originalData.UserId}, {originalData.DisplayName}");
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
        //private async Task SaveChanges()
        //{
        //    try
        //    {
        //        userData.LastModified = DateTime.UtcNow;
        //        await DatabaseService.SaveDataAsync(userData);
        //        originalData = CloneSaveData(userData);
        //        isEditing = false;
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"Erreur lors de la sauvegarde des modifications: {ex.Message}");
        //    }
        //}

        private async Task SaveChanges()
        {
            try
            {
                Console.WriteLine($"Sauvegarde pour {userData.UserId}");
                userData.LastModified = DateTime.UtcNow;
                await DatabaseService.SaveDataAsync(userData); 
                originalData = CloneSaveData(userData);
                isEditing = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur complète: {ex}");
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
