using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace SpiritWeb.Client.Shared
{
    /// <summary>
    /// Classe représentant le menu de navigation de l'application.
    /// Gère l'état d'authentification et met à jour l'interface utilisateur en conséquence.
    /// </summary>
    public partial class NavMenu
    {
        /// <summary>
        /// Service d'authentification injecté pour gérer l'état d'authentification de l'utilisateur.
        /// </summary>
        [Parameter]
        public EventCallback OnLinkClick { get; set; }
        public bool _isMenuExpanded { get; set; }= true; // État par défaut


        /// <summary>
        /// Méthode appelée lors de l'initialisation du composant.
        /// S'abonne aux changements d'état d'authentification et force un re-rendu initial.
        /// </summary>
        protected override async Task OnInitializedAsync()
        {
            AuthService.OnAuthStateChanged += OnAuthStateChanged;
            await AuthService.AuthInitialized; // Attendre que l'initialisation de l'authentification soit terminée

            Navigation.LocationChanged += OnLocationChanged;
            StateHasChanged(); // Forcer un re-rendu initial
        }

        private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
        {
            Console.WriteLine($"Navigation détectée vers : {e.Location} && statut de menu : {_isMenuExpanded}");

            // Fermer automatiquement le menu ici
            _isMenuExpanded = false;
            StateHasChanged(); // Mettre à jour l'affichage si besoin
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
            Navigation.LocationChanged -= OnLocationChanged;

        }

        private async Task HandleNavClick()
        {
            // 1. Ferme le drawer parent (via l'event du MainLayout)
            if (OnLinkClick.HasDelegate)
                await OnLinkClick.InvokeAsync();

            // 2. Ferme le NavMenu lui-même
            _isMenuExpanded = false;
            StateHasChanged(); // Force le rafraîchissement
        }
    }
}
