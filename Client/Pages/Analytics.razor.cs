using MudBlazor;
using SpiritWeb.Shared.Models;

namespace SpiritWeb.Client.Pages
{
    public partial class Analytics
    {
        private List<SurveyModel> suggestions = new List<SurveyModel>();
        private bool isLoading = true;

        // Données pour le graphique de satisfaction
        private double[] satisfactionData = new double[5];
        private string[] satisfactionLabels = { "À améliorer", "Passable", "Bon", "Très bon", "Excellent" };

        // Données pour le graphique de fréquence de jeu
        private double[] frequencyData = new double[5];
        private string[] frequencyLabels = { "Tous les jours", "Plusieurs fois/semaine", "Une fois/semaine", "Quelques fois/mois", "Rarement" };

        // Données pour le graphique des votes
        private double[] voteData;
        private string[] voteLabels;
        private List<SurveyModel> topSuggestions = new List<SurveyModel>();

        // Données pour le graphique de satisfaction par fonctionnalité
        private List<ChartSeries> featureSatisfactionSeries = new List<ChartSeries>();
        private string[] featureLabels;


        /// <summary>
        /// Méthode d'initialisation de la page
        /// </summary>
        /// <returns></returns>
        protected override async Task OnInitializedAsync()
        {
            // Vérifier les droits d'accès (optionnel: limiter aux administrateurs)
            await AuthService.AuthInitialized;

            if (!AuthService.IsAuthenticated ||
                (AuthService.UserRole != "contributor" && AuthService.UserRole != "admin"))
            {
                NavigationManager.NavigateTo("/");
                Snackbar.Add("Vous n'avez pas les droits pour accéder à cette page", Severity.Warning);
                return;
            }

            await LoadData();
        }

        /// <summary>
        /// Méthode pour charger les données de l'enquête
        /// </summary>
        /// <returns></returns>
        private async Task LoadData()
        {
            try
            {
                // Charger toutes les suggestions
                suggestions = await SurveyService.GetAllSuggestionsAsync();

                // Préparer les données pour les graphiques
                PrepareChartData();

                isLoading = false;
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Erreur lors du chargement des données: {ex.Message}", Severity.Error);
                isLoading = false;
            }
        }

        /// <summary>
        /// Préparer les données pour les graphiques
        /// </summary>
        private void PrepareChartData()
        {
            // 1. Données de satisfaction
            for (int i = 0; i < 5; i++)
            {
                satisfactionData[i] = suggestions.Count(s => s.SatisfactionRating == i + 1);
            }

           
            // 2. Données de fréquence de jeu
            var validSuggestions = suggestions.Where(s => s.PlayFrequency >= 1 && s.PlayFrequency <= 5).ToList();
            for (int i = 0; i < 5; i++)
            {
                frequencyData[i] = validSuggestions.Count(s => s.PlayFrequency == i + 1);
            }

            for (int i = 0; i < 5; i++)
            {
                Console.WriteLine($"Label {frequencyLabels[i]} : {frequencyData[i]}");
            }

           
            // 3. Top 5 des suggestions les plus votées
            topSuggestions = suggestions
                .OrderByDescending(s => s.VotesCount)
                .Take(5)
                .ToList();

            voteData = topSuggestions.Select(s => (double)s.VotesCount).ToArray();

            // Tronquer les suggestions trop longues pour l'affichage dans le graphique
            voteLabels = topSuggestions.Select(s => {
                var text = s.OptimizationSuggestion;
                return text.Length > 20 ? text.Substring(0, 17) + "..." : text;
            }).ToArray();

            // 4. Données de satisfaction par fonctionnalité
            PrepareFeatureSatisfactionData();
        }

        /// <summary>
        /// Préparer les données de satisfaction par fonctionnalité
        /// </summary>
        private void PrepareFeatureSatisfactionData()
        {
            // Extraire les fonctionnalités uniques, en ignorant les valeurs nulles
            var features = suggestions
                .Where(s => !string.IsNullOrEmpty(s.FavoriteFeature))
                .Select(s => s.FavoriteFeature)
                .Distinct()
                .OrderBy(f => f)
                .ToArray();

            featureLabels = features;
            featureSatisfactionSeries.Clear();

            // Créer une série pour chaque niveau de satisfaction
            for (int i = 1; i <= 5; i++)
            {
                int satisfactionLevel = i;
                var seriesData = new double[features.Length];

                for (int j = 0; j < features.Length; j++)
                {
                    string feature = features[j];
                    seriesData[j] = suggestions
                        .Count(s => s.FavoriteFeature == feature && s.SatisfactionRating == satisfactionLevel);
                }

                string seriesName = satisfactionLevel switch
                {
                    1 => "À améliorer",
                    2 => "Passable",
                    3 => "Bon",
                    4 => "Très bon",
                    5 => "Excellent",
                    _ => "Inconnu"
                };

                featureSatisfactionSeries.Add(new ChartSeries { Name = seriesName, Data = seriesData });
            }
        }

        /// <summary>
        /// Récupérer la fonctionnalité la plus populaire
        /// </summary>
        /// <returns></returns>
        private string GetMostPopularFeature()
        {
            if (!suggestions.Any())
                return "-";

            return suggestions
                .GroupBy(s => s.FavoriteFeature)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault()?.Key ?? "-";
        }
    }
}
