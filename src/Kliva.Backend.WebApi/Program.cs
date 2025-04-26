using Kliva.Backend.WebApi.Endpoints.Auth;
using Kliva.Backend.WebApi.Endpoints.Todo;
using Kliva.Backend.WebApi.Extensions;
using Kliva.Backend.WebApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
ConfigureServices(builder.Services, builder.Configuration, builder.Environment);

var app = builder.Build();

// Configure the HTTP request pipeline
ConfigureMiddleware(app);

app.Run();

// Service configuration
void ConfigureServices(IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
{
    // CORS
    services.AddCors(options =>
    {
        options.AddPolicy("AllowLocalhost3000", policyBuilder =>
        {
            policyBuilder.WithOrigins("http://localhost:3000") // Frontend URL
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
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

// Middleware configuration
void ConfigureMiddleware(WebApplication app)
{
    // CORS must be first in the pipeline
    app.UseCors("AllowLocalhost3000");

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