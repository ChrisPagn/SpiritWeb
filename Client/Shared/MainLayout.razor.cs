using Microsoft.AspNetCore.Components;

namespace SpiritWeb.Client.Shared
{
    /// <summary>
    /// Classe représentant la mise en page principale de l'application.
    /// Gère l'état du tiroir de navigation et les interactions utilisateur associées.
    /// </summary>
    public partial class MainLayout
    {
        private bool _drawerOpen = false;

        /// <summary>
        /// Bascule l'état d'ouverture du tiroir de navigation.
        /// </summary>
        private void DrawerToggle()
        {
            _drawerOpen = !_drawerOpen;
        }

        /// <summary>
        /// Gère l'événement de clic sur le tiroir de navigation.
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();
            // Vous pouvez ajouter ici toute logique supplémentaire nécessaire lors de l'initialisation
            StateHasChanged();
        }

        /// <summary>
        /// Ferme le tiroir de navigation.
        /// </summary>
        private void DrawerClose()
        {
            _drawerOpen = false;
            StateHasChanged();
        }
    }
}
