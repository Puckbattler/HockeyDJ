# v1.6.0 Release Notes

## 🎩 New Features (v1.6.0)

### 🎩 Hat Trick Celebrations
• Automatic hat trick detection when any player scores their 3rd goal
• Full celebration sequence: goal horn → hat-trick sound → goal horn encore
• Animated "🎩🎩🎩 HAT TRICK!" banner with player name
• Confetti particle effects for a spectacular celebration
• Configurable hat trick song: set a specific Spotify track for hat trick celebrations
• Falls back to a random goal horn playlist song if no hat trick song is configured

### 🏒 Goal Tracking
• Per-player goal count tracking throughout the session
• Visual goal count badges displayed on player selection buttons
• "🔄 RESET GOALS" button to clear all counts for a new game
• Goal counts persist in server-side session storage

### ⚙️ Hat Trick Song Configuration (Setup Page)
• New "Hat Trick Celebration" section on the Settings page
• Save a Spotify track URL or URI as the hat trick song
• Preview button opens the track in Spotify for quick listening
• Clear button removes the configured hat trick song
• Accepts both `open.spotify.com/track/...` URLs and `spotify:track:...` URIs
• Falls back to raw URL if format is unrecognized

## ⚙️ Improvements

### 📥 Enhanced Configuration Management
• Configuration export now includes `hatTrickSongUri` setting
• Configuration import properly restores hat trick song preference
• Hat trick song URI loads into Setup page on import

### 🔧 Build Script Enhancement
• Audio files are now automatically removed from publish output
• Users supply their own audio files, keeping release packages smaller and cleaner

### 🧹 Code Quality
• Extracted magic numbers to named constants for maintainability

## 🧪 Testing

### ✅ Comprehensive Test Coverage
• Added 13 new unit tests for hat trick functionality:
  - Goal recording and counting (first, second, third goal detection)
  - Hat trick detection on 3rd goal with configured song URI
  - Multiple player goal tracking independence
  - SaveHatTrickSong with Spotify URL, URI, and unrecognized formats
  - SaveHatTrickSong with empty string returns error
  - ClearHatTrickSong removes song from session
  - GetHatTrickSong when configured and when not configured
  - ResetGoalCounts clears all player goals
• All 70 tests passing (57 existing + 13 new)

## 📋 Requirements
- .NET 10.0 SDK
- Spotify Premium account
- Modern web browser

## 🛠️ Quick Start
1. Download and extract the release
2. Add your audio files to `wwwroot/audio/` (including optional `hat-trick.mp3`)
3. Run the executable
4. Navigate to https://127.0.0.1:7001 in your browser
5. Navigate to Settings and configure your Spotify app
6. Optionally configure a hat trick celebration song
7. Add your playlists and team rosters
8. Start celebrating! 🎊

## 📖 Documentation
- [Full Setup Guide](README.md)
- [Troubleshooting](README.md#troubleshooting)
- [Contributing](README.md#contributing)

---
**Let's drop the puck and celebrate every hat trick! 🏒🎩🎵**

**Full Changelog**: https://github.com/Puckbattler/HockeyDJ/compare/v1.5.0...v1.6.0
