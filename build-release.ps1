# build-release.ps1
param(
    [string]$Version = "1.1.1",
    [string]$ProjectPath = "HockeyDJ.csproj"
)

$ErrorActionPreference = "Stop"

Write-Host "🏒 Building HockeyDJ v$Version releases..." -ForegroundColor Green

# Define target platforms
$platforms = @(
    @{RID="win-x64"; Archive="zip"},
    @{RID="win-x86"; Archive="zip"},
    @{RID="win-arm64"; Archive="zip"},
    @{RID="linux-x64"; Archive="tar.gz"},
    @{RID="linux-arm"; Archive="tar.gz"},
    @{RID="linux-arm64"; Archive="tar.gz"},
    @{RID="osx-x64"; Archive="tar.gz"},
    @{RID="osx-arm64"; Archive="tar.gz"}
)

# Clean previous builds
Write-Host "🧹 Cleaning previous builds..." -ForegroundColor Yellow
Remove-Item -Path "./publish" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path "./releases/v$Version" -Recurse -Force -ErrorAction SilentlyContinue

# Create directories
New-Item -ItemType Directory -Path "./releases/v$Version" -Force | Out-Null

foreach ($platform in $platforms) {
    $rid = $platform.RID
    $archiveType = $platform.Archive
    
    Write-Host "🔨 Building for $rid..." -ForegroundColor Cyan
    
    # Publish
    dotnet publish $ProjectPath `
        -c Release `
        -r $rid `
        --self-contained true `
        -p:PublishSingleFile=true `
        -p:PublishTrimmed=false `
        -p:IncludeNativeLibrariesForSelfExtract=true `
        -p:Version=$Version `
        --output "./publish/$rid"
    
    if ($LASTEXITCODE -ne 0) {
        Write-Error "❌ Build failed for $rid"
        exit 1
    }
    
    # Copy additional files
    Copy-Item -Path "README.md" -Destination "./publish/$rid/" -Force -ErrorAction SilentlyContinue
    Copy-Item -Path "hockeydj_license.md" -Destination "./publish/$rid/" -Force -ErrorAction SilentlyContinue
    
    # Copy Properties folder with launchSettings.json
    if (Test-Path "Properties") {
        Copy-Item -Path "Properties" -Destination "./publish/$rid/" -Recurse -Force
    }
    
    # Create appsettings.Production.json with launch settings configuration
    $productionSettings = @{
        "Logging" = @{
            "LogLevel" = @{
                "Default" = "Information"
                "Microsoft.AspNetCore" = "Warning"
            }
        }
        "AllowedHosts" = "*"
        "Urls" = "https://127.0.0.1:7001;http://127.0.0.1:5000"
    } | ConvertTo-Json -Depth 3
    
    $productionSettings | Out-File -FilePath "./publish/$rid/appsettings.Production.json" -Encoding UTF8
    
    # Create startup script for each platform
    if ($rid -like "win-*") {
        # Create Windows batch file
        $batchContent = @"
@echo off
echo 🏒 Starting HockeyDJ...
echo.
echo Open your browser and navigate to:
echo   https://127.0.0.1:7001
echo   or
echo   http://127.0.0.1:5000
echo.
echo Press Ctrl+C to stop the server
echo.
set ASPNETCORE_ENVIRONMENT=Production
set ASPNETCORE_URLS=https://127.0.0.1:7001;http://127.0.0.1:5000
HockeyDJ.exe
pause
"@
        $batchContent | Out-File -FilePath "./publish/$rid/start-hockeydj.bat" -Encoding ASCII
        
        # Create PowerShell script
        $psContent = @"
Write-Host "🏒 Starting HockeyDJ..." -ForegroundColor Green
Write-Host ""
Write-Host "Open your browser and navigate to:" -ForegroundColor Yellow
Write-Host "  https://127.0.0.1:7001" -ForegroundColor Cyan
Write-Host "  or" -ForegroundColor Yellow  
Write-Host "  http://127.0.0.1:5000" -ForegroundColor Cyan
Write-Host ""
Write-Host "Press Ctrl+C to stop the server" -ForegroundColor Yellow
Write-Host ""

`$env:ASPNETCORE_ENVIRONMENT = "Production"
`$env:ASPNETCORE_URLS = "https://127.0.0.1:7001;http://127.0.0.1:5000"

try {
    & "./HockeyDJ.exe"
} catch {
    Write-Host "Error starting HockeyDJ: `$_" -ForegroundColor Red
    Read-Host "Press Enter to exit"
}
"@
        $psContent | Out-File -FilePath "./publish/$rid/start-hockeydj.ps1" -Encoding UTF8
    } else {
        # Create Unix shell script
        $shellContent = @'
#!/bin/bash

echo "🏒 Starting HockeyDJ..."
echo ""
echo "Open your browser and navigate to:"
echo "  https://127.0.0.1:7001"
echo "  or"
echo "  http://127.0.0.1:5000"
echo ""
echo "Press Ctrl+C to stop the server"
echo ""

export ASPNETCORE_ENVIRONMENT=Production
export ASPNETCORE_URLS="https://127.0.0.1:7001;http://127.0.0.1:5000"

./HockeyDJ
'@
        $shellContent | Out-File -FilePath "./publish/$rid/start-hockeydj.sh" -Encoding UTF8
        
        # Make shell script executable (this will work if run on Unix systems)
        if ($IsLinux -or $IsMacOS) {
            chmod +x "./publish/$rid/start-hockeydj.sh"
        }
    }
    
    # Create audio directory and copy audio files if they exist
    New-Item -ItemType Directory -Path "./publish/$rid/wwwroot/audio" -Force | Out-Null
    
    # Copy audio files if they exist in the source
    $audioFiles = @("goal-horn.mp3", "mushroom.mp3", "clock.mp3")
    foreach ($audioFile in $audioFiles) {
        $sourcePath = "./wwwroot/audio/$audioFile"
        if (Test-Path $sourcePath) {
            Copy-Item -Path $sourcePath -Destination "./publish/$rid/wwwroot/audio/" -Force
            Write-Host "  📢 Copied audio file: $audioFile" -ForegroundColor Magenta
        } else {
            Write-Host "  ⚠️  Audio file not found: $audioFile (users will need to add this)" -ForegroundColor Yellow
        }
    }
    
    # Copy any other audio files that might exist
    if (Test-Path "./wwwroot/audio/*") {
        $additionalFiles = Get-ChildItem "./wwwroot/audio/" -File | Where-Object { $_.Name -notin $audioFiles }
        foreach ($file in $additionalFiles) {
            Copy-Item -Path $file.FullName -Destination "./publish/$rid/wwwroot/audio/" -Force
            Write-Host "  🎵 Copied additional audio file: $($file.Name)" -ForegroundColor Magenta
        }
    }
    
    # Create README for the release
    $releaseReadme = @"
# HockeyDJ v$Version

## Quick Start

### Windows Users:
- **Easy Start**: Double-click `start-hockeydj.bat`
- **PowerShell**: Right-click `start-hockeydj.ps1` → Run with PowerShell
- **Manual**: Run `HockeyDJ.exe` directly

### Linux/Mac Users:
- **Shell Script**: Run `./start-hockeydj.sh`
- **Manual**: Run `./HockeyDJ` directly

The application will start and be available at:
- https://127.0.0.1:7001 (secure)
- http://127.0.0.1:5000 (fallback)

## Setup Required:
1. Add your audio files to the `wwwroot/audio/` folder:
   - goal-horn.mp3
   - mushroom.mp3  
   - clock.mp3
2. Configure your Spotify API credentials in the web interface
3. Add your Spotify playlists

## Troubleshooting:
- If the HTTPS URL doesn't work, try the HTTP version
- Make sure no other applications are using ports 7001 or 5000
- On Linux/Mac, you may need to run: `chmod +x HockeyDJ` and `chmod +x start-hockeydj.sh`

See the full README.md for detailed setup instructions.
"@
    $releaseReadme | Out-File -FilePath "./publish/$rid/QUICK-START.md" -Encoding UTF8
    
    # Create archive
    $archiveName = "HockeyDJ-v$Version-$rid"
    if ($archiveType -eq "zip") {
        Compress-Archive -Path "./publish/$rid/*" -DestinationPath "./releases/v$Version/$archiveName.zip" -Force
        Write-Host "✅ Created $archiveName.zip" -ForegroundColor Green
    } else {
        # Use tar for Unix systems
        Push-Location "./publish/$rid"
        tar -czf "../../releases/v$Version/$archiveName.tar.gz" *
        Pop-Location
        Write-Host "✅ Created $archiveName.tar.gz" -ForegroundColor Green
    }
}

Write-Host "🎉 All builds completed successfully!" -ForegroundColor Green
Write-Host "📁 Release files are in: ./releases/v$Version/" -ForegroundColor Yellow
Write-Host ""
Write-Host "Each release includes:" -ForegroundColor Cyan
Write-Host "  • HockeyDJ executable" -ForegroundColor White
Write-Host "  • Configuration files" -ForegroundColor White
Write-Host "  • Startup scripts for easy launching" -ForegroundColor White
Write-Host "  • Quick start guide" -ForegroundColor White
Write-Host ""
Write-Host "Users can start the app by running the startup script or executable directly!" -ForegroundColor Green
