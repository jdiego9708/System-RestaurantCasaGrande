[CmdletBinding()]
param([Parameter(Mandatory=$true)][string]$ApplicationPath, [switch]$RunRendererSelfTest)
$ErrorActionPreference = 'Stop'
$required = @(
    'SISMistico.exe', 'CapaPresentacion.dll', 'CapaNegocio.dll', 'CapaDatos.dll',
    'Alegra\Render-AlegraFiscalRepresentation.py',
    'Alegra\Tools\SumatraPDF.exe', 'Alegra\Tools\LEEME-HERRAMIENTAS.txt',
    'Alegra\Tools\Renderer\CasaGrandeFiscalPdf.exe',
    'Alegra\Tools\Renderer\_internal\python312.dll',
    'Alegra\Tools\Renderer\_internal\base_library.zip'
)
foreach ($relative in $required) {
    $file = Join-Path $ApplicationPath $relative
    if (-not (Test-Path -LiteralPath $file -PathType Leaf) -or (Get-Item -LiteralPath $file).Length -eq 0) {
        throw "Paquete incompleto: falta $relative. Recompile el instalador sin -SinCompilar."
    }
}
if ($RunRendererSelfTest) {
    & (Join-Path $ApplicationPath 'Alegra\Tools\Renderer\CasaGrandeFiscalPdf.exe') --self-test
    if ($LASTEXITCODE -ne 0) { throw 'El renderer incluido no puede generar/leer un PDF de prueba. Recompile las herramientas.' }
}
Write-Host 'Paquete Alegra verificado: generador de 80 mm, dependencias y lector de impresion.'
