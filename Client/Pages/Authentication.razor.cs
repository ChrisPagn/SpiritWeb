namespace SpiritWeb.Client.Pages
{
    public partial class Authentication
    {
        private string email = "";
        private string password = "";
        private string displayName = "";
        private string errorMessage = "";
        private bool isRegistering = false;
        private bool isProcessing = false;

        private void ToggleMode()
        {
            isRegistering = !isRegistering;
            errorMessage = "";
        }

        private async Task HandleAuthentication()
        {
            try
            {
                isProcessing = true;
                errorMessage = "";

                if (isRegistering)
                {
                    var success = await AuthService.RegisterWithEmailAndPasswordAsync(email, password, displayName);
                    if (success)
                    {
                        // Créer les données initiales
                        await DatabaseService.CreateInitialDataAsync(AuthService.UserId, displayName);
                    }
                }
                else
                {
                    await AuthService.SignInWithEmailAndPasswordAsync(email, password);
                }

                // Rediriger vers la page d'accueil
                NavigationManager.NavigateTo("/");
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
