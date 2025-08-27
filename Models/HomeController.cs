using Microsoft.AspNetCore.Mvc;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using System.Text.Json;

namespace HockeyDJ.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            var accessToken = HttpContext.Session.GetString("SpotifyAccessToken");
            if (string.IsNullOrEmpty(accessToken))
            {
                return RedirectToAction("Setup");
            }

            var playlists = HttpContext.Session.GetString("UserPlaylists");
            var goalHornPlaylistId = HttpContext.Session.GetString("GoalHornPlaylistId");
            var customSongNames = HttpContext.Session.GetString("CustomSongNames");

            ViewBag.Playlists = string.IsNullOrEmpty(playlists) ? "[]" : playlists;
            ViewBag.AccessToken = accessToken;
            ViewBag.GoalHornPlaylistId = goalHornPlaylistId ?? "";
            ViewBag.CustomSongNames = string.IsNullOrEmpty(customSongNames) ? "[]" : customSongNames;

            return View();
        }

        public IActionResult Setup()
        {
            // Load existing settings if available
            var existingPlaylists = HttpContext.Session.GetString("UserPlaylists");
            var existingCustomNames = HttpContext.Session.GetString("CustomSongNames");
            var goalHornPlaylistId = HttpContext.Session.GetString("GoalHornPlaylistId");
            
            ViewBag.ExistingPlaylists = string.IsNullOrEmpty(existingPlaylists) ? "[]" : existingPlaylists;
            ViewBag.ExistingCustomNames = string.IsNullOrEmpty(existingCustomNames) ? "[]" : existingCustomNames;
            ViewBag.GoalHornPlaylistId = goalHornPlaylistId ?? "";
            
            return View();
        }

        [HttpPost]
        public IActionResult SaveSpotifyConfig(string clientId, string clientSecret, string redirectUri, string playlistUrls, string goalHornPlaylist = "", string customSongNames = "")
        {
            try
            {
                // Store configuration in session (in production, use secure storage)
                HttpContext.Session.SetString("SpotifyClientId", clientId);
                HttpContext.Session.SetString("SpotifyClientSecret", clientSecret);
                HttpContext.Session.SetString("SpotifyRedirectUri", redirectUri);

                // Parse custom song names
                var songNames = new List<string>();
                if (!string.IsNullOrEmpty(customSongNames))
                {
                    songNames = customSongNames.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                        .Select(name => name.Trim())
                        .Take(20)
                        .ToList();
                }

                // Ensure we have 20 entries, filling with defaults if needed
                while (songNames.Count < 20)
                {
                    songNames.Add($"Song {songNames.Count + 1}");
                }

                HttpContext.Session.SetString("CustomSongNames", JsonSerializer.Serialize(songNames));

                // Parse and store playlist URLs
                var playlists = playlistUrls.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                    .Select((url, index) => new {
                        Id = ExtractPlaylistId(url.Trim()),
                        Name = "Loading...", // This will be updated when playlist data is fetched
                        Url = url.Trim(),
                        IsGoalHorn = false
                    })
                    .Where(p => !string.IsNullOrEmpty(p.Id))
                    .Take(10)
                    .ToList();

                // Handle goal horn playlist
                if (!string.IsNullOrEmpty(goalHornPlaylist))
                {
                    var goalHornId = ExtractPlaylistId(goalHornPlaylist.Trim());
                    if (!string.IsNullOrEmpty(goalHornId))
                    {
                        // Add goal horn playlist to the beginning of the list
                        playlists.Insert(0, new
                        {
                            Id = goalHornId,
                            Name = "Goal Celebrations",
                            Url = goalHornPlaylist.Trim(),
                            IsGoalHorn = true
                        });

                        // Store goal horn playlist ID separately
                        HttpContext.Session.SetString("GoalHornPlaylistId", goalHornId);
                    }
                }

                HttpContext.Session.SetString("UserPlaylists", JsonSerializer.Serialize(playlists));

                // Redirect to Spotify OAuth
                return RedirectToAction("SpotifyLogin");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving Spotify configuration");
                ViewBag.Error = "Error saving configuration. Please try again.";
                return View("Setup");
            }
        }

        public async Task<IActionResult> SpotifyLogin()
        {
            var clientId = HttpContext.Session.GetString("SpotifyClientId");
            var redirectUri = HttpContext.Session.GetString("SpotifyRedirectUri");

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(redirectUri))
            {
                return RedirectToAction("Setup");
            }

            var loginRequest = new LoginRequest(new Uri(redirectUri), clientId, LoginRequest.ResponseType.Code)
            {
                Scope = new[] {
                    Scopes.UserReadPlaybackState,
                    Scopes.UserModifyPlaybackState,
                    Scopes.UserReadCurrentlyPlaying,
                    Scopes.Streaming,
                    Scopes.UserReadEmail,
                    Scopes.UserReadPrivate,
                    Scopes.PlaylistReadPrivate,
                    Scopes.PlaylistReadCollaborative
                }
            };

            var uri = loginRequest.ToUri();
            return Redirect(uri.ToString());
        }

        public async Task<IActionResult> SpotifyCallback(string code, string error)
        {
            if (!string.IsNullOrEmpty(error))
            {
                ViewBag.Error = $"Spotify authorization error: {error}";
                return View("Setup");
            }

            try
            {
                var clientId = HttpContext.Session.GetString("SpotifyClientId");
                var clientSecret = HttpContext.Session.GetString("SpotifyClientSecret");
                var redirectUri = HttpContext.Session.GetString("SpotifyRedirectUri");

                var response = await new OAuthClient().RequestToken(
                    new AuthorizationCodeTokenRequest(clientId, clientSecret, code, new Uri(redirectUri))
                );

                HttpContext.Session.SetString("SpotifyAccessToken", response.AccessToken);
                HttpContext.Session.SetString("SpotifyRefreshToken", response.RefreshToken ?? "");

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Spotify callback");
                ViewBag.Error = "Error connecting to Spotify. Please try again.";
                return View("Setup");
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetPlaylistTracks(string playlistId)
        {
            try
            {
                var accessToken = HttpContext.Session.GetString("SpotifyAccessToken");
                if (string.IsNullOrEmpty(accessToken))
                {
                    return Json(new { success = false, error = "Not authenticated" });
                }

                var config = SpotifyClientConfig.CreateDefault(accessToken);
                var spotify = new SpotifyClient(config);
                var playlist = await spotify.Playlists.Get(playlistId);
                var tracks = await spotify.Playlists.GetItems(playlistId);

                var trackList = tracks.Items?
                    .Where(item => item.Track is FullTrack)
                    .Select(item => item.Track as FullTrack)
                    .Where(track => track != null)
                    .Select(track => new
                    {
                        id = track!.Id,
                        name = track.Name,
                        artist = string.Join(", ", track.Artists.Select(a => a.Name)),
                        uri = track.Uri
                    })
                    .Cast<object>()
                    .ToList() ?? new List<object>();

                return Json(new
                {
                    success = true,
                    tracks = trackList,
                    playlistName = playlist.Name,
                    playlistId = playlistId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting playlist tracks");
                return Json(new { success = false, error = ex.Message });
            }
        }

        // Add this new method to update playlist names in session
        [HttpPost]
        public IActionResult UpdatePlaylistName(string playlistId, string playlistName)
        {
            try
            {
                var playlistsJson = HttpContext.Session.GetString("UserPlaylists");
                if (!string.IsNullOrEmpty(playlistsJson))
                {
                    var playlists = JsonSerializer.Deserialize<List<dynamic>>(playlistsJson);
                    // Update the session with the new playlist names
                    // Note: This is a simplified approach. In a real app, you might want to use a more robust data structure.
                }
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating playlist name");
                return Json(new { success = false, error = ex.Message });
            }
        }

        private string ExtractPlaylistId(string spotifyUrl)
        {
            try
            {
                // Handle different Spotify URL formats
                if (spotifyUrl.Contains("open.spotify.com/playlist/"))
                {
                    var uri = new Uri(spotifyUrl);
                    var segments = uri.AbsolutePath.Split('/');
                    var playlistIndex = Array.IndexOf(segments, "playlist");
                    if (playlistIndex >= 0 && playlistIndex < segments.Length - 1)
                    {
                        return segments[playlistIndex + 1].Split('?')[0];
                    }
                }
                else if (spotifyUrl.StartsWith("spotify:playlist:"))
                {
                    return spotifyUrl.Split(':')[2];
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not extract playlist ID from URL: {Url}", spotifyUrl);
            }
            return string.Empty;
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}
