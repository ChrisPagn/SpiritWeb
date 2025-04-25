using Microsoft.AspNetCore.Components;
using SpiritWeb.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SpiritWeb.Client.Pages
{
    public partial class News
    {
        private List<NewsModel> _newsList;
        private bool _loading = true;
        private string _errorMessage;

        protected override async Task OnInitializedAsync()
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
                _errorMessage = $"Erreur lors du chargement des actualités: {ex.Message}";
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                _loading = false;
                StateHasChanged();
            }
        }

        private string FormatContent(string content)
        {
            if (string.IsNullOrEmpty(content))
                return string.Empty;

            // Convertir les URLs en liens cliquables
            var urlRegex = new Regex(@"(https?:\/\/[^\s]+)", RegexOptions.Compiled);
            content = urlRegex.Replace(content, "<a href=\"$1\" target=\"_blank\" rel=\"noopener noreferrer\">$1</a>");

            // Convertir les sauts de ligne en <br>
            content = content.Replace("\n", "<br>");

            return content;
        }
    }
}