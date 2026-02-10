# HockeyDJ Feature Implementation Plans

This document contains 6 detailed implementation plans for proposed HockeyDJ features.

---

## Table of Contents
1. [Playlist Shuffle Mode](#1-playlist-shuffle-mode)
2. [Hat Trick Mode](#2-hat-trick-mode)
3. [Sound Upload Through UI](#3-sound-upload-through-ui)
4. [Hotkey Support](#4-hotkey-support)
5. [Scoreboard API](#5-scoreboard-api)
6. [Multi-Horn Support](#6-multi-horn-support)

---

## 1. Playlist Shuffle Mode

### Overview
Add configurable shuffle modes for playlist playback: True Random, Smart Shuffle (avoids recent plays), and Sequential modes.

### User Story
> As a DJ, I want to control how songs are selected from playlists so I can prevent repetitive playback during long games.

### Requirements
- [ ] Add shuffle mode selector per playlist (Random/Smart/Sequential)
- [ ] Smart Shuffle: Avoid songs played in last N tracks (configurable)
- [ ] Sequential: Play tracks in playlist order
- [ ] Persist shuffle settings in session/config export
- [ ] Visual indicator of current mode on playlist card
- [ ] **Remove legacy `playedSongs` deduplication** - replaced by new shuffle modes

### Code to Remove (Legacy Deduplication)

The existing `playedSongs` Set and related logic should be removed since the new shuffle modes provide this functionality in a more configurable way.

#### Remove from Index.cshtml:

```javascript
// DELETE: Global playedSongs tracking (around line 1200-1210)
let playedSongs = new Set();

// DELETE: In playRandomFromPlaylist() - the filtering logic (around lines 1600-1620)
// Old code that filters out played songs:
const unplayedTracks = tracks.filter(t => !playedSongs.has(t.id));
if (unplayedTracks.length === 0) {
    playedSongs.clear(); // Reset when all played
    // ... pick from all tracks
}
// ... pick from unplayedTracks

// DELETE: Adding to playedSongs after playing (around line 1640)
playedSongs.add(selectedTrack.id);

// DELETE: Any playedSongs.clear() calls in reset functions
```

#### Remove from README.md:

```markdown
// UPDATE: Remove or modify this line in Features section:
- **Played-Song Deduplication**: Prevents immediate repeats; when all tracks have played, the list resets automatically

// REPLACE WITH:
- **Configurable Shuffle Modes**: Choose Random, Smart Shuffle (avoids recent plays), or Sequential playback per playlist
```

### Technical Design

#### Backend Changes (HomeController.cs)

```csharp
// New session keys
"PlaylistShuffleModes"    // JSON: {playlistId: "random"|"smart"|"sequential"}
"PlaylistPlayIndexes"     // JSON: {playlistId: currentIndex} for sequential mode
"SmartShuffleHistory"     // JSON: {playlistId: [trackId, trackId, ...]} last N played

// New endpoint
[HttpPost]
public IActionResult SetShuffleMode(string playlistId, string mode)
{
    // Validate mode is one of: random, smart, sequential
    // Update PlaylistShuffleModes in session
    // Reset PlaylistPlayIndexes if switching to sequential
    return Json(new { success = true });
}

// Update GetPlaylistTracks to return shuffle mode
// Update ExportConfiguration/ImportConfiguration to include shuffle settings
```

#### Frontend Changes (Index.cshtml)

```javascript
// Add shuffle mode dropdown to each playlist card
// HTML structure:
// <select class="shuffle-mode-select" data-playlist-id="...">
//   <option value="random">🎲 Random</option>
//   <option value="smart">🧠 Smart Shuffle</option>
//   <option value="sequential">📋 Sequential</option>
// </select>

// Modify playRandomFromPlaylist() function:
async function playFromPlaylist(playlistId) {
    const mode = playlistShuffleModes[playlistId] || 'random';
    let selectedTrack;
    
    switch(mode) {
        case 'sequential':
            selectedTrack = getNextSequentialTrack(playlistId);
            break;
        case 'smart':
            selectedTrack = getSmartShuffleTrack(playlistId);
            break;
        default:
            selectedTrack = getRandomTrack(playlistId);
    }
    // ... play track
}

function getSmartShuffleTrack(playlistId) {
    const history = smartShuffleHistory[playlistId] || [];
    const available = tracks.filter(t => !history.includes(t.id));
    // If all played, keep last 20% as exclusion and reset
    if (available.length === 0) {
        smartShuffleHistory[playlistId] = history.slice(-Math.floor(tracks.length * 0.2));
        return getRandomTrack(playlistId);
    }
    const track = available[Math.floor(Math.random() * available.length)];
    history.push(track.id);
    if (history.length > Math.floor(tracks.length * 0.8)) {
        history.shift(); // Sliding window
    }
    return track;
}
```

#### UI Mockup
```
┌─────────────────────────────────────┐
│ 🎵 Game Night Playlist    [🎲 ▼]   │
│ 45 tracks                           │
│ [▶️ Play] [⏸️ Pause]               │
└─────────────────────────────────────┘
         ↓ dropdown
    ┌──────────────────┐
    │ 🎲 Random        │
    │ 🧠 Smart Shuffle │
    │ 📋 Sequential    │
    └──────────────────┘
```

### Files to Modify
| File | Changes |
|------|---------|
| `HockeyDJ/Models/HomeController.cs` | Add SetShuffleMode endpoint, update config export/import |
| `HockeyDJ/Views/Home/Index.cshtml` | Add dropdown UI, modify playback logic, **remove `playedSongs` Set and related logic** |
| `HockeyDJ/Views/Home/Setup.cshtml` | Add default shuffle mode setting |
| `README.md` | Update "Played-Song Deduplication" feature description to "Configurable Shuffle Modes" |

### Testing
- [ ] Verify sequential mode plays tracks in order
- [ ] Verify smart shuffle avoids recent tracks
- [ ] Verify settings persist across page refreshes
- [ ] Verify settings export/import correctly
- [ ] **Verify legacy `playedSongs` code is fully removed**
- [ ] **Verify no regressions in goal horn playlist (also uses track selection)**

---

## 2. Hat Trick Mode

### Overview
Special extended celebration sequence when a player scores their third goal of the game.

### User Story
> As a DJ, I want an automatic special celebration when a player scores a hat trick so the crowd gets an epic moment.

### Requirements
- [ ] Track goals per player during session
- [ ] Detect when player reaches 3 goals
- [ ] Play extended celebration sequence (horn → special sound → extended song)
- [ ] **Optional: Configure specific Spotify song URL for hat trick celebrations in Setup**
- [ ] **If hat trick song configured, play that instead of normal victory song**
- [ ] Visual "HAT TRICK!" indicator with animation
- [ ] Optional confetti/visual effect
- [ ] Reset goal counts button for new games

### Technical Design

#### Backend Changes (HomeController.cs)

```csharp
// New session keys
"PlayerGoalCounts"  // JSON: {playerNumber: goalCount}
"HatTrickSongUri"   // Spotify track URI (e.g., "spotify:track:4iV5W9uYEdYUVa79Axb7Rh")

// New/updated endpoints
[HttpPost]
public IActionResult RecordGoal(int playerNumber)
{
    var counts = GetPlayerGoalCounts();
    counts[playerNumber] = counts.GetValueOrDefault(playerNumber, 0) + 1;
    SavePlayerGoalCounts(counts);
    
    bool isHatTrick = counts[playerNumber] == 3;
    string? hatTrickSongUri = isHatTrick 
        ? HttpContext.Session.GetString("HatTrickSongUri") 
        : null;
    
    return Json(new { 
        success = true, 
        goalCount = counts[playerNumber],
        isHatTrick = isHatTrick,
        hatTrickSongUri = hatTrickSongUri  // Return configured song if hat trick
    });
}

[HttpPost]
public IActionResult SaveHatTrickSong(string songUrl)
{
    // Extract track ID from URL or URI
    // Supports: https://open.spotify.com/track/xxx or spotify:track:xxx
    var trackUri = ExtractTrackUri(songUrl);
    if (string.IsNullOrEmpty(trackUri))
        return Json(new { success = false, error = "Invalid Spotify track URL" });
    
    HttpContext.Session.SetString("HatTrickSongUri", trackUri);
    return Json(new { success = true, uri = trackUri });
}

[HttpPost]
public IActionResult ClearHatTrickSong()
{
    HttpContext.Session.Remove("HatTrickSongUri");
    return Json(new { success = true });
}

private string? ExtractTrackUri(string url)
{
    // Handle spotify:track:xxx format
    if (url.StartsWith("spotify:track:"))
        return url;
    
    // Handle https://open.spotify.com/track/xxx format
    var match = Regex.Match(url, @"open\.spotify\.com/track/([a-zA-Z0-9]+)");
    if (match.Success)
        return $"spotify:track:{match.Groups[1].Value}";
    
    return null;
}

[HttpPost]
public IActionResult ResetGoalCounts()
{
    HttpContext.Session.SetString("PlayerGoalCounts", "{}");
    return Json(new { success = true });
}

[HttpGet]
public IActionResult GetGoalCounts()
{
    return Json(GetPlayerGoalCounts());
}

// Update ExportConfiguration to include HatTrickSongUri
// Update ImportConfiguration to restore HatTrickSongUri
```

#### Frontend Changes (Index.cshtml)

```javascript
// New audio file
let hatTrickAudio; // /audio/hat-trick.mp3 (special fanfare)

// Track goals client-side for immediate feedback
let playerGoalCounts = {};

async function playGoalHorn() {
    const playerNumber = getSelectedPlayerNumber();
    
    // Record goal on server
    const response = await fetch('/Home/RecordGoal', {
        method: 'POST',
        headers: {'Content-Type': 'application/json'},
        body: JSON.stringify({ playerNumber })
    });
    const result = await response.json();
    
    if (result.isHatTrick) {
        await playHatTrickCelebration(playerNumber, result.hatTrickSongUri);
    } else {
        await playStandardGoalHorn(playerNumber);
    }
    
    updateGoalCountDisplay(playerNumber, result.goalCount);
}

async function playHatTrickCelebration(playerNumber, hatTrickSongUri) {
    // 1. Show HAT TRICK banner with animation
    showHatTrickBanner(playerNumber);
    
    // 2. Play extended horn sequence
    await playAudioSequence([
        goalHornAudio,
        hatTrickAudio,  // Special fanfare
        goalHornAudio   // Second horn blast
    ]);
    
    // 3. Play hat trick song if configured, otherwise play normal victory song
    if (hatTrickSongUri) {
        await playSpotifyTrack(hatTrickSongUri);
    } else {
        await playVictorySong(playerNumber, { extended: true });
    }
    
    // 4. Trigger confetti effect
    launchConfetti();
}

function showHatTrickBanner(playerNumber) {
    const banner = document.getElementById('hat-trick-banner');
    const playerName = getPlayerName(playerNumber);
    banner.innerHTML = `🎩🎩🎩 HAT TRICK! ${playerName} 🎩🎩🎩`;
    banner.classList.add('show', 'animate-pulse');
    setTimeout(() => banner.classList.remove('show'), 10000);
}

// Optional: Simple confetti using CSS animations
function launchConfetti() {
    const container = document.getElementById('confetti-container');
    for (let i = 0; i < 50; i++) {
        const confetti = document.createElement('div');
        confetti.className = 'confetti';
        confetti.style.left = Math.random() * 100 + '%';
        confetti.style.animationDelay = Math.random() * 2 + 's';
        confetti.style.backgroundColor = ['#ff6b35', '#1e3c72', '#ffd700'][Math.floor(Math.random() * 3)];
        container.appendChild(confetti);
    }
    setTimeout(() => container.innerHTML = '', 5000);
}
```

#### CSS Additions

```css
/* Hat Trick Banner */
#hat-trick-banner {
    position: fixed;
    top: 50%;
    left: 50%;
    transform: translate(-50%, -50%) scale(0);
    background: linear-gradient(135deg, #ffd700, #ff6b35);
    color: white;
    font-size: 3rem;
    font-weight: bold;
    padding: 2rem 4rem;
    border-radius: 20px;
    z-index: 9999;
    text-shadow: 2px 2px 4px rgba(0,0,0,0.5);
    transition: transform 0.5s ease;
}

#hat-trick-banner.show {
    transform: translate(-50%, -50%) scale(1);
}

/* Confetti */
.confetti {
    position: fixed;
    width: 10px;
    height: 10px;
    top: -10px;
    animation: confetti-fall 3s ease-out forwards;
}

@keyframes confetti-fall {
    to {
        top: 100vh;
        transform: rotate(720deg);
    }
}

/* Goal count badges */
.goal-count-badge {
    background: #ff6b35;
    color: white;
    border-radius: 50%;
    width: 24px;
    height: 24px;
    display: inline-flex;
    align-items: center;
    justify-content: center;
    font-size: 0.8rem;
    margin-left: 5px;
}
```

#### UI Mockup
```
Normal goal:           Hat trick celebration:
┌──────────────┐      ┌──────────────────────────────────┐
│ #7 scored!   │      │  🎩🎩🎩 HAT TRICK! #7 🎩🎩🎩   │
│ Goal: 1      │      │     ★ CONNOR MCDAVID ★          │
└──────────────┘      │        🎉 CONFETTI 🎉            │
                      └──────────────────────────────────┘

Player buttons with goal counts:
[🎲] [1⚫] [4①] [7③🎩] [12⚫] [19②]
```

#### Setup Page UI (Setup.cshtml)

```html
<!-- Hat Trick Song Configuration -->
<div class="config-section">
    <h3>🎩 Hat Trick Celebration</h3>
    <p class="help-text">Configure a special song to play when any player scores a hat trick</p>
    
    <div class="form-group">
        <label for="hatTrickSongUrl">Hat Trick Song (Spotify URL)</label>
        <input type="text" id="hatTrickSongUrl" name="hatTrickSongUrl" 
               placeholder="https://open.spotify.com/track/... or leave blank for normal celebration"
               value="@ViewBag.HatTrickSongUrl" />
        <small class="help-text">
            Paste a Spotify track URL or URI. When a player scores their 3rd goal, 
            this song will play instead of the normal victory song.
        </small>
    </div>
    
    <div class="button-group">
        <button type="button" onclick="previewHatTrickSong()" class="btn-secondary">
            ▶️ Preview Song
        </button>
        <button type="button" onclick="clearHatTrickSong()" class="btn-danger">
            🗑️ Clear
        </button>
    </div>
    
    <span id="hatTrickSongStatus" class="status-message"></span>
</div>
```

```javascript
// Setup page JavaScript
async function saveHatTrickSong() {
    const songUrl = document.getElementById('hatTrickSongUrl').value.trim();
    const statusEl = document.getElementById('hatTrickSongStatus');
    
    if (!songUrl) {
        await clearHatTrickSong();
        return;
    }
    
    const response = await fetch('/Home/SaveHatTrickSong', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ songUrl })
    });
    const result = await response.json();
    
    if (result.success) {
        statusEl.textContent = '✓ Hat trick song saved';
        statusEl.className = 'status-message success';
    } else {
        statusEl.textContent = '✗ ' + result.error;
        statusEl.className = 'status-message error';
    }
}

async function clearHatTrickSong() {
    await fetch('/Home/ClearHatTrickSong', { method: 'POST' });
    document.getElementById('hatTrickSongUrl').value = '';
    document.getElementById('hatTrickSongStatus').textContent = 'Hat trick song cleared';
}

async function previewHatTrickSong() {
    const songUrl = document.getElementById('hatTrickSongUrl').value.trim();
    if (!songUrl) {
        alert('Enter a Spotify track URL first');
        return;
    }
    // Extract track ID and play preview (or open in Spotify)
    window.open(songUrl, '_blank');
}

// Call saveHatTrickSong when form is submitted (add to existing save flow)
```

### Files to Modify
| File | Changes |
|------|---------|
| `HockeyDJ/Models/HomeController.cs` | Add goal tracking endpoints, hat trick song save/clear endpoints |
| `HockeyDJ/Views/Home/Index.cshtml` | Hat trick detection, celebration UI, confetti |
| `HockeyDJ/Views/Home/Setup.cshtml` | **Add hat trick song URL configuration section** |
| `wwwroot/audio/` | Add `hat-trick.mp3` sound file |

### Testing
- [ ] First two goals play normal celebration
- [ ] Third goal triggers hat trick celebration
- [ ] **Third goal plays configured hat trick song if URL is set**
- [ ] **Third goal plays normal victory song if no hat trick song configured**
- [ ] Fourth+ goals play normal celebration
- [ ] Reset button clears all counts
- [ ] Goal counts persist during session
- [ ] **Hat trick song URL validates correctly (Spotify URLs and URIs)**
- [ ] **Hat trick song exports/imports with configuration**

---

## 3. Sound Upload Through UI

### Overview
Allow users to upload custom audio files directly through the web interface instead of manually adding files to the filesystem.

### User Story
> As a DJ, I want to upload my own goal horn and sound effects through the app so I don't need server file access.

### Requirements
- [ ] File upload UI for each sound type (goal horn, mushroom, clock, etc.)
- [ ] Support MP3, WAV, OGG formats
- [ ] File size limit (e.g., 10MB)
- [ ] Preview uploaded sound before saving
- [ ] Replace default sounds or add custom sounds
- [ ] Persist across sessions (save to wwwroot/audio/custom/)
- [ ] **Upload additional custom sounds with any filename**
- [ ] **Auto-generate buttons for custom sounds based on filename**
- [ ] **Auto-style buttons to match existing sound effect buttons**
- [ ] **Parse emoji from filename prefix (e.g., "🎸 Guitar Riff.mp3" → button shows 🎸)**

### Technical Design

#### Backend Changes (HomeController.cs)

```csharp
// New endpoints
[HttpPost]
[RequestSizeLimit(10_000_000)] // 10MB limit
public async Task<IActionResult> UploadSound(IFormFile file, string soundType)
{
    // Validate soundType is one of: goalhorn, mushroom, clock, trombone, 
    //                               charge, gohawks, letsgo-cowbell, letsgo-organ, hattrick
    var validTypes = new[] { "goalhorn", "mushroom", "clock", "trombone", 
                             "charge", "gohawks", "letsgo-cowbell", "letsgo-organ", "hattrick" };
    if (!validTypes.Contains(soundType))
        return BadRequest(new { success = false, error = "Invalid sound type" });
    
    // Validate file extension
    var ext = Path.GetExtension(file.FileName).ToLower();
    if (!new[] { ".mp3", ".wav", ".ogg" }.Contains(ext))
        return BadRequest(new { success = false, error = "Invalid file type. Use MP3, WAV, or OGG." });
    
    // Save to custom folder
    var customDir = Path.Combine(_env.WebRootPath, "audio", "custom");
    Directory.CreateDirectory(customDir);
    
    var fileName = $"{soundType}{ext}";
    var filePath = Path.Combine(customDir, fileName);
    
    using (var stream = new FileStream(filePath, FileMode.Create))
    {
        await file.CopyToAsync(stream);
    }
    
    // Store custom sound mapping in session
    var customSounds = GetCustomSoundMappings();
    customSounds[soundType] = $"/audio/custom/{fileName}";
    SaveCustomSoundMappings(customSounds);
    
    return Json(new { success = true, path = customSounds[soundType] });
}

[HttpPost]
public IActionResult ResetSound(string soundType)
{
    var customSounds = GetCustomSoundMappings();
    customSounds.Remove(soundType);
    SaveCustomSoundMappings(customSounds);
    
    // Optionally delete the file
    var customDir = Path.Combine(_env.WebRootPath, "audio", "custom");
    var files = Directory.GetFiles(customDir, $"{soundType}.*");
    foreach (var file in files) System.IO.File.Delete(file);
    
    return Json(new { success = true });
}

[HttpGet]
public IActionResult GetCustomSounds()
{
    return Json(GetCustomSoundMappings());
}

// Add IWebHostEnvironment injection
private readonly IWebHostEnvironment _env;
public HomeController(ILogger<HomeController> logger, IConfiguration config, IWebHostEnvironment env)
{
    _logger = logger;
    _configuration = config;
    _env = env;
}

// NEW: Upload additional custom sound with auto-generated button
[HttpPost]
[RequestSizeLimit(10_000_000)] // 10MB limit
public async Task<IActionResult> UploadCustomSound(IFormFile file)
{
    // Validate file extension
    var ext = Path.GetExtension(file.FileName).ToLower();
    if (!new[] { ".mp3", ".wav", ".ogg" }.Contains(ext))
        return BadRequest(new { success = false, error = "Invalid file type. Use MP3, WAV, OGG." });
    
    // Save to custom folder with original filename (sanitized)
    var customDir = Path.Combine(_env.WebRootPath, "audio", "custom");
    Directory.CreateDirectory(customDir);
    
    // Sanitize filename but preserve emoji at start
    var originalName = Path.GetFileNameWithoutExtension(file.FileName);
    var sanitizedName = SanitizeFileName(originalName);
    var fileName = $"{sanitizedName}{ext}";
    var filePath = Path.Combine(customDir, fileName);
    
    // Prevent overwriting - add number suffix if exists
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
    
    // Parse button metadata from filename
    var buttonInfo = ParseButtonInfoFromFileName(originalName);
    
    // Add to custom sounds list
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
    
    return Json(new { 
        success = true, 
        button = customSoundsList.Last()
    });
}

// Parse emoji and name from filename like "🎸 Guitar Riff.mp3"
private (string Emoji, string DisplayName, string ColorClass) ParseButtonInfoFromFileName(string fileName)
{
    var emoji = "🔊"; // Default emoji
    var displayName = fileName;
    
    // Check if filename starts with emoji (emoji are typically 2-4 chars)
    if (fileName.Length > 0)
    {
        var firstCodePoint = char.ConvertToUtf32(fileName, 0);
        if (firstCodePoint > 0x1F300) // Unicode emoji range
        {
            var emojiLength = char.IsSurrogatePair(fileName, 0) ? 2 : 1;
            emoji = fileName.Substring(0, emojiLength);
            displayName = fileName.Substring(emojiLength).Trim();
        }
    }
    
    // Assign color based on hash of name for variety
    var colorClasses = new[] { "btn-sound-red", "btn-sound-blue", "btn-sound-gold", 
                                "btn-sound-green", "btn-sound-purple" };
    var colorClass = colorClasses[Math.Abs(displayName.GetHashCode()) % colorClasses.Length];
    
    return (emoji, displayName, colorClass);
}

private string SanitizeFileName(string fileName)
{
    var invalid = Path.GetInvalidFileNameChars();
    return new string(fileName.Where(c => !invalid.Contains(c)).ToArray());
}

// Model for custom sound buttons
public class CustomSoundButton
{
    public string Id { get; set; }
    public string FileName { get; set; }
    public string Path { get; set; }
    public string DisplayName { get; set; }
    public string Emoji { get; set; }
    public string ColorClass { get; set; }
}

[HttpGet]
public IActionResult GetCustomSoundsList()
{
    return Json(GetCustomSoundsListFromSession());
}

[HttpPost]
public IActionResult DeleteCustomSound(string id)
{
    var customSoundsList = GetCustomSoundsListFromSession();
    var sound = customSoundsList.FirstOrDefault(s => s.Id == id);
    if (sound != null)
    {
        var filePath = Path.Combine(_env.WebRootPath, sound.Path.TrimStart('/'));
        if (System.IO.File.Exists(filePath))
            System.IO.File.Delete(filePath);
        
        customSoundsList.Remove(sound);
        SaveCustomSoundsList(customSoundsList);
    }
    return Json(new { success = true });
}

[HttpPost]
public IActionResult UpdateCustomSound(string id, string emoji, string displayName, string colorClass)
{
    var customSoundsList = GetCustomSoundsListFromSession();
    var sound = customSoundsList.FirstOrDefault(s => s.Id == id);
    if (sound != null)
    {
        sound.Emoji = emoji;
        sound.DisplayName = displayName;
        sound.ColorClass = colorClass;
        SaveCustomSoundsList(customSoundsList);
    }
    return Json(new { success = true });
}
```

#### Frontend Changes (Setup.cshtml)

```html
<!-- Sound Upload Section -->
<div class="config-section">
    <h3>🔊 Custom Sound Effects</h3>
    <p class="help-text">Upload custom audio files (MP3, WAV, OGG - max 10MB)</p>
    
    <!-- Replace Default Sounds -->
    <h4>Replace Default Sounds</h4>
    <div class="sound-upload-grid">
        <!-- Goal Horn -->
        <div class="sound-upload-card">
            <label>🚨 Goal Horn</label>
            <div class="upload-controls">
                <input type="file" id="upload-goalhorn" accept=".mp3,.wav,.ogg" 
                       onchange="uploadSound('goalhorn', this)">
                <button onclick="previewSound('goalhorn')" class="btn-preview">▶️ Preview</button>
                <button onclick="resetSound('goalhorn')" class="btn-reset">↩️ Reset</button>
            </div>
            <span id="status-goalhorn" class="upload-status"></span>
        </div>
        
        <!-- Repeat for each sound type... -->
        <div class="sound-upload-card">
            <label>🍄 Mushroom</label>
            <!-- ... -->
        </div>
        <!-- etc. -->
    </div>
    
    <!-- NEW: Add Custom Sound Buttons -->
    <h4>Add Custom Sound Buttons</h4>
    <p class="help-text">
        Upload any audio file to create a new button. 
        <strong>Tip:</strong> Start filename with an emoji for auto-styling (e.g., "🎸 Guitar Riff.mp3")
    </p>
    
    <div class="custom-sound-upload">
        <input type="file" id="upload-custom-sound" accept=".mp3,.wav,.ogg" 
               onchange="uploadCustomSound(this)">
        <span id="custom-upload-status" class="upload-status"></span>
    </div>
    
    <!-- List of uploaded custom sounds with edit/delete -->
    <div id="custom-sounds-list" class="custom-sounds-list">
        <!-- Dynamically populated -->
    </div>
</div>
```

```javascript
// Sound upload functions
async function uploadSound(soundType, input) {
    const file = input.files[0];
    if (!file) return;
    
    const statusEl = document.getElementById(`status-${soundType}`);
    statusEl.textContent = 'Uploading...';
    statusEl.className = 'upload-status uploading';
    
    const formData = new FormData();
    formData.append('file', file);
    formData.append('soundType', soundType);
    
    try {
        const response = await fetch('/Home/UploadSound', {
            method: 'POST',
            body: formData
        });
        const result = await response.json();
        
        if (result.success) {
            statusEl.textContent = '✓ Uploaded';
            statusEl.className = 'upload-status success';
            customSoundPaths[soundType] = result.path;
        } else {
            statusEl.textContent = '✗ ' + result.error;
            statusEl.className = 'upload-status error';
        }
    } catch (err) {
        statusEl.textContent = '✗ Upload failed';
        statusEl.className = 'upload-status error';
    }
}

function previewSound(soundType) {
    const path = customSoundPaths[soundType] || defaultSoundPaths[soundType];
    const audio = new Audio(path);
    audio.play();
}

async function resetSound(soundType) {
    if (!confirm('Reset to default sound?')) return;
    
    await fetch('/Home/ResetSound', {
        method: 'POST',
        headers: {'Content-Type': 'application/json'},
        body: JSON.stringify({ soundType })
    });
    
    delete customSoundPaths[soundType];
    document.getElementById(`status-${soundType}`).textContent = 'Using default';
}

// NEW: Upload custom sound and auto-generate button
async function uploadCustomSound(input) {
    const file = input.files[0];
    if (!file) return;
    
    const statusEl = document.getElementById('custom-upload-status');
    statusEl.textContent = 'Uploading...';
    statusEl.className = 'upload-status uploading';
    
    const formData = new FormData();
    formData.append('file', file);
    
    try {
        const response = await fetch('/Home/UploadCustomSound', {
            method: 'POST',
            body: formData
        });
        const result = await response.json();
        
        if (result.success) {
            statusEl.textContent = '✓ Button created!';
            statusEl.className = 'upload-status success';
            addCustomSoundToList(result.button);
            input.value = ''; // Clear input
        } else {
            statusEl.textContent = '✗ ' + result.error;
            statusEl.className = 'upload-status error';
        }
    } catch (err) {
        statusEl.textContent = '✗ Upload failed';
        statusEl.className = 'upload-status error';
    }
}

// Render custom sound in the list with edit controls
function addCustomSoundToList(button) {
    const list = document.getElementById('custom-sounds-list');
    const item = document.createElement('div');
    item.className = 'custom-sound-item';
    item.id = `custom-sound-${button.id}`;
    item.innerHTML = `
        <div class="custom-sound-preview">
            <button class="sound-btn ${button.colorClass}" onclick="previewCustomSound('${button.path}')">
                ${button.emoji}
            </button>
            <span class="custom-sound-name">${button.displayName}</span>
        </div>
        <div class="custom-sound-controls">
            <input type="text" value="${button.emoji}" maxlength="4" class="emoji-input" 
                   onchange="updateCustomSound('${button.id}', this.value, null, null)">
            <input type="text" value="${button.displayName}" class="name-input"
                   onchange="updateCustomSound('${button.id}', null, this.value, null)">
            <select onchange="updateCustomSound('${button.id}', null, null, this.value)">
                <option value="btn-sound-red" ${button.colorClass === 'btn-sound-red' ? 'selected' : ''}>🔴 Red</option>
                <option value="btn-sound-blue" ${button.colorClass === 'btn-sound-blue' ? 'selected' : ''}>🔵 Blue</option>
                <option value="btn-sound-gold" ${button.colorClass === 'btn-sound-gold' ? 'selected' : ''}>🟡 Gold</option>
                <option value="btn-sound-green" ${button.colorClass === 'btn-sound-green' ? 'selected' : ''}>🟢 Green</option>
                <option value="btn-sound-purple" ${button.colorClass === 'btn-sound-purple' ? 'selected' : ''}>🟣 Purple</option>
            </select>
            <button onclick="deleteCustomSound('${button.id}')" class="btn-delete">🗑️</button>
        </div>
    `;
    list.appendChild(item);
}

function previewCustomSound(path) {
    const audio = new Audio(path);
    audio.play();
}

async function updateCustomSound(id, emoji, displayName, colorClass) {
    await fetch('/Home/UpdateCustomSound', {
        method: 'POST',
        headers: {'Content-Type': 'application/json'},
        body: JSON.stringify({ id, emoji, displayName, colorClass })
    });
    // Reload to refresh button preview
    loadCustomSoundsList();
}

async function deleteCustomSound(id) {
    if (!confirm('Delete this sound button?')) return;
    
    await fetch('/Home/DeleteCustomSound', {
        method: 'POST',
        headers: {'Content-Type': 'application/json'},
        body: JSON.stringify({ id })
    });
    
    document.getElementById(`custom-sound-${id}`)?.remove();
}

async function loadCustomSoundsList() {
    const response = await fetch('/Home/GetCustomSoundsList');
    const sounds = await response.json();
    
    const list = document.getElementById('custom-sounds-list');
    list.innerHTML = '';
    sounds.forEach(button => addCustomSoundToList(button));
}

// Load on page init
document.addEventListener('DOMContentLoaded', loadCustomSoundsList);
```

#### Index.cshtml Changes

```javascript
// Modify initializeAudio() to check for custom sounds first
async function initializeAudio() {
    // Fetch custom sound mappings
    const response = await fetch('/Home/GetCustomSounds');
    const customSounds = await response.json();
    
    // Use custom path if available, otherwise default
    goalHornAudio = new Audio(customSounds.goalhorn || '/audio/goal-horn.mp3');
    mushroomAudio = new Audio(customSounds.mushroom || '/audio/mushroom.mp3');
    clockAudio = new Audio(customSounds.clock || '/audio/clock.mp3');
    // ... etc
}
```

```javascript
// NEW: Load custom sound buttons and render them in the UI
let customSoundButtons = [];

async function loadCustomSoundButtons() {
    const response = await fetch('/Home/GetCustomSoundsList');
    customSoundButtons = await response.json();
    renderCustomSoundButtons();
}

function renderCustomSoundButtons() {
    const container = document.getElementById('custom-sounds-container');
    if (!container || customSoundButtons.length === 0) {
        if (container) container.style.display = 'none';
        return;
    }
    
    container.style.display = 'flex';
    container.innerHTML = '';
    
    customSoundButtons.forEach(button => {
        const btn = document.createElement('button');
        btn.className = `sound-btn ${button.colorClass}`;
        btn.innerHTML = button.emoji;
        btn.title = button.displayName;
        btn.onclick = () => playCustomSound(button.path, btn);
        
        const wrapper = document.createElement('div');
        wrapper.className = 'sound-btn-wrapper';
        wrapper.appendChild(btn);
        
        const label = document.createElement('span');
        label.className = 'sound-btn-label';
        label.textContent = button.displayName;
        wrapper.appendChild(label);
        
        container.appendChild(wrapper);
    });
}

async function playCustomSound(path, buttonEl) {
    buttonEl.disabled = true;
    try {
        const audio = new Audio(path);
        audio.volume = 0.8;
        await audio.play();
    } catch (err) {
        console.warn('Failed to play custom sound:', err);
    }
    setTimeout(() => buttonEl.disabled = false, 1000);
}
```

```html
<!-- Add to Index.cshtml after existing sound buttons -->
<div id="custom-sounds-container" class="custom-sounds-container" style="display: none;">
    <!-- Dynamically populated with custom sound buttons -->
</div>
```

#### CSS for Custom Sound Buttons

```css
/* Custom sound button container */
.custom-sounds-container {
    display: flex;
    flex-wrap: wrap;
    gap: 1rem;
    margin-top: 1rem;
    padding: 1rem;
    background: rgba(255, 255, 255, 0.1);
    border-radius: 10px;
}

.sound-btn-wrapper {
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 0.25rem;
}

.sound-btn-label {
    font-size: 0.7rem;
    color: rgba(255, 255, 255, 0.8);
    max-width: 60px;
    text-align: center;
    overflow: hidden;
    text-overflow: ellipsis;
}

/* Color variants matching existing buttons */
.btn-sound-red { background: radial-gradient(circle, #ff6b6b, #c0392b); }
.btn-sound-blue { background: radial-gradient(circle, #74b9ff, #0984e3); }
.btn-sound-gold { background: radial-gradient(circle, #ffeaa7, #fdcb6e); color: #2d3436; }
.btn-sound-green { background: radial-gradient(circle, #55efc4, #00b894); }
.btn-sound-purple { background: radial-gradient(circle, #a29bfe, #6c5ce7); }
```

#### Updated UI Mockup
```
Setup Page - Add Custom Sound Buttons:
+---------------------------------------------------------------------+
| Add Custom Sound Buttons                                            |
| Upload any audio file to create a new button.                       |
| Tip: Start filename with emoji (e.g., "Guitar Riff.mp3")            |
|                                                                     |
| [Choose File] Button created!                                       |
+---------------------------------------------------------------------+
| [btn] Guitar Riff    [emoji] [Guitar Riff___] [Blue v] [Delete]    |
| [btn] Power Up       [emoji] [Power Up______] [Gold v] [Delete]    |
+---------------------------------------------------------------------+

Index Page - Custom Buttons appear with built-in sounds:
+--------------------------------------------------------------+
| Sound Effects                                                 |
| [Mush] [Clock] [Tromb] [Charge] [GoHawks] [LetsGo] [Organ]   |
|                                                               |
| Custom Sounds                                                 |
| [Guitar] [Power] [Fanfare]                                    |
+--------------------------------------------------------------+
```

### Files to Modify
| File | Changes |
|------|---------|
| `HockeyDJ/Models/HomeController.cs` | Add upload/reset endpoints, UploadCustomSound endpoint, CustomSoundButton model, IWebHostEnvironment injection |
| `HockeyDJ/Views/Home/Setup.cshtml` | Add sound upload UI section, custom sound button management UI |
| `HockeyDJ/Views/Home/Index.cshtml` | Modify initializeAudio() to load custom sounds, add custom-sounds-container div, add renderCustomSoundButtons() |
| `wwwroot/audio/custom/` | New directory for uploaded files |

### Testing
- [ ] Upload MP3/WAV/OGG files successfully
- [ ] Reject files over 10MB
- [ ] Reject invalid file types
- [ ] Preview plays correct audio
- [ ] Reset restores default sound
- [ ] Custom sounds persist across sessions
- [ ] Custom sounds work in Index.cshtml playback
- [ ] **Custom sound upload creates button automatically**
- [ ] **Button emoji parsed from filename prefix**
- [ ] **Button color assigned automatically (can be changed)**
- [ ] **Custom buttons appear in Index.cshtml with correct styling**
- [ ] **Custom buttons match existing sound button appearance**
- [ ] **Edit emoji/name/color updates button in real-time**
- [ ] **Delete removes button and file**

---

## 4. Hotkey Support

### Overview
Keyboard shortcuts for quick access to common actions during live games.

### User Story
> As a DJ, I want keyboard shortcuts so I can trigger sounds quickly without clicking buttons.

### Requirements
- [ ] Configurable hotkey mappings
- [ ] Default hotkey set for common actions
- [ ] Visual hotkey hints on buttons
- [ ] Hotkey help modal (press '?' to show)
- [ ] Prevent conflicts with browser shortcuts
- [ ] Disable hotkeys when typing in input fields

### Default Hotkey Mapping
| Key | Action |
|-----|--------|
| `G` | Goal Horn |
| `M` | Mushroom sound |
| `C` | Clock sound |
| `T` | Sad Trombone |
| `O` | Charge Organ |
| `H` | Go Hawks Cowbell |
| `L` | Let's Go Cowbell |
| `P` | Pause/Resume |
| `Space` | Play random from active playlist |
| `1-9, 0` | Select player 1-10 for goal song |
| `?` | Show hotkey help |
| `Esc` | Close modals |

### Technical Design

#### Frontend Changes (Index.cshtml)

```javascript
// Hotkey configuration (can be made customizable later)
const defaultHotkeys = {
    'g': { action: 'goalhorn', description: 'Goal Horn' },
    'm': { action: 'mushroom', description: 'Mushroom Sound' },
    'c': { action: 'clock', description: 'Clock Sound' },
    't': { action: 'trombone', description: 'Sad Trombone' },
    'o': { action: 'organ', description: 'Charge Organ' },
    'h': { action: 'gohawks', description: 'Go Hawks Cowbell' },
    'l': { action: 'letsgo', description: "Let's Go Cowbell" },
    'p': { action: 'pause', description: 'Pause/Resume' },
    ' ': { action: 'playrandom', description: 'Play Random' },
    '1': { action: 'player1', description: 'Select Player 1' },
    '2': { action: 'player2', description: 'Select Player 2' },
    // ... through 0 for player 10
    '?': { action: 'help', description: 'Show Hotkey Help' },
    'Escape': { action: 'closemodal', description: 'Close Modal' }
};

let hotkeysEnabled = true;

// Initialize hotkey listener
document.addEventListener('DOMContentLoaded', () => {
    document.addEventListener('keydown', handleHotkey);
    
    // Disable hotkeys when focused on input fields
    document.querySelectorAll('input, textarea, select').forEach(el => {
        el.addEventListener('focus', () => hotkeysEnabled = false);
        el.addEventListener('blur', () => hotkeysEnabled = true);
    });
    
    addHotkeyHints();
});

function handleHotkey(event) {
    // Skip if typing in input or hotkeys disabled
    if (!hotkeysEnabled) return;
    if (event.target.matches('input, textarea, select')) return;
    
    // Skip if modifier keys held (allow browser shortcuts)
    if (event.ctrlKey || event.altKey || event.metaKey) return;
    
    const key = event.key.toLowerCase();
    const hotkey = defaultHotkeys[key];
    
    if (!hotkey) return;
    
    event.preventDefault();
    executeHotkeyAction(hotkey.action);
}

function executeHotkeyAction(action) {
    switch(action) {
        case 'goalhorn':
            document.getElementById('goal-horn-btn')?.click();
            break;
        case 'mushroom':
            playMushroomSound();
            break;
        case 'clock':
            playClockSound();
            break;
        case 'trombone':
            playTromboneSound();
            break;
        case 'organ':
            playChargeOrgan();
            break;
        case 'gohawks':
            playGoHawksCowbell();
            break;
        case 'letsgo':
            playLetsGoCowbell();
            break;
        case 'pause':
            togglePause();
            break;
        case 'playrandom':
            playRandomFromActivePlaylist();
            break;
        case 'player1': case 'player2': case 'player3': case 'player4':
        case 'player5': case 'player6': case 'player7': case 'player8':
        case 'player9': case 'player10':
            selectPlayerByHotkey(parseInt(action.replace('player', '')));
            break;
        case 'help':
            showHotkeyHelp();
            break;
        case 'closemodal':
            closeAllModals();
            break;
    }
}

function selectPlayerByHotkey(number) {
    // Map 0 to 10 for player selection
    const playerNum = number === 0 ? 10 : number;
    const radio = document.querySelector(`input[name="songSelection"][value="${playerNum}"]`);
    if (radio) {
        radio.checked = true;
        radio.dispatchEvent(new Event('change'));
    }
}

function showHotkeyHelp() {
    const modal = document.getElementById('hotkey-help-modal');
    modal.classList.add('show');
}

// Add visual hints to buttons
function addHotkeyHints() {
    const hints = {
        'goal-horn-btn': 'G',
        'mushroom-btn': 'M',
        'clock-btn': 'C',
        'trombone-btn': 'T',
        'organ-btn': 'O'
    };
    
    Object.entries(hints).forEach(([btnId, key]) => {
        const btn = document.getElementById(btnId);
        if (btn) {
            const hint = document.createElement('span');
            hint.className = 'hotkey-hint';
            hint.textContent = key;
            btn.appendChild(hint);
        }
    });
}
```

#### CSS Additions

```css
/* Hotkey hints on buttons */
.hotkey-hint {
    position: absolute;
    top: -8px;
    right: -8px;
    background: rgba(0, 0, 0, 0.7);
    color: white;
    font-size: 0.7rem;
    padding: 2px 6px;
    border-radius: 4px;
    font-family: monospace;
}

/* Hotkey help modal */
#hotkey-help-modal {
    display: none;
    position: fixed;
    top: 50%;
    left: 50%;
    transform: translate(-50%, -50%);
    background: rgba(30, 60, 114, 0.95);
    border-radius: 15px;
    padding: 2rem;
    z-index: 10000;
    max-width: 500px;
    max-height: 80vh;
    overflow-y: auto;
}

#hotkey-help-modal.show {
    display: block;
}

.hotkey-list {
    display: grid;
    grid-template-columns: auto 1fr;
    gap: 0.5rem 1rem;
}

.hotkey-key {
    background: #333;
    color: white;
    padding: 4px 8px;
    border-radius: 4px;
    font-family: monospace;
    text-align: center;
    min-width: 30px;
}
```

#### HTML for Hotkey Modal

```html
<!-- Add to Index.cshtml -->
<div id="hotkey-help-modal">
    <h2>⌨️ Keyboard Shortcuts</h2>
    <div class="hotkey-list">
        <span class="hotkey-key">G</span><span>Goal Horn</span>
        <span class="hotkey-key">M</span><span>Mushroom Sound</span>
        <span class="hotkey-key">C</span><span>Clock Sound</span>
        <span class="hotkey-key">T</span><span>Sad Trombone</span>
        <span class="hotkey-key">O</span><span>Charge Organ</span>
        <span class="hotkey-key">H</span><span>Go Hawks Cowbell</span>
        <span class="hotkey-key">L</span><span>Let's Go Cowbell</span>
        <span class="hotkey-key">P</span><span>Pause/Resume</span>
        <span class="hotkey-key">Space</span><span>Play Random</span>
        <span class="hotkey-key">1-0</span><span>Select Player 1-10</span>
        <span class="hotkey-key">?</span><span>This Help</span>
        <span class="hotkey-key">Esc</span><span>Close Modal</span>
    </div>
    <button onclick="this.parentElement.classList.remove('show')">Close</button>
</div>
```

### Files to Modify
| File | Changes |
|------|---------|
| `HockeyDJ/Views/Home/Index.cshtml` | Add hotkey listener, help modal, visual hints |

### Testing
- [ ] All hotkeys trigger correct actions
- [ ] Hotkeys disabled when typing in search box
- [ ] Browser shortcuts (Ctrl+C, etc.) still work
- [ ] Help modal opens with '?'
- [ ] Escape closes modals
- [ ] Visual hints appear on buttons

---

## 5. Scoreboard API

### Overview
REST API endpoints to integrate with external scoreboard systems, game clocks, and arena display software.

### User Story
> As an arena operator, I want HockeyDJ to receive events from our scoreboard system so celebrations trigger automatically.

### Requirements
- [ ] REST API for external system integration
- [ ] Webhook support for push events
- [ ] API key authentication
- [ ] Events: goal scored, penalty, period change, timeout
- [ ] Get current state (playing track, queue, etc.)
- [ ] Configurable auto-actions per event type

### API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/events/goal` | Trigger goal celebration |
| POST | `/api/events/penalty` | Trigger penalty announcement |
| POST | `/api/events/period` | Period start/end event |
| POST | `/api/events/timeout` | Timeout event |
| GET | `/api/status` | Get current playback status |
| GET | `/api/queue` | Get priority queue |
| POST | `/api/queue` | Add to priority queue |
| DELETE | `/api/queue/{id}` | Remove from queue |
| POST | `/api/playback/pause` | Pause playback |
| POST | `/api/playback/resume` | Resume playback |

### Technical Design

#### New Controller (ApiController.cs)

```csharp
using Microsoft.AspNetCore.Mvc;

namespace HockeyDJ.Controllers
{
    [ApiController]
    [Route("api")]
    public class ScoreboardApiController : ControllerBase
    {
        private readonly ILogger<ScoreboardApiController> _logger;
        
        public ScoreboardApiController(ILogger<ScoreboardApiController> logger)
        {
            _logger = logger;
        }
        
        // Simple API key authentication
        private bool ValidateApiKey()
        {
            var apiKey = HttpContext.Session.GetString("ApiKey");
            var providedKey = Request.Headers["X-API-Key"].FirstOrDefault();
            return !string.IsNullOrEmpty(apiKey) && apiKey == providedKey;
        }
        
        // POST /api/events/goal
        [HttpPost("events/goal")]
        public IActionResult TriggerGoal([FromBody] GoalEventRequest request)
        {
            if (!ValidateApiKey())
                return Unauthorized(new { error = "Invalid API key" });
            
            _logger.LogInformation("Goal event received: Player {Player}, Team {Team}", 
                request.PlayerNumber, request.Team);
            
            // Store event for client polling
            var eventData = new {
                type = "goal",
                playerNumber = request.PlayerNumber,
                team = request.Team,
                assists = request.Assists,
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };
            
            HttpContext.Session.SetString("PendingEvent", 
                System.Text.Json.JsonSerializer.Serialize(eventData));
            
            return Ok(new { success = true, message = "Goal event queued" });
        }
        
        // POST /api/events/penalty
        [HttpPost("events/penalty")]
        public IActionResult TriggerPenalty([FromBody] PenaltyEventRequest request)
        {
            if (!ValidateApiKey())
                return Unauthorized(new { error = "Invalid API key" });
            
            var eventData = new {
                type = "penalty",
                playerNumber = request.PlayerNumber,
                team = request.Team,
                penaltyType = request.PenaltyType,
                duration = request.Duration,
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };
            
            HttpContext.Session.SetString("PendingEvent", 
                System.Text.Json.JsonSerializer.Serialize(eventData));
            
            return Ok(new { success = true });
        }
        
        // POST /api/events/period
        [HttpPost("events/period")]
        public IActionResult PeriodEvent([FromBody] PeriodEventRequest request)
        {
            if (!ValidateApiKey())
                return Unauthorized(new { error = "Invalid API key" });
            
            var eventData = new {
                type = "period",
                period = request.Period,
                action = request.Action, // "start", "end", "intermission"
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };
            
            HttpContext.Session.SetString("PendingEvent", 
                System.Text.Json.JsonSerializer.Serialize(eventData));
            
            return Ok(new { success = true });
        }
        
        // POST /api/events/timeout
        [HttpPost("events/timeout")]
        public IActionResult TimeoutEvent([FromBody] TimeoutEventRequest request)
        {
            if (!ValidateApiKey())
                return Unauthorized(new { error = "Invalid API key" });
            
            var eventData = new {
                type = "timeout",
                team = request.Team,
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };
            
            HttpContext.Session.SetString("PendingEvent", 
                System.Text.Json.JsonSerializer.Serialize(eventData));
            
            return Ok(new { success = true });
        }
        
        // GET /api/status
        [HttpGet("status")]
        public IActionResult GetStatus()
        {
            if (!ValidateApiKey())
                return Unauthorized(new { error = "Invalid API key" });
            
            return Ok(new {
                authenticated = !string.IsNullOrEmpty(HttpContext.Session.GetString("SpotifyAccessToken")),
                hasPlaylists = !string.IsNullOrEmpty(HttpContext.Session.GetString("UserPlaylists")),
                queueLength = GetQueueLength()
            });
        }
        
        // GET /api/events/poll
        [HttpGet("events/poll")]
        public IActionResult PollEvents()
        {
            // Called by client to check for pending events
            var pendingEvent = HttpContext.Session.GetString("PendingEvent");
            if (string.IsNullOrEmpty(pendingEvent))
                return Ok(new { hasEvent = false });
            
            // Clear the event after reading
            HttpContext.Session.Remove("PendingEvent");
            
            return Ok(new { 
                hasEvent = true, 
                eventData = System.Text.Json.JsonSerializer.Deserialize<object>(pendingEvent) 
            });
        }
        
        private int GetQueueLength()
        {
            var queue = HttpContext.Session.GetString("PriorityQueue");
            if (string.IsNullOrEmpty(queue)) return 0;
            var list = System.Text.Json.JsonSerializer.Deserialize<List<object>>(queue);
            return list?.Count ?? 0;
        }
    }
    
    // Request models
    public class GoalEventRequest
    {
        public int PlayerNumber { get; set; }
        public string Team { get; set; } = "home";
        public int[]? Assists { get; set; }
    }
    
    public class PenaltyEventRequest
    {
        public int PlayerNumber { get; set; }
        public string Team { get; set; } = "home";
        public string PenaltyType { get; set; } = "Minor";
        public int Duration { get; set; } = 2;
    }
    
    public class PeriodEventRequest
    {
        public int Period { get; set; }
        public string Action { get; set; } = "start"; // start, end, intermission
    }
    
    public class TimeoutEventRequest
    {
        public string Team { get; set; } = "home";
    }
}
```

#### Setup Page Changes (Setup.cshtml)

```html
<!-- API Configuration Section -->
<div class="config-section">
    <h3>🔌 Scoreboard API</h3>
    <p class="help-text">Configure API access for external scoreboard integration</p>
    
    <div class="form-group">
        <label>API Key</label>
        <div class="input-with-button">
            <input type="text" id="api-key" readonly value="@ViewBag.ApiKey" />
            <button onclick="generateApiKey()">🔄 Generate New</button>
            <button onclick="copyApiKey()">📋 Copy</button>
        </div>
    </div>
    
    <div class="form-group">
        <label>API Endpoint</label>
        <input type="text" readonly value="@($"{Request.Scheme}://{Request.Host}/api")" />
    </div>
    
    <details>
        <summary>📖 API Documentation</summary>
        <div class="api-docs">
            <h4>Authentication</h4>
            <p>Include header: <code>X-API-Key: YOUR_API_KEY</code></p>
            
            <h4>Goal Event</h4>
            <pre>POST /api/events/goal
{
  "playerNumber": 7,
  "team": "home",
  "assists": [19, 4]
}</pre>
            
            <h4>Penalty Event</h4>
            <pre>POST /api/events/penalty
{
  "playerNumber": 22,
  "team": "away",
  "penaltyType": "Tripping",
  "duration": 2
}</pre>
        </div>
    </details>
</div>
```

#### Client Polling (Index.cshtml)

```javascript
// Poll for scoreboard events
let eventPollInterval;

function startEventPolling() {
    eventPollInterval = setInterval(async () => {
        try {
            const response = await fetch('/api/events/poll');
            const data = await response.json();
            
            if (data.hasEvent) {
                handleScoreboardEvent(data.eventData);
            }
        } catch (err) {
            console.warn('Event polling error:', err);
        }
    }, 1000); // Poll every second
}

function handleScoreboardEvent(event) {
    console.log('Scoreboard event received:', event);
    
    switch(event.type) {
        case 'goal':
            if (event.team === 'home') {
                selectPlayer(event.playerNumber);
                playGoalHorn();
            } else {
                playTromboneSound(); // Away team goal
            }
            break;
            
        case 'penalty':
            openAnnouncementModal('penalty', event);
            break;
            
        case 'period':
            if (event.action === 'intermission') {
                // Could auto-play intermission playlist
                showNotification(`Period ${event.period} ended - Intermission`);
            }
            break;
            
        case 'timeout':
            playClockSound();
            break;
    }
}

// Start polling when page loads
document.addEventListener('DOMContentLoaded', () => {
    startEventPolling();
});
```

### Files to Create/Modify
| File | Changes |
|------|---------|
| `HockeyDJ/Controllers/ScoreboardApiController.cs` | New file - API controller |
| `HockeyDJ/Views/Home/Setup.cshtml` | API key configuration UI |
| `HockeyDJ/Views/Home/Index.cshtml` | Event polling and handling |
| `HockeyDJ/Models/HomeController.cs` | API key generation endpoint |

### Testing
- [ ] API returns 401 without valid key
- [ ] Goal event triggers celebration on home goals
- [ ] Goal event triggers trombone on away goals
- [ ] Penalty event opens announcement modal
- [ ] Period events show notifications
- [ ] Timeout event plays clock sound
- [ ] /api/status returns correct information

---

## 6. Multi-Horn Support

### Overview
Configure different goal horn sounds per player, for special occasions (overtime, playoffs), or for different teams.

### User Story
> As a DJ, I want to assign custom goal horns to star players so their goals have unique celebrations.

### Requirements
- [ ] Default horn for all players
- [ ] Per-player horn override option
- [ ] Special occasion horns (overtime, playoffs mode)
- [ ] Different horn for opponent goals (optional funny/sad horn)
- [ ] Preview horns in setup
- [ ] Horn audio upload (ties into Sound Upload feature)

### Technical Design

#### Data Structure

```javascript
// Horn configuration stored in session
{
    "default": "/audio/goal-horn.mp3",
    "overtime": "/audio/overtime-horn.mp3",
    "playoffs": "/audio/playoffs-horn.mp3",
    "awayTeam": "/audio/sad-horn.mp3",
    "players": {
        "7": "/audio/custom/horn-player7.mp3",
        "19": "/audio/custom/horn-player19.mp3"
    }
}
```

#### Backend Changes (HomeController.cs)

```csharp
// Session key
"HornConfiguration"  // JSON as shown above

// Endpoints
[HttpPost]
public IActionResult SaveHornConfig([FromBody] HornConfigRequest config)
{
    HttpContext.Session.SetString("HornConfiguration", 
        System.Text.Json.JsonSerializer.Serialize(config));
    return Json(new { success = true });
}

[HttpGet]
public IActionResult GetHornConfig()
{
    var config = HttpContext.Session.GetString("HornConfiguration");
    if (string.IsNullOrEmpty(config))
    {
        // Return defaults
        return Json(new {
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
public IActionResult SetPlayerHorn(int playerNumber, string hornPath)
{
    var config = GetHornConfiguration();
    config.Players[playerNumber.ToString()] = hornPath;
    SaveHornConfiguration(config);
    return Json(new { success = true });
}

[HttpPost]
public IActionResult SetGameMode(string mode) // "normal", "overtime", "playoffs"
{
    HttpContext.Session.SetString("GameMode", mode);
    return Json(new { success = true });
}
```

#### Frontend Changes (Index.cshtml)

```javascript
let hornConfig = {};
let gameMode = 'normal'; // normal, overtime, playoffs

async function loadHornConfig() {
    const response = await fetch('/Home/GetHornConfig');
    hornConfig = await response.json();
}

function getHornForPlayer(playerNumber, team = 'home') {
    // Away team horn
    if (team === 'away') {
        return hornConfig.awayTeam || '/audio/Sad Trombone.mp3';
    }
    
    // Check for player-specific horn
    if (hornConfig.players && hornConfig.players[playerNumber]) {
        return hornConfig.players[playerNumber];
    }
    
    // Check for game mode horn
    switch(gameMode) {
        case 'overtime':
            return hornConfig.overtime || hornConfig.default;
        case 'playoffs':
            return hornConfig.playoffs || hornConfig.default;
        default:
            return hornConfig.default || '/audio/goal-horn.mp3';
    }
}

// Modify playGoalHorn to use dynamic horn
async function playGoalHorn(team = 'home') {
    const playerNumber = getSelectedPlayerNumber();
    const hornPath = getHornForPlayer(playerNumber, team);
    
    // Create/update horn audio with correct source
    if (goalHornAudio.src !== hornPath) {
        goalHornAudio = new Audio(hornPath);
        goalHornAudio.volume = 0.8;
    }
    
    // ... rest of goal horn logic
}
```

#### Setup Page Changes (Setup.cshtml)

```html
<!-- Multi-Horn Configuration -->
<div class="config-section">
    <h3>🚨 Goal Horn Configuration</h3>
    
    <div class="form-group">
        <label>Default Horn</label>
        <select id="default-horn" onchange="updateHornConfig('default', this.value)">
            <option value="/audio/goal-horn.mp3">Standard Horn</option>
            <option value="/audio/custom/horn1.mp3">Custom Horn 1</option>
            <!-- Dynamic options from uploaded sounds -->
        </select>
        <button onclick="previewHorn('default')">▶️</button>
    </div>
    
    <div class="form-group">
        <label>Overtime Horn</label>
        <select id="overtime-horn">
            <option value="">Same as Default</option>
            <option value="/audio/overtime-horn.mp3">Epic Overtime Horn</option>
        </select>
        <button onclick="previewHorn('overtime')">▶️</button>
    </div>
    
    <div class="form-group">
        <label>Away Team Goal Sound</label>
        <select id="away-horn">
            <option value="/audio/Sad Trombone.mp3">Sad Trombone</option>
            <option value="">Silent</option>
        </select>
        <button onclick="previewHorn('away')">▶️</button>
    </div>
    
    <h4>Player-Specific Horns</h4>
    <p class="help-text">Assign custom horns to specific players</p>
    <div id="player-horn-list">
        <!-- Dynamically generated from roster -->
    </div>
    <button onclick="addPlayerHorn()">+ Add Player Horn</button>
</div>
```

#### Game Mode Toggle (Index.cshtml)

```html
<!-- Add game mode toggle to main UI -->
<div class="game-mode-toggle">
    <label>Game Mode:</label>
    <div class="mode-buttons">
        <button onclick="setGameMode('normal')" class="mode-btn active">🏒 Regular</button>
        <button onclick="setGameMode('overtime')" class="mode-btn">⚡ Overtime</button>
        <button onclick="setGameMode('playoffs')" class="mode-btn">🏆 Playoffs</button>
    </div>
</div>
```

```javascript
async function setGameMode(mode) {
    gameMode = mode;
    document.querySelectorAll('.mode-btn').forEach(btn => btn.classList.remove('active'));
    event.target.classList.add('active');
    
    await fetch('/Home/SetGameMode', {
        method: 'POST',
        headers: {'Content-Type': 'application/json'},
        body: JSON.stringify({ mode })
    });
    
    showNotification(`Game mode: ${mode.toUpperCase()}`);
}
```

#### UI Mockup
```
Setup Page:
┌─────────────────────────────────────────────────┐
│ 🚨 Goal Horn Configuration                      │
├─────────────────────────────────────────────────┤
│ Default Horn:    [Standard Horn      ▼] [▶️]   │
│ Overtime Horn:   [Epic Overtime Horn ▼] [▶️]   │
│ Playoffs Horn:   [Same as Default    ▼] [▶️]   │
│ Away Team:       [Sad Trombone       ▼] [▶️]   │
├─────────────────────────────────────────────────┤
│ Player-Specific Horns:                          │
│ #7  McDavid  [Custom Star Horn ▼] [▶️] [🗑️]   │
│ #97 Connor   [Same as Default  ▼] [▶️] [🗑️]   │
│ [+ Add Player Horn]                             │
└─────────────────────────────────────────────────┘

Main UI:
┌──────────────────────────────────────┐
│ Game Mode: [🏒 Regular] [⚡ OT] [🏆] │
└──────────────────────────────────────┘
```

### Files to Modify
| File | Changes |
|------|---------|
| `HockeyDJ/Models/HomeController.cs` | Horn config endpoints, game mode |
| `HockeyDJ/Views/Home/Setup.cshtml` | Horn configuration UI |
| `HockeyDJ/Views/Home/Index.cshtml` | Dynamic horn selection, game mode toggle |

### Testing
- [ ] Default horn plays for unassigned players
- [ ] Player-specific horn plays for assigned players
- [ ] Overtime mode uses overtime horn
- [ ] Playoffs mode uses playoffs horn
- [ ] Away team goals use away horn sound
- [ ] Preview buttons play correct horns
- [ ] Configuration persists in session

---

## Implementation Priority Recommendation

| Priority | Feature | Complexity | Dependencies |
|----------|---------|------------|--------------|
| 1 | Hotkey Support | Low | None |
| 2 | Hat Trick Mode | Medium | None |
| 3 | Playlist Shuffle Mode | Medium | None |
| 4 | Sound Upload Through UI | Medium | None |
| 5 | Multi-Horn Support | Medium | Sound Upload (optional) |
| 6 | Scoreboard API | High | None |

**Suggested approach**: Start with Hotkeys (quick win, high impact), then Hat Trick Mode and Shuffle (user-facing improvements), then Sound Upload + Multi-Horn (they complement each other), and finally Scoreboard API (requires external testing).
