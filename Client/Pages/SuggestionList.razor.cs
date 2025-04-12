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

        protected override async Task OnInitializedAsync()
        {
            await LoadSuggestions();
        }

        private async Task LoadSuggestions()
        {
            isLoading = true;
            try
            {
                suggestions = await SurveyService.GetAllSuggestionsAsync();

                // Vérifier les votes de l'utilisateur connecté
                if (AuthService.IsAuthenticated)
                {
                    //foreach (var suggestion in suggestions)
                    //{
                    //    userVotes[suggestion.Id] = await SurveyService.HasUserVotedAsync(suggestion.Id);
                    //}
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

        private async Task VoteForSuggestion(string suggestionId)
        {
            if (!AuthService.IsAuthenticated)
            {
                Snackbar.Add("Vous devez être connecté pour voter", Severity.Warning);
                return;
            }

            try
            {
                bool success = await SurveyService.VoteForSuggestionAsync(suggestionId);
                if (success)
                {
                    // Mettre à jour l'interface utilisateur
                    userVotes[suggestionId] = true;

                    // Mettre à jour le compteur de votes
                    //var suggestion = suggestions.FirstOrDefault(s => s.Id == suggestionId);
                    //if (suggestion != null)
                    //{
                    //    //suggestion.VotesCount++;
                    //}

                    Snackbar.Add("Votre vote a été enregistré", Severity.Success);
                    StateHasChanged();
                }
                else
                {
                    Snackbar.Add("Une erreur est survenue lors de l'enregistrement du vote", Severity.Error);
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Erreur: {ex.Message}", Severity.Error);
            }
        }

        private bool FilterFunc(SurveyModel suggestion)
        {
            if (string.IsNullOrWhiteSpace(searchString))
                return true;

            if (suggestion.OptimizationSuggestion.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                return true;

            //if (suggestion.UserDisplayName.Contains(searchString, StringComparison.OrdinalIgnoreCase))
            //    return true;

            if (suggestion.FavoriteFeature.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        }

        public async Task RefreshData()
        {
            await LoadSuggestions();
        }
    }
}