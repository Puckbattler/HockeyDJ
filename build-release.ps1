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
    Copy-Item -Path "C:\Users\puckb\source\repos\HockeyDJ\HockeyDJ\README.md" -Destination "./publish/$rid/" -Force
    Copy-Item -Path "C:\Users\puckb\source\repos\HockeyDJ\HockeyDJ\hockeydj_license.md" -Destination "./publish/$rid/" -Force
    
    # Create audio directory with samples
    New-Item -ItemType Directory -Path "./publish/$rid/wwwroot/audio" -Force | Out-Null
    
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
