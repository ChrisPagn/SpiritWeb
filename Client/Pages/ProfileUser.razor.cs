using SpiritWeb.Shared.Models;

namespace SpiritWeb.Client.Pages
{
    public partial class ProfileUser
    {
        private SaveData userData;
        private SaveData originalData;
        private bool isLoading = true;
        private bool isEditing = false;

        protected override async Task OnInitializedAsync()
        {
            await AuthService.AuthInitialized; // Attendez que l'auth soit prête

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

        private void StartEdit()
        {
            isEditing = true;
        }

        private void CancelEdit()
        {
            userData = CloneSaveData(originalData);
            isEditing = false;
        }

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

        private async Task SignOut()
        {
            await AuthService.SignOut();
            NavigationManager.NavigateTo("/authentication");
        }
    }
}
