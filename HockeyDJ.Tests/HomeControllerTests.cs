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
}
