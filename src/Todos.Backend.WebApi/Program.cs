using Todos.Backend.WebApi.Endpoints.Auth;
using Todos.Backend.WebApi.Endpoints.Todo;
using Todos.Backend.WebApi.Extensions;
using Todos.Backend.WebApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Set environment variable for Firebase credentials
var credentialPath = builder.Configuration["Firebase:CredentialPath"];
if (!string.IsNullOrEmpty(credentialPath) && !Path.IsPathRooted(credentialPath))
{
    // Make the path absolute if it's relative
    var fullPath = Path.Combine(builder.Environment.ContentRootPath, credentialPath);

    // Set the environment variable
    Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", fullPath);
}

ConfigureServices(builder.Services, builder.Configuration, builder.Environment);
var app = builder.Build();
ConfigureMiddleware(app);

app.Run();

void ConfigureServices(IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
{
    services.AddCors(options =>
    {
        options.AddPolicy("AllowFrontend", policyBuilder =>
        {
            if (environment.IsDevelopment())
            {
                // In development, allow any origin for easier testing
                policyBuilder
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            }
            else
            {
                // In production, restrict to specific origins
                policyBuilder
                    .WithOrigins(configuration.GetValue<string>("Frontend:Url") ??
                                 throw new Exception("Frontend URL is not configured"))
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            }
        });
    });

    // Firebase Authentication
    services.AddFirebaseAuthentication(configuration);
    services.AddAuthorization();

    // API Documentation
    services.AddEndpointsApiExplorer();
    services.AddSwaggerWithJwtAuth();

    // Application Services
    services.AddSingleton<IFirebaseService, FirebaseService>();
    services.AddScoped<ITodoService, TodoService>();
}

void ConfigureMiddleware(WebApplication app)
{
    // CORS must be first in the pipeline
    app.UseCors("AllowFrontend");

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    // Security middleware
    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();

    // Static files
    app.UseStaticFiles();
    app.MapGet("/", context =>
    {
        context.Response.Redirect("/index.html");
        return Task.CompletedTask;
    });

    // API endpoints
    app.MapAuthEndpoints();
    app.MapTodoEndpoints();
}