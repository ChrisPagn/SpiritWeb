namespace SpiritWeb.Client.Pages
{
    /// <summary>
    /// Ce composant est la page d'administration du tableau de bord.
    /// </summary>
    public partial class DashboardAdmin
    {
        protected override async Task OnInitializedAsync()
        {
            await AuthService.AuthInitialized;

            if (!AuthService.IsAuthenticated || AuthService.UserRole != "admin")
            {
                NavigationManager.NavigateTo("/");
            }
        }

    }
}
