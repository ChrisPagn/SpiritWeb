using Microsoft.AspNetCore.Components;
using MudBlazor;
using SpiritWeb.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpiritWeb.Client.Pages
{
    /// <summary>
    /// Ce composant est la page d'administration des actualités du jeu Spirit
    /// </summary>
    public partial class NewsAdmin
    {
        private List<NewsModel> _newsList;
        private NewsModel _editingNews;
        private NewsModel _deletingNews;
        private bool _loading = true;
        private bool _showNewsDialog;
        private bool _showDeleteDialog;
        private bool _formIsValid;
        private string _errorMessage;
        private MudForm _form;

        // Options de configuration pour les dialogues
        private DialogOptions dialogOptions = new DialogOptions
        {
            CloseOnEscapeKey = true,
            MaxWidth = MaxWidth.Large,
            FullWidth = true,
            //BackdropClick = true,
            CloseButton = true,
        };

        /// <summary>
        /// Initialise le composant et vérifie les autorisations de l'utilisateur
        /// </summary>
        protected override async Task OnInitializedAsync()
        {
            await AuthService.AuthInitialized;

            // Vérification des permissions administrateur
            if (!AuthService.IsAuthenticated || AuthService.UserRole != "admin")
            {
                NavigationManager.NavigateTo("/");
                return;
            }

            await LoadNewsAsync();
        }

        /// <summary>
        /// Charge la liste des actualités
        /// </summary>
        private async Task LoadNewsAsync()
        {
            try
            {
                _loading = true;
                _errorMessage = null;

                _newsList = await NewsService.GetAllNewsAsync();

                // Tri : d'abord les actualités épinglées, puis par date de publication décroissante
                _newsList = _newsList
                    .OrderByDescending(n => n.IsPinned)
                    .ThenByDescending(n => n.PublishDate)
                    .ToList();
            }
            catch (Exception ex)
            {
                //Snackbar.Add($"Erreur lors du chargement des actualités: {ex.Message}", Severity.Error);
                Snackbar.Add($"Attention erreur lors du chargement ou vous n'avez pas encore créé d'actualités ! ", Severity.Error);
            }
            finally
            {
                _loading = false;
                StateHasChanged();
            }
        }

        /// <summary>
        /// Ouvre la boîte de dialogue pour créer ou modifier une actualité
        /// </summary>
        /// <param name="news">Actualité à modifier (null pour une création)</param>
        private void OpenNewsDialog(NewsModel news = null)
        {
            Snackbar.Add("Vous pouvez créer une actualité", Severity.Info);
            if (news == null)
            {
                // Création d'une nouvelle actualité
                _editingNews = new NewsModel
                {
                    AdminId = AuthService.UserId,
                    AdminDisplayName = AuthService.DisplayName,
                    PublishDate = DateTime.UtcNow,
                    LastModified = DateTime.UtcNow,
                    IsPinned = false
                };
            }
            else
            {
                // Modification d'une actualité existante (clone pour éviter les modifications directes)
                _editingNews = new NewsModel
                {
                    Id = news.Id,
                    Title = news.Title,
                    Content = news.Content,
                    PublishDate = news.PublishDate,
                    AdminId = AuthService.UserId,  // Mise à jour avec l'administrateur actuel
                    AdminDisplayName = AuthService.DisplayName,
                    LastModified = DateTime.UtcNow,
                    IsPinned = news.IsPinned,
                    ImagePath = news.ImagePath
                };
            }

            _showNewsDialog = true;
            StateHasChanged();
        }

        /// <summary>
        /// Ferme la boîte de dialogue d'édition
        /// </summary>
        private void CloseDialog()
        {
            _showNewsDialog = false;
        }

        /// <summary>
        /// Ouvre la boîte de dialogue de confirmation de suppression
        /// </summary>
        /// <param name="news">Actualité à supprimer</param>
        private void OpenDeleteDialog(NewsModel news)
        {
            _deletingNews = news;
            _showDeleteDialog = true;
        }

        /// <summary>
        /// Ferme la boîte de dialogue de confirmation de suppression
        /// </summary>
        private void CloseDeleteDialog()
        {
            _showDeleteDialog = false;
            _deletingNews = null;
        }

        /// <summary>
        /// Enregistre l'actualité (création ou modification)
        /// </summary>
        private async Task SaveNewsAsync()
        {
            try
            {
                _form.Validate();

                if (!_formIsValid)
                    return;

                bool success;
                string message;

                if (string.IsNullOrEmpty(_editingNews.Id) || _editingNews.Id == "0")
                {
                    // Création d'une nouvelle actualité
                    string newsId = await NewsService.AddNewsAsync(_editingNews);
                    success = !string.IsNullOrEmpty(newsId);
                    message = success ? "Actualité créée avec succès" : "Erreur lors de la création de l'actualité";
                }
                else
                {
                    // Mise à jour d'une actualité existante
                    success = await NewsService.UpdateNewsAsync(_editingNews);
                    message = success ? "Actualité mise à jour avec succès" : "Erreur lors de la mise à jour de l'actualité";
                }

                if (success)
                {
                    Snackbar.Add(message, Severity.Success);
                    CloseDialog();
                    await LoadNewsAsync();
                }
                else
                {
                    Snackbar.Add(message, Severity.Error);
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Erreur: {ex.Message}", Severity.Error);
                Console.WriteLine(ex.ToString());
            }
        }

        /// <summary>
        /// Supprime une actualité
        /// </summary>
        private async Task DeleteNewsAsync()
        {
            try
            {
                if (_deletingNews == null)
                    return;

                bool success = await NewsService.DeleteNewsAsync(_deletingNews.Id);

                if (success)
                {
                    Snackbar.Add("Actualité supprimée avec succès", Severity.Success);
                    CloseDeleteDialog();
                    await LoadNewsAsync();
                }
                else
                {
                    Snackbar.Add("Erreur lors de la suppression de l'actualité", Severity.Error);
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Erreur: {ex.Message}", Severity.Error);
                Console.WriteLine(ex.ToString());
            }
        }

        /// <summary>
        /// Change le statut épinglé d'une actualité
        /// </summary>
        /// <param name="news">Actualité concernée</param>
        /// <param name="isPinned">Nouveau statut d'épinglage</param>
        private async Task TogglePin(NewsModel news, bool isPinned)
        {
            try
            {
                bool success = await NewsService.TogglePinNewsAsync(news.Id, isPinned);

                if (success)
                {
                    news.IsPinned = isPinned; // Mise à jour locale pour l'UI

                    string message = isPinned ? "Actualité épinglée avec succès" : "Actualité désépinglée avec succès";
                    Snackbar.Add(message, Severity.Success);

                    // Recharger les actualités pour maintenir l'ordre correct
                    await LoadNewsAsync();
                }
                else
                {
                    Snackbar.Add("Erreur lors du changement du statut d'épinglage", Severity.Error);
                }
            }
            catch (Exception ex)
            {
                Snackbar.Add($"Erreur: {ex.Message}", Severity.Error);
                Console.WriteLine(ex.ToString());
            }
        }
    }
}