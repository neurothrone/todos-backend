using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;

namespace Todos.Backend.WebApi.Services;

public interface IFirebaseService
{
    Task<FirebaseToken> VerifyIdTokenAsync(string idToken);
    FirestoreDb GetFirestoreDb();
}

public class FirebaseService : IFirebaseService
{
    private readonly FirestoreDb _firestoreDb;
    private readonly ILogger<FirebaseService> _logger;
    
    public FirebaseService(IConfiguration configuration, ILogger<FirebaseService> logger)
    {
        _logger = logger;
        
        try
        {
            // Initialize Firebase Admin SDK if not already initialized
            if (FirebaseApp.DefaultInstance == null)
            {
                var projectId = configuration["Firebase:ProjectId"];
                
                if (string.IsNullOrEmpty(projectId))
                    throw new InvalidOperationException("Firebase ProjectId is not configured");

                // NOTE: This will use the GOOGLE_APPLICATION_CREDENTIALS environment variable 
                // which was set in Program.cs
                var credential = GoogleCredential.GetApplicationDefault();
                
                // Initialize Firebase Admin SDK with service account credentials
                FirebaseApp.Create(new AppOptions
                {
                    Credential = credential,
                    ProjectId = projectId
                });
                
                // Initialize Firestore
                _firestoreDb = FirestoreDb.Create(projectId);
                
                _logger.LogInformation("Firebase initialized successfully with project ID: {ProjectId}", projectId);
            }
            else
            {
                // Get the Firestore DB from the existing Firebase instance
                var projectId = configuration["Firebase:ProjectId"];
                _firestoreDb = FirestoreDb.Create(projectId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Firebase");
            throw;
        }
    }

    public async Task<FirebaseToken> VerifyIdTokenAsync(string idToken)
    {
        try
        {
            return await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(idToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to verify Firebase ID token");
            throw;
        }
    }

    public FirestoreDb GetFirestoreDb()
    {
        return _firestoreDb;
    }
} 