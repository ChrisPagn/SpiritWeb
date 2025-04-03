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

await builder.Build().RunAsync();
