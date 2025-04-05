namespace SpiritWeb.Client.Shared
{
    /// <summary>
    /// Classe représentant le menu de navigation de l'application.
    /// Gère l'état d'authentification et met à jour l'interface utilisateur en conséquence.
    /// </summary>
    public partial class NavMenu
    {
        /// <summary>
        /// Méthode appelée lors de l'initialisation du composant.
        /// S'abonne aux changements d'état d'authentification et force un re-rendu initial.
        /// </summary>
        protected override async Task OnInitializedAsync()
        {
            AuthService.OnAuthStateChanged += OnAuthStateChanged;
            await AuthService.AuthInitialized; // Attendre que l'initialisation de l'authentification soit terminée
            StateHasChanged(); // Forcer un re-rendu initial
        }

        /// <summary>
        /// Gère les changements d'état d'authentification.
        /// </summary>
        private void OnAuthStateChanged()
        {
            InvokeAsync(StateHasChanged);
        }

        /// <summary>
        /// Libère les ressources en se désabonnant des événements d'état d'authentification.
        /// </summary>
        public void Dispose()
        {
            AuthService.OnAuthStateChanged -= OnAuthStateChanged;
        }
    }
}
