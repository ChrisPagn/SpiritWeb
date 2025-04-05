using MudBlazor;
using SpiritWeb.Shared.Models;

namespace SpiritWeb.Client.Pages
{
    public partial class UsersList
    {
        private List<SaveData> users = new();
        private string searchString = string.Empty;
        private bool isLoading = true;

        private List<SaveData> filteredUsers => string.IsNullOrWhiteSpace(searchString)
             ? users
             : users.Where(u =>
                 (u.DisplayName?.Contains(searchString, StringComparison.OrdinalIgnoreCase) ?? false)
                 || u.LevelReached.ToString().Contains(searchString)
                 || u.CoinsCount.ToString().Contains(searchString)
                 || (u.LastLevelPlayed?.ToString().Contains(searchString) ?? false)
             ).ToList();


        protected override async Task OnInitializedAsync()
        {
            await AuthService.AuthInitialized;

            if (!AuthService.IsAuthenticated)
            {
                NavigationManager.NavigateTo("/authentication");
                return;
            }

            try
            {
                users = await DatabaseService.GetAllUsersAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors du chargement des utilisateurs: {ex.Message}");
            }
            isLoading = false;
        }

        private bool TableFilter(SaveData user)
        {
            if (string.IsNullOrWhiteSpace(searchString))
                return true;

            return (user.DisplayName?.Contains(searchString, StringComparison.OrdinalIgnoreCase) ?? false)
                   || user.LevelReached.ToString().Contains(searchString)
                   || user.CoinsCount.ToString().Contains(searchString)
                   || (user.LastLevelPlayed?.ToString().Contains(searchString) ?? false);
        }


        private Task<TableData<SaveData>> LoadServerData(TableState state, CancellationToken cancellationToken)
        {
            var data = filteredUsers
                .Skip(state.Page * state.PageSize)
                .Take(state.PageSize)
                .ToList();

            return Task.FromResult(new TableData<SaveData>()
            {
                TotalItems = filteredUsers.Count,
                Items = data
            });
        }

        string GetRowClass(SaveData user, int index)
        {
            return user.LevelReached >= 10 ? "high-level" : "low-level";
        }

    }
}
