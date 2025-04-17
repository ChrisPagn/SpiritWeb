using MudBlazor;
using SpiritWeb.Shared.Models;

namespace SpiritWeb.Client.Pages
{
    /// <summary>
    /// Classe représentant la liste des utilisateurs dans l'application.
    /// Gère le chargement, le filtrage et l'affichage des utilisateurs.
    /// </summary>
    public partial class UsersList
    {
        private List<SaveData> users = new();
        private SaveData? selectedUser;
        private string searchString = string.Empty;
        private bool isLoading = true;

        /// <summary>
        /// Liste des utilisateurs filtrés en fonction de la chaîne de recherche.
        /// </summary>
        private List<SaveData> filteredUsers => string.IsNullOrWhiteSpace(searchString)
             ? users
             : users.Where(u =>
                 (u.DisplayName?.Contains(searchString, StringComparison.OrdinalIgnoreCase) ?? false)
                 || u.LevelReached.ToString().Contains(searchString)
                 || u.CoinsCount.ToString().Contains(searchString)
                 || (u.LastLevelPlayed?.ToString().Contains(searchString) ?? false)
             ).ToList();

        /// <summary>
        /// Méthode appelée lors de l'initialisation du composant.
        /// Charge les utilisateurs depuis la base de données si l'utilisateur est authentifié.
        /// </summary>
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

        /// <summary>
        /// Filtre les utilisateurs en fonction de la chaîne de recherche.
        /// </summary>
        /// <param name="user">L'utilisateur à filtrer.</param>
        /// <returns>True si l'utilisateur correspond aux critères de recherche, sinon False.</returns>
        private bool TableFilter(SaveData user)
        {
            if (string.IsNullOrWhiteSpace(searchString))
                return true;

            return (user.DisplayName?.Contains(searchString, StringComparison.OrdinalIgnoreCase) ?? false)
                   || user.LevelReached.ToString().Contains(searchString)
                   || user.CoinsCount.ToString().Contains(searchString)
                   || (user.LastLevelPlayed?.ToString().Contains(searchString) ?? false);
        }

        /// <summary>
        /// Charge les données de la table en fonction de l'état de pagination.
        /// </summary>
        /// <param name="state">L'état actuel de la table, incluant la page et la taille de la page.</param>
        /// <param name="cancellationToken">Le jeton d'annulation.</param>
        /// <returns>Les données de la table pour la page actuelle.</returns>
        private async Task<TableData<SaveData>> LoadServerData(TableState state, CancellationToken cancellationToken)
        {
            try
            {
                // Version côté client avec filtrage
                var filtered = filteredUsers;
                var totalItems = filtered.Count;
                var pagedData = filtered
                    .Skip(state.Page * state.PageSize)
                    .Take(state.PageSize)
                    .ToList();

                return new TableData<SaveData>()
                {
                    TotalItems = totalItems,
                    Items = pagedData
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors du chargement des données: {ex.Message}");
                return new TableData<SaveData>()
                {
                    TotalItems = 0,
                    Items = new List<SaveData>()
                };
            }
        }

        /// <summary>
        /// Détermine la classe CSS à appliquer à une ligne de la table en fonction du niveau atteint par l'utilisateur.
        /// </summary>
        /// <param name="user">L'utilisateur pour lequel déterminer la classe CSS.</param>
        /// <param name="index">L'index de la ligne.</param>
        /// <returns>La classe CSS à appliquer à la ligne.</returns>
        string GetRowClass(SaveData user, int index)
        {
            return user.LevelReached >= 10 ? "high-level" : "low-level";
        }
    }
}
