using Microsoft.AspNetCore.Components;

namespace SpiritWeb.Client.Shared
{
    /// <summary>
    /// Classe représentant la mise en page principale de l'application.
    /// Gère l'état du tiroir de navigation et les interactions utilisateur associées.
    /// </summary>
    public partial class MainLayout
    {
        private bool _drawerOpen = true;

        /// <summary>
        /// Bascule l'état d'ouverture du tiroir de navigation.
        /// </summary>
        private void DrawerToggle()
        {
            _drawerOpen = !_drawerOpen;
        }
    }
}
