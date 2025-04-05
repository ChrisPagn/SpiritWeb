namespace SpiritWeb.Client.Shared
{
    public partial class NavMenu
    {
        protected override async Task OnInitializedAsync()
        {
            AuthService.OnAuthStateChanged += OnAuthStateChanged;
            await AuthService.AuthInitialized; // Attendre que l'initialisation de l'authentification soit terminée
            StateHasChanged(); // Forcer un re-rendu initial
        }

        private void OnAuthStateChanged()
        {
            InvokeAsync(StateHasChanged);
        }

        public void Dispose()
        {
            AuthService.OnAuthStateChanged -= OnAuthStateChanged;
        }
    }
}
