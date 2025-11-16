# HockeyDJ Announcement Feature

## Overview
The HockeyDJ app now includes a comprehensive announcement system for goals, penalties, and starting lineups for both home and away teams.

## Setup

### Team Roster Configuration
During the setup phase, you can now configure:
1. **Home Team Name** - The name of your home team
2. **Home Team Roster** - Player numbers and names (format: `Number:Name`, one per line)
3. **Away Team Name** - The name of the away team
4. **Away Team Roster** - Player numbers and names (format: `Number:Name`, one per line)

**Example Roster Format:**
```
1:John Smith
4:Mike Johnson
7:Chris Williams
8:Dave Brown
9:Tom Davis
```

### Audio Files Required
For home team announcements, place these audio files in `wwwroot/audio/`:

**Goal Announcements:**
- `LHA Goal.mp3` - Goal announcement intro
- `Scored By.mp3` - "Scored by" announcement
- `Assited By.mp3` - Assist announcement
- `And.mp3` - Connector between assists
- Player number files (e.g., `1.mp3`, `4.mp3`, `7.mp3`, etc.)

**Penalty Announcements:**
- Penalty type files: `Minor.mp3`, `Major.mp3`, `Misconduct.mp3`, `Game Misconduct.mp3`, `Match.mp3`
- Penalty name files: `Tripping.mp3`, `Hooking.mp3`, `Holding.mp3`, etc.
- Player number files (same as above)

**Starting Lineup Announcements:**
- `Starting Lineup.mp3` - Starting lineup intro
- `And.mp3` - Connector between players
- Player number files (same as above)

## Usage

### Opening the Announcement Modal
Click the **📢 ANNOUNCEMENT** button next to the Goal Horn button on the main page.

### Making Announcements

#### Goal Announcement

1. **Select "Goal"** from the announcement type options
2. **Choose Team** - Home or Away
3. **Select Players:**
   - Scoring Player (required)
   - First Assist (optional)
   - Second Assist (optional)
4. Click **"📣 Announce Goal"**

**Away Team:** Uses Web Speech API with the script:
```
"[Away Team Name] Goal! Scored by [Player Name]. Assisted by [Assist 1] and [Assist 2]!"
```

**Home Team:** Plays audio files in sequence:
```
LHA Goal.mp3 → Scored By.mp3 → [Scorer Number].mp3 → Assited By.mp3 → [Assist 1 Number].mp3 → And.mp3 → [Assist 2 Number].mp3
```

#### Penalty Announcement

1. **Select "Penalty"** from the announcement type options
2. **Choose Team** - Home or Away
3. **Select Details:**
   - Penalty Type (Minor, Major, Misconduct, Game Misconduct, Match)
   - Penalty Name (Tripping, Hooking, Holding, etc.)
   - Serving Player
4. Click **"📣 Announce Penalty"**

**Away Team:** Uses Web Speech API with the script:
```
"[Type] penalty for [Penalty Name] on [Player Name], number [Number]"
```

**Home Team:** Plays audio files in sequence:
```
[Type].mp3 → [Penalty Name].mp3 → [Player Number].mp3
```

#### Starting Lineup Announcement

1. **Select "Starting Lineup"** from the announcement type options
2. **Choose Team** - Home or Away
3. **Select Players** - Check up to 6 players for the starting lineup
4. Click **"📣 Announce Starting Lineup"**

**Away Team:** Uses Web Speech API with the script:
```
"Starting lineup for [Team Name]: number [#], [Name] and number [#], [Name]..."
```

**Home Team:** Plays audio files in sequence:
```
Starting Lineup.mp3 → [Player 1 Number].mp3 → And.mp3 → [Player 2 Number].mp3 → And.mp3 → ...
```

## Penalties List

The following penalties are available:
- Tripping
- Hooking
- Holding
- Interference
- Slashing
- Cross-checking
- Boarding
- Charging
- Elbowing
- High-sticking
- Roughing
- Delay of Game
- Too Many Players on Ice
- Unsportsmanlike Conduct
- Face-off Interference
- Instigator of Fighting
- Fighting
- Spearing
- Butt-ending
- Kicking
- Head Contact
- Checking from Behind
- Intentional Injury

## Penalty Types

- **Minor** - 2 minute penalty
- **Major** - 5 minute penalty
- **Misconduct** - 10 minute penalty
- **Game Misconduct** - Player ejected
- **Match** - Player ejected with review

## Technical Details

### Web Speech API
Away team announcements use the browser's built-in Web Speech Synthesis API for text-to-speech playback. This requires no additional audio files but may vary in voice quality across different browsers.

### Audio File Playback
Home team announcements use pre-recorded audio files for professional, consistent quality. Files are played sequentially with a 200ms pause between each file.

### Session Storage
Team roster data is stored in the server session along with other HockeyDJ configuration. This data can be exported and imported using the configuration tools on the Setup page.

## Browser Compatibility

- **Web Speech API**: Supported in most modern browsers (Chrome, Edge, Safari, Firefox)
- **Audio Playback**: Supported in all modern browsers
- **Recommended**: Chrome or Edge for best Web Speech API performance

## Tips

1. **Test Audio Files**: Ensure all required audio files are present before the game
2. **Volume Levels**: All audio files should be normalized to similar volume levels
3. **Web Speech Rate**: The speech rate is set to 0.9 (slightly slower than normal) for clarity
4. **Player Numbers**: Make sure player numbers in rosters match your available audio files
5. **Configuration Export**: Always export your configuration after setting up rosters to save time

## Troubleshooting

**Audio files not playing:**
- Check that files exist in `wwwroot/audio/`
- Verify file names match exactly (case-sensitive)
- Check browser console for specific error messages

**Web Speech not working:**
- Ensure your browser supports Web Speech API
- Check browser permissions for speech synthesis
- Try a different browser (Chrome recommended)

**Players not appearing:**
- Verify roster format: `Number:Name` with colon separator
- Check that roster data was saved in Setup
- Confirm you selected the correct team (Home vs Away)
