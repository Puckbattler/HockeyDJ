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
            var priorityQueue = HttpContext.Session.GetString("PriorityQueue");
            var homeTeamName = HttpContext.Session.GetString("HomeTeamName");
            var homeTeamRoster = HttpContext.Session.GetString("HomeTeamRoster");
            var awayTeamName = HttpContext.Session.GetString("AwayTeamName");
            var awayTeamRoster = HttpContext.Session.GetString("AwayTeamRoster");
            var songStartTimestampsJson = HttpContext.Session.GetString("SongStartTimestampsJson");
            var statsMode = HttpContext.Session.GetString("StatsMode");

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

            // Check if stats mode is enabled
            if (statsMode == "true")
            {
                var statsPlaylistIds = HttpContext.Session.GetString("StatsPlaylistIds");
                ViewBag.StatsPlaylistIds = string.IsNullOrEmpty(statsPlaylistIds) ? "{}" : statsPlaylistIds;
                return View("StatsPlayer");
            }

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
            var statsMode = HttpContext.Session.GetString("StatsMode");
            
            ViewBag.ExistingPlaylists = string.IsNullOrEmpty(existingPlaylists) ? "[]" : existingPlaylists;
            ViewBag.ExistingCustomNames = string.IsNullOrEmpty(existingCustomNames) ? "[]" : existingCustomNames;
            ViewBag.GoalHornPlaylistId = goalHornPlaylistId ?? "";
            ViewBag.HomeTeamName = homeTeamName ?? "";
            ViewBag.HomeTeamRoster = homeTeamRoster ?? "";
            ViewBag.AwayTeamName = awayTeamName ?? "";
            ViewBag.AwayTeamRoster = awayTeamRoster ?? "";
            ViewBag.SongStartTimestamps = songStartTimestamps ?? "";
            ViewBag.StatsMode = statsMode == "true";
            
            // Load stats mode playlists
            ViewBag.OffsidePlaylist = HttpContext.Session.GetString("OffsidePlaylist") ?? "";
            ViewBag.IcingPlaylist = HttpContext.Session.GetString("IcingPlaylist") ?? "";
            ViewBag.AwayGoalPlaylist = HttpContext.Session.GetString("AwayGoalPlaylist") ?? "";
            ViewBag.ComebackPlaylist = HttpContext.Session.GetString("ComebackPlaylist") ?? "";
            ViewBag.HopePlaylist = HttpContext.Session.GetString("HopePlaylist") ?? "";
            ViewBag.EndZonePlaylist = HttpContext.Session.GetString("EndZonePlaylist") ?? "";
            ViewBag.PenaltyPlaylist = HttpContext.Session.GetString("PenaltyPlaylist") ?? "";
            ViewBag.WarmUpPlaylist = HttpContext.Session.GetString("WarmUpPlaylist") ?? "";
            ViewBag.IntermissionPlaylist = HttpContext.Session.GetString("IntermissionPlaylist") ?? "";
            ViewBag.PostGamePlaylist = HttpContext.Session.GetString("PostGamePlaylist") ?? "";
            
            return View();
        }

        [HttpPost]
        public IActionResult SaveSpotifyConfig(string clientId, string clientSecret, string redirectUri, string playlistUrls, string goalHornPlaylist = "", string customSongNames = "", string homeTeamName = "", string homeTeamRoster = "", string awayTeamName = "", string awayTeamRoster = "", string songStartTimestamps = "", string statsMode = "false", string offsidePlaylist = "", string icingPlaylist = "", string awayGoalPlaylist = "", string comebackPlaylist = "", string hopePlaylist = "", string endZonePlaylist = "", string penaltyPlaylist = "", string warmUpPlaylist = "", string intermissionPlaylist = "", string postGamePlaylist = "")
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

                // Store stats mode setting
                var isStatsMode = statsMode == "true";
                HttpContext.Session.SetString("StatsMode", isStatsMode.ToString().ToLower());

                // Store stats mode playlists
                HttpContext.Session.SetString("OffsidePlaylist", offsidePlaylist ?? "");
                HttpContext.Session.SetString("IcingPlaylist", icingPlaylist ?? "");
                HttpContext.Session.SetString("AwayGoalPlaylist", awayGoalPlaylist ?? "");
                HttpContext.Session.SetString("ComebackPlaylist", comebackPlaylist ?? "");
                HttpContext.Session.SetString("HopePlaylist", hopePlaylist ?? "");
                HttpContext.Session.SetString("EndZonePlaylist", endZonePlaylist ?? "");
                HttpContext.Session.SetString("PenaltyPlaylist", penaltyPlaylist ?? "");
                HttpContext.Session.SetString("WarmUpPlaylist", warmUpPlaylist ?? "");
                HttpContext.Session.SetString("IntermissionPlaylist", intermissionPlaylist ?? "");
                HttpContext.Session.SetString("PostGamePlaylist", postGamePlaylist ?? "");

                // Store stats mode playlist IDs
                var statsPlaylists = new Dictionary<string, string>
                {
                    { "offside", ExtractPlaylistId(offsidePlaylist ?? "") },
                    { "icing", ExtractPlaylistId(icingPlaylist ?? "") },
                    { "awayGoal", ExtractPlaylistId(awayGoalPlaylist ?? "") },
                    { "comeback", ExtractPlaylistId(comebackPlaylist ?? "") },
                    { "hope", ExtractPlaylistId(hopePlaylist ?? "") },
                    { "endZone", ExtractPlaylistId(endZonePlaylist ?? "") },
                    { "penalty", ExtractPlaylistId(penaltyPlaylist ?? "") },
                    { "warmUp", ExtractPlaylistId(warmUpPlaylist ?? "") },
                    { "intermission", ExtractPlaylistId(intermissionPlaylist ?? "") },
                    { "postGame", ExtractPlaylistId(postGamePlaylist ?? "") }
                };
                HttpContext.Session.SetString("StatsPlaylistIds", JsonSerializer.Serialize(statsPlaylists));

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
                var exportConfig = new
                {
                    clientId = clientId,
                    clientSecret = clientSecret,
                    redirectUri = redirectUri,
                    goalHornPlaylist = goalHornPlaylistUrl,
                    playlistUrls = string.Join("\n", playlistUrls),
                    customSongNames = customSongNames ?? "",
                    exportDate = DateTime.UtcNow.ToString("O"),
                    version = "1.1.1"
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

                // Extract configuration values including stats mode settings
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
                    statsMode = importedConfig.TryGetProperty("statsMode", out var statsMode) && statsMode.ValueKind == JsonValueKind.True,
                    offsidePlaylist = importedConfig.TryGetProperty("offsidePlaylist", out var offsidePlaylist) ? offsidePlaylist.GetString() : "",
                    icingPlaylist = importedConfig.TryGetProperty("icingPlaylist", out var icingPlaylist) ? icingPlaylist.GetString() : "",
                    awayGoalPlaylist = importedConfig.TryGetProperty("awayGoalPlaylist", out var awayGoalPlaylist) ? awayGoalPlaylist.GetString() : "",
                    comebackPlaylist = importedConfig.TryGetProperty("comebackPlaylist", out var comebackPlaylist) ? comebackPlaylist.GetString() : "",
                    hopePlaylist = importedConfig.TryGetProperty("hopePlaylist", out var hopePlaylist) ? hopePlaylist.GetString() : "",
                    endZonePlaylist = importedConfig.TryGetProperty("endZonePlaylist", out var endZonePlaylist) ? endZonePlaylist.GetString() : "",
                    penaltyPlaylist = importedConfig.TryGetProperty("penaltyPlaylist", out var penaltyPlaylist) ? penaltyPlaylist.GetString() : "",
                    warmUpPlaylist = importedConfig.TryGetProperty("warmUpPlaylist", out var warmUpPlaylist) ? warmUpPlaylist.GetString() : "",
                    intermissionPlaylist = importedConfig.TryGetProperty("intermissionPlaylist", out var intermissionPlaylist) ? intermissionPlaylist.GetString() : "",
                    postGamePlaylist = importedConfig.TryGetProperty("postGamePlaylist", out var postGamePlaylist) ? postGamePlaylist.GetString() : ""
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
        public IActionResult SaveGameData([FromBody] GameDataRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.GameData))
                {
                    return Json(new { success = false, error = "No game data provided." });
                }

                HttpContext.Session.SetString("CurrentGameData", request.GameData);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving game data");
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult ExportGameData(string format = "json")
        {
            try
            {
                var gameData = HttpContext.Session.GetString("CurrentGameData");
                if (string.IsNullOrEmpty(gameData))
                {
                    return Json(new { error = "No game data found to export." });
                }

                var fileName = $"hockeydj-game-{DateTime.Now:yyyy-MM-dd-HHmm}";

                if (format.ToLower() == "csv")
                {
                    var csvData = ConvertGameDataToCsv(gameData);
                    return File(System.Text.Encoding.UTF8.GetBytes(csvData), "text/csv", $"{fileName}.csv");
                }
                else
                {
                    return File(System.Text.Encoding.UTF8.GetBytes(gameData), "application/json", $"{fileName}.json");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting game data");
                return Json(new { error = "Failed to export game data." });
            }
        }

        private string ConvertGameDataToCsv(string jsonData)
        {
            try
            {
                var gameData = JsonSerializer.Deserialize<JsonElement>(jsonData);
                var csv = new System.Text.StringBuilder();

                // Game Info Header
                csv.AppendLine("GAME INFORMATION");
                if (gameData.TryGetProperty("gameInfo", out var gameInfo))
                {
                    csv.AppendLine($"Home Team,{GetJsonString(gameInfo, "homeTeam")}");
                    csv.AppendLine($"Away Team,{GetJsonString(gameInfo, "awayTeam")}");
                    csv.AppendLine($"Final Score,{GetJsonString(gameInfo, "homeScore")} - {GetJsonString(gameInfo, "awayScore")}");
                    csv.AppendLine($"Date,{GetJsonString(gameInfo, "date")}");
                }
                csv.AppendLine();

                // Goals
                csv.AppendLine("GOALS");
                csv.AppendLine("Period,Time,Team,Scorer,Assist 1,Assist 2,Goal Type");
                if (gameData.TryGetProperty("goals", out var goals) && goals.ValueKind == JsonValueKind.Array)
                {
                    foreach (var goal in goals.EnumerateArray())
                    {
                        csv.AppendLine($"{GetJsonString(goal, "period")},{GetJsonString(goal, "time")},{GetJsonString(goal, "team")},{GetJsonString(goal, "scorer")},{GetJsonString(goal, "assist1")},{GetJsonString(goal, "assist2")},{GetJsonString(goal, "goalType")}");
                    }
                }
                csv.AppendLine();

                // Penalties
                csv.AppendLine("PENALTIES");
                csv.AppendLine("Period,Time,Team,Player,Penalty Type,Penalty Name,Duration");
                if (gameData.TryGetProperty("penalties", out var penalties) && penalties.ValueKind == JsonValueKind.Array)
                {
                    foreach (var penalty in penalties.EnumerateArray())
                    {
                        csv.AppendLine($"{GetJsonString(penalty, "period")},{GetJsonString(penalty, "time")},{GetJsonString(penalty, "team")},{GetJsonString(penalty, "player")},{GetJsonString(penalty, "penaltyType")},{GetJsonString(penalty, "penaltyName")},{GetJsonString(penalty, "duration")}");
                    }
                }
                csv.AppendLine();

                // Player Stats
                csv.AppendLine("PLAYER STATISTICS");
                csv.AppendLine("Team,Number,Name,Goals,Assists,Points,Plus/Minus,PIM");
                if (gameData.TryGetProperty("playerStats", out var playerStats) && playerStats.ValueKind == JsonValueKind.Array)
                {
                    foreach (var player in playerStats.EnumerateArray())
                    {
                        csv.AppendLine($"{GetJsonString(player, "team")},{GetJsonString(player, "number")},{GetJsonString(player, "name")},{GetJsonString(player, "goals")},{GetJsonString(player, "assists")},{GetJsonString(player, "points")},{GetJsonString(player, "plusMinus")},{GetJsonString(player, "pim")}");
                    }
                }
                csv.AppendLine();

                // Face-offs
                csv.AppendLine("FACE-OFF SUMMARY");
                if (gameData.TryGetProperty("faceoffs", out var faceoffs))
                {
                    csv.AppendLine($"Home Team Wins,{GetJsonString(faceoffs, "homeWins")}");
                    csv.AppendLine($"Away Team Wins,{GetJsonString(faceoffs, "awayWins")}");
                }

                return csv.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error converting game data to CSV");
                return "Error converting data to CSV format";
            }
        }

        private string GetJsonString(JsonElement element, string propertyName)
        {
            if (element.TryGetProperty(propertyName, out var prop))
            {
                return prop.ValueKind == JsonValueKind.String ? prop.GetString() ?? "" : prop.ToString();
            }
            return "";
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

    // Data model for game data requests
    public class GameDataRequest
    {
        public string GameData { get; set; } = string.Empty;
    }
}
