# Todos Backend

A .NET Web API backend that integrates with Firebase Authentication and Firestore.

## Features

- Firebase Authentication token validation
- Firestore data operations through a secure API
- Sample Todo CRUD operations
- Token-based access control

## Setup Instructions

### Prerequisites

- .NET 8.0 SDK
- Firebase project with Authentication and Firestore enabled
- Firebase Admin SDK service account key

### Firebase Setup (Simplified)

1. Create a Firebase project at https://console.firebase.google.com/
2. Enable Authentication and Firestore in the Firebase console
3. Generate a service account key:
    - Go to Project Settings > Service accounts
    - Click "Generate new private key"
    - Save the JSON file as `firebase-credentials.json` in the `todos-backend/src/Todos.Backend.WebApi` directory

### Configuration

Simply update `appsettings.json` with your Firebase project ID:

```json
{
  "Firebase": {
    "ProjectId": "your-firebase-project-id",
    "CredentialPath": "firebase-credentials.json"
  }
}
```

The application automatically sets the `GOOGLE_APPLICATION_CREDENTIALS` environment variable to point to your
credentials' file.

### Running the API

```bash
cd src/Todos.Backend.WebApi
dotnet run
```

The API will be available at https://localhost:7001

## Development Testing

For development purposes, a simple testing page is included at the root URL (https://localhost:7001):

1. Open the URL in your browser
2. Use the sign-up form to create a test user or log in with Google
3. Click "Get ID Token" to retrieve your Firebase authentication token
4. Test API endpoints like "Validate Token" and "Get Todos"

## API Authentication

1. Frontend clients should authenticate with Firebase directly
2. After successful authentication, get the ID token
3. Include the token in API requests with the Authorization header:
   ```
   Authorization: Bearer {firebase-id-token}
   ```

## API Endpoints

- `GET /api/v1/auth/validate` - Validate authentication token
- `GET /api/v1/todos` - Get all todos for authenticated user
- `GET /api/v1/todos/{id}` - Get a specific todo
- `POST /api/v1/todos` - Create a new todo
- `PUT /api/v1/todos/{id}` - Update a todo
- `DELETE /api/v1/todos/{id}` - Delete a todo
- `PATCH /api/v1/todos/{id}/toggle` - Toggle todo completion status

## Security Model

- Frontend app handles user registration and login through Firebase Auth
- Backend only validates tokens and authorizes requests
- Each Firestore document includes a userId field for ownership verification
- API endpoints enforce user-specific data access

## Troubleshooting

### CORS Issues

- Ensure the CORS policy in Program.cs allows your frontend origin
- For local development, the default configuration allows any origin

### Authentication Errors

- Verify your Firebase project ID is correct in appsettings.json
- If using a credentials' file, ensure the file exists and has the correct path
- If embedding credentials, make sure all required fields are included
- Firebase tokens expire after 1 hour, get a new token if needed

### Firestore Access

- The service account must have access to the Firestore
- Verify the security rules in Firestore allow access to the service account
