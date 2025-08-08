# PowerShell script to setup HTTPS for Docker
# Run this from your project root directory

Write-Host "Setting up HTTPS certificates for Docker..." -ForegroundColor Green

# Create certificates directory
if (!(Test-Path "certs")) {
    New-Item -ItemType Directory -Path "certs"
}

# Generate development certificate
Write-Host "Generating development certificate..." -ForegroundColor Yellow
dotnet dev-certs https -ep "certs/aspnetapp.pfx" -p "YourPassword123!" --trust

# Verify certificate was created
if (Test-Path "certs/aspnetapp.pfx") {
    Write-Host "Certificate generated successfully!" -ForegroundColor Green
} else {
    Write-Host "Certificate generation failed!" -ForegroundColor Red
    exit 1
}

# Build Docker image
Write-Host "Building Docker image..." -ForegroundColor Yellow
docker build -t hockeydj .

# Get your computer's IP address
$ipAddress = (Get-NetIPAddress -AddressFamily IPv4 -InterfaceAlias "Wi-Fi" | Where-Object {$_.IPAddress -like "192.168.*" -or $_.IPAddress -like "10.*" -or $_.IPAddress -like "172.*"}).IPAddress

if (!$ipAddress) {
    $ipAddress = (Get-NetIPAddress -AddressFamily IPv4 | Where-Object {$_.IPAddress -ne "127.0.0.1" -and $_.IPAddress -ne "::1"}).IPAddress | Select-Object -First 1
}

Write-Host "Your computer's IP address appears to be: $ipAddress" -ForegroundColor Cyan
Write-Host ""
Write-Host "IMPORTANT: Update your Spotify App redirect URI to:" -ForegroundColor Yellow
Write-Host "https://${ipAddress}:5001/Home/SpotifyCallback" -ForegroundColor White
Write-Host ""
Write-Host "To run the container:" -ForegroundColor Green
Write-Host "docker run -p 5000:5000 -p 5001:5001 -v `"$(pwd)/certs:/https`" hockeydj" -ForegroundColor White
Write-Host ""
Write-Host "Then access the app at: https://${ipAddress}:5001" -ForegroundColor Cyan