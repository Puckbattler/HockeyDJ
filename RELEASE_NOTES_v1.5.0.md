# v1.5.0 Release Notes

## 🎵 New Features (v1.5.0)

### 🔀 Configurable Playlist Shuffle Modes
• Three shuffle modes for ultimate playlist control:
  - **🎲 Random**: Picks tracks randomly from the playlist
  - **🧠 Smart Shuffle**: Intelligently avoids recently played tracks (keeps last 20% excluded after cycling through playlist)
  - **📋 Sequential**: Plays tracks in playlist order, automatically wrapping to start
• Per-playlist shuffle mode selection with dropdown UI
• Smart shuffle maintains separate history for each playlist
• Shuffle settings persist in session and included in configuration export/import
• Goal horn playlist also benefits from smart shuffle to avoid repeats

### 🗑️ Legacy Code Removal
• Removed global `playedSongs` Set deduplication (replaced by configurable shuffle modes)
• Cleaner, more maintainable codebase with mode-specific logic

## ⚙️ Improvements

### 📥 Enhanced Configuration Management
• Configuration export now includes `playlistShuffleModes` settings
• Configuration import properly restores shuffle mode preferences
• Version bumped to 1.2.0 for configuration format

### 🎨 UI/UX Enhancements
• Shuffle mode dropdown added to each playlist card
• Styled select elements match application theme
• Real-time mode switching without page refresh
• Visual feedback for current shuffle mode per playlist

### 📚 Documentation Updates
• README.md updated to reflect "Configurable Shuffle Modes" feature
• Removed reference to old "Played-Song Deduplication" feature

## 🧪 Testing

### ✅ Comprehensive Test Coverage
• Added 8 new unit tests for shuffle mode functionality:
  - Validation of all three shuffle modes (random, smart, sequential)
  - Invalid mode handling
  - Multi-playlist mode management
  - Configuration export/import with shuffle settings
• All 57 tests passing (49 existing + 8 new)

## 📋 Requirements
- .NET 10.0 SDK
- Spotify Premium account
- Modern web browser

## 🛠️ Quick Start
1. Download and extract the release
2. Run the executable
3. Navigate to Settings and configure your Spotify app
4. Add your playlists
5. Select shuffle mode for each playlist
6. Start celebrating! 🎊

## 📖 Documentation
- [Full Setup Guide](README.md)
- [Troubleshooting](README.md#troubleshooting)
- [Contributing](README.md#contributing)

---
**Let's drop the puck and crank the music with smarter shuffle! 🏒🎵**

**Full Changelog**: https://github.com/Puckbattler/HockeyDJ/compare/v1.4.0...v1.5.0
