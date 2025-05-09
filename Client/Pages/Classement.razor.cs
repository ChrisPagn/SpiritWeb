﻿using MudBlazor;
using SpiritWeb.Shared.Models;

namespace SpiritWeb.Client.Pages
{
    public partial class Classement
    {
        private List<SaveData> users = new();
        private bool isLoading = true;
        private int activeTabIndex = 0;
        //private int currentPageLevel = 1;
        //private int currentPageCoins = 1;
        //private int currentPageItems = 1;

        protected override async Task OnInitializedAsync()
        {
            await AuthService.AuthInitialized;

            if (!AuthService.IsAuthenticated)
            {
                NavigationManager.NavigateTo("/authentication");
                return;
            }

            try
            {
                users = await DatabaseService.GetAllUsersAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur lors du chargement des utilisateurs: {ex.Message}");
            }
            isLoading = false;
        }


    }
}
