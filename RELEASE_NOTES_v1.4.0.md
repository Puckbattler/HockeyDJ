# v1.4.0 Release Notes

## 📣 New Features (v1.4.0)

### 🎵 Song Start Timestamps for Goal Celebrations
• Configure per-player start timestamps for goal celebration songs
• Set custom start positions using format `PlayerNumber:Minutes:Seconds`
• Timestamps stored as JSON with milliseconds precision
• Perfect for starting songs at the best part of the track
• Validation ensures non-negative values and seconds < 60

### 🏒 Dynamic Player Number Radio Buttons
• Radio buttons now dynamically generated from home team roster
• Player numbers displayed match actual jersey numbers instead of sequential indices
• Improved goal song selection experience

## 🔧 Bug Fixes

### 🎯 Radio Button Index Fix
• Fixed goal song radio button index to match player jersey number instead of sequential index
• Now selecting a player correctly plays their configured goal song

### ▶️ Playback Control Fix
• All tracks now guaranteed to start from beginning (`position_ms: 0`)
• Prevents tracks from resuming from previous position

## ⚙️ Improvements

### 📥 Enhanced Import/Export
• Added Import/Export capability for ClientSecrets in configuration
• Song start timestamps included in configuration export/import

### 📚 Documentation Updates
• Updated documentation to reflect .NET 10.0 SDK requirement
• Improved structure and removed outdated announcements
• Better organized Quick Start guide

### 🚀 Platform Update
• Upgraded to .NET 10.0 SDK

## 📋 Requirements
- .NET 10.0 SDK (upgraded from 8.0)
- Spotify Premium account
- Modern web browser

## 🛠️ Quick Start
1. Download and extract the release
2. Run the executable
3. Navigate to Settings and configure your Spotify app
4. Add your playlists and start celebrating! 🎊

## 📖 Documentation
- [Full Setup Guide](README.md)
- [Troubleshooting](README.md#troubleshooting)
- [Contributing](README.md#contributing)

---
**Let's drop the puck and crank the music! 🏒🎵**

**Full Changelog**: https://github.com/Puckbattler/HockeyDJ/compare/v1.3.0...v1.4.0
