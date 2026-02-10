# 🏒 HockeyDJ

**The Ultimate Hockey-Themed Spotify Music Controller**

HockeyDJ is an ASP.NET Core MVC web application that brings the excitement of hockey to your music experience. Control multiple Spotify playlists with a hockey-themed interface, complete with goal horn celebrations, sound effects, announcement capabilities, and random track playback — perfect for hockey games, parties, or just getting pumped up!

![HockeyDJ Screenshot](https://github.com/user-attachments/assets/1b91fee4-ddf7-4eae-aac2-8668125bdc15)

## ✨ Features

### 🎵 Music Control
- **Multiple Playlist Support**: Manage up to 10 Spotify playlists simultaneously
- **Random Track Playback**: Play random songs from any playlist with one click
- **Real-time Track Display**: See what's currently playing with artist and song information
- **Spotify Web Playback SDK Integration**: Full playback control through your browser
- **Configurable Shuffle Modes**: Choose Random, Smart Shuffle (avoids recent plays), or Sequential playback per playlist

### 🚨 Goal Celebrations
- **Goal Horn Button**: Trigger epic goal celebrations with sound effects
- **Victory Songs**: Automatically plays random celebration songs after goal horn, or select a specific indexed song (e.g., player #4's requested victory song)
- **Custom Audio Support**: Add your own goal horn sound file
- **Song Selection Grid**: Radio-grid for selecting victory songs (random or numbered choices 1-30)

### 📣 Announcement System
- **Full In-App Announcement Modal**: Create Goal, Penalty, and Starting Lineup announcements
- **Home Team Announcements**: Play pre-recorded audio files for professional-sounding announcements
- **Away Team Announcements**: Fall back to browser Text-To-Speech (TTS) for dynamic announcements
- **Team Roster Support**: Configure home and away team rosters with player numbers and names
- **Goal Announcements**: Select scoring player plus up to two assists
- **Penalty Announcements**: Support for penalty type and name plus the serving player
- **Starting Lineup Announcements**: Support selecting up to 6 players

### 🎯 Priority Queue (Song Requests)
- **Search Spotify**: Search for songs directly from the UI
- **Priority Playback**: Queued songs play before any random track selection (unless a Goal Horn celebration is triggered)
- **Queue Management**: Add, remove, and clear songs with confirmation
- **Real-time Updates**: Queue persists in server-side session storage and updates the UI in real time

### 🎮 Sound Effects
- **🍄 Mushroom Button**: Power-up sound effect (Even Strength / Extra Life)
- **🕐 Clock Button**: Timeout sound effect (One Minute Remaining)
- **🎺 Sad Trombone Button**: Sad trombone sound for away team goals or disappointments
- **🎹 Charge Organ Button**: Classic arena charge organ sound
- **🛎️ Go Hawks Go Cowbell Button**: Team spirit cowbell chant
- **🔔 Let's Go Cowbell Button**: Generic team cowbell chant
- **🎹 Let's Go Organ Button**: Arena organ chant
- **Cooldown Protection**: Each button has a short cooldown to prevent double-triggering

### 🔧 Configuration Management
- **Export Configuration**: Save your settings to a JSON file for backup or sharing
- **Import Configuration**: Restore settings from a previously exported file
- **Secure Token Storage**: Spotify tokens stored in server sessions

### 🏒 Hockey-Themed UI
- **Ice Hockey Design**: Beautiful gradient backgrounds and hockey-inspired styling
- **Responsive Layout**: Works on desktop, tablet, and mobile devices
- **Animated Elements**: Glowing buttons and smooth hover effects
- **Real-time Updates**: Live track information and playback status

## 🚀 Quick Start

### Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) or later
- [Spotify Premium Account](https://www.spotify.com/premium/) (required for playback control)
- [Spotify Developer App](https://developer.spotify.com/dashboard) (free to create)

### Installation

1. **Download and extract the release** or clone the repository
   ```bash
   git clone https://github.com/Puckbattler/HockeyDJ.git
   cd HockeyDJ
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Add sound files** (optional but recommended)
   Create the `wwwroot/audio/` directory and add these audio files:
   - `goal-horn.mp3` - Your favorite goal horn sound
   - `mushroom.mp3` - Power-up sound effect
   - `clock.mp3` - Timeout/buzzer sound
   - `Sad Trombone.mp3` - Sad trombone sound
   - `ChargeOrgan.wav` - Charge arena sound
   - `GoHawksGoCowbell.wav` - Team chant cowbell
   - `LetsGoCowbell.wav` - Generic chant cowbell
   - `LetsGoOrgan.wav` - Arena organ chant

4. **Run the application**
   ```bash
   dotnet run
   ```

5. **Open your browser**
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

1. **Access Setup Page**: Click the "⚙️ Settings" button in the top-right corner
2. **Enter Spotify Credentials**:
   - Spotify Client ID
   - Spotify Client Secret
   - Redirect URI (pre-filled)

3. **Configure Goal Horn Playlist** (optional):
   - Add a Spotify playlist URL for victory songs
   - This playlist will play random celebration songs after the goal horn, or you can select a specific indexed song

4. **Configure Team Rosters** (optional):
   - Enter Home Team Name and roster (format: `Number:Name`, one per line)
   - Enter Away Team Name and roster
   - This enables the announcement feature with player selection

5. **Add Regular Playlists**:
   - Add up to 10 Spotify playlist URLs (one per line)
   - Right-click any Spotify playlist → "Share" → "Copy link"

6. **Connect to Spotify**: Click "Connect to Spotify & Start HockeyDJ"

### Configuration Backup

- **Export Configuration**: Click "📤 Export Configuration" to save your settings to a JSON file
- **Import Configuration**: Click "📥 Import Configuration" to restore settings from a backup file
- Note: Client Secret is not exported for security reasons; you'll need to re-enter it after importing

## 🎮 How to Use

### Basic Playback
- **Play Random**: Click the "▶️ Play Random" button on any playlist to play a random song
- **Pause**: Use the "⏸️ Pause" button to stop playback
- **Track Info**: See current track information in real-time

### Goal Celebrations
1. **Select Song**: Use the song selection grid to choose a specific number (1-30) or leave on "🎲" for random
2. **Score a Goal**: Click the big "📯 GOAL HORN 📯" button
3. **Celebration Sequence**:
   - Goal horn sound plays first
   - Victory song starts playing from your goal playlist (selected index or random)

### Announcements
1. Click the "📢 ANNOUNCEMENT 📢" button to open the announcement modal
2. **Select Announcement Type**: Goal, Penalty, or Starting Lineup
3. **Select Team**: Home or Away
4. **Fill in Details**: Select players and relevant information
5. **Make Announcement**: Click the announce button
   - Home team: Plays pre-recorded audio files if available
   - Away team: Uses browser Text-To-Speech

### Priority Queue (Song Requests)
1. **Search**: Type a song name in the search box and click "🔍 Search"
2. **Add to Queue**: Click "➕ Add to Queue" on any search result
3. **Queue Playback**: When you click "Play Random" on any playlist, queued songs play first
4. **Manage Queue**: Remove individual songs or clear the entire queue
5. Note: Goal Horn celebrations bypass the priority queue

### Sound Effects
- **🍄 Mushroom**: Even Strength / Extra Life sound
- **🕐 Clock**: One Minute Remaining sound
- **🎺 Sad Trombone**: For away team goals or disappointments
- **🎹 Charge Organ**: Classic arena charge chant
- **🛎️ Go Hawks Go Cowbell**: Team spirit cowbell
- **🔔 Let's Go Cowbell**: Generic team chant cowbell
- **🎹 Let's Go Organ**: Arena organ chant

## 📁 Audio File Expectations

Place custom audio files under `wwwroot/audio/`. Common files used by the application include (exact filenames matter):

**Goal Horn & Sound Effects:**
- `goal-horn.mp3` - Goal horn celebration sound
- `mushroom.mp3` - Power-up sound effect
- `clock.mp3` - Timeout buzzer sound
- `Sad Trombone.mp3` - Sad trombone sound
- `ChargeOrgan.wav` - Charge arena sound
- `GoHawksGoCowbell.wav` - Team chant cowbell
- `LetsGoCowbell.wav` - Generic chant cowbell
- `LetsGoOrgan.wav` - Arena organ chant

**Home Team Announcement Audio:**
- `LHA Goal.mp3` - Goal announcement intro
- `Scored By.mp3` - "Scored by" announcement
- `Assited By.mp3` - Assist announcement (note the filename spelling)
- `And.mp3` - Connector between assists/players
- `Starting Lineup.mp3` - Starting lineup intro
- Penalty type files: `Minor.mp3`, `Major.mp3`, `Misconduct.mp3`, `Game Misconduct.mp3`, `Match.mp3`
- Penalty name files: `Tripping.mp3`, `Hooking.mp3`, `Holding.mp3`, etc.
- Player number files: `1.mp3`, `4.mp3`, `7.mp3`, etc. (matching your roster numbers)

If an audio file is missing, the application will either skip that clip with a console warning or fall back to speech synthesis for dynamic announcements.

## 🛠️ Technical Details

### Built With
- **ASP.NET Core 10.0 MVC** - Web framework
- **Spotify Web API** - Music integration (SpotifyAPI.Web 7.2.1)
- **Spotify Web Playback SDK** - Browser-based music control
- **HTML5 Audio API** - Sound effects playback
- **Web Speech API** - Text-to-speech for away team announcements
- **Session Management** - Secure token and queue storage

### Architecture
- **MVC Pattern**: Clean separation of concerns
- **Session-based Authentication**: Secure Spotify token management
- **Client-side Playback**: Uses Spotify Web Playback SDK for seamless control
- **Responsive Design**: Mobile-friendly interface

### Reliability Features
- **Played-song Deduplication**: Avoids repeats until the playlist cycles
- **Priority Queue**: Ensures requested songs are honored regardless of which playlist triggers playback
- **Announcement Audio Sequencing**: Robust fallback to speech synthesis when audio files are missing
- **Graceful Error Handling**: Missing audio files fall back to TTS or skip with a console warning

### Security Features
- **Secure Token Storage**: Spotify tokens stored in server sessions
- **OAuth 2.0 Integration**: Standard Spotify authentication flow
- **HTTPS Support**: Secure communication
- **Input Validation**: Playlist URL validation and sanitization

## 🚨 Troubleshooting

### Common Issues

**"Not authenticated" errors**
- Ensure you have Spotify Premium
- Check that your Client ID and Secret are correct
- Verify your redirect URI matches exactly (use `https://127.0.0.1:7001/Home/SpotifyCallback`)

**Audio files not playing**
- Confirm MP3/WAV files are in `wwwroot/audio/`
- Check browser console for loading errors
- Ensure files are properly named (filenames are case-sensitive)
- For announcement audio, verify filenames match expected naming conventions

**Playlist loading issues**
- Verify playlist URLs are public or you have access
- Check that playlist IDs are extracted correctly
- Ensure Spotify app permissions include playlist access

**Playback not working**
- Make sure Spotify app isn't already playing music
- Check that the HockeyDJ device appears in your Spotify connect devices
- Refresh the page if the Spotify Web SDK doesn't initialize

**Announcements not working**
- For home team: Ensure audio files exist with correct names in `wwwroot/audio/`
- For away team: Ensure your browser supports Web Speech API (Chrome recommended)
- Verify roster format is `Number:Name` with colon separator

**Priority Queue issues**
- Queued songs only play when clicking "Play Random" on a playlist
- Goal Horn celebrations bypass the priority queue
- Clear browser cache if queue display doesn't update

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

This project is licensed under the MIT License - see the [hockeydj_license.md](HockeyDJ/hockeydj_license.md) file for details.

## 🙏 Acknowledgments

- **Spotify** for their excellent Web API and Web Playback SDK
- **Hockey Community** for the inspiration and energy
- **Open Source Contributors** who made the libraries we depend on
- **Sound Effect Creators** for the audio samples

## 📞 Support

- **Issues**: [GitHub Issues](https://github.com/Puckbattler/HockeyDJ/issues)
- **Discussions**: [GitHub Discussions](https://github.com/Puckbattler/HockeyDJ/discussions)

---

**Ready to get your hockey game on? Let's drop the puck and crank the music! 🏒🎵**

[![Made with ❤️ for Hockey](https://img.shields.io/badge/Made%20with%20❤️%20for-Hockey-red)](https://github.com/Puckbattler/HockeyDJ)
[![.NET 10.0](https://img.shields.io/badge/.NET-10.0-purple)](https://dotnet.microsoft.com/download/dotnet/10.0)
[![Spotify API](https://img.shields.io/badge/Spotify-API-green)](https://developer.spotify.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
