using Microsoft.AspNetCore.Components;
using MudBlazor;
using SpiritWeb.Client.Services;
using SpiritWeb.Shared.Models;
using System;
using System.Threading.Tasks;

namespace SpiritWeb.Client.Pages
{
    public partial class Suggestion
    {
        private SurveyModel surveyModel = new SurveyModel();
        private bool success;
        private string[] errors = { };
        private MudForm form;
        private bool isSubmitting = false;

        

        protected override async Task OnInitializedAsync()
        {
            // Vérifier si l'utilisateur est un contributeur
            await AuthService.AuthInitialized;

            if (!AuthService.IsAuthenticated ||
                (AuthService.UserRole != "contributor" && AuthService.UserRole != "admin"))
            {
                NavigationManager.NavigateTo("/");
                Snackbar.Add("Vous n'avez pas les droits pour accéder à cette page", Severity.Warning);
            }
        }

        private async Task Submit()
        {
            await form.Validate();

            if (success)
            {
                isSubmitting = true;

                try
                {
                    // Compléter le modèle avec les informations utilisateur
                    surveyModel.UserId = AuthService.UserId;
                    //surveyModel.UserDisplayName = AuthService.DisplayName;
                    surveyModel.SubmissionDate = DateTime.UtcNow;

                    // Enregistrer dans Firestore
                    await SaveSurveyToFirestore(surveyModel);

                    Snackbar.Add("Votre suggestion a été enregistrée avec succès!", Severity.Success);

                    // Réinitialiser le formulaire
                    surveyModel = new SurveyModel();
                    form.ResetAsync();
                    StateHasChanged();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erreur lors de l'enregistrement de la suggestion: {ex.Message}");
                    Snackbar.Add("Une erreur est survenue lors de l'enregistrement", Severity.Error);
                }
                finally
                {
                    isSubmitting = false;
                }
            }
        }

        /// <summary>
        /// Enregistre le modèle d'enquête dans Firestore
        /// </summary>
        /// <param name="surveyModel"></param>
        /// <returns></returns>
        private async Task SaveSurveyToFirestore(SurveyModel surveyModel)
        {
            try
            {
                await SurveyService.SaveSurveyAsync(surveyModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors de l'enregistrement : {ex.Message}");
                throw; // Pour que le Snackbar d'erreur dans Submit() soit déclenché
            }
        }
    }
}