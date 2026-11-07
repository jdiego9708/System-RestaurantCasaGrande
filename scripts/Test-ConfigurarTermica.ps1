$ErrorActionPreference='Stop'
. (Join-Path $PSScriptRoot 'Configurar-Impresora-Termica.ps1')
$cap=Read-SafeXml @'
<f:PrintCapabilities xmlns:f="http://schemas.microsoft.com/windows/2003/08/printing/printschemaframework" xmlns:k="http://schemas.microsoft.com/windows/2003/08/printing/printschemakeywords" xmlns:vendor="urn:test:pos80">
<f:Feature name="k:PageMediaSize">
<f:Option name="vendor:Short"><f:ScoredProperty name="k:MediaSizeWidth"><f:Value>72071</f:Value></f:ScoredProperty><f:ScoredProperty name="k:MediaSizeHeight"><f:Value>297042</f:Value></f:ScoredProperty></f:Option>
<f:Option name="vendor:Long"><f:ScoredProperty name="k:MediaSizeWidth"><f:Value>72071</f:Value></f:ScoredProperty><f:ScoredProperty name="k:MediaSizeHeight"><f:Value>3275974</f:Value></f:ScoredProperty></f:Option>
</f:Feature></f:PrintCapabilities>
'@
$original=@'
<f:PrintTicket xmlns:f="http://schemas.microsoft.com/windows/2003/08/printing/printschemaframework" xmlns:k="http://schemas.microsoft.com/windows/2003/08/printing/printschemakeywords" xmlns:vendor="urn:test:pos80">
<f:Feature name="k:PageMediaSize"><f:Option name="vendor:Short"/></f:Feature>
<f:Feature name="vendor:Cut"><f:Option name="vendor:KeepCurrentCut"/></f:Feature>
<f:ParameterInit name="vendor:PrivateSettings"><f:Value>unchanged-private-data</f:Value></f:ParameterInit>
</f:PrintTicket>
'@
$roll=Get-RollOptions $cap | Sort-Object Alto_mm -Descending | Select-Object -First 1
if($roll.Alto_mm -ne 3275.974 -or $roll.Ancho_mm -ne 72.071){throw 'Seleccion de formato incorrecta'}
$changed=Set-RollInTicket $original $cap $roll
$xml=Read-SafeXml $changed
if(!$changed.Contains('KeepCurrentCut') -or !$changed.Contains('unchanged-private-data')){throw 'Cambio preferencias ajenas al papel'}
$ns=New-Object Xml.XmlNamespaceManager $xml.NameTable;$ns.AddNamespace('f',$framework)
$feature=$xml.SelectNodes('//f:Feature',$ns)|Where-Object {($_.GetAttribute('name') -split ':')[-1] -eq 'PageMediaSize'}
$option=$feature.SelectSingleNode('f:Option',$ns)
if($option.GetAttribute('name') -ne 'vendor:Long' -or $option.GetNamespaceOfPrefix('vendor') -ne 'urn:test:pos80'){throw 'Opcion o prefijo de fabricante incorrecto'}
$rejected=$false
try{[void](Read-SafeXml '<!DOCTYPE x [<!ENTITY e SYSTEM "file:///missing">]><x>&e;</x>')}catch{$rejected=$true}
if(!$rejected){throw 'No rechazo entidades externas'}
Write-Host 'OK: formato POS-80 largo, XML valido, prefijos, preferencias privadas conservadas y rechazo de DTD. No se modificaron impresoras.'
