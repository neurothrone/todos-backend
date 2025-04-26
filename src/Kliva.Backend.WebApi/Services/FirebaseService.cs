using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;

namespace Kliva.Backend.WebApi.Services;

public interface IFirebaseService
{
    Task<FirebaseToken> VerifyIdTokenAsync(string idToken);
    FirestoreDb GetFirestoreDb();
}

public class FirebaseService : IFirebaseService
{
    private readonly FirestoreDb _firestoreDb;
    
    public FirebaseService(IConfiguration configuration)
    {
        // Initialize Firebase Admin SDK if not already initialized
        if (FirebaseApp.DefaultInstance == null)
        {
            var projectId = configuration["Firebase:ProjectId"];
            var credentialPath = configuration["Firebase:CredentialPath"];
            
            if (string.IsNullOrEmpty(projectId))
                throw new InvalidOperationException("Firebase ProjectId is not configured");
                
            if (string.IsNullOrEmpty(credentialPath))
                throw new InvalidOperationException("Firebase CredentialPath is not configured");

            // Initialize Firebase Admin SDK with service account credentials
            FirebaseApp.Create(new AppOptions
            {
                Credential = GoogleCredential.FromFile(credentialPath),
                ProjectId = projectId
            });
            
            // Initialize Firestore
            _firestoreDb = FirestoreDb.Create(projectId);
        }
        else
        {
            // Get the Firestore DB from the existing Firebase instance
            var projectId = configuration["Firebase:ProjectId"];
            _firestoreDb = FirestoreDb.Create(projectId);
        }
    }

    public async Task<FirebaseToken> VerifyIdTokenAsync(string idToken)
    {
        return await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(idToken);
    }

    public FirestoreDb GetFirestoreDb()
    {
        return _firestoreDb;
    }
} 