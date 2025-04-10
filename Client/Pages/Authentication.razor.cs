namespace SpiritWeb.Client.Pages
{
    /// <summary>
    /// Classe gérant l'authentification des utilisateurs, incluant l'inscription et la connexion.
    /// </summary>
    public partial class Authentication
    {
        private string email = "";
        private string password = "";
        private string displayName = "";
        private string errorMessage = "";
        private bool isRegistering = false;
        private bool isProcessing = false;

        /// <summary>
        /// Bascule entre le mode d'inscription et le mode de connexion.
        /// </summary>
        private void ToggleMode()
        {
            isRegistering = !isRegistering;
            errorMessage = "";
        }


        /// <summary>
        /// Gère l'authentification de l'utilisateur, que ce soit pour l'inscription ou la connexion.
        /// </summary>
        /// <returns></returns>
        private async Task HandleAuthentication()
        {
            try
            {
                isProcessing = true;
                errorMessage = "";

                if (isRegistering)
                {
                    // Inscrire un nouvel utilisateur avec email, mot de passe et nom d'affichage
                    var success = await AuthService.RegisterWithEmailAndPasswordAsync(email, password, displayName);
                    if (success)
                    {
                        // Attendre que l'ID utilisateur soit disponible
                        while (string.IsNullOrEmpty(AuthService.UserId))
                        {
                            await Task.Delay(100);
                        }

                        // Créer les données initiales pour le nouvel utilisateur
                        await DatabaseService.CreateInitialDataAsync(AuthService.UserId, displayName);

                        // Rediriger seulement après que tout est complet
                        NavigationManager.NavigateTo("/");
                    }
                }
                else
                {
                    // Connecter l'utilisateur avec email et mot de passe
                    await AuthService.SignInWithEmailAndPasswordAsync(email, password);
                    NavigationManager.NavigateTo("/");
                }
            }
            catch (Exception ex)
            {
                errorMessage = TranslateFirebaseError(ex.Message);
            }
            finally
            {
                isProcessing = false;
            }
        }


        /// <summary>
        /// Traduit les messages d'erreur Firebase en messages compréhensibles pour l'utilisateur.
        /// </summary>
        /// <param name="message">Le message d'erreur à traduire.</param>
        /// <returns>Un message d'erreur compréhensible pour l'utilisateur.</returns>
        private string TranslateFirebaseError(string message)
        {
            if (message.Contains("EMAIL_EXISTS"))
                return "Cet email est déjà utilisé.";
            if (message.Contains("INVALID_EMAIL"))
                return "Email invalide.";
            if (message.Contains("WEAK_PASSWORD"))
                return "Le mot de passe doit contenir au moins 6 caractères.";
            if (message.Contains("EMAIL_NOT_FOUND") || message.Contains("INVALID_PASSWORD"))
                return "Email ou mot de passe incorrect.";

            return $"Erreur: {message}";
        }
    }
}
