using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.IO;

namespace Kliva.Backend.WebApi.Endpoints.Auth;

/// <summary>
/// Endpoint mapping for authentication-related API routes
/// </summary>
public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("api/v1/auth");

        // Authentication endpoints
        group.MapGet("validate", AuthHandlers.ValidateTokenAsync)
            .WithSummary("Validate the authentication token.")
            .RequireAuthorization();
            
        // Diagnostic endpoint - only available in development
        if (routes.ServiceProvider.GetService<IWebHostEnvironment>()?.IsDevelopment() == true)
        {
            group.MapGet("diagnostic", AuthHandlers.DiagnosticCheck)
                .WithSummary("Check Firebase credentials and configuration status.");
        }

        // Note: Token generation is handled by Firebase Auth directly.
        // The frontend should authenticate with Firebase, and this API 
        // only validates the tokens issued by Firebase.
    }
}

/// <summary>
/// Handlers for authentication-related endpoints
/// </summary>
public static class AuthHandlers
{
    /// <summary>
    /// Validates the user's authentication token
    /// </summary>
    public static IResult ValidateTokenAsync(ClaimsPrincipal user)
    {
        // This endpoint is protected, so if we get here, the user is authenticated
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

        return TypedResults.Ok(new
        {
            IsAuthenticated = true,
            UserId = userId
        });
    }
    
    /// <summary>
    /// Diagnostic endpoint to check Firebase credential status
    /// </summary>
    public static IResult DiagnosticCheck(HttpContext context, IConfiguration configuration, IWebHostEnvironment environment)
    {
        var projectId = configuration["Firebase:ProjectId"];
        var credentialPath = configuration["Firebase:CredentialPath"];
        var googleCredPath = Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS");
        
        // Resolve full path
        string fullPath = null;
        if (!string.IsNullOrEmpty(credentialPath))
        {
            if (Path.IsPathRooted(credentialPath))
            {
                fullPath = credentialPath;
            }
            else
            {
                fullPath = Path.Combine(environment.ContentRootPath, credentialPath);
            }
        }
        
        // Check file existence
        bool fileExists = fullPath != null && File.Exists(fullPath);
        
        // Get directory contents if the file doesn't exist
        List<string> directoryContents = new List<string>();
        if (fullPath != null && !fileExists)
        {
            var directory = Path.GetDirectoryName(fullPath);
            if (Directory.Exists(directory))
            {
                directoryContents = Directory.GetFiles(directory).Select(Path.GetFileName).ToList();
            }
        }
        
        return TypedResults.Ok(new
        {
            ConfigCheck = new
            {
                ProjectId = projectId,
                CredentialPathFromConfig = credentialPath,
                ResolvedFullPath = fullPath,
                GoogleAppCredEnvVar = googleCredPath,
                FileExists = fileExists,
                ContentRootPath = environment.ContentRootPath,
                DirectoryContents = directoryContents
            },
            EnvironmentVariables = Environment.GetEnvironmentVariables()
                .Cast<System.Collections.DictionaryEntry>()
                .Where(e => e.Key.ToString().Contains("GOOGLE") || e.Key.ToString().Contains("FIREBASE"))
                .ToDictionary(e => e.Key.ToString(), e => e.Value?.ToString())
        });
    }
} 