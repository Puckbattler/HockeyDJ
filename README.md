# 🏒 HockeyDJ

**The Ultimate Hockey-Themed Spotify Music Controller**
### Priority Queue Is Here! 
- **Scroll Down for more details**
HockeyDJ is an ASP.NET Core MVC web application that brings the excitement of hockey to your music experience. Control multiple Spotify playlists with a hockey-themed interface, complete with goal horn celebrations, sound effects, announcement capabilities, and random track playback — perfect for hockey games, parties, or just getting pumped up!

![HockeyDJ Screenshot](https://github.com/user-attachments/assets/1b91fee4-ddf7-4eae-aac2-8668125bdc15)

## ✨ New & Notable Features (Updated)

### 📣 Announcement System
- Full in-app announcement modal to create Goal, Penalty, and Starting Lineup announcements.
- Home-team announcements can play pre-recorded audio files (e.g., `LHA Goal.mp3`, `Scored By.mp3`, `Assited By.mp3`, `And.mp3`, `Starting Lineup.mp3`, and per-player voice clips like `4.mp3`).
- Away-team announcements fall back to browser Text-To-Speech (TTS) for dynamic announcements when pre-recorded audio isn't available.
- Roster parsing supports synchronized "number:name" lists (newline separated) for `HomeTeamRoster` and `AwayTeamRoster` in setup.
- Select scoring player plus up to two assists for goal announcements, with automatic sequencing of audio files or TTS fallback.
- Penalty announcements support penalty type and name plus the serving player.
- Starting lineup announcements support selecting up to 6 players and will play the correct sequence of audio files or TTS.

### 🎯 Priority Queue (Song Requests)
- Search Spotify from the UI and add requested songs to a Priority Queue.
- Priority queue songs play before any random track selection across all players, unless a Goal Horn celebration is triggered.
- Queue operations supported: add, remove, and clear (with confirmation).
- Queue persists in server-side session storage and updates the UI in real time.

### 🎵 Playback & Player Enhancements
- Multiple playlists shown in a responsive grid; each player can trigger a random track from its playlist.
- Played-song deduplication prevents immediate repeats using a `playedSongs` set; when all tracks have played, the list resets automatically.
- Goal Horn celebration plays a local goal horn audio first, then transfers playback to a configurable goal playlist and plays either the selected indexed song or a random victory song.
- Uses Spotify Web Playback SDK to create a controllable in-browser player device for reliable remote playback control.

### 🔊 Additional Sound Effects
- Several themed sound buttons were added: Mushroom (power-up), Clock (timeout buzzer), Sad Trombone, Charge Organ, Go Hawks Go Cowbell, Let's Go Cowbell, Let's Go Organ, and more.
- Sound effects play via HTML5 Audio from `wwwroot/audio/` and each button has a short cooldown to prevent double-triggering.

### 🧩 UI & Usability Improvements
- Beautiful responsive hockey-themed styling, with animated buttons and modal dialogs for announcements.
- Radio-grid for selecting victory songs (random or numbered choices for player-requested goal songs).
- Setup link is available top-right to configure Spotify credentials, playlists, goal playlist, and rosters.
- Visual feedback for search results, queue contents, and playing track information.

### 🛠️ Reliability & Error Handling
- Improved client-side error messages with auto-hide and non-blocking notifications.
- Graceful audio fallback: missing pre-recorded audio falls back to TTS or skips with a console warning.
- Playback and API errors surface to the UI with clear messages.

### 🔧 Configuration Management
- Export your configuration to a JSON file for backup or sharing between devices.
- Import previously exported configurations to quickly restore settings.
- Client Secret is excluded from exports for security.

## 🚀 Quick Start

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Spotify Premium Account](https://www.spotify.com/premium/) (required for playback control)
- [Spotify Developer App](https://developer.spotify.com/dashboard) (free to create)

### Installation
1. **Download and extract the release**

2. **Run the executable**
3. **Or Run the Powershell Script**

4. **Open your browser**
   Navigate to `https://127.0.0.1:7001` (or the URL shown in your terminal)

## ⚙️ Setup & Configuration

### Spotify Developer Setup

1. Go to the [Spotify Developer Dashboard](https://developer.spotify.com/dashboard)
2. Create a new app or use an existing one
3. Add your redirect URI to the app settings:
   ```
   https://127.0.0.1:7001/Home/SpotifyCallback
   ```
4. Note your **Client ID** and **Client Secret**

### Application Setup

1. **Access Setup Page**: Click the "⚙️ Settings" button in the top-right corner of the main UI
2. **Enter Spotify Credentials**:
   - Spotify Client ID
   - Spotify Client Secret
   - Redirect URI (pre-filled)
3. **Configure Goal Horn Playlist** (optional):
   - Add a Spotify playlist URL for victory songs
   - This playlist will play random celebration songs after the goal horn or you can select a specific indexed song (e.g., player #4's requested victory song).
4. **Add Regular Playlists**:
   - Add up to 10 Spotify playlist URLs (one per line)
5. **Configure Team Rosters** (optional):
   - Paste roster text as `number:name` lines for Home and Away teams so announcement selections populate correctly.
6. **Add Optional Audio Files**:
   - Place custom audio files in `wwwroot/audio/` for better home-team announcements and sound effects.

## 🎮 How to Use

### Basic Playback
- **Play Random**: Click the "▶️ Play Random" button on any playlist to play a random song
- **Pause**: Use the "⏸️ Pause" button to stop playback
- **Track Info**: See current track information in real-time

### Goal Celebrations
1. **Score a Goal**: Click the big "📯 GOAL HORN 📯" button
2. **Celebration Sequence**:
   - Goal horn sound plays first (played from `wwwroot/audio/goal-horn.mp3` by default)
   - Victory song starts playing from your configured goal playlist (selected index or random)

### Announcement Workflows
- Open the Announcement modal to create Goal, Penalty, or Starting Lineup announcements.
- For Home team: if audio files exist in `wwwroot/audio/` matching the required filenames and numbers, those audio clips will be sequenced for a professional-sounding announcement.
- For Away team or missing audio files: browser speech synthesis (TTS) will be used to speak the announcement.

### Priority Queue (Song Requests)
- Use the search box in the Priority Queue section to find songs on Spotify
- Click "➕ Add to Queue" to push a song to the front of playback order
- Queued songs will play next regardless of which playlist's "Play Random" was pressed (unless a goal celebration is in progress)
- Remove songs or clear the queue from the UI

## 📁 Audio File Expectations
Place custom audio files under `wwwroot/audio/`. Common files used by Announcement features include (exact filenames matter):
- `goal-horn.mp3` (UI goal horn sound)
- `mushroom.mp3`, `clock.mp3` (sound effect buttons)
- `Sad Trombone.mp3`, `ChargeOrgan.wav`, `GoHawksGoCowbell.wav`, `LetsGoCowbell.wav`, `LetsGoOrgan.wav`
- Home announcement sequencing files: `LHA Goal.mp3`, `Scored By.mp3`, `Assited By.mp3`, `And.mp3`, `Starting Lineup.mp3`, and numeric voice clips like `4.mp3`, `12.mp3` corresponding to player numbers.

If an audio file is missing, the application will either skip that clip with a console warning or fall back to speech synthesis for dynamic announcements.

## 🛠️ Technical Details

### Built With
- **ASP.NET Core 8.0 MVC** - Web framework
- **Spotify Web API** - Music integration (SpotifyAPI.Web 7.2.1)
- **Spotify Web Playback SDK** - Browser-based music control
- **HTML5 Audio API** - Sound effects playback
- **Session Management** - Secure token storage

### Architecture
- **MVC Pattern**: Clean separation of concerns
- **Session-based Authentication**: Secure Spotify token management
- **Client-side Playback**: Uses Spotify Web Playback SDK for seamless control
- **Responsive Design**: Mobile-friendly interface

### Reliability Features
- Played-song deduplication to avoid repeats until the playlist cycles
- Priority queue ensures requested songs are honored regardless of which playlist triggers playback
- Announcement audio sequencing with robust fallback to speech synthesis

## 🚨 Troubleshooting

### Common Issues

**"Not authenticated" errors**
- Ensure you have Spotify Premium
- Check that your Client ID and Secret are correct
- Verify your redirect URI matches exactly

**Audio files not playing**
- Confirm MP3/WAV files are in `wwwroot/audio/`
- Check browser console for loading errors
- Ensure filenames match the expected names in the Announcement feature

**Playlist loading issues**
- Verify playlist URLs are public or you have access
- Check that playlist IDs are extracted correctly
- Ensure Spotify app permissions include playlist access

**Playback not working**
- Make sure Spotify app isn't already playing music
- Check that the HockeyDJ device appears in your Spotify connect devices
- Refresh the page if the Spotify Web SDK doesn't initialize

## 🤝 Contributing

We welcome contributions! Here are some ways you can help:

1. **Report Bugs**: Open an issue with details about the problem
2. **Suggest Features**: Share ideas for new hockey-themed features
3. **Submit Pull Requests**: Fix bugs or add new functionality
4. **Improve Documentation**: Help make the setup process clearer
5. **Share Sound Packs**: Create themed audio file collections

### Development Setup
1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Make your changes
4. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
5. Push to the branch (`git push origin feature/AmazingFeature`)
6. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the [hockeydj_license.md](hockeydj_license.md) file for details.

---

**Ready to get your hockey game on? Let's drop the puck and crank the music! 🏒🎵**

[![Made with ❤️ for Hockey](https://img.shields.io/badge/Made%20with%20❤️%20for-Hockey-red)](https://github.com/Puckbattler/HockeyDJ)
[![.NET 8.0](https://img.shields.io/badge/.NET-8.0-purple)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![Spotify API](https://img.shields.io/badge/Spotify-API-green)](https://developer.spotify.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
