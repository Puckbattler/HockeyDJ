# v1.7.0 Release Notes

## 🔊 New Features (v1.7.0)

### 🔊 Sound Upload via UI
• Replace any built-in sound effect directly from the Settings page — no file manager needed
• Supported sound types: Goal Horn, Mushroom, Clock, Sad Trombone, Charge Organ, Go Hawks Go Cowbell, Let's Go Cowbell, Let's Go Organ, Hat Trick
• Upload MP3, WAV, or OGG files up to 10 MB each
• One-click "Reset" reverts any replaced sound back to the built-in default
• Uploaded replacements are saved to `wwwroot/audio/custom/` and persist across sessions

### 🎵 Custom Sound Buttons
• Upload additional sound files to create brand-new playable buttons on the main page
• Custom buttons appear in a dedicated section alongside the built-in sound effects
• Customize the emoji, display name, and button color for each custom sound
• Auto-detects emoji from filenames (e.g., uploading `🎺 Charge.mp3` sets the button emoji to 🎺)
• Manage (edit / delete) custom sound buttons from the Settings page

### 💾 Sound Config Persistence
• Custom sound mappings are stored in session and auto-rediscovered from the filesystem
• Restarting the server automatically picks up previously uploaded custom sounds
• No manual configuration file editing required

## ⚙️ Improvements

### 🛡️ Security & Validation
• File upload size limited to 10 MB via `[RequestSizeLimit]` attribute
• Strict file-extension allowlist: only `.mp3`, `.wav`, and `.ogg` accepted
• Filename sanitization removes invalid characters and blocks Windows reserved device names (CON, PRN, etc.)
• Custom button color classes validated against a server-side allowlist
• Emoji length capped at 4 characters; display names capped at 100 characters

### 🧹 Code Quality
• Added `IWebHostEnvironment` dependency for safe file-path resolution (no hard-coded paths)
• Extracted sound upload helpers into dedicated methods for maintainability
• Added `CustomSoundButton`, `ResetSoundRequest`, `DeleteCustomSoundRequest`, and `UpdateCustomSoundRequest` model classes

## 🧪 Testing

### ✅ Test Coverage
• All 70 existing tests continue to pass
• Test infrastructure updated to mock `IWebHostEnvironment` for the new file-upload code path

## 📋 Requirements
- .NET 10.0 SDK
- Spotify Premium account
- Modern web browser

## 🛠️ Quick Start
1. Download and extract the release
2. Add your audio files to `wwwroot/audio/` (or upload them from the Settings page!)
3. Run the executable
4. Navigate to https://127.0.0.1:7001 in your browser
5. Navigate to Settings and configure your Spotify app
6. Upload or replace sound effects from the new Sound Upload section
7. Add your playlists and team rosters
8. Start celebrating! 🎊

## 📖 Documentation
- [Full Setup Guide](README.md)
- [Troubleshooting](README.md#troubleshooting)
- [Contributing](README.md#contributing)

---
**Upload your sounds, customize your buttons, and make every game night unique! 🏒🔊🎵**

**Full Changelog**: https://github.com/Puckbattler/HockeyDJ/compare/v1.6.0...v1.7.0
