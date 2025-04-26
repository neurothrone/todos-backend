using Kliva.Backend.WebApi.Endpoints;
using Kliva.Backend.WebApi.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpContextAccessor();
builder.Services.AddAntiforgery();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "Kliva.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.LoginPath = "/login";
        options.LogoutPath = "/";
        options.AccessDeniedPath = "/access-denied";
    });
builder.Services.AddAuthorization();

// Configure Firebase
var firebaseConfig = builder.Configuration.GetSection("Firebase");
var apiKey = firebaseConfig["ApiKey"];
var authDomain = firebaseConfig["AuthDomain"];

if (string.IsNullOrEmpty(apiKey))
    throw new Exception("Firebase API key is not configured");
if (string.IsNullOrEmpty(authDomain))
    throw new Exception("Firebase Auth Domain is not configured");

builder.Services.AddScoped(provider => new FirebaseAuthService(
    provider.GetRequiredService<ILogger<FirebaseAuthService>>(),
    provider.GetRequiredService<IConfiguration>(),
    provider.GetRequiredService<IHttpContextAccessor>()));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.UseStaticFiles();
app.MapGet("/", context =>
{
    context.Response.Redirect("/index.html");
    return Task.CompletedTask;
});

app.MapAuthEndpoints();

app.Run();