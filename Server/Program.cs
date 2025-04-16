using Google.Cloud.Firestore;
using Microsoft.AspNetCore.ResponseCompression;
using SpiritWeb.Server.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// comment a ajouter
var credentialPath = Path.Combine(Directory.GetCurrentDirectory(), "Secrets/firebase-service-account.json");

//comment a ajouter
Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialPath);

// Récupération de l'ID de projet depuis la configuration
var firebaseProjectId = builder.Configuration["Firebase:ProjectId"];
if (string.IsNullOrEmpty(firebaseProjectId))
{
    throw new InvalidOperationException("Firebase ProjectId n'est pas configuré dans appsettings.json");
}
// Enregistrement de FirestoreDb
builder.Services.AddSingleton(FirestoreDb.Create(firebaseProjectId));

// Enregistrement des services Firebase
builder.Services.AddScoped<FirebaseAuthService>();
builder.Services.AddScoped<FirebaseDatabaseService>();
builder.Services.AddScoped<FirestoreSurveyService>();
builder.Services.AddScoped<FirestoreVoteService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();


app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
