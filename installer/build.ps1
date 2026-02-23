# build.ps1 - Build and compile installer
# Medoc API COM Integration
#
# Usage:
#   .\build.ps1            - publish + compile installer
#   .\build.ps1 -SkipInno  - only dotnet publish (no compile)

param(
    [switch]$SkipInno = $false
)

$ErrorActionPreference = "Stop"

# Paths
$ScriptDir  = Split-Path -Parent $MyInvocation.MyCommand.Path
$RootDir    = Split-Path -Parent $ScriptDir
$PublishDir = Join-Path $ScriptDir "publish"
$OutputDir  = Join-Path $ScriptDir "output"

Write-Host ""
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "  Medoc API COM Integration - Build Installer"  -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

# --- Clean publish dir ---
if (Test-Path $PublishDir) {
    Write-Host "[1/5] Cleaning publish directory..." -ForegroundColor Yellow
    Remove-Item $PublishDir -Recurse -Force
}

New-Item -ItemType Directory -Force -Path "$PublishDir\Service"   | Out-Null
New-Item -ItemType Directory -Force -Path "$PublishDir\DesktopUI" | Out-Null
New-Item -ItemType Directory -Force -Path $OutputDir              | Out-Null

# --- Publish Service ---
Write-Host "[2/5] Publishing Service..." -ForegroundColor Yellow

dotnet publish "$RootDir\src\Service\Service.csproj" `
    -c Release `
    -r win-x64 `
    --self-contained false `
    -o "$PublishDir\Service" `
    /p:PublishSingleFile=false `
    --nologo

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Service publish failed!" -ForegroundColor Red
    exit 1
}
Write-Host "Service published successfully" -ForegroundColor Green
Write-Host ""

# --- Publish DesktopUI ---
Write-Host "[3/5] Publishing DesktopUI..." -ForegroundColor Yellow

dotnet publish "$RootDir\src\DesktopUI\DesktopUI.csproj" `
    -c Release `
    -r win-x64 `
    --self-contained false `
    -o "$PublishDir\DesktopUI" `
    /p:PublishSingleFile=false `
    --nologo

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: DesktopUI publish failed!" -ForegroundColor Red
    exit 1
}
Write-Host "DesktopUI published successfully" -ForegroundColor Green
Write-Host ""

# --- Show publish sizes ---
$ServiceSize = (Get-ChildItem "$PublishDir\Service"   -Recurse | Measure-Object -Property Length -Sum).Sum / 1MB
$DesktopSize = (Get-ChildItem "$PublishDir\DesktopUI" -Recurse | Measure-Object -Property Length -Sum).Sum / 1MB
Write-Host "Publish sizes:" -ForegroundColor Gray
Write-Host "  Service:   $([math]::Round($ServiceSize,  1)) MB" -ForegroundColor Gray
Write-Host "  DesktopUI: $([math]::Round($DesktopSize, 1)) MB" -ForegroundColor Gray
Write-Host ""

# --- Compile Inno Setup ---
if ($SkipInno) {
    Write-Host "[4/5] Skipping Inno Setup compile (-SkipInno)" -ForegroundColor Gray
    Write-Host ""
    Write-Host "=== PUBLISH COMPLETE ==="            -ForegroundColor Green
    Write-Host "Files are located in: $PublishDir"  -ForegroundColor Cyan
    exit 0
}

Write-Host "[4/5] Searching for Inno Setup compiler..." -ForegroundColor Yellow

$InnoCompiler = ""
$PossiblePaths = @(
    "C:\Program Files (x86)\Inno Setup 6\ISCC.exe",
    "C:\Program Files\Inno Setup 6\ISCC.exe"
)

foreach ($Path in $PossiblePaths) {
    if (Test-Path $Path) {
        $InnoCompiler = $Path
        break
    }
}

if ($InnoCompiler -eq "") {
    Write-Host "WARNING: Inno Setup 6 not found." -ForegroundColor Yellow
    Write-Host "Download from: https://jrsoftware.org/isinfo.php" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Publish files saved to: $PublishDir" -ForegroundColor Cyan
    Write-Host "After installing Inno Setup run build.ps1 again." -ForegroundColor Cyan
    exit 0
}

Write-Host "Found: $InnoCompiler" -ForegroundColor Green
Write-Host ""
Write-Host "[5/5] Compiling installer..." -ForegroundColor Yellow

& $InnoCompiler "$ScriptDir\setup.iss" /O"$OutputDir"

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Installer compilation failed!" -ForegroundColor Red
    exit 1
}

Write-Host "" 
Write-Host "================================================" -ForegroundColor Green
Write-Host "  BUILD COMPLETE!"                               -ForegroundColor Green
Write-Host "================================================" -ForegroundColor Green
Write-Host "  Installer: $OutputDir\MedocIntegration_Setup_1.0.0.exe" -ForegroundColor Cyan
Write-Host ""
