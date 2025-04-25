//Program.cs // client
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SpiritWeb.Client;
using MudBlazor.Services;
using SpiritWeb.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
// Ajout des services MubBlazor
builder.Services.AddMudServices();

// Ajout des services d'authentification et de base de données
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<DatabaseService>();
// Ajout du service d'autorisation
builder.Services.AddScoped<AuthorizationService>();
// Ajout du service de gestion des enquêtes
builder.Services.AddScoped<SurveyService>();
builder.Services.AddScoped<VoteService>();
//ajout du service de gestion des actualités
builder.Services.AddScoped<NewsService>();


await builder.Build().RunAsync();
