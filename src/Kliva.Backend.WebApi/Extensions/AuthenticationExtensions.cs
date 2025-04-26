using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Kliva.Backend.WebApi.Services;
using System.Security.Claims;

namespace Kliva.Backend.WebApi.Extensions;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddFirebaseAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = $"https://securetoken.google.com/{configuration["Firebase:ProjectId"]}";
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = $"https://securetoken.google.com/{configuration["Firebase:ProjectId"]}",
                    ValidateAudience = true,
                    ValidAudience = configuration["Firebase:ProjectId"],
                    ValidateLifetime = true
                };
                // Custom token validation to handle Firebase auth
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        // Get the Firebase service
                        var firebaseService = context.HttpContext.RequestServices.GetRequiredService<IFirebaseService>();
                        
                        // Get the token from the authorization header
                        string bearerToken = context.HttpContext.Request.Headers.Authorization.ToString().Replace("Bearer ", "");
                        
                        try
                        {
                            // Verify the token with Firebase Auth
                            var firebaseToken = await firebaseService.VerifyIdTokenAsync(bearerToken);
                            
                            // Add the user ID claim if not present
                            var identity = context.Principal?.Identity as ClaimsIdentity;
                            if (identity != null && !identity.HasClaim(c => c.Type == ClaimTypes.NameIdentifier))
                            {
                                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, firebaseToken.Uid));
                            }
                        }
                        catch (Exception ex)
                        {
                            context.Fail($"Firebase token validation failed: {ex.Message}");
                        }
                    }
                };
            });

        return services;
    }
} 