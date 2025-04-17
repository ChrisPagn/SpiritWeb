using Microsoft.AspNetCore.Components;
using SpiritWeb.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MudBlazor;
using SpiritWeb.Client.Services;

namespace SpiritWeb.Client.Pages
{
    public partial class SuggestionList
    {
        private string userId = "";
        private string userDisplayName = "";

        private List<SurveyModel> suggestions = new List<SurveyModel>();
        private bool isLoading = true;
        private string searchString = "";
        private Dictionary<string, bool> userVotes = new Dictionary<string, bool>();
        public bool buttonDisabled{ get; set; } = false;

        /// <summary>
        /// Initialise la page
        /// </summary>
        /// <returns></returns>
        protected override async Task OnInitializedAsync()
        {
            await LoadSuggestions();
        }


        /// <summary>
        /// Charge toutes les suggestions
        /// </summary>
        /// <returns></returns>
        private async Task LoadSuggestions()
        {
            isLoading = true;
            try
            {
                suggestions = await SurveyService.GetAllSuggestionsAsync();

                // Vérifier les votes de l'utilisateur connecté
                if (AuthService.IsAuthenticated)
                {
                    foreach (var suggestion in suggestions)
                    {
                        userVotes[suggestion.Id] = await VoteService.HasUserVotedAsync(suggestion.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Erreur lors du chargement des suggestions: {ex.Message}", Severity.Error);
            }
            finally
            {
                isLoading = false;
            }
        }

        /// <summary>
        /// Vote pour une suggestion
        /// </summary>
        /// <param name="suggestionId"></param>
        /// <returns></returns>
        private async Task VoteForSuggestion(string suggestionId)
        {
            buttonDisabled = true;

            if (!AuthService.IsAuthenticated)
            {
                Snackbar.Add("Vous devez être connecté pour voter", Severity.Warning);
                buttonDisabled = false;
                return;
            }

            try
            {
                var statusCode = await VoteService.VoteForSuggestionAsync(suggestionId);

                switch (statusCode)
                {
                    case 200:
                        
                        userVotes[suggestionId] = true;

                        // Mettre à jour le compteur de votes
                        var suggestion = suggestions.FirstOrDefault(s => s.Id == suggestionId);
                        if (suggestion != null)
                        {
                            suggestion.VotesCount++;
                        }

                        Snackbar.Add("Votre vote a été enregistré", Severity.Success);
                        StateHasChanged();
                        break;

                    case 0:
                        Snackbar.Add("Vous devez être connecté pour voter", Severity.Warning);
                        break;

                    case 1:
                        Snackbar.Add("Une erreur interne est survenue", Severity.Error);
                        break;

                    case 409:
                        Snackbar.Add("Vous avez déjà voté pour cette suggestion", Severity.Warning);
                        break;

                    case 400:
                        Snackbar.Add("Requête invalide. Veuillez réessayer.", Severity.Error);
                        break;

                    case 500:
                        Snackbar.Add("Erreur interne du serveur. Veuillez réessayer plus tard.", Severity.Error);
                        break;

                    default:
                        Snackbar.Add($"Une erreur inattendue est survenue (Code: {statusCode})", Severity.Error);
                        break;
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Erreur inattendue: {ex.Message}", Severity.Error);
            }
            finally
            {
                buttonDisabled = false;
            }
        }

        /// <summary>
        /// Filtre les suggestions en fonction de la chaîne de recherche
        /// </summary>
        /// <param name="suggestion"></param>
        /// <returns></returns>
        private bool FilterFunc(SurveyModel suggestion)
        {
            if (string.IsNullOrWhiteSpace(searchString))
                return true;

            if (suggestion.OptimizationSuggestion.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                return true;

            if (suggestion.UserDisplayName.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                return true;

            if (suggestion.FavoriteFeature.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        }


        /// <summary>
        /// Actualise les données
        /// </summary>
        /// <returns></returns>
        public async Task RefreshData()
        {
            await LoadSuggestions();
        }
    }
}