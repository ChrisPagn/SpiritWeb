using Microsoft.AspNetCore.Components;
using SpiritWeb.Client.Services;
using SpiritWeb.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritWeb.Client.Pages
{
    /// <summary>
    /// Composant page d'accueil qui affiche l'animation du personnage et les actualités récentes
    /// </summary>
    public partial class Index
    {
        private List<NewsModel> _latestNews;
        private List<NewsModel> _newsList;
        private bool _loadingNews = true;

        [Inject]
        private NewsService NewsService { get; set; } // Injection de dépendance pour NewsService

        /// <summary>
        /// Initialise le composant et charge les dernières actualités
        /// </summary>
        protected override async Task OnInitializedAsync()
        {
            await LoadLatestNewsAsync();
        }

        /// <summary>
        /// Charge les 3 dernières actualités
        /// </summary>
        private async Task LoadLatestNewsAsync()
        {
            try
            {
                _loadingNews = true;

                // Récupérer toutes les actualités
                var allNews = await NewsService.GetAllNewsAsync();

                // Filtrer les actualités épinglées et les 3 plus récentes
                var pinnedNews = allNews.Where(n => n.IsPinned).ToList();
                var recentNews = allNews.Where(n => !n.IsPinned)
                                      .OrderByDescending(n => n.PublishDate)
                                      .Take(3 - Math.Min(2, pinnedNews.Count)) // Prendre les plus récentes, en laissant de la place pour au max 2 épinglées
                                      .ToList();

                // Combiner les actualités épinglées et récentes
                _latestNews = pinnedNews.Take(2) // Au maximum 2 actualités épinglées
                            .Concat(recentNews)
                            .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors du chargement des actualités récentes: {ex.Message}");
                _latestNews = new List<NewsModel>();
            }
            finally
            {
                _loadingNews = false;
                StateHasChanged();
            }
        }
    }
}