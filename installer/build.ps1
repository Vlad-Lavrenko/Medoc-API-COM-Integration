# =============================================================
# build.ps1 — збірка та компіляція інсталятора
# Medoc API COM Integration
# =============================================================
# Використання:
#   .\build.ps1              — publish + compile installer
#   .\build.ps1 -SkipInno   — тільки dotnet publish (без компіляції)
# =============================================================

param(
    [switch]$SkipInno = $false
)

$ErrorActionPreference = "Stop"

# ── Шляхи ────────────────────────────────────────────────────
$ScriptDir  = Split-Path -Parent $MyInvocation.MyCommand.Path
$RootDir    = Split-Path -Parent $ScriptDir
$PublishDir = Join-Path $ScriptDir "publish"
$OutputDir  = Join-Path $ScriptDir "output"

Write-Host ""
Write-Host "=================================================" -ForegroundColor Cyan
Write-Host "  Medoc API COM Integration — Build Installer" -ForegroundColor Cyan
Write-Host "=================================================" -ForegroundColor Cyan
Write-Host ""

# ── Очищення ─────────────────────────────────────────────────
if (Test-Path $PublishDir) {
    Write-Host "[1/5] Очищення publish директорії..." -ForegroundColor Yellow
    Remove-Item $PublishDir -Recurse -Force
}

New-Item -ItemType Directory -Force -Path "$PublishDir\Service"   | Out-Null
New-Item -ItemType Directory -Force -Path "$PublishDir\DesktopUI" | Out-Null
New-Item -ItemType Directory -Force -Path $OutputDir              | Out-Null

# ── Publish Service ───────────────────────────────────────────
Write-Host "[2/5] Публікація Service..." -ForegroundColor Yellow

dotnet publish "$RootDir\src\Service\Service.csproj" `
    -c Release `
    -r win-x64 `
    --self-contained false `
    -o "$PublishDir\Service" `
    /p:PublishSingleFile=false `
    --nologo

if ($LASTEXITCODE -ne 0) {
    Write-Host "ПОМИЛКА: Публікація Service завершилась з помилкою!" -ForegroundColor Red
    exit 1
}
Write-Host "Service опубліковано успішно" -ForegroundColor Green
Write-Host ""

# ── Publish DesktopUI ─────────────────────────────────────────
Write-Host "[3/5] Публікація DesktopUI..." -ForegroundColor Yellow

dotnet publish "$RootDir\src\DesktopUI\DesktopUI.csproj" `
    -c Release `
    -r win-x64 `
    --self-contained false `
    -o "$PublishDir\DesktopUI" `
    /p:PublishSingleFile=false `
    --nologo

if ($LASTEXITCODE -ne 0) {
    Write-Host "ПОМИЛКА: Публікація DesktopUI завершилась з помилкою!" -ForegroundColor Red
    exit 1
}
Write-Host "DesktopUI опубліковано успішно" -ForegroundColor Green
Write-Host ""

# ── Перевірка розміру publish ─────────────────────────────────
$ServiceSize  = (Get-ChildItem "$PublishDir\Service"   -Recurse | Measure-Object -Property Length -Sum).Sum / 1MB
$DesktopSize  = (Get-ChildItem "$PublishDir\DesktopUI" -Recurse | Measure-Object -Property Length -Sum).Sum / 1MB
Write-Host "Розмір publish:" -ForegroundColor Gray
Write-Host "  Service:   $([math]::Round($ServiceSize,  1)) MB" -ForegroundColor Gray
Write-Host "  DesktopUI: $([math]::Round($DesktopSize, 1)) MB" -ForegroundColor Gray
Write-Host ""

# ── Компіляція Inno Setup ─────────────────────────────────────
if ($SkipInno) {
    Write-Host "[4/5] Пропускаємо компіляцію Inno Setup (-SkipInno)" -ForegroundColor Gray
    Write-Host ""
    Write-Host "=== PUBLISH ЗАВЕРШЕНО ==="       -ForegroundColor Green
    Write-Host "Файли знаходяться у: $PublishDir" -ForegroundColor Cyan
    exit 0
}

Write-Host "[4/5] Пошук Inno Setup компілятора..." -ForegroundColor Yellow

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
    Write-Host "УВАГА: Inno Setup 6 не знайдено." -ForegroundColor Yellow
    Write-Host "Завантажте з: https://jrsoftware.org/isinfo.php" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Publish файли збережено у: $PublishDir" -ForegroundColor Cyan
    Write-Host "Після встановлення Inno Setup запустіть build.ps1 повторно." -ForegroundColor Cyan
    exit 0
}

Write-Host "Знайдено: $InnoCompiler" -ForegroundColor Green
Write-Host ""
Write-Host "[5/5] Компіляція інсталятора..." -ForegroundColor Yellow

& $InnoCompiler "$ScriptDir\setup.iss" /O"$OutputDir"

if ($LASTEXITCODE -ne 0) {
    Write-Host "ПОМИЛКА: Компіляція інсталятора завершилась з помилкою!" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "================================================" -ForegroundColor Green
Write-Host "  ЗБІРКА ЗАВЕРШЕНА УСПІШНО!"                     -ForegroundColor Green
Write-Host "================================================" -ForegroundColor Green
Write-Host "  Інсталятор: $OutputDir\MedocIntegration_Setup_1.0.0.exe" -ForegroundColor Cyan
Write-Host ""
