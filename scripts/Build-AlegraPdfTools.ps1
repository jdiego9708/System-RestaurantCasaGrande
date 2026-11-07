[CmdletBinding()]
param([switch]$Rebuild, [string]$Python='python')
$ErrorActionPreference = 'Stop'
$repo = Split-Path $PSScriptRoot -Parent
$buildRoot = Join-Path $repo 'installer\build\pdf-tools'
$runtime = Join-Path $buildRoot 'runtime'
New-Item -ItemType Directory -Path $runtime -Force | Out-Null
$renderer = Join-Path $runtime 'Renderer\CasaGrandeFiscalPdf.exe'
$source = Join-Path $PSScriptRoot 'Render-AlegraFiscalRepresentation.py'
function Invoke-PdfBuildPython([string[]]$Arguments) {
    $previousPreference=$ErrorActionPreference
    try {
        # PyInstaller escribe INFO por stderr; en Windows PowerShell no es un fallo por si mismo.
        $ErrorActionPreference='Continue'
        & $Python @Arguments 2>&1 | ForEach-Object { Write-Host $_.ToString() }
        return $LASTEXITCODE
    } finally { $ErrorActionPreference=$previousPreference }
}
$importCode=Invoke-PdfBuildPython @('-c','import sys, reportlab, pymupdf, PyInstaller; assert sys.version_info[:2] == (3,12) and sys.maxsize > 2**32')
if ($importCode -ne 0) { throw 'Use Python 3.12 de 64 bits e instale: python -m pip install -r scripts/requirements-alegra-pdf.txt' }
$requirements = Join-Path $PSScriptRoot 'requirements-alegra-pdf.txt'
$fingerprint = ((Get-FileHash -LiteralPath $source).Hash + (Get-FileHash -LiteralPath $requirements).Hash + (Get-FileHash -LiteralPath $PSCommandPath).Hash)
$stamp = Join-Path $buildRoot 'renderer-build.sha256'
$previous = if(Test-Path -LiteralPath $stamp) { [IO.File]::ReadAllText($stamp).Trim() } else { '' }
if ($Rebuild -or -not (Test-Path $renderer) -or $previous -ne $fingerprint) {
    $dist = Join-Path $buildRoot 'dist'
    $buildCode=Invoke-PdfBuildPython @('-m','PyInstaller','--noconfirm','--clean','--onedir','--console','--name','CasaGrandeFiscalPdf',
        '--collect-all','reportlab','--exclude-module','tkinter','--exclude-module','numpy','--exclude-module','matplotlib',
        '--distpath',$dist,'--workpath',(Join-Path $buildRoot 'work'),'--specpath',$buildRoot,$source)
    if ($buildCode -ne 0) { throw 'No se pudo compilar el generador de PDF fiscal.' }
    New-Item -ItemType Directory -Path (Split-Path $renderer -Parent) -Force | Out-Null
    Get-ChildItem -LiteralPath (Join-Path $dist 'CasaGrandeFiscalPdf') | Copy-Item -Destination (Split-Path $renderer -Parent) -Recurse -Force
    [IO.File]::WriteAllText($stamp,$fingerprint)
}
& $renderer --self-test
if($LASTEXITCODE -ne 0) { throw 'Fallo el renderer portable. Repita Build-AlegraPdfTools.ps1 -Rebuild.' }
$sumatra = Join-Path $runtime 'SumatraPDF.exe'
$archive = Join-Path $buildRoot 'SumatraPDF-3.6.1.zip'
$expected = '98B33A518D42986856D225064B0CD2D3643ECF78CBF84AB873D26CC51877A544'
if (-not (Test-Path $archive)) {
    Invoke-WebRequest -UseBasicParsing -Uri 'https://www.sumatrapdfreader.org/dl/rel/3.6.1/SumatraPDF-3.6.1-64.zip' -OutFile $archive
}
if ((Get-FileHash -LiteralPath $archive -Algorithm SHA256).Hash -ne $expected) { throw 'SumatraPDF no coincide con SHA-256 esperado.' }
$unpack = Join-Path $buildRoot 'sumatra-3.6.1'
Expand-Archive -LiteralPath $archive -DestinationPath $unpack -Force
$exe = @(Get-ChildItem -LiteralPath $unpack -Filter '*.exe')
if ($exe.Count -ne 1) { throw 'El paquete de SumatraPDF no contiene un ejecutable unico.' }
Copy-Item -LiteralPath $exe[0].FullName -Destination $sumatra -Force
@'
SumatraPDF 3.6.1 portable, GPLv3: https://www.sumatrapdfreader.org/
Codigo fuente: https://github.com/sumatrapdfreader/sumatrapdf/tree/3.6.1
Licencia: https://www.gnu.org/licenses/gpl-3.0.html
CasaGrandeFiscalPdf incluye Python, reportlab, PyMuPDF (AGPLv3) y Pillow; avisos en Renderer/_internal.
El equipo destino no necesita instalar Python. El PDF oficial se conserva y la impresion usa el renderer de 80 mm.
No incluye credenciales ni datos de clientes.
'@ | Set-Content -LiteralPath (Join-Path $runtime 'LEEME-HERRAMIENTAS.txt') -Encoding UTF8
Write-Host 'Generador de 80 mm y SumatraPDF portables listos; el destino no requiere Python.'
