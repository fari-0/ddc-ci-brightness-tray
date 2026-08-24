$ErrorActionPreference = 'Stop'

$root = $PSScriptRoot
$cscCandidates = @(
    (Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'),
    (Join-Path $env:WINDIR 'Microsoft.NET\Framework\v4.0.30319\csc.exe')
)
$csc = $cscCandidates | Where-Object { Test-Path -LiteralPath $_ } | Select-Object -First 1
if (-not $csc) { throw "csc.exe bulunamadi (.NET Framework 4.x gerekli)" }

$srcDir = Join-Path $root 'src'
$outDir = Join-Path $root 'bin'
$outExe = Join-Path $outDir 'BrightnessTray.exe'
New-Item -ItemType Directory -Force -Path $outDir | Out-Null

$csSources = @(Get-ChildItem -LiteralPath $srcDir -Recurse -Filter *.cs | ForEach-Object { $_.FullName })
if ($csSources.Count -eq 0) { throw "Kaynak dosya bulunamadi: $srcDir" }

$cscArgs = @(
    '/nologo',
    '/target:winexe',
    '/platform:anycpu',
    '/optimize+',
    '/codepage:65001',
    ('/win32manifest:' + (Join-Path $root 'app.manifest')),
    ('/out:' + $outExe),
    '/r:System.dll',
    '/r:System.Core.dll',
    '/r:System.Drawing.dll',
    '/r:System.Windows.Forms.dll'
) + $csSources

& $csc @cscArgs
if ($LASTEXITCODE -ne 0) { throw "Derleme basarisiz (exit code: $LASTEXITCODE)" }

Write-Host "OK -> $outExe"
