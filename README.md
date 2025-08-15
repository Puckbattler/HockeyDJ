# 🏒 HockeyDJ

**The Ultimate Hockey-Themed Spotify Music Controller**

HockeyDJ is an ASP.NET Core MVC web application that brings the excitement of hockey to your music experience. Control multiple Spotify playlists with a hockey-themed interface, complete with goal horn celebrations, sound effects, and random track playback perfect for hockey games, parties, or just getting pumped up!

## ✨ Features

### 🎵 Music Control
- **Multiple Playlist Support**: Manage up to 10 Spotify playlists simultaneously
- **Random Track Playback**: Play random songs from any playlist with one click
- **Real-time Track Display**: See what's currently playing with artist and song information
- **Spotify Web Playback SDK Integration**: Full playback control through your browser
- **You can add a playlist for each situation/stoppage so when a penalty occurs all you have to do it hit the play button for your penalty list and it would play a random songs that fits the situation, i.e. _Breaking The Law_. 

### 🚨 Goal Celebrations
- **Goal Horn Button**: Trigger epic goal celebrations with sound effects
- **Victory Songs**: Automatically plays random celebration songs after goal horn
- **Custom Audio Support**: Add your own goal horn, power-up, and timeout sounds

### 🎮 Sound Effects
- **🍄 Mushroom Button**: Power-up sound effect (inspired by classic video games) for when a penalty expires and the game goes back to even strength
- **🕐 Clock Button**: _One Minute Remaining_ Recording for the end of the periods.  
- **Audio File Support**: MP3 files for custom sound effects

### 🏒 Hockey-Themed UI
- **Ice Hockey Design**: Beautiful gradient backgrounds and hockey-inspired styling
- **Responsive Layout**: Works on desktop, tablet, and mobile devices
- **Animated Elements**: Glowing buttons and smooth hover effects
- **Real-time Updates**: Live track information and playback status

## 🚀 Quick Start

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Spotify Premium Account](https://www.spotify.com/premium/) (required for playback control)
- [Spotify Developer App](https://developer.spotify.com/dashboard) (free to create)

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/yourusername/HockeyDJ.git
   cd HockeyDJ
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Run the application**
   ```bash
   dotnet run
   ```

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

1. **Access Setup Page**: Click the "⚙️ Settings" button in the top-right corner
2. **Enter Spotify Credentials**:
   - Spotify Client ID
   - Spotify Client Secret
   - Redirect URI (pre-filled)

3. **Configure Goal Horn Playlist** (optional):
   - Add a Spotify playlist URL for victory songs
   - This playlist will play random celebration songs after the goal horn

4. **Add Regular Playlists**:
   - Add up to 10 Spotify playlist URLs (one per line)
   - Right-click any Spotify playlist → "Share" → "Copy link"

5. **Connect to Spotify**: Click "Connect to Spotify & Start HockeyDJ"

## 🎮 How to Use

### Basic Playback
- **Play Random**: Click the "▶️ Play Random" button on any playlist to play a random song
- **Pause**: Use the "⏸️ Pause" button to stop playback
- **Track Info**: See current track information in real-time

### Goal Celebrations
1. **Score a Goal**: Click the big "📯 GOAL HORN 📯" button
2. **Celebration Sequence**:
   - Goal horn sound plays first (3 seconds)
   - Random victory song starts playing from your goal playlist
   - Celebrate! 🎉

### Sound Effects
- **🍄 Mushroom**: Power-up sound for momentum changes
- **🕐 Clock**: Timeout sound for breaks in action

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

### Security Features
- **Secure Token Storage**: Spotify tokens stored in server sessions
- **OAuth 2.0 Integration**: Standard Spotify authentication flow
- **HTTPS Support**: Secure communication
- **Input Validation**: Playlist URL validation and sanitization

## 📁 Project Structure

```
HockeyDJ/
├── Controllers/
│   └── HomeController.cs          # Main application logic
├── Models/
│   └── ErrorViewModel.cs          # Error handling model
├── Views/
│   ├── Home/
│   │   ├── Index.cshtml          # Main music controller interface
│   │   ├── Setup.cshtml          # Configuration page
│   │   └── Privacy.cshtml        # Privacy policy
│   └── Shared/
├── wwwroot/
│   ├── css/
│   │   └── site.css              # Custom styles
│   ├── js/
│   │   └── site.js               # Custom JavaScript
│   └── audio/                    # Sound effects directory
│       ├── goal-horn.mp3         # Goal celebration sound
│       ├── mushroom.mp3          # Power-up sound
│       └── clock.mp3             # Timeout sound
├── Program.cs                     # Application startup
├── appsettings.json              # Configuration
└── HockeyDJ.csproj               # Project file
```

## 🎵 Playlist Recommendations

### Goal Celebration Playlists
- Victory songs and pump-up tracks
- Classic rock anthems
- Sports arena favorites
- Team goal songs

### Regular Playlists Ideas
- **Period 1**: High-energy warm-up tracks
- **Period 2**: Intensity maintenance songs  
- **Period 3**: Championship-level motivation
- **Power Play**: Extra energetic tracks
- **Intermission**: Crowd favorites and classics

## 🔧 Customization

### Adding Your Own Sounds
1. Add MP3 files to `wwwroot/audio/`
2. Update the JavaScript in `Index.cshtml` to reference new files
3. Customize button actions and audio playback

### Styling Modifications
- Edit `wwwroot/css/site.css` for global styles
- Modify inline styles in `Index.cshtml` for page-specific changes
- Adjust color schemes, animations, and layout

### Feature Extensions
- Add more sound effect buttons
- Implement playlist scheduling
- Add scoreboard integration
- Create team-specific themes

## 🚨 Troubleshooting

### Common Issues

**"Not authenticated" errors**
- Ensure you have Spotify Premium
- Check that your Client ID and Secret are correct
- Verify your redirect URI matches exactly

**Audio files not playing**
- Confirm MP3 files are in `wwwroot/audio/`
- Check browser console for loading errors
- Ensure files are properly named

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

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- **Spotify** for their excellent Web API and Web Playback SDK
- **Hockey Community** for the inspiration and energy
- **Open Source Contributors** who made the libraries we depend on
- **Sound Effect Creators** for the audio samples

## 📞 Support

- **Issues**: [GitHub Issues](https://github.com/yourusername/HockeyDJ/issues)
- **Discussions**: [GitHub Discussions](https://github.com/yourusername/HockeyDJ/discussions)
- **Email**: your-email@example.com

---

**Ready to get your hockey game on? Let's drop the puck and crank the music! 🏒🎵**

[![Made with ❤️ for Hockey](https://img.shields.io/badge/Made%20with%20❤️%20for-Hockey-red)](https://github.com/yourusername/HockeyDJ)
[![.NET 8.0](https://img.shields.io/badge/.NET-8.0-purple)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![Spotify API](https://img.shields.io/badge/Spotify-API-green)](https://developer.spotify.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
