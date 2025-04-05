using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace SpiritWeb.Client.Pages
{
    /// <summary>
    /// Composant Blazor pour gérer les transitions de page avec des animations.
    /// </summary>
    public partial class PageTransitionLayout : IDisposable
    {
        /// <summary>
        /// Contenu enfant du composant.
        /// </summary>
        [Parameter]
        public RenderFragment? ChildContent { get; set; }

        /// <summary>
        /// Indique si le composant est en cours de chargement.
        /// </summary>
        public bool IsLoading { get; private set; }

        /// <summary>
        /// Classe CSS actuelle pour les animations.
        /// </summary>
        private string CurrentClass = "";

        /// <summary>
        /// Classe CSS du wrapper.
        /// </summary>
        private string WrapperClass = "";

        /// <summary>
        /// URI actuelle.
        /// </summary>
        private string CurrentUri = "";

        /// <summary>
        /// Méthode appelée lorsque le composant est initialisé.
        /// </summary>
        protected override void OnInitialized()
        {
            // Enregistre l'URI actuelle
            CurrentUri = NavigationManager.Uri;
            // S'abonne à l'événement LocationChanged du NavigationManager
            NavigationManager.LocationChanged += OnLocationChanged;
        }

        /// <summary>
        /// Méthode appelée lorsque l'URI change. Gère les animations de transition de page.
        /// </summary>
        /// <param name="sender">L'objet source de l'événement.</param>
        /// <param name="e">Les arguments de l'événement LocationChanged.</param>
        private async void OnLocationChanged(object? sender, LocationChangedEventArgs e)
        {
            // Indique que le composant est en cours de chargement
            IsLoading = true;
            StateHasChanged();

            // Animation de sortie
            CurrentClass = "page-transition-leave";
            StateHasChanged();

            // Attendre la fin de l'animation de sortie
            await Task.Delay(600);

            // Changer l'URL
            CurrentUri = e.Location;

            // Animation d'entrée
            CurrentClass = "page-transition-enter";
            WrapperClass = "loading";
            StateHasChanged();

            // Simuler un temps de chargement
            await Task.Delay(1000);

            // Fin du chargement
            WrapperClass = "";
            IsLoading = false;
            StateHasChanged();
        }

        /// <summary>
        /// Méthode appelée lorsque le composant est détruit. Libère les ressources.
        /// </summary>
        public void Dispose()
        {
            // Se désabonne de l'événement LocationChanged du NavigationManager
            NavigationManager.LocationChanged -= OnLocationChanged;
        }
    }
}
