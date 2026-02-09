# HockeyDJ - Copilot Instructions

## Build, Test, and Lint Commands

```powershell
# Restore dependencies
dotnet restore

# Build the solution
dotnet build HockeyDJ\HockeyDJ.sln

# Run the application (from HockeyDJ subfolder)
cd HockeyDJ && dotnet run

# Run all tests
dotnet test HockeyDJ.Tests\HockeyDJ.Tests.csproj

# Run a single test
dotnet test HockeyDJ.Tests\HockeyDJ.Tests.csproj --filter "FullyQualifiedName~HomeControllerTests.Index_WithAccessToken_ReturnsView"

# Build release packages (multi-platform)
cd HockeyDJ && .\build-release.ps1 -Version "1.4.0"
```

## Architecture

This is an ASP.NET Core 10 MVC application that controls Spotify playback with a hockey-themed interface.

### Project Structure

- **HockeyDJ/** - Main web application
  - `Models/HomeController.cs` - The single controller handling all routes (despite being in Models folder)
  - `Views/Home/` - Razor views for Index (main UI) and Setup (configuration)
  - `wwwroot/audio/` - Audio files for goal horns and sound effects (not in source control)
- **HockeyDJ.Tests/** - xUnit tests with Moq for controller testing

### Key Integration Points

- **Spotify API**: Uses `SpotifyAPI.Web` library for OAuth, playlist management, and track search
- **Spotify Web Playback SDK**: Client-side JavaScript SDK for browser-based playback control
- **Session Storage**: All configuration (Spotify tokens, playlists, team rosters, priority queue) stored in server-side session

### Data Flow

1. User configures Spotify credentials and playlists via `/Home/Setup`
2. OAuth flow stores access token in session
3. Main UI (`/Home/Index`) loads playlists and enables playback controls
4. Priority queue and played-song tracking persists in session for deduplication

## Key Conventions

### Controller Pattern
The `HomeController` handles all application logic in a single controller, returning JSON for AJAX endpoints and Views for page navigation. AJAX endpoints return `{ success: bool, ... }` format.

### Spotify URL Parsing
The `ExtractPlaylistId` method handles both URL formats:
- `open.spotify.com/playlist/{id}`
- `spotify:playlist:{id}`

### Test Structure
Tests use Moq to mock `ISession` with a dictionary-backed storage. Session values are stored as UTF8 byte arrays. Each test class sets up its own controller instance with mocked dependencies.

### Audio File Naming
Audio files in `wwwroot/audio/` must match exact expected filenames (case-sensitive):
- Sound effects: `goal-horn.mp3`, `mushroom.mp3`, `clock.mp3`, `Sad Trombone.mp3`
- Announcement audio: Player numbers like `1.mp3`, `4.mp3`, penalty types like `Minor.mp3`, `Hooking.mp3`
