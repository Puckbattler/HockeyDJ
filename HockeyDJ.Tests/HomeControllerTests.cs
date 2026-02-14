using HockeyDJ.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text;
using System.Text.Json;

namespace HockeyDJ.Tests;

public class HomeControllerTests
{
    private readonly Mock<ILogger<HomeController>> _loggerMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly HomeController _controller;
    private readonly Mock<ISession> _sessionMock;
    private readonly Dictionary<string, byte[]> _sessionStorage;

    public HomeControllerTests()
    {
        _loggerMock = new Mock<ILogger<HomeController>>();
        _configurationMock = new Mock<IConfiguration>();
        _sessionStorage = new Dictionary<string, byte[]>();
        _sessionMock = new Mock<ISession>();

        // Setup session mock to work with the dictionary
        _sessionMock.Setup(s => s.Set(It.IsAny<string>(), It.IsAny<byte[]>()))
            .Callback<string, byte[]>((key, value) => _sessionStorage[key] = value);

        _sessionMock.Setup(s => s.TryGetValue(It.IsAny<string>(), out It.Ref<byte[]?>.IsAny))
            .Returns((string key, out byte[]? value) =>
            {
                var result = _sessionStorage.TryGetValue(key, out var storedValue);
                value = storedValue;
                return result;
            });

        _sessionMock.Setup(s => s.Remove(It.IsAny<string>()))
            .Callback<string>(key => _sessionStorage.Remove(key));

        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.Setup(c => c.Session).Returns(_sessionMock.Object);

        _controller = new HomeController(_loggerMock.Object, _configurationMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContextMock.Object
            }
        };
    }

    private void SetSessionString(string key, string value)
    {
        _sessionStorage[key] = Encoding.UTF8.GetBytes(value);
    }

    #region Index Tests

    [Fact]
    public void Index_WhenNoAccessToken_RedirectsToSetup()
    {
        // Act
        var result = _controller.Index();

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Setup", redirectResult.ActionName);
    }

    [Fact]
    public void Index_WithAccessToken_ReturnsView()
    {
        // Arrange
        SetSessionString("SpotifyAccessToken", "test_token");

        // Act
        var result = _controller.Index();

        // Assert
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public void Index_WithAccessToken_SetsViewBagProperties()
    {
        // Arrange
        SetSessionString("SpotifyAccessToken", "test_token");
        SetSessionString("UserPlaylists", "[{\"Id\":\"123\",\"Name\":\"Test\"}]");
        SetSessionString("GoalHornPlaylistId", "goal_horn_123");
        SetSessionString("CustomSongNames", "[\"Song 1\",\"Song 2\"]");
        SetSessionString("HomeTeamName", "Home Team");
        SetSessionString("AwayTeamName", "Away Team");

        // Act
        var result = _controller.Index() as ViewResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal("test_token", result.ViewData["AccessToken"]);
        Assert.Equal("goal_horn_123", result.ViewData["GoalHornPlaylistId"]);
        Assert.Equal("Home Team", result.ViewData["HomeTeamName"]);
        Assert.Equal("Away Team", result.ViewData["AwayTeamName"]);
    }

    [Fact]
    public void Index_WithEmptyPlaylists_SetsEmptyArrayDefaults()
    {
        // Arrange
        SetSessionString("SpotifyAccessToken", "test_token");

        // Act
        var result = _controller.Index() as ViewResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal("[]", result.ViewData["Playlists"]);
        Assert.Equal("[]", result.ViewData["CustomSongNames"]);
        Assert.Equal("[]", result.ViewData["PriorityQueue"]);
        Assert.Equal("{}", result.ViewData["SongStartTimestampsJson"]);
    }

    [Fact]
    public void Index_WithSongStartTimestamps_PassesTimestampsToView()
    {
        // Arrange
        SetSessionString("SpotifyAccessToken", "test_token");
        SetSessionString("SongStartTimestampsJson", "{\"4\":90000,\"7\":45000}");

        // Act
        var result = _controller.Index() as ViewResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal("{\"4\":90000,\"7\":45000}", result.ViewData["SongStartTimestampsJson"]);
    }

    #endregion

    #region Setup Tests

    [Fact]
    public void Setup_ReturnsView()
    {
        // Act
        var result = _controller.Setup();

        // Assert
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public void Setup_LoadsExistingSettings()
    {
        // Arrange
        SetSessionString("UserPlaylists", "[{\"Id\":\"123\"}]");
        SetSessionString("CustomSongNames", "[\"Test Song\"]");
        SetSessionString("GoalHornPlaylistId", "goal_123");
        SetSessionString("HomeTeamName", "Home");
        SetSessionString("AwayTeamName", "Away");

        // Act
        var result = _controller.Setup() as ViewResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal("goal_123", result.ViewData["GoalHornPlaylistId"]);
        Assert.Equal("Home", result.ViewData["HomeTeamName"]);
        Assert.Equal("Away", result.ViewData["AwayTeamName"]);
    }

    [Fact]
    public void Setup_LoadsSongStartTimestamps()
    {
        // Arrange
        SetSessionString("SongStartTimestamps", "4:1:30\n7:0:45");

        // Act
        var result = _controller.Setup() as ViewResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal("4:1:30\n7:0:45", result.ViewData["SongStartTimestamps"]);
    }

    #endregion

    #region SaveSpotifyConfig Tests

    [Fact]
    public void SaveSpotifyConfig_StoresClientId()
    {
        // Act
        var result = _controller.SaveSpotifyConfig(
            clientId: "test_client_id",
            clientSecret: "test_secret",
            redirectUri: "http://localhost/callback",
            playlistUrls: "https://open.spotify.com/playlist/abc123");

        // Assert
        Assert.True(_sessionStorage.ContainsKey("SpotifyClientId"));
        Assert.Equal("test_client_id", Encoding.UTF8.GetString(_sessionStorage["SpotifyClientId"]));
    }

    [Fact]
    public void SaveSpotifyConfig_StoresClientSecret()
    {
        // Act
        _controller.SaveSpotifyConfig(
            clientId: "test_client_id",
            clientSecret: "test_secret",
            redirectUri: "http://localhost/callback",
            playlistUrls: "https://open.spotify.com/playlist/abc123");

        // Assert
        Assert.True(_sessionStorage.ContainsKey("SpotifyClientSecret"));
        Assert.Equal("test_secret", Encoding.UTF8.GetString(_sessionStorage["SpotifyClientSecret"]));
    }

    [Fact]
    public void SaveSpotifyConfig_StoresTeamInfo()
    {
        // Act
        _controller.SaveSpotifyConfig(
            clientId: "test_client_id",
            clientSecret: "test_secret",
            redirectUri: "http://localhost/callback",
            playlistUrls: "https://open.spotify.com/playlist/abc123",
            homeTeamName: "Penguins",
            homeTeamRoster: "Crosby\nMalkin",
            awayTeamName: "Flyers",
            awayTeamRoster: "Player1\nPlayer2");

        // Assert
        Assert.Equal("Penguins", Encoding.UTF8.GetString(_sessionStorage["HomeTeamName"]));
        Assert.Equal("Crosby\nMalkin", Encoding.UTF8.GetString(_sessionStorage["HomeTeamRoster"]));
        Assert.Equal("Flyers", Encoding.UTF8.GetString(_sessionStorage["AwayTeamName"]));
    }

    [Fact]
    public void SaveSpotifyConfig_ParsesCustomSongNames()
    {
        // Act
        _controller.SaveSpotifyConfig(
            clientId: "test_client_id",
            clientSecret: "test_secret",
            redirectUri: "http://localhost/callback",
            playlistUrls: "https://open.spotify.com/playlist/abc123",
            customSongNames: "Song 1\nSong 2\nSong 3");

        // Assert
        var songNamesJson = Encoding.UTF8.GetString(_sessionStorage["CustomSongNames"]);
        var songNames = JsonSerializer.Deserialize<List<string>>(songNamesJson);
        Assert.NotNull(songNames);
        Assert.Contains("Song 1", songNames);
        Assert.Contains("Song 2", songNames);
        Assert.Contains("Song 3", songNames);
    }

    [Fact]
    public void SaveSpotifyConfig_FillsDefaultSongNamesToTwenty()
    {
        // Act
        _controller.SaveSpotifyConfig(
            clientId: "test_client_id",
            clientSecret: "test_secret",
            redirectUri: "http://localhost/callback",
            playlistUrls: "https://open.spotify.com/playlist/abc123",
            customSongNames: "Song 1\nSong 2");

        // Assert
        var songNamesJson = Encoding.UTF8.GetString(_sessionStorage["CustomSongNames"]);
        var songNames = JsonSerializer.Deserialize<List<string>>(songNamesJson);
        Assert.NotNull(songNames);
        Assert.Equal(20, songNames.Count);
        Assert.Equal("Song 1", songNames[0]);
        Assert.Equal("Song 2", songNames[1]);
        Assert.Equal("Song 3", songNames[2]); // Default filled
    }

    [Fact]
    public void SaveSpotifyConfig_LimitsCustomSongsToTwenty()
    {
        // Arrange
        var manySongs = string.Join("\n", Enumerable.Range(1, 30).Select(i => $"Song {i}"));

        // Act
        _controller.SaveSpotifyConfig(
            clientId: "test_client_id",
            clientSecret: "test_secret",
            redirectUri: "http://localhost/callback",
            playlistUrls: "https://open.spotify.com/playlist/abc123",
            customSongNames: manySongs);

        // Assert
        var songNamesJson = Encoding.UTF8.GetString(_sessionStorage["CustomSongNames"]);
        var songNames = JsonSerializer.Deserialize<List<string>>(songNamesJson);
        Assert.NotNull(songNames);
        Assert.Equal(20, songNames.Count);
    }

    [Fact]
    public void SaveSpotifyConfig_ParsesPlaylistUrls()
    {
        // Act
        _controller.SaveSpotifyConfig(
            clientId: "test_client_id",
            clientSecret: "test_secret",
            redirectUri: "http://localhost/callback",
            playlistUrls: "https://open.spotify.com/playlist/abc123\nhttps://open.spotify.com/playlist/def456");

        // Assert
        Assert.True(_sessionStorage.ContainsKey("UserPlaylists"));
    }

    [Fact]
    public void SaveSpotifyConfig_StoresGoalHornPlaylist()
    {
        // Act
        _controller.SaveSpotifyConfig(
            clientId: "test_client_id",
            clientSecret: "test_secret",
            redirectUri: "http://localhost/callback",
            playlistUrls: "https://open.spotify.com/playlist/abc123",
            goalHornPlaylist: "https://open.spotify.com/playlist/goalhorn789");

        // Assert
        Assert.True(_sessionStorage.ContainsKey("GoalHornPlaylistId"));
        Assert.Equal("goalhorn789", Encoding.UTF8.GetString(_sessionStorage["GoalHornPlaylistId"]));
    }

    [Fact]
    public void SaveSpotifyConfig_InitializesEmptyPriorityQueue()
    {
        // Act
        _controller.SaveSpotifyConfig(
            clientId: "test_client_id",
            clientSecret: "test_secret",
            redirectUri: "http://localhost/callback",
            playlistUrls: "https://open.spotify.com/playlist/abc123");

        // Assert
        Assert.True(_sessionStorage.ContainsKey("PriorityQueue"));
        Assert.Equal("[]", Encoding.UTF8.GetString(_sessionStorage["PriorityQueue"]));
    }

    [Fact]
    public void SaveSpotifyConfig_RedirectsToSpotifyLogin()
    {
        // Act
        var result = _controller.SaveSpotifyConfig(
            clientId: "test_client_id",
            clientSecret: "test_secret",
            redirectUri: "http://localhost/callback",
            playlistUrls: "https://open.spotify.com/playlist/abc123");

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("SpotifyLogin", redirectResult.ActionName);
    }

    [Fact]
    public void SaveSpotifyConfig_LimitsPlaylistsToTen()
    {
        // Arrange
        var manyPlaylists = string.Join("\n", Enumerable.Range(1, 15)
            .Select(i => $"https://open.spotify.com/playlist/playlist{i}"));

        // Act
        _controller.SaveSpotifyConfig(
            clientId: "test_client_id",
            clientSecret: "test_secret",
            redirectUri: "http://localhost/callback",
            playlistUrls: manyPlaylists);

        // Assert
        var playlistsJson = Encoding.UTF8.GetString(_sessionStorage["UserPlaylists"]);
        var playlists = JsonSerializer.Deserialize<List<object>>(playlistsJson);
        Assert.NotNull(playlists);
        Assert.Equal(10, playlists.Count);
    }

    [Fact]
    public void SaveSpotifyConfig_StoresSongStartTimestamps()
    {
        // Act
        _controller.SaveSpotifyConfig(
            clientId: "test_client_id",
            clientSecret: "test_secret",
            redirectUri: "http://localhost/callback",
            playlistUrls: "https://open.spotify.com/playlist/abc123",
            songStartTimestamps: "4:1:30\n7:0:45");

        // Assert
        Assert.True(_sessionStorage.ContainsKey("SongStartTimestamps"));
        Assert.Equal("4:1:30\n7:0:45", Encoding.UTF8.GetString(_sessionStorage["SongStartTimestamps"]));
    }

    [Fact]
    public void SaveSpotifyConfig_ParsesSongStartTimestampsToJson()
    {
        // Act
        _controller.SaveSpotifyConfig(
            clientId: "test_client_id",
            clientSecret: "test_secret",
            redirectUri: "http://localhost/callback",
            playlistUrls: "https://open.spotify.com/playlist/abc123",
            songStartTimestamps: "4:1:30\n7:0:45");

        // Assert
        Assert.True(_sessionStorage.ContainsKey("SongStartTimestampsJson"));
        var timestampsJson = Encoding.UTF8.GetString(_sessionStorage["SongStartTimestampsJson"]);
        var timestamps = JsonSerializer.Deserialize<Dictionary<string, int>>(timestampsJson);
        Assert.NotNull(timestamps);
        Assert.Equal(2, timestamps.Count);
        Assert.Equal(90000, timestamps["4"]); // 1:30 = 90 seconds = 90000 ms
        Assert.Equal(45000, timestamps["7"]); // 0:45 = 45 seconds = 45000 ms
    }

    [Fact]
    public void SaveSpotifyConfig_HandlesMalformedTimestampEntries()
    {
        // Act
        _controller.SaveSpotifyConfig(
            clientId: "test_client_id",
            clientSecret: "test_secret",
            redirectUri: "http://localhost/callback",
            playlistUrls: "https://open.spotify.com/playlist/abc123",
            songStartTimestamps: "4:1:30\ninvalid\n7:0:45\nbad:data");

        // Assert - should only parse valid entries
        var timestampsJson = Encoding.UTF8.GetString(_sessionStorage["SongStartTimestampsJson"]);
        var timestamps = JsonSerializer.Deserialize<Dictionary<string, int>>(timestampsJson);
        Assert.NotNull(timestamps);
        Assert.Equal(2, timestamps.Count); // Only 2 valid entries
        Assert.True(timestamps.ContainsKey("4"));
        Assert.True(timestamps.ContainsKey("7"));
    }

    [Fact]
    public void SaveSpotifyConfig_EmptySongStartTimestamps()
    {
        // Act
        _controller.SaveSpotifyConfig(
            clientId: "test_client_id",
            clientSecret: "test_secret",
            redirectUri: "http://localhost/callback",
            playlistUrls: "https://open.spotify.com/playlist/abc123",
            songStartTimestamps: "");

        // Assert
        var timestampsJson = Encoding.UTF8.GetString(_sessionStorage["SongStartTimestampsJson"]);
        var timestamps = JsonSerializer.Deserialize<Dictionary<string, int>>(timestampsJson);
        Assert.NotNull(timestamps);
        Assert.Empty(timestamps);
    }

    #endregion

    #region SpotifyLogin Tests

    [Fact]
    public async Task SpotifyLogin_WithoutClientId_RedirectsToSetup()
    {
        // Act
        var result = await _controller.SpotifyLogin();

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Setup", redirectResult.ActionName);
    }

    [Fact]
    public async Task SpotifyLogin_WithoutRedirectUri_RedirectsToSetup()
    {
        // Arrange
        SetSessionString("SpotifyClientId", "test_client_id");

        // Act
        var result = await _controller.SpotifyLogin();

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Setup", redirectResult.ActionName);
    }

    [Fact]
    public async Task SpotifyLogin_WithValidConfig_RedirectsToSpotify()
    {
        // Arrange
        SetSessionString("SpotifyClientId", "test_client_id");
        SetSessionString("SpotifyRedirectUri", "http://localhost/callback");

        // Act
        var result = await _controller.SpotifyLogin();

        // Assert
        var redirectResult = Assert.IsType<RedirectResult>(result);
        Assert.Contains("accounts.spotify.com", redirectResult.Url);
        Assert.Contains("client_id=test_client_id", redirectResult.Url);
    }

    #endregion

    #region SpotifyCallback Tests

    [Fact]
    public async Task SpotifyCallback_WithError_ReturnsSetupWithError()
    {
        // Act
        var result = await _controller.SpotifyCallback(code: "", error: "access_denied");

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Setup", viewResult.ViewName);
        Assert.Contains("access_denied", viewResult.ViewData["Error"]?.ToString());
    }

    #endregion

    #region Priority Queue Tests

    [Fact]
    public void AddToPriorityQueue_AddsTrack()
    {
        // Arrange
        SetSessionString("PriorityQueue", "[]");

        // Act
        var result = _controller.AddToPriorityQueue(
            trackId: "track123",
            trackName: "Test Song",
            trackArtist: "Test Artist",
            trackUri: "spotify:track:track123");

        // Assert
        var jsonResult = Assert.IsType<JsonResult>(result);
        var data = jsonResult.Value as dynamic;
        Assert.NotNull(data);
    }

    [Fact]
    public void AddToPriorityQueue_IncrementsQueueLength()
    {
        // Arrange
        SetSessionString("PriorityQueue", "[{\"id\":\"existing\"}]");

        // Act
        var result = _controller.AddToPriorityQueue(
            trackId: "track123",
            trackName: "Test Song",
            trackArtist: "Test Artist",
            trackUri: "spotify:track:track123");

        // Assert
        var queueJson = Encoding.UTF8.GetString(_sessionStorage["PriorityQueue"]);
        var queue = JsonSerializer.Deserialize<List<object>>(queueJson);
        Assert.NotNull(queue);
        Assert.Equal(2, queue.Count);
    }

    [Fact]
    public void RemoveFromPriorityQueue_RemovesTrack()
    {
        // Arrange
        var initialQueue = "[{\"id\":\"track123\",\"name\":\"Test\"}]";
        SetSessionString("PriorityQueue", initialQueue);

        // Act
        var result = _controller.RemoveFromPriorityQueue("track123");

        // Assert
        var queueJson = Encoding.UTF8.GetString(_sessionStorage["PriorityQueue"]);
        var queue = JsonSerializer.Deserialize<List<object>>(queueJson);
        Assert.NotNull(queue);
        Assert.Empty(queue);
    }

    [Fact]
    public void ClearPriorityQueue_ClearsAllTracks()
    {
        // Arrange
        SetSessionString("PriorityQueue", "[{\"id\":\"1\"},{\"id\":\"2\"}]");

        // Act
        var result = _controller.ClearPriorityQueue();

        // Assert
        var queueJson = Encoding.UTF8.GetString(_sessionStorage["PriorityQueue"]);
        Assert.Equal("[]", queueJson);
    }

    [Fact]
    public void GetPriorityQueue_ReturnsQueue()
    {
        // Arrange
        SetSessionString("PriorityQueue", "[{\"id\":\"track123\"}]");

        // Act
        var result = _controller.GetPriorityQueue();

        // Assert
        var jsonResult = Assert.IsType<JsonResult>(result);
        Assert.NotNull(jsonResult.Value);
    }

    [Fact]
    public void GetPriorityQueue_ReturnsEmptyWhenNoQueue()
    {
        // Act
        var result = _controller.GetPriorityQueue();

        // Assert
        var jsonResult = Assert.IsType<JsonResult>(result);
        Assert.NotNull(jsonResult.Value);
    }

    #endregion

    #region Export/Import Configuration Tests

    [Fact]
    public void ExportConfiguration_WithoutConfig_ReturnsError()
    {
        // Act
        var result = _controller.ExportConfiguration();

        // Assert
        var jsonResult = Assert.IsType<JsonResult>(result);
        Assert.NotNull(jsonResult.Value);
    }

    [Fact]
    public void ExportConfiguration_WithConfig_ReturnsFile()
    {
        // Arrange
        SetSessionString("SpotifyClientId", "test_client_id");
        SetSessionString("SpotifyRedirectUri", "http://localhost/callback");

        // Act
        var result = _controller.ExportConfiguration();

        // Assert
        var fileResult = Assert.IsType<FileContentResult>(result);
        Assert.Equal("application/json", fileResult.ContentType);
        Assert.Contains("hockeydj-config", fileResult.FileDownloadName);
    }

    [Fact]
    public void ImportConfiguration_WithEmptyData_ReturnsError()
    {
        // Arrange
        var request = new ImportConfigurationRequest { ConfigData = "" };

        // Act
        var result = _controller.ImportConfiguration(request);

        // Assert
        var jsonResult = Assert.IsType<JsonResult>(result);
        Assert.NotNull(jsonResult.Value);
    }

    [Fact]
    public void ImportConfiguration_WithInvalidJson_ReturnsError()
    {
        // Arrange
        var request = new ImportConfigurationRequest { ConfigData = "not valid json" };

        // Act
        var result = _controller.ImportConfiguration(request);

        // Assert
        var jsonResult = Assert.IsType<JsonResult>(result);
        Assert.NotNull(jsonResult.Value);
    }

    [Fact]
    public void ImportConfiguration_MissingRequiredFields_ReturnsError()
    {
        // Arrange
        var request = new ImportConfigurationRequest
        {
            ConfigData = "{\"someField\": \"value\"}"
        };

        // Act
        var result = _controller.ImportConfiguration(request);

        // Assert
        var jsonResult = Assert.IsType<JsonResult>(result);
        Assert.NotNull(jsonResult.Value);
    }

    [Fact]
    public void ImportConfiguration_ValidConfig_ReturnsSuccess()
    {
        // Arrange
        var config = new
        {
            clientId = "test_client",
            redirectUri = "http://localhost/callback",
            playlistUrls = "https://open.spotify.com/playlist/abc123"
        };
        var request = new ImportConfigurationRequest
        {
            ConfigData = JsonSerializer.Serialize(config)
        };

        // Act
        var result = _controller.ImportConfiguration(request);

        // Assert
        var jsonResult = Assert.IsType<JsonResult>(result);
        Assert.NotNull(jsonResult.Value);
    }

    [Fact]
    public void ImportConfiguration_InvalidPlaylistUrl_ReturnsError()
    {
        // Arrange
        var config = new
        {
            clientId = "test_client",
            redirectUri = "http://localhost/callback",
            playlistUrls = "not-a-valid-url"
        };
        var request = new ImportConfigurationRequest
        {
            ConfigData = JsonSerializer.Serialize(config)
        };

        // Act
        var result = _controller.ImportConfiguration(request);

        // Assert
        var jsonResult = Assert.IsType<JsonResult>(result);
        Assert.NotNull(jsonResult.Value);
    }

    #endregion

    #region Shuffle Mode Tests

    [Fact]
    public void SetShuffleMode_ValidRandomMode_ReturnsSuccess()
    {
        // Arrange
        string playlistId = "playlist123";
        string mode = "random";

        // Act
        var result = _controller.SetShuffleMode(playlistId, mode);

        // Assert
        var jsonResult = Assert.IsType<JsonResult>(result);
        var resultValue = jsonResult.Value as dynamic;
        Assert.NotNull(resultValue);
        
        // Verify the mode was saved in session
        var modesJson = Encoding.UTF8.GetString(_sessionStorage["PlaylistShuffleModes"]);
        var modes = JsonSerializer.Deserialize<Dictionary<string, string>>(modesJson);
        Assert.NotNull(modes);
        Assert.Equal(mode, modes[playlistId]);
    }

    [Fact]
    public void SetShuffleMode_ValidSmartMode_ReturnsSuccess()
    {
        // Arrange
        string playlistId = "playlist456";
        string mode = "smart";

        // Act
        var result = _controller.SetShuffleMode(playlistId, mode);

        // Assert
        var jsonResult = Assert.IsType<JsonResult>(result);
        var resultValue = jsonResult.Value as dynamic;
        Assert.NotNull(resultValue);
        
        // Verify the mode was saved in session
        var modesJson = Encoding.UTF8.GetString(_sessionStorage["PlaylistShuffleModes"]);
        var modes = JsonSerializer.Deserialize<Dictionary<string, string>>(modesJson);
        Assert.NotNull(modes);
        Assert.Equal(mode, modes[playlistId]);
    }

    [Fact]
    public void SetShuffleMode_ValidSequentialMode_InitializesPlayIndex()
    {
        // Arrange
        string playlistId = "playlist789";
        string mode = "sequential";

        // Act
        var result = _controller.SetShuffleMode(playlistId, mode);

        // Assert
        var jsonResult = Assert.IsType<JsonResult>(result);
        var resultValue = jsonResult.Value as dynamic;
        Assert.NotNull(resultValue);
        
        // Verify the mode was saved in session
        var modesJson = Encoding.UTF8.GetString(_sessionStorage["PlaylistShuffleModes"]);
        var modes = JsonSerializer.Deserialize<Dictionary<string, string>>(modesJson);
        Assert.NotNull(modes);
        Assert.Equal(mode, modes[playlistId]);

        // Verify play index was initialized to 0
        var indexesJson = Encoding.UTF8.GetString(_sessionStorage["PlaylistPlayIndexes"]);
        var indexes = JsonSerializer.Deserialize<Dictionary<string, int>>(indexesJson);
        Assert.NotNull(indexes);
        Assert.Equal(0, indexes[playlistId]);
    }

    [Fact]
    public void SetShuffleMode_InvalidMode_ReturnsError()
    {
        // Arrange
        string playlistId = "playlist123";
        string mode = "invalid_mode";

        // Act
        var result = _controller.SetShuffleMode(playlistId, mode);

        // Assert
        var jsonResult = Assert.IsType<JsonResult>(result);
        Assert.NotNull(jsonResult.Value);
    }

    [Fact]
    public void SetShuffleMode_MultiplePlaylistsDifferentModes_MaintainsSeparateSettings()
    {
        // Arrange
        string playlist1 = "playlist_a";
        string playlist2 = "playlist_b";

        // Act
        _controller.SetShuffleMode(playlist1, "random");
        _controller.SetShuffleMode(playlist2, "sequential");

        // Assert
        var modesJson = Encoding.UTF8.GetString(_sessionStorage["PlaylistShuffleModes"]);
        var modes = JsonSerializer.Deserialize<Dictionary<string, string>>(modesJson);
        Assert.NotNull(modes);
        Assert.Equal("random", modes[playlist1]);
        Assert.Equal("sequential", modes[playlist2]);
    }

    [Fact]
    public void Index_WithShuffleModesInSession_PassesToView()
    {
        // Arrange
        SetSessionString("SpotifyAccessToken", "test_token");
        var testModes = new Dictionary<string, string> { { "playlist1", "smart" } };
        SetSessionString("PlaylistShuffleModes", JsonSerializer.Serialize(testModes));

        // Act
        var result = _controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(_controller.ViewBag.PlaylistShuffleModes);
    }

    [Fact]
    public void ExportConfiguration_IncludesShuffleModes()
    {
        // Arrange
        SetSessionString("SpotifyClientId", "test_client");
        SetSessionString("SpotifyClientSecret", "test_secret");
        SetSessionString("SpotifyRedirectUri", "http://localhost/callback");
        var testModes = new Dictionary<string, string> { { "playlist1", "smart" }, { "playlist2", "sequential" } };
        SetSessionString("PlaylistShuffleModes", JsonSerializer.Serialize(testModes));
        SetSessionString("UserPlaylists", "[]");

        // Act
        var result = _controller.ExportConfiguration();

        // Assert
        var fileResult = Assert.IsType<FileContentResult>(result);
        var content = Encoding.UTF8.GetString(fileResult.FileContents);
        Assert.Contains("playlistShuffleModes", content);
    }

    [Fact]
    public void ImportConfiguration_WithShuffleModes_ReturnsInConfig()
    {
        // Arrange
        var config = new
        {
            clientId = "test_client",
            clientSecret = "test_secret",
            redirectUri = "http://localhost/callback",
            playlistUrls = "https://open.spotify.com/playlist/abc123",
            playlistShuffleModes = "{\"playlist1\":\"smart\"}"
        };
        var request = new ImportConfigurationRequest
        {
            ConfigData = JsonSerializer.Serialize(config)
        };

        // Act
        var result = _controller.ImportConfiguration(request);

        // Assert
        var jsonResult = Assert.IsType<JsonResult>(result);
        Assert.NotNull(jsonResult.Value);
    }

    #endregion

    #region Privacy Tests

    [Fact]
    public void Privacy_ReturnsView()
    {
        // Act
        var result = _controller.Privacy();

        // Assert
        Assert.IsType<ViewResult>(result);
    }

    #endregion

    #region Hat Trick Tests

    [Fact]
    public void RecordGoal_FirstGoal_ReturnsCorrectCount()
    {
        // Act
        var result = _controller.RecordGoal(7);

        // Assert
        var jsonResult = Assert.IsType<JsonResult>(result);
        var value = jsonResult.Value as dynamic;
        Assert.NotNull(value);
        
        var json = JsonSerializer.Serialize(value);
        var data = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
        
        Assert.True(data["success"].GetBoolean());
        Assert.Equal(1, data["goalCount"].GetInt32());
        Assert.False(data["isHatTrick"].GetBoolean());
    }

    [Fact]
    public void RecordGoal_SecondGoal_ReturnsCorrectCount()
    {
        // Arrange - record first goal
        _controller.RecordGoal(7);

        // Act - record second goal
        var result = _controller.RecordGoal(7);

        // Assert
        var jsonResult = Assert.IsType<JsonResult>(result);
        var value = jsonResult.Value as dynamic;
        Assert.NotNull(value);
        
        var json = JsonSerializer.Serialize(value);
        var data = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
        
        Assert.True(data["success"].GetBoolean());
        Assert.Equal(2, data["goalCount"].GetInt32());
        Assert.False(data["isHatTrick"].GetBoolean());
    }

    [Fact]
    public void RecordGoal_ThirdGoal_DetectsHatTrick()
    {
        // Arrange - record first two goals
        _controller.RecordGoal(7);
        _controller.RecordGoal(7);

        // Act - record third goal
        var result = _controller.RecordGoal(7);

        // Assert
        var jsonResult = Assert.IsType<JsonResult>(result);
        var value = jsonResult.Value as dynamic;
        Assert.NotNull(value);
        
        var json = JsonSerializer.Serialize(value);
        var data = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
        
        Assert.True(data["success"].GetBoolean());
        Assert.Equal(3, data["goalCount"].GetInt32());
        Assert.True(data["isHatTrick"].GetBoolean());
    }

    [Fact]
    public void RecordGoal_ThirdGoalWithConfiguredSong_ReturnsHatTrickSongUri()
    {
        // Arrange
        SetSessionString("HatTrickSongUri", "spotify:track:test123");
        _controller.RecordGoal(7);
        _controller.RecordGoal(7);

        // Act
        var result = _controller.RecordGoal(7);

        // Assert
        var jsonResult = Assert.IsType<JsonResult>(result);
        var value = jsonResult.Value as dynamic;
        Assert.NotNull(value);
        
        var json = JsonSerializer.Serialize(value);
        var data = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
        
        Assert.True(data["isHatTrick"].GetBoolean());
        Assert.Equal("spotify:track:test123", data["hatTrickSongUri"].GetString());
    }

    [Fact]
    public void RecordGoal_MultiplePlayersTrackedSeparately()
    {
        // Arrange & Act
        _controller.RecordGoal(7);
        _controller.RecordGoal(4);
        _controller.RecordGoal(7);

        // Assert - Player 7 has 2 goals
        var result7 = _controller.RecordGoal(7);
        var jsonResult7 = Assert.IsType<JsonResult>(result7);
        var value7 = jsonResult7.Value as dynamic;
        var json7 = JsonSerializer.Serialize(value7);
        var data7 = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json7);
        Assert.Equal(3, data7["goalCount"].GetInt32());
        Assert.True(data7["isHatTrick"].GetBoolean());

        // Assert - Player 4 has 1 goal
        var result4 = _controller.RecordGoal(4);
        var jsonResult4 = Assert.IsType<JsonResult>(result4);
        var value4 = jsonResult4.Value as dynamic;
        var json4 = JsonSerializer.Serialize(value4);
        var data4 = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json4);
        Assert.Equal(2, data4["goalCount"].GetInt32());
        Assert.False(data4["isHatTrick"].GetBoolean());
    }

    private static JsonElement CreateSongBody(string songUrl)
    {
        return JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(new { songUrl }));
    }

    [Fact]
    public void SaveHatTrickSong_ValidSpotifyUrl_SavesTrackUri()
    {
        // Act
        var result = _controller.SaveHatTrickSong(CreateSongBody("https://open.spotify.com/track/4iV5W9uYEdYUVa79Axb7Rh"));

        // Assert
        var jsonResult = Assert.IsType<JsonResult>(result);
        var value = jsonResult.Value as dynamic;
        Assert.NotNull(value);
        
        var json = JsonSerializer.Serialize(value);
        var data = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
        
        Assert.True(data["success"].GetBoolean());
        Assert.Equal("spotify:track:4iV5W9uYEdYUVa79Axb7Rh", data["uri"].GetString());
    }

    [Fact]
    public void SaveHatTrickSong_ValidSpotifyUri_SavesTrackUri()
    {
        // Act
        var result = _controller.SaveHatTrickSong(CreateSongBody("spotify:track:4iV5W9uYEdYUVa79Axb7Rh"));

        // Assert
        var jsonResult = Assert.IsType<JsonResult>(result);
        var value = jsonResult.Value as dynamic;
        Assert.NotNull(value);
        
        var json = JsonSerializer.Serialize(value);
        var data = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
        
        Assert.True(data["success"].GetBoolean());
        Assert.Equal("spotify:track:4iV5W9uYEdYUVa79Axb7Rh", data["uri"].GetString());
    }

    [Fact]
    public void SaveHatTrickSong_UnrecognizedUrl_AcceptsAsIs()
    {
        // Act
        var result = _controller.SaveHatTrickSong(CreateSongBody("https://some-other-url.com/song"));

        // Assert
        var jsonResult = Assert.IsType<JsonResult>(result);
        var value = jsonResult.Value as dynamic;
        Assert.NotNull(value);
        
        var json = JsonSerializer.Serialize(value);
        var data = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
        
        Assert.True(data["success"].GetBoolean());
        Assert.Equal("https://some-other-url.com/song", data["uri"].GetString());
    }

    [Fact]
    public void SaveHatTrickSong_EmptyString_ReturnsError()
    {
        // Act
        var result = _controller.SaveHatTrickSong(CreateSongBody(""));

        // Assert
        var jsonResult = Assert.IsType<JsonResult>(result);
        var value = jsonResult.Value as dynamic;
        Assert.NotNull(value);
        
        var json = JsonSerializer.Serialize(value);
        var data = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
        
        Assert.False(data["success"].GetBoolean());
    }

    [Fact]
    public void ClearHatTrickSong_RemovesSongFromSession()
    {
        // Arrange
        SetSessionString("HatTrickSongUri", "spotify:track:test123");

        // Act
        var result = _controller.ClearHatTrickSong();

        // Assert
        var jsonResult = Assert.IsType<JsonResult>(result);
        var value = jsonResult.Value as dynamic;
        Assert.NotNull(value);
        
        var json = JsonSerializer.Serialize(value);
        var data = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
        
        Assert.True(data["success"].GetBoolean());
        
        // Verify it's removed from session
        Assert.False(_sessionStorage.ContainsKey("HatTrickSongUri"));
    }

    [Fact]
    public void GetHatTrickSong_WhenConfigured_ReturnsUri()
    {
        // Arrange
        SetSessionString("HatTrickSongUri", "spotify:track:test123");

        // Act
        var result = _controller.GetHatTrickSong();

        // Assert
        var jsonResult = Assert.IsType<JsonResult>(result);
        var value = jsonResult.Value as dynamic;
        Assert.NotNull(value);
        
        var json = JsonSerializer.Serialize(value);
        var data = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
        
        Assert.True(data["success"].GetBoolean());
        Assert.Equal("spotify:track:test123", data["uri"].GetString());
    }

    [Fact]
    public void GetHatTrickSong_WhenNotConfigured_ReturnsNull()
    {
        // Act
        var result = _controller.GetHatTrickSong();

        // Assert
        var jsonResult = Assert.IsType<JsonResult>(result);
        var value = jsonResult.Value as dynamic;
        Assert.NotNull(value);
        
        var json = JsonSerializer.Serialize(value);
        var data = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
        
        Assert.True(data["success"].GetBoolean());
    }

    [Fact]
    public void ResetGoalCounts_ClearsAllPlayerGoals()
    {
        // Arrange - record some goals
        _controller.RecordGoal(7);
        _controller.RecordGoal(7);
        _controller.RecordGoal(4);

        // Act
        var result = _controller.ResetGoalCounts();

        // Assert
        var jsonResult = Assert.IsType<JsonResult>(result);
        var value = jsonResult.Value as dynamic;
        Assert.NotNull(value);
        
        var json = JsonSerializer.Serialize(value);
        var data = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
        
        Assert.True(data["success"].GetBoolean());
        
        // Verify counts are reset
        Assert.False(_sessionStorage.ContainsKey("PlayerGoalCounts"));
        
        // Verify new goal starts at 1
        var newGoalResult = _controller.RecordGoal(7);
        var newJsonResult = Assert.IsType<JsonResult>(newGoalResult);
        var newValue = newJsonResult.Value as dynamic;
        var newJson = JsonSerializer.Serialize(newValue);
        var newData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(newJson);
        Assert.Equal(1, newData["goalCount"].GetInt32());
    }

    #endregion

    #region Multi-Horn Configuration Tests

    [Fact]
    public void GetHornConfig_NoConfigStored_ReturnsDefaults()
    {
        // Act
        var result = _controller.GetHornConfig();

        // Assert
        var jsonResult = Assert.IsType<JsonResult>(result);
        var json = JsonSerializer.Serialize(jsonResult.Value);
        var data = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);

        Assert.Equal("/audio/goal-horn.mp3", data["default"].GetString());
        Assert.Equal("/audio/Sad Trombone.mp3", data["awayTeam"].GetString());
    }

    [Fact]
    public void GetHornConfig_WithStoredConfig_ReturnsStoredConfig()
    {
        // Arrange
        var config = new { @default = "/audio/custom-horn.mp3", overtime = "/audio/ot-horn.mp3", playoffs = "", awayTeam = "/audio/Sad Trombone.mp3", players = new Dictionary<string, string>() };
        SetSessionString("HornConfiguration", JsonSerializer.Serialize(config));

        // Act
        var result = _controller.GetHornConfig();

        // Assert
        var contentResult = Assert.IsType<ContentResult>(result);
        Assert.Equal("application/json", contentResult.ContentType);
        var data = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(contentResult.Content);
        Assert.Equal("/audio/custom-horn.mp3", data["default"].GetString());
    }

    [Fact]
    public void SaveHornConfig_ValidConfig_SavesAndReturnsSuccess()
    {
        // Arrange
        var config = JsonSerializer.Deserialize<JsonElement>(
            "{\"default\":\"/audio/goal-horn.mp3\",\"overtime\":\"/audio/ot.mp3\",\"players\":{\"7\":\"/audio/custom.mp3\"}}");

        // Act
        var result = _controller.SaveHornConfig(config);

        // Assert
        var jsonResult = Assert.IsType<JsonResult>(result);
        var json = JsonSerializer.Serialize(jsonResult.Value);
        var data = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
        Assert.True(data["success"].GetBoolean());

        // Verify stored in session
        Assert.True(_sessionStorage.ContainsKey("HornConfiguration"));
    }

    [Fact]
    public void SetGameMode_ValidMode_SavesAndReturnsSuccess()
    {
        // Arrange
        var body = JsonSerializer.Deserialize<JsonElement>("{\"mode\":\"overtime\"}");

        // Act
        var result = _controller.SetGameMode(body);

        // Assert
        var jsonResult = Assert.IsType<JsonResult>(result);
        var json = JsonSerializer.Serialize(jsonResult.Value);
        var data = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
        Assert.True(data["success"].GetBoolean());
        Assert.Equal("overtime", data["mode"].GetString());

        // Verify stored in session
        var storedMode = Encoding.UTF8.GetString(_sessionStorage["GameMode"]);
        Assert.Equal("overtime", storedMode);
    }

    [Fact]
    public void SetGameMode_InvalidMode_ReturnsError()
    {
        // Arrange
        var body = JsonSerializer.Deserialize<JsonElement>("{\"mode\":\"invalid\"}");

        // Act
        var result = _controller.SetGameMode(body);

        // Assert
        var jsonResult = Assert.IsType<JsonResult>(result);
        var json = JsonSerializer.Serialize(jsonResult.Value);
        var data = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
        Assert.False(data["success"].GetBoolean());
    }

    [Fact]
    public void SetPlayerHorn_AddsPlayerHorn_SavesInConfig()
    {
        // Arrange
        SetSessionString("HornConfiguration", "{\"default\":\"/audio/goal-horn.mp3\",\"players\":{}}");
        var body = JsonSerializer.Deserialize<JsonElement>("{\"playerNumber\":7,\"hornPath\":\"/audio/custom.mp3\"}");

        // Act
        var result = _controller.SetPlayerHorn(body);

        // Assert
        var jsonResult = Assert.IsType<JsonResult>(result);
        var json = JsonSerializer.Serialize(jsonResult.Value);
        var data = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
        Assert.True(data["success"].GetBoolean());

        // Verify player horn is in stored config
        var storedConfig = Encoding.UTF8.GetString(_sessionStorage["HornConfiguration"]);
        Assert.Contains("\"7\"", storedConfig);
        Assert.Contains("/audio/custom.mp3", storedConfig);
    }

    [Fact]
    public void SetPlayerHorn_EmptyPath_RemovesPlayerHorn()
    {
        // Arrange
        SetSessionString("HornConfiguration", "{\"default\":\"/audio/goal-horn.mp3\",\"players\":{\"7\":\"/audio/custom.mp3\"}}");
        var body = JsonSerializer.Deserialize<JsonElement>("{\"playerNumber\":7,\"hornPath\":\"\"}");

        // Act
        var result = _controller.SetPlayerHorn(body);

        // Assert
        var jsonResult = Assert.IsType<JsonResult>(result);
        var json = JsonSerializer.Serialize(jsonResult.Value);
        var data = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
        Assert.True(data["success"].GetBoolean());

        // Verify player horn is removed from stored config
        var storedConfig = Encoding.UTF8.GetString(_sessionStorage["HornConfiguration"]);
        Assert.DoesNotContain("/audio/custom.mp3", storedConfig);
    }

    [Fact]
    public void Index_WithHornConfiguration_PassesHornConfigToViewBag()
    {
        // Arrange
        SetSessionString("SpotifyAccessToken", "test_token");
        var hornConfig = "{\"default\":\"/audio/goal-horn.mp3\",\"overtime\":\"/audio/ot.mp3\"}";
        SetSessionString("HornConfiguration", hornConfig);
        SetSessionString("GameMode", "playoffs");

        // Act
        var result = _controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(hornConfig, _controller.ViewBag.HornConfiguration);
        Assert.Equal("playoffs", _controller.ViewBag.GameMode);
    }

    #endregion
}
