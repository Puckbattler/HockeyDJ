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
        private readonly IWebHostEnvironment _env;

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration, IWebHostEnvironment env)
        {
            _logger = logger;
            _configuration = configuration;
            _env = env;
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
            var priorityQueue = HttpContext.Session.GetString("PriorityQueue");
            var homeTeamName = HttpContext.Session.GetString("HomeTeamName");
            var homeTeamRoster = HttpContext.Session.GetString("HomeTeamRoster");
            var awayTeamName = HttpContext.Session.GetString("AwayTeamName");
            var awayTeamRoster = HttpContext.Session.GetString("AwayTeamRoster");
            var songStartTimestampsJson = HttpContext.Session.GetString("SongStartTimestampsJson");
            var playlistShuffleModes = HttpContext.Session.GetString("PlaylistShuffleModes");
            var playlistPlayIndexes = HttpContext.Session.GetString("PlaylistPlayIndexes");
            var smartShuffleHistory = HttpContext.Session.GetString("SmartShuffleHistory");
            var hatTrickSongUri = HttpContext.Session.GetString("HatTrickSongUri");
            var playerGoalCounts = HttpContext.Session.GetString("PlayerGoalCounts");
            var hornConfiguration = HttpContext.Session.GetString("HornConfiguration");
            var gameMode = HttpContext.Session.GetString("GameMode");

            ViewBag.Playlists = string.IsNullOrEmpty(playlists) ? "[]" : playlists;
            ViewBag.AccessToken = accessToken;
            ViewBag.GoalHornPlaylistId = goalHornPlaylistId ?? "";
            ViewBag.CustomSongNames = string.IsNullOrEmpty(customSongNames) ? "[]" : customSongNames;
            ViewBag.PriorityQueue = string.IsNullOrEmpty(priorityQueue) ? "[]" : priorityQueue;
            ViewBag.HomeTeamName = homeTeamName ?? "";
            ViewBag.HomeTeamRoster = homeTeamRoster ?? "";
            ViewBag.AwayTeamName = awayTeamName ?? "";
            ViewBag.AwayTeamRoster = awayTeamRoster ?? "";
            ViewBag.SongStartTimestampsJson = string.IsNullOrEmpty(songStartTimestampsJson) ? "{}" : songStartTimestampsJson;
            ViewBag.PlaylistShuffleModes = string.IsNullOrEmpty(playlistShuffleModes) ? "{}" : playlistShuffleModes;
            ViewBag.PlaylistPlayIndexes = string.IsNullOrEmpty(playlistPlayIndexes) ? "{}" : playlistPlayIndexes;
            ViewBag.SmartShuffleHistory = string.IsNullOrEmpty(smartShuffleHistory) ? "{}" : smartShuffleHistory;
            ViewBag.HatTrickSongUri = hatTrickSongUri ?? "";
            ViewBag.PlayerGoalCounts = string.IsNullOrEmpty(playerGoalCounts) ? "{}" : playerGoalCounts;
            ViewBag.HornConfiguration = string.IsNullOrEmpty(hornConfiguration) ? "{}" : hornConfiguration;
            ViewBag.GameMode = gameMode ?? "normal";

            return View();
        }

        public IActionResult Setup()
        {
            // Load existing settings if available
            var existingPlaylists = HttpContext.Session.GetString("UserPlaylists");
            var existingCustomNames = HttpContext.Session.GetString("CustomSongNames");
            var goalHornPlaylistId = HttpContext.Session.GetString("GoalHornPlaylistId");
            var homeTeamName = HttpContext.Session.GetString("HomeTeamName");
            var homeTeamRoster = HttpContext.Session.GetString("HomeTeamRoster");
            var awayTeamName = HttpContext.Session.GetString("AwayTeamName");
            var awayTeamRoster = HttpContext.Session.GetString("AwayTeamRoster");
            var songStartTimestamps = HttpContext.Session.GetString("SongStartTimestamps");
            var hatTrickSongUri = HttpContext.Session.GetString("HatTrickSongUri");
            
            ViewBag.ExistingPlaylists = string.IsNullOrEmpty(existingPlaylists) ? "[]" : existingPlaylists;
            ViewBag.ExistingCustomNames = string.IsNullOrEmpty(existingCustomNames) ? "[]" : existingCustomNames;
            ViewBag.GoalHornPlaylistId = goalHornPlaylistId ?? "";
            ViewBag.HomeTeamName = homeTeamName ?? "";
            ViewBag.HomeTeamRoster = homeTeamRoster ?? "";
            ViewBag.AwayTeamName = awayTeamName ?? "";
            ViewBag.AwayTeamRoster = awayTeamRoster ?? "";
            ViewBag.SongStartTimestamps = songStartTimestamps ?? "";
            ViewBag.HatTrickSongUri = hatTrickSongUri ?? "";
            
            return View();
        }

        [HttpPost]
        public IActionResult SaveSpotifyConfig(string clientId, string clientSecret, string redirectUri, string playlistUrls, string goalHornPlaylist = "", string customSongNames = "", string homeTeamName = "", string homeTeamRoster = "", string awayTeamName = "", string awayTeamRoster = "", string songStartTimestamps = "")
        {
            try
            {
                // Store configuration in session (in production, use secure storage)
                HttpContext.Session.SetString("SpotifyClientId", clientId);
                HttpContext.Session.SetString("SpotifyClientSecret", clientSecret);
                HttpContext.Session.SetString("SpotifyRedirectUri", redirectUri);

                // Store team roster information
                HttpContext.Session.SetString("HomeTeamName", homeTeamName ?? "");
                HttpContext.Session.SetString("HomeTeamRoster", homeTeamRoster ?? "");
                HttpContext.Session.SetString("AwayTeamName", awayTeamName ?? "");
                HttpContext.Session.SetString("AwayTeamRoster", awayTeamRoster ?? "");

                // Store song start timestamps (raw string, will be parsed as JSON for the view)
                HttpContext.Session.SetString("SongStartTimestamps", songStartTimestamps ?? "");

                // Parse and store song start timestamps as JSON for use in the view
                var timestamps = new Dictionary<string, int>();
                if (!string.IsNullOrEmpty(songStartTimestamps))
                {
                    var lines = songStartTimestamps.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                    foreach (var line in lines)
                    {
                        var parts = line.Trim().Split(':');
                        if (parts.Length == 3 && int.TryParse(parts[0], out var playerNumber) 
                            && int.TryParse(parts[1], out var minutes) 
                            && int.TryParse(parts[2], out var seconds)
                            && playerNumber > 0 && minutes >= 0 && seconds >= 0 && seconds < 60)
                        {
                            var timestampMs = (minutes * 60 + seconds) * 1000;
                            timestamps[playerNumber.ToString()] = timestampMs;
                        }
                    }
                }
                HttpContext.Session.SetString("SongStartTimestampsJson", JsonSerializer.Serialize(timestamps));

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

                // Initialize empty priority queue
                HttpContext.Session.SetString("PriorityQueue", "[]");

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

        [HttpPost]
        public async Task<IActionResult> SearchSpotify(string query)
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
                
                var searchRequest = new SearchRequest(SearchRequest.Types.Track, query)
                {
                    Limit = 10
                };
                
                var searchResult = await spotify.Search.Item(searchRequest);

                var tracks = searchResult.Tracks.Items?
                    .Select(track => new
                    {
                        id = track.Id,
                        name = track.Name,
                        artist = string.Join(", ", track.Artists.Select(a => a.Name)),
                        uri = track.Uri,
                        album = track.Album.Name,
                        duration_ms = track.DurationMs,
                        preview_url = track.PreviewUrl
                    })
                    .Cast<object>()
                    .ToList() ?? new List<object>();

                return Json(new
                {
                    success = true,
                    tracks = tracks
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching Spotify");
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult AddToPriorityQueue(string trackId, string trackName, string trackArtist, string trackUri)
        {
            try
            {
                var priorityQueueJson = HttpContext.Session.GetString("PriorityQueue") ?? "[]";
                var priorityQueue = JsonSerializer.Deserialize<List<object>>(priorityQueueJson) ?? new List<object>();

                var newTrack = new
                {
                    id = trackId,
                    name = trackName,
                    artist = trackArtist,
                    uri = trackUri,
                    addedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                };

                priorityQueue.Add(newTrack);

                // Store updated queue
                HttpContext.Session.SetString("PriorityQueue", JsonSerializer.Serialize(priorityQueue));

                return Json(new { success = true, queueLength = priorityQueue.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding track to priority queue");
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult RemoveFromPriorityQueue(string trackId)
        {
            try
            {
                var priorityQueueJson = HttpContext.Session.GetString("PriorityQueue") ?? "[]";
                var priorityQueue = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(priorityQueueJson) ?? new List<Dictionary<string, object>>();

                // Remove the track with matching ID
                priorityQueue.RemoveAll(track => track.ContainsKey("id") && track["id"].ToString() == trackId);

                // Store updated queue
                HttpContext.Session.SetString("PriorityQueue", JsonSerializer.Serialize(priorityQueue));

                return Json(new { success = true, queueLength = priorityQueue.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing track from priority queue");
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult ClearPriorityQueue()
        {
            try
            {
                HttpContext.Session.SetString("PriorityQueue", "[]");
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing priority queue");
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult GetPriorityQueue()
        {
            try
            {
                var priorityQueueJson = HttpContext.Session.GetString("PriorityQueue") ?? "[]";
                var priorityQueue = JsonSerializer.Deserialize<List<object>>(priorityQueueJson) ?? new List<object>();

                return Json(new { success = true, queue = priorityQueue });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting priority queue");
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

        [HttpPost]
        public IActionResult SetShuffleMode(string playlistId, string mode)
        {
            try
            {
                // Validate mode
                var validModes = new[] { "random", "smart", "sequential" };
                if (!validModes.Contains(mode))
                {
                    return Json(new { success = false, error = $"Invalid shuffle mode: {mode}" });
                }

                // Get or create shuffle modes dictionary
                var shuffleModesJson = HttpContext.Session.GetString("PlaylistShuffleModes");
                var shuffleModes = string.IsNullOrEmpty(shuffleModesJson) 
                    ? new Dictionary<string, string>() 
                    : JsonSerializer.Deserialize<Dictionary<string, string>>(shuffleModesJson) ?? new Dictionary<string, string>();

                // Update the mode for this playlist
                shuffleModes[playlistId] = mode;

                // Save back to session
                HttpContext.Session.SetString("PlaylistShuffleModes", JsonSerializer.Serialize(shuffleModes));

                // If switching to sequential mode, initialize the play index to 0
                if (mode == "sequential")
                {
                    var playIndexesJson = HttpContext.Session.GetString("PlaylistPlayIndexes");
                    var playIndexes = string.IsNullOrEmpty(playIndexesJson) 
                        ? new Dictionary<string, int>() 
                        : JsonSerializer.Deserialize<Dictionary<string, int>>(playIndexesJson) ?? new Dictionary<string, int>();
                    
                    playIndexes[playlistId] = 0;
                    HttpContext.Session.SetString("PlaylistPlayIndexes", JsonSerializer.Serialize(playIndexes));
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting shuffle mode");
                return Json(new { success = false, error = ex.Message });
            }
        }

        // NEW EXPORT/IMPORT FUNCTIONALITY - START
        
        [HttpGet]
        public IActionResult ExportConfiguration()
        {
            try
            {
                // Get current configuration from session
                var clientId = HttpContext.Session.GetString("SpotifyClientId");
                var clientSecret = HttpContext.Session.GetString("SpotifyClientSecret");
                var redirectUri = HttpContext.Session.GetString("SpotifyRedirectUri");
                var goalHornPlaylistId = HttpContext.Session.GetString("GoalHornPlaylistId");
                var userPlaylists = HttpContext.Session.GetString("UserPlaylists");
                var customSongNames = HttpContext.Session.GetString("CustomSongNames");

                if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(redirectUri))
                {
                    return Json(new { error = "No configuration found to export. Please configure your settings first." });
                }

                // Parse playlists to get URLs
                var playlistUrls = new List<string>();
                var goalHornPlaylistUrl = "";

                if (!string.IsNullOrEmpty(userPlaylists))
                {
                    try
                    {
                        var playlists = JsonSerializer.Deserialize<List<dynamic>>(userPlaylists);
                        if (playlists != null)
                        {
                            foreach (var playlist in playlists)
                            {
                                var playlistJson = JsonSerializer.Serialize(playlist);
                                var playlistDict = JsonSerializer.Deserialize<Dictionary<string, object>>(playlistJson);
                                
                                if (playlistDict != null && playlistDict.ContainsKey("Url"))
                                {
                                    var url = playlistDict["Url"].ToString();
                                    var isGoalHorn = playlistDict.ContainsKey("IsGoalHorn") && 
                                                   bool.Parse(playlistDict["IsGoalHorn"].ToString() ?? "false");
                                    
                                    if (isGoalHorn)
                                    {
                                        goalHornPlaylistUrl = url;
                                    }
                                    else
                                    {
                                        playlistUrls.Add(url);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error parsing playlists for export");
                    }
                }

                // Create export object (excluding sensitive data like client secret)
                var playlistShuffleModes = HttpContext.Session.GetString("PlaylistShuffleModes");
                var hatTrickSongUri = HttpContext.Session.GetString("HatTrickSongUri");
                var hornConfiguration = HttpContext.Session.GetString("HornConfiguration");
                
                var exportConfig = new
                {
                    clientId = clientId,
                    clientSecret = clientSecret,
                    redirectUri = redirectUri,
                    goalHornPlaylist = goalHornPlaylistUrl,
                    playlistUrls = string.Join("\n", playlistUrls),
                    customSongNames = customSongNames ?? "",
                    playlistShuffleModes = playlistShuffleModes ?? "{}",
                    hatTrickSongUri = hatTrickSongUri ?? "",
                    hornConfiguration = hornConfiguration ?? "{}",
                    exportDate = DateTime.UtcNow.ToString("O"),
                    version = "1.2.0"
                };

                var json = JsonSerializer.Serialize(exportConfig, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });

                return File(System.Text.Encoding.UTF8.GetBytes(json), "application/json", 
                    $"hockeydj-config-{DateTime.Now:yyyy-MM-dd}.json");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting configuration");
                return Json(new { error = "Failed to export configuration." });
            }
        }

        [HttpPost]
        public IActionResult ImportConfiguration([FromBody] ImportConfigurationRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.ConfigData))
                {
                    return Json(new { success = false, error = "No configuration data provided." });
                }

                // Parse the imported configuration
                var importedConfig = JsonSerializer.Deserialize<JsonElement>(request.ConfigData);
                
                // Validate required fields exist
                if (!importedConfig.TryGetProperty("clientId", out _) ||
                    !importedConfig.TryGetProperty("redirectUri", out _) || !importedConfig.TryGetProperty("clientSecret", out _))
                {
                    return Json(new { success = false, error = "Invalid configuration file format. Missing required fields." });
                }

                // Extract configuration values
                var config = new
                {
                    clientId = importedConfig.TryGetProperty("clientId", out var clientId) ? clientId.GetString() : "",
                    clientSecret = importedConfig.TryGetProperty("clientSecret", out var clientSecret) ? clientSecret.GetString() : "",
                    redirectUri = importedConfig.TryGetProperty("redirectUri", out var redirectUri) ? redirectUri.GetString() : "",
                    goalHornPlaylist = importedConfig.TryGetProperty("goalHornPlaylist", out var goalHornPlaylist) ? goalHornPlaylist.GetString() : "",
                    playlistUrls = importedConfig.TryGetProperty("playlistUrls", out var playlistUrls) ? playlistUrls.GetString() : "",
                    customSongNames = importedConfig.TryGetProperty("customSongNames", out var customSongNames) ? customSongNames.GetString() : "",
                    homeTeamName = importedConfig.TryGetProperty("homeTeamName", out var homeTeamName) ? homeTeamName.GetString() : "",
                    homeTeamRoster = importedConfig.TryGetProperty("homeTeamRoster", out var homeTeamRoster) ? homeTeamRoster.GetString() : "",
                    awayTeamName = importedConfig.TryGetProperty("awayTeamName", out var awayTeamName) ? awayTeamName.GetString() : "",
                    awayTeamRoster = importedConfig.TryGetProperty("awayTeamRoster", out var awayTeamRoster) ? awayTeamRoster.GetString() : "",
                    songStartTimestamps = importedConfig.TryGetProperty("songStartTimestamps", out var songStartTimestamps) ? songStartTimestamps.GetString() : "",
                    playlistShuffleModes = importedConfig.TryGetProperty("playlistShuffleModes", out var playlistShuffleModes) ? playlistShuffleModes.GetString() : "{}",
                    hatTrickSongUri = importedConfig.TryGetProperty("hatTrickSongUri", out var hatTrickSongUri) ? hatTrickSongUri.GetString() : "",
                    hornConfiguration = importedConfig.TryGetProperty("hornConfiguration", out var hornConfiguration) ? hornConfiguration.GetString() : "{}"
                };

                // Validate URLs
                if (!string.IsNullOrEmpty(config.redirectUri))
                {
                    if (!Uri.TryCreate(config.redirectUri, UriKind.Absolute, out _))
                    {
                        return Json(new { success = false, error = "Invalid redirect URI format in configuration." });
                    }
                }

                // Validate playlist URLs
                if (!string.IsNullOrEmpty(config.playlistUrls))
                {
                    var urls = config.playlistUrls.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                    foreach (var url in urls)
                    {
                        if (!Uri.TryCreate(url.Trim(), UriKind.Absolute, out var uri) || 
                            !uri.Host.Contains("spotify.com"))
                        {
                            return Json(new { success = false, error = $"Invalid Spotify URL in configuration: {url.Trim()}" });
                        }
                    }
                }

                return Json(new { success = true, config = config });
            }
            catch (JsonException)
            {
                return Json(new { success = false, error = "Invalid JSON format in configuration file." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing configuration");
                return Json(new { success = false, error = "Failed to import configuration." });
            }
        }

        // NEW EXPORT/IMPORT FUNCTIONALITY - END

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

        [HttpPost]
        public IActionResult RecordGoal(int playerNumber)
        {
            try
            {
                // Get current goal counts
                var countsJson = HttpContext.Session.GetString("PlayerGoalCounts") ?? "{}";
                var counts = JsonSerializer.Deserialize<Dictionary<int, int>>(countsJson) ?? new Dictionary<int, int>();
                
                // Increment goal count for player
                counts[playerNumber] = counts.GetValueOrDefault(playerNumber, 0) + 1;
                
                // Save updated counts
                HttpContext.Session.SetString("PlayerGoalCounts", JsonSerializer.Serialize(counts));
                
                // Check if this is a hat trick (3rd goal)
                bool isHatTrick = counts[playerNumber] == 3;
                
                // Get hat trick song URI if configured
                string? hatTrickSongUri = null;
                if (isHatTrick)
                {
                    hatTrickSongUri = HttpContext.Session.GetString("HatTrickSongUri");
                }
                
                return Json(new
                {
                    success = true,
                    goalCount = counts[playerNumber],
                    isHatTrick = isHatTrick,
                    hatTrickSongUri = hatTrickSongUri
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording goal for player {PlayerNumber}", playerNumber);
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult SaveHatTrickSong([FromBody] JsonElement body)
        {
            try
            {
                var songUrl = body.TryGetProperty("songUrl", out var urlProp) ? urlProp.GetString()?.Trim() : null;

                if (string.IsNullOrWhiteSpace(songUrl))
                {
                    return Json(new { success = false, error = "Song URL is required" });
                }

                // Try to extract a Spotify track URI; fall back to the raw URL
                var trackUri = ExtractTrackUri(songUrl);
                if (string.IsNullOrEmpty(trackUri))
                {
                    trackUri = songUrl;
                }

                HttpContext.Session.SetString("HatTrickSongUri", trackUri);
                return Json(new { success = true, uri = trackUri });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving hat trick song");
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult ClearHatTrickSong()
        {
            try
            {
                HttpContext.Session.Remove("HatTrickSongUri");
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing hat trick song");
                return Json(new { success = false, error = ex.Message });
            }
        }

        public IActionResult GetHatTrickSong()
        {
            try
            {
                var hatTrickSongUri = HttpContext.Session.GetString("HatTrickSongUri");
                return Json(new { success = true, uri = hatTrickSongUri });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting hat trick song");
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult ResetGoalCounts()
        {
            try
            {
                HttpContext.Session.Remove("PlayerGoalCounts");
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting goal counts");
                return Json(new { success = false, error = ex.Message });
            }
        }

        private string ExtractTrackUri(string spotifyUrl)
        {
            try
            {
                // Handle different Spotify URL formats
                if (spotifyUrl.Contains("open.spotify.com/track/"))
                {
                    var uri = new Uri(spotifyUrl);
                    var segments = uri.AbsolutePath.Split('/');
                    var trackIndex = Array.IndexOf(segments, "track");
                    if (trackIndex >= 0 && trackIndex < segments.Length - 1)
                    {
                        var trackId = segments[trackIndex + 1].Split('?')[0];
                        return $"spotify:track:{trackId}";
                    }
                }
                else if (spotifyUrl.StartsWith("spotify:track:"))
                {
                    return spotifyUrl;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not extract track URI from URL: {Url}", spotifyUrl);
            }
            return string.Empty;
        }

        [HttpPost]
        public IActionResult SaveHornConfig([FromBody] JsonElement config)
        {
            try
            {
                HttpContext.Session.SetString("HornConfiguration", config.GetRawText());
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving horn configuration");
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult GetHornConfig()
        {
            var config = HttpContext.Session.GetString("HornConfiguration");
            if (string.IsNullOrEmpty(config))
            {
                return Json(new
                {
                    @default = "/audio/goal-horn.mp3",
                    overtime = "/audio/goal-horn.mp3",
                    playoffs = "/audio/goal-horn.mp3",
                    awayTeam = "/audio/Sad Trombone.mp3",
                    players = new Dictionary<string, string>()
                });
            }
            return Content(config, "application/json");
        }

        [HttpPost]
        public IActionResult SetPlayerHorn([FromBody] JsonElement body)
        {
            try
            {
                var playerNumber = body.GetProperty("playerNumber").GetInt32().ToString();
                var hornPath = body.GetProperty("hornPath").GetString() ?? "";

                var configJson = HttpContext.Session.GetString("HornConfiguration") ?? "{}";
                var config = JsonSerializer.Deserialize<Dictionary<string, object>>(configJson) ?? new Dictionary<string, object>();

                // Get or create the players dictionary
                Dictionary<string, string> players;
                if (config.ContainsKey("players") && config["players"] is JsonElement playersElement)
                {
                    players = JsonSerializer.Deserialize<Dictionary<string, string>>(playersElement.GetRawText()) ?? new Dictionary<string, string>();
                }
                else
                {
                    players = new Dictionary<string, string>();
                }

                if (string.IsNullOrEmpty(hornPath))
                {
                    players.Remove(playerNumber);
                }
                else
                {
                    players[playerNumber] = hornPath;
                }

                config["players"] = JsonSerializer.SerializeToElement(players);
                HttpContext.Session.SetString("HornConfiguration", JsonSerializer.Serialize(config));
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting player horn");
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult SetGameMode([FromBody] JsonElement body)
        {
            try
            {
                var mode = body.GetProperty("mode").GetString() ?? "normal";
                if (mode != "normal" && mode != "overtime" && mode != "playoffs")
                {
                    return Json(new { success = false, error = "Invalid game mode. Must be 'normal', 'overtime', or 'playoffs'." });
                }
                HttpContext.Session.SetString("GameMode", mode);
                return Json(new { success = true, mode });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting game mode");
                return Json(new { success = false, error = ex.Message });
            }
        // ==================== Sound Upload Endpoints ====================

        [HttpPost]
        [RequestSizeLimit(10_000_000)] // 10MB limit
        public async Task<IActionResult> UploadSound(IFormFile file, string soundType)
        {
            var validTypes = new[] { "goalhorn", "mushroom", "clock", "trombone",
                                     "charge", "gohawks", "letsgo-cowbell", "letsgo-organ", "hattrick" };
            if (!validTypes.Contains(soundType))
                return BadRequest(new { success = false, error = "Invalid sound type" });

            if (file == null)
            {
                return BadRequest(new { success = false, error = "No file uploaded." });
            }

            if (file.Length <= 0)
            {
                return BadRequest(new { success = false, error = "Uploaded file is empty." });
            }

            var ext = Path.GetExtension(file.FileName).ToLower();
            if (!new[] { ".mp3", ".wav", ".ogg" }.Contains(ext))
                return BadRequest(new { success = false, error = "Invalid file type. Use MP3, WAV, or OGG." });

            try
            {
                var customDir = Path.Combine(_env.WebRootPath, "audio", "custom");
                Directory.CreateDirectory(customDir);

                var fileName = $"{soundType}{ext}";
                var filePath = Path.Combine(customDir, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var customSounds = GetCustomSoundMappings();
                customSounds[soundType] = $"/audio/custom/{fileName}";
                SaveCustomSoundMappings(customSounds);

                return Json(new { success = true, path = customSounds[soundType] });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading sound {SoundType}", soundType);
                return StatusCode(500, new { success = false, error = "An error occurred while saving the uploaded sound." });
            }
        }

        [HttpPost]
        public IActionResult ResetSound([FromBody] ResetSoundRequest request)
        {
            var validTypes = new[] { "goalhorn", "mushroom", "clock", "trombone",
                                     "charge", "gohawks", "letsgo-cowbell", "letsgo-organ", "hattrick" };
            if (request == null || !validTypes.Contains(request.SoundType))
                return BadRequest(new { success = false, error = "Invalid sound type" });

            var customSounds = GetCustomSoundMappings();
            customSounds.Remove(request.SoundType);
            SaveCustomSoundMappings(customSounds);

            var customDir = Path.Combine(_env.WebRootPath, "audio", "custom");
            if (Directory.Exists(customDir))
            {
                var files = Directory.GetFiles(customDir, $"{request.SoundType}.*");
                foreach (var file in files) System.IO.File.Delete(file);
            }

            return Json(new { success = true });
        }

        [HttpGet]
        public IActionResult GetCustomSounds()
        {
            return Json(GetCustomSoundMappings());
        }

        [HttpPost]
        [RequestSizeLimit(10_000_000)]
        public async Task<IActionResult> UploadCustomSound(IFormFile file)
        {
            var ext = Path.GetExtension(file.FileName).ToLower();
            if (!new[] { ".mp3", ".wav", ".ogg" }.Contains(ext))
                return BadRequest(new { success = false, error = "Invalid file type. Use MP3, WAV, or OGG." });

            var customDir = Path.Combine(_env.WebRootPath, "audio", "custom");
            Directory.CreateDirectory(customDir);

            var originalName = Path.GetFileNameWithoutExtension(file.FileName);
            var sanitizedName = SanitizeFileName(originalName);
            var fileName = $"{sanitizedName}{ext}";
            var filePath = Path.Combine(customDir, fileName);

            var counter = 1;
            while (System.IO.File.Exists(filePath))
            {
                fileName = $"{sanitizedName}_{counter}{ext}";
                filePath = Path.Combine(customDir, fileName);
                counter++;
            }

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var buttonInfo = ParseButtonInfoFromFileName(originalName);

            var customSoundsList = GetCustomSoundsList();
            customSoundsList.Add(new CustomSoundButton
            {
                Id = Guid.NewGuid().ToString("N")[..8],
                FileName = fileName,
                Path = $"/audio/custom/{fileName}",
                DisplayName = buttonInfo.DisplayName,
                Emoji = buttonInfo.Emoji,
                ColorClass = buttonInfo.ColorClass
            });
            SaveCustomSoundsList(customSoundsList);

            return Json(new { success = true, button = customSoundsList.Last() });
        }

        [HttpGet]
        public IActionResult GetCustomSoundButtons()
        {
            return Json(GetCustomSoundsList());
        }

        [HttpPost]
        public IActionResult DeleteCustomSound([FromBody] DeleteCustomSoundRequest request)
        {
            var customSoundsList = GetCustomSoundsList();
            var sound = customSoundsList.FirstOrDefault(s => s.Id == request.Id);
            if (sound != null)
            {
                var filePath = Path.Combine(_env.WebRootPath, sound.Path.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (System.IO.File.Exists(filePath))
                    System.IO.File.Delete(filePath);

                customSoundsList.Remove(sound);
                SaveCustomSoundsList(customSoundsList);
            }
            return Json(new { success = true });
        }

        private static readonly HashSet<string> AllowedColorClasses = new HashSet<string>(StringComparer.Ordinal)
        {
            "btn-sound-red", "btn-sound-blue", "btn-sound-gold", "btn-sound-green", "btn-sound-purple"
        };

        [HttpPost]
        public IActionResult UpdateCustomSound([FromBody] UpdateCustomSoundRequest request)
        {
            var customSoundsList = GetCustomSoundsList();
            var sound = customSoundsList.FirstOrDefault(s => s.Id == request.Id);
            if (sound != null)
            {
                if (request.Emoji != null)
                {
                    // Limit emoji to 4 characters max
                    sound.Emoji = request.Emoji.Length > 4 ? request.Emoji[..4] : request.Emoji;
                }
                if (request.DisplayName != null)
                {
                    // Limit display name length and trim
                    var name = request.DisplayName.Trim();
                    sound.DisplayName = name.Length > 100 ? name[..100] : name;
                }
                if (request.ColorClass != null && AllowedColorClasses.Contains(request.ColorClass))
                {
                    sound.ColorClass = request.ColorClass;
                }
                SaveCustomSoundsList(customSoundsList);
            }
            return Json(new { success = true });
        }

        // ==================== Sound Upload Helpers ====================

        private Dictionary<string, string> GetCustomSoundMappings()
        {
            var json = HttpContext.Session.GetString("CustomSoundMappings");
            if (!string.IsNullOrEmpty(json))
                return JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();

            // Fall back to scanning the custom directory for known sound types
            var validTypes = new[] { "goalhorn", "mushroom", "clock", "trombone",
                                     "charge", "gohawks", "letsgo-cowbell", "letsgo-organ", "hattrick" };
            var mappings = new Dictionary<string, string>();
            var customDir = Path.Combine(_env.WebRootPath, "audio", "custom");
            if (Directory.Exists(customDir))
            {
                foreach (var soundType in validTypes)
                {
                    var match = Directory.GetFiles(customDir, $"{soundType}.*")
                        .FirstOrDefault(f => new[] { ".mp3", ".wav", ".ogg" }.Contains(Path.GetExtension(f).ToLower()));
                    if (match != null)
                        mappings[soundType] = $"/audio/custom/{Path.GetFileName(match)}";
                }
            }

            if (mappings.Count > 0)
                SaveCustomSoundMappings(mappings);

            return mappings;
        }

        private void SaveCustomSoundMappings(Dictionary<string, string> mappings)
        {
            HttpContext.Session.SetString("CustomSoundMappings", JsonSerializer.Serialize(mappings));
        }

        private List<CustomSoundButton> GetCustomSoundsList()
        {
            var json = HttpContext.Session.GetString("CustomSoundButtons");
            if (!string.IsNullOrEmpty(json))
                return JsonSerializer.Deserialize<List<CustomSoundButton>>(json) ?? new List<CustomSoundButton>();

            // Fall back to scanning the custom directory for non-default-type files
            var validTypes = new[] { "goalhorn", "mushroom", "clock", "trombone",
                                     "charge", "gohawks", "letsgo-cowbell", "letsgo-organ", "hattrick" };
            var sounds = new List<CustomSoundButton>();
            var customDir = Path.Combine(_env.WebRootPath, "audio", "custom");
            if (Directory.Exists(customDir))
            {
                var audioExtensions = new[] { ".mp3", ".wav", ".ogg" };
                foreach (var file in Directory.GetFiles(customDir))
                {
                    var ext = Path.GetExtension(file).ToLower();
                    if (!audioExtensions.Contains(ext)) continue;

                    var nameWithoutExt = Path.GetFileNameWithoutExtension(file);
                    if (validTypes.Contains(nameWithoutExt)) continue;

                    var buttonInfo = ParseButtonInfoFromFileName(nameWithoutExt);
                    sounds.Add(new CustomSoundButton
                    {
                        Id = Guid.NewGuid().ToString("N")[..8],
                        FileName = Path.GetFileName(file),
                        Path = $"/audio/custom/{Path.GetFileName(file)}",
                        DisplayName = buttonInfo.DisplayName,
                        Emoji = buttonInfo.Emoji,
                        ColorClass = buttonInfo.ColorClass
                    });
                }
            }

            if (sounds.Count > 0)
                SaveCustomSoundsList(sounds);

            return sounds;
        }

        private void SaveCustomSoundsList(List<CustomSoundButton> sounds)
        {
            HttpContext.Session.SetString("CustomSoundButtons", JsonSerializer.Serialize(sounds));
        }

        private (string Emoji, string DisplayName, string ColorClass) ParseButtonInfoFromFileName(string fileName)
        {
            var emoji = "🔊";
            var displayName = fileName;

            if (fileName.Length > 0)
            {
                var firstCodePoint = char.ConvertToUtf32(fileName, 0);
                if (firstCodePoint > 0x1F300)
                {
                    var emojiLength = char.IsSurrogatePair(fileName, 0) ? 2 : 1;
                    emoji = fileName.Substring(0, emojiLength);
                    displayName = fileName.Substring(emojiLength).Trim();
                }
            }

            var colorClasses = new[] { "btn-sound-red", "btn-sound-blue", "btn-sound-gold",
                                        "btn-sound-green", "btn-sound-purple" };
            var colorClass = colorClasses[Math.Abs(displayName.GetHashCode()) % colorClasses.Length];

            return (emoji, displayName, colorClass);
        }

        private static readonly HashSet<string> WindowsReservedDeviceNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "CON", "PRN", "AUX", "NUL",
            "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
            "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
        };

        private string SanitizeFileName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                fileName = "untitled";

            var invalid = Path.GetInvalidFileNameChars();
            var sanitized = new string(fileName.Where(c => !invalid.Contains(c)).ToArray());

            if (string.IsNullOrWhiteSpace(sanitized))
                sanitized = "untitled";

            const int MaxFileNameLength = 255;
            if (sanitized.Length > MaxFileNameLength)
                sanitized = sanitized[..MaxFileNameLength];

            var nameWithoutExtension = Path.GetFileNameWithoutExtension(sanitized);
            var extension = Path.GetExtension(sanitized);

            if (!string.IsNullOrEmpty(nameWithoutExtension) && WindowsReservedDeviceNames.Contains(nameWithoutExtension))
            {
                sanitized = "_" + nameWithoutExtension + extension;
            }

            return sanitized;
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }

    // Data model for import requests
    public class ImportConfigurationRequest
    {
        public string ConfigData { get; set; } = string.Empty;
    }

    public class CustomSoundButton
    {
        public string Id { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Emoji { get; set; } = "🔊";
        public string ColorClass { get; set; } = "btn-sound-blue";
    }

    public class ResetSoundRequest
    {
        public string SoundType { get; set; } = string.Empty;
    }

    public class DeleteCustomSoundRequest
    {
        public string Id { get; set; } = string.Empty;
    }

    public class UpdateCustomSoundRequest
    {
        public string Id { get; set; } = string.Empty;
        public string? Emoji { get; set; }
        public string? DisplayName { get; set; }
        public string? ColorClass { get; set; }
    }
}
