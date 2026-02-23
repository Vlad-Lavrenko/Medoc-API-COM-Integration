# build.ps1 - Build and compile installer
# Medoc API COM Integration
#
# Usage:
#   .\build.ps1            - publish + compile installer
#   .\build.ps1 -SkipInno  - only dotnet publish (no compile)
#   .\build.ps1 -InnoPath "C:\custom\path\ISCC.exe"  - explicit path

param(
    [switch]$SkipInno = $false,
    [string]$InnoPath = ""
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
    Write-Host "=== PUBLISH COMPLETE ==="           -ForegroundColor Green
    Write-Host "Files are located in: $PublishDir" -ForegroundColor Cyan
    exit 0
}

Write-Host "[4/5] Searching for Inno Setup compiler..." -ForegroundColor Yellow

$InnoCompiler = ""

# 1. Explicit path from parameter
if ($InnoPath -ne "" -and (Test-Path $InnoPath)) {
    $InnoCompiler = $InnoPath
    Write-Host "Using provided path: $InnoCompiler" -ForegroundColor Green
}

# 2. Known install paths
if ($InnoCompiler -eq "") {
    $KnownPaths = @(
        "C:\Program Files (x86)\Inno Setup 6\ISCC.exe",
        "C:\Program Files\Inno Setup 6\ISCC.exe",
        "C:\Program Files (x86)\Inno Setup 5\ISCC.exe",
        "C:\Program Files\Inno Setup 5\ISCC.exe",
        "D:\Program Files (x86)\Inno Setup 6\ISCC.exe",
        "D:\Program Files\Inno Setup 6\ISCC.exe"
    )
    foreach ($P in $KnownPaths) {
        if (Test-Path $P) {
            $InnoCompiler = $P
            break
        }
    }
}

# 3. Registry lookup
if ($InnoCompiler -eq "") {
    $RegKeys = @(
        "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
        "HKLM:\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
    )
    foreach ($RegBase in $RegKeys) {
        if (Test-Path $RegBase) {
            $SubKeys = Get-ChildItem $RegBase -ErrorAction SilentlyContinue
            foreach ($Key in $SubKeys) {
                $DisplayName = (Get-ItemProperty $Key.PSPath -ErrorAction SilentlyContinue).DisplayName
                if ($DisplayName -like "*Inno Setup*") {
                    $InstallLoc = (Get-ItemProperty $Key.PSPath -ErrorAction SilentlyContinue).InstallLocation
                    if ($InstallLoc -ne $null) {
                        $Candidate = Join-Path $InstallLoc "ISCC.exe"
                        if (Test-Path $Candidate) {
                            $InnoCompiler = $Candidate
                            Write-Host "Found via registry: $InnoCompiler" -ForegroundColor Green
                            break
                        }
                    }
                }
            }
        }
        if ($InnoCompiler -ne "") { break }
    }
}

# 4. Search PATH
if ($InnoCompiler -eq "") {
    $FromPath = Get-Command "ISCC.exe" -ErrorAction SilentlyContinue
    if ($FromPath) {
        $InnoCompiler = $FromPath.Source
        Write-Host "Found in PATH: $InnoCompiler" -ForegroundColor Green
    }
}

if ($InnoCompiler -eq "") {
    Write-Host "" 
    Write-Host "ERROR: Inno Setup 6 not found!" -ForegroundColor Red
    Write-Host ""
    Write-Host "Options:" -ForegroundColor Yellow
    Write-Host "  1. Download and install: https://jrsoftware.org/isinfo.php" -ForegroundColor Yellow
    Write-Host "  2. Or provide path explicitly:" -ForegroundColor Yellow
    Write-Host "     .\build.ps1 -InnoPath \"C:\your\path\ISCC.exe\"" -ForegroundColor White
    Write-Host ""
    Write-Host "To find ISCC.exe manually run:" -ForegroundColor Gray
    Write-Host "  Get-ChildItem 'C:\Program Files*' -Recurse -Filter ISCC.exe -ErrorAction SilentlyContinue" -ForegroundColor Gray
    Write-Host ""
    exit 1
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
