using Microsoft.AspNetCore.Components;
using MudBlazor;
using SpiritWeb.Shared.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
        private NewsModel _editingNews = new();
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
            CloseButton = true,
            Position = DialogPosition.Center 
        };

        /// <summary>
        /// Initialise le composant et vérifie les autorisations de l'utilisateur
        /// </summary>
        protected override async Task OnInitializedAsync()
        {
            await AuthService.AuthInitialized;
            if (!AuthService.IsAuthenticated || AuthService.UserRole != "admin")
            {
                NavigationManager.NavigateTo("/");
                return;
            }
            //    await LoadNewsAsync();
            _ = LoadNewsAsync(); // Lance le chargement sans await pour ne pas bloquer l'UI
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
                _newsList = await NewsService.GetAllNewsAsync() ?? new List<NewsModel>(); // Garantit une liste non-nulle
            }
            catch (Exception ex)
            {
                _newsList = new List<NewsModel>(); // Liste vide en cas d'erreur
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
        private async Task OpenNewsDialog(NewsModel news = null)
        {
            Snackbar.Add("Vous pouvez créer une actualité", Severity.Info);
            _editingNews = news ?? new NewsModel
            {
                Id = "0",
                Title = "",
                Content = "",
                AdminId = AuthService.UserId,
                AdminDisplayName = AuthService.DisplayName,
                PublishDate = DateTime.UtcNow,
                LastModified = DateTime.UtcNow,
                IsPinned = false,
                ImagePath = ""
            };

            _showNewsDialog = true;
            await InvokeAsync(StateHasChanged);
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
        /// Valide le modèle d'actualité avant de l'enregistrer
        /// </summary>
        /// <param name="news"></param>
        /// <returns></returns>
        public async Task<bool> ValidateNewsAsync(NewsModel news)
        {
            // Vérifie que le modèle est valide avant sauvegarde
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(news,
                new ValidationContext(news),
                validationResults,
                true);

            if (!isValid)
            {
                // Log ou gestion des erreurs
                return false;
            }

            return true;
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