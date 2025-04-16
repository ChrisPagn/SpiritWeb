namespace SpiritWeb.Client.Pages
{
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
