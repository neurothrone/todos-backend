using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
} 