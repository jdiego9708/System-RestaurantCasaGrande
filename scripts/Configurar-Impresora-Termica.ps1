<#
Windows PowerShell 5.1, en el PC del restaurante y con el mismo usuario que usa el sistema.
Configurar/Restaurar necesitan elevacion. Estado solo consulta.
Guarda tickets originales antes de cambiar el formato de papel y revierte si falla la verificacion.
No imprime, no cambia de controlador/puerto, no vacia la cola ni reinicia servicios.
#>
[CmdletBinding()]
param(
    [ValidateSet('Configurar','Estado','Restaurar')][string]$Modo='Configurar',
    [string]$NombreImpresora,
    [string]$Respaldo,
    [switch]$NoPause
)
$ErrorActionPreference='Stop'
$framework='http://schemas.microsoft.com/windows/2003/08/printing/printschemaframework'
$keywords='http://schemas.microsoft.com/windows/2003/08/printing/printschemakeywords'

function Read-SafeXml([string]$text) {
    $settings=New-Object Xml.XmlReaderSettings
    $settings.DtdProcessing=[Xml.DtdProcessing]::Prohibit
    $settings.XmlResolver=$null
    $reader=[Xml.XmlReader]::Create((New-Object IO.StringReader $text),$settings)
    try { $xml=New-Object Xml.XmlDocument; $xml.XmlResolver=$null; $xml.Load($reader); return ,$xml }
    finally { $reader.Dispose() }
}
function Get-RollOptions([Xml.XmlDocument]$xml) {
    $ns=New-Object Xml.XmlNamespaceManager $xml.NameTable
    $ns.AddNamespace('f',$framework)
    foreach($feature in $xml.SelectNodes('//f:Feature',$ns)) {
        if(($feature.GetAttribute('name') -split ':')[-1] -ne 'PageMediaSize') { continue }
        foreach($option in $feature.SelectNodes('f:Option',$ns)) {
            $size=@{}
            foreach($property in $option.SelectNodes('f:ScoredProperty',$ns)) {
                $value=$property.SelectSingleNode('f:Value',$ns)
                if($value) { $size[($property.GetAttribute('name') -split ':')[-1]]=[double]$value.InnerText }
            }
            if($size.MediaSizeWidth -ge 65000 -and $size.MediaSizeWidth -le 85000 -and $size.MediaSizeHeight -gt 0) {
                [pscustomobject]@{Nombre=$option.GetAttribute('name'); Ancho_mm=$size.MediaSizeWidth/1000; Alto_mm=$size.MediaSizeHeight/1000; Option=$option}
            }
        }
    }
}
function Set-RollInTicket([string]$original,[Xml.XmlDocument]$capabilities,$roll) {
    $xml=Read-SafeXml $original
    $ns=New-Object Xml.XmlNamespaceManager $xml.NameTable; $ns.AddNamespace('f',$framework)
    $feature=$xml.SelectNodes('//f:Feature',$ns) | Where-Object {($_.GetAttribute('name') -split ':')[-1] -eq 'PageMediaSize'} | Select-Object -First 1
    if(!$feature) {
        $feature=$xml.CreateElement('psf','Feature',$framework)
        $feature.SetAttribute('xmlns:psk',$keywords)
        $feature.SetAttribute('name','psk:PageMediaSize')
        [void]$xml.DocumentElement.AppendChild($feature)
    }
    foreach($old in @($feature.SelectNodes('f:Option',$ns))) { [void]$feature.RemoveChild($old) }
    $option=$xml.CreateElement('psf','Option',$framework)
    $option.SetAttribute('name',$roll.Nombre)
    # Conservar los prefijos del fabricante sin alterar los de otras opciones del ticket.
    foreach($attribute in $capabilities.DocumentElement.Attributes) {
        if($attribute.Prefix -eq 'xmlns') { $option.SetAttribute($attribute.Name,$attribute.Value) }
    }
    foreach($property in $roll.Option.SelectNodes('f:ScoredProperty',$ns)) { [void]$option.AppendChild($xml.ImportNode($property,$true)) }
    [void]$feature.AppendChild($option)
    return $xml.OuterXml
}
function Ticket-Xml($ticket) {
    $stream=$ticket.GetXmlStream()
    try { $reader=New-Object IO.StreamReader $stream; try { return $reader.ReadToEnd() } finally { $reader.Dispose() } }
    finally { $stream.Dispose() }
}
function Ticket-FromXml([string]$text) {
    [void](Read-SafeXml $text)
    $stream=New-Object IO.MemoryStream (,[Text.Encoding]::UTF8.GetBytes($text))
    try { return New-Object System.Printing.PrintTicket $stream } finally { $stream.Dispose() }
}
function Save-Utf8([string]$path,[string]$text) { [IO.File]::WriteAllText($path,$text,(New-Object Text.UTF8Encoding $true)) }
function Ticket-Size($ticket) {
    [pscustomobject]@{Ancho_mm=[math]::Round($ticket.PageMediaSize.Width*25.4/96,2); Alto_mm=[math]::Round($ticket.PageMediaSize.Height*25.4/96,2)}
}
function Assert-Roll($ticket,$roll) {
    $size=Ticket-Size $ticket
    if([math]::Abs($size.Ancho_mm-$roll.Ancho_mm) -gt 1 -or [math]::Abs($size.Alto_mm-$roll.Alto_mm) -gt 1) {
        throw "El controlador no acepto el formato solicitado. Devolvio $($size.Ancho_mm) x $($size.Alto_mm) mm."
    }
}

# Permite pruebas de XML con datos ficticios sin abrir ni modificar impresoras.
if($MyInvocation.InvocationName -eq '.') { return }

$server=$null;$queue=$null
try {
    Import-Module PrintManagement -ErrorAction Stop
    Add-Type -AssemblyName System.Printing,ReachFramework,System.Drawing
    $sid=[Security.Principal.WindowsIdentity]::GetCurrent().User.Value
    $metadata=$null
    if($Modo -eq 'Restaurar') {
        if(!$Respaldo) { throw 'Indique la carpeta del respaldo con -Respaldo.' }
        $Respaldo=(Resolve-Path -LiteralPath $Respaldo).Path
        $metadata=Get-Content -LiteralPath (Join-Path $Respaldo 'respaldo.json') -Raw | ConvertFrom-Json
        if($metadata.Equipo -ne $env:COMPUTERNAME -or $metadata.UsuarioSID -ne $sid) {
            throw 'El respaldo pertenece a otro equipo/usuario. Use el mismo usuario de Windows del restaurante.'
        }
        if($NombreImpresora -and $NombreImpresora -ne $metadata.Impresora) { throw 'La impresora indicada no coincide con el respaldo.' }
        $NombreImpresora=$metadata.Impresora
    }
    if(!$NombreImpresora) {
        $printer=Get-CimInstance Win32_Printer | Where-Object Default | Select-Object -First 1
        if(!$printer) { throw 'No hay impresora predeterminada para este usuario.' }
        $NombreImpresora=$printer.Name
    }
    $printer=Get-Printer -ErrorAction Stop | Where-Object Name -eq $NombreImpresora | Select-Object -First 1
    if(!$printer) { throw "No existe la impresora: $NombreImpresora" }
    if($Modo -ne 'Estado') {
        $identity=[Security.Principal.WindowsIdentity]::GetCurrent()
        if(!(New-Object Security.Principal.WindowsPrincipal $identity).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
            throw 'Abra Windows PowerShell como administrador con el MISMO usuario habitual.'
        }
        if(Get-Process SISMistico -ErrorAction SilentlyContinue) { throw 'Cierre SISMistico antes de aplicar/restaurar; debe volver a abrirlo para tomar las preferencias nuevas.' }
        if(@(Get-PrintJob -PrinterName $NombreImpresora -ErrorAction Stop).Count -gt 0) { throw 'Hay trabajos pendientes. Espere a que terminen antes de configurar/restaurar.' }
    }
    $server=New-Object System.Printing.LocalPrintServer
    $access=if($Modo -eq 'Estado') {[System.Printing.PrintSystemDesiredAccess]::UsePrinter} else {[System.Printing.PrintSystemDesiredAccess]::AdministratePrinter}
    $queue=New-Object System.Printing.PrintQueue -ArgumentList $server,$NombreImpresora,$access
    $defaultXml=Ticket-Xml $queue.DefaultPrintTicket
    $userInherited=($null -eq $queue.UserPrintTicket)
    $userXml=if($userInherited) {$defaultXml} else {Ticket-Xml $queue.UserPrintTicket}
    Write-Host "Impresora: $NombreImpresora | Controlador: $($printer.DriverName) | Puerto: $($printer.PortName)"
    Write-Host 'Preferencia efectiva actual del usuario:'
    Ticket-Size (Ticket-FromXml $userXml) | Format-List | Out-Host

    if($Modo -eq 'Estado') {
        $cap=Read-SafeXml ((Get-PrintConfiguration -PrinterName $NombreImpresora).PrintCapabilitiesXML)
        Get-RollOptions $cap | Select-Object Nombre,Ancho_mm,Alto_mm | Format-Table -AutoSize | Out-Host
    } elseif($Modo -eq 'Restaurar') {
        if($printer.DriverName -ne $metadata.Controlador) { throw 'El controlador cambio desde el respaldo; no se aplica un ticket de otro controlador.' }
        foreach($name in @('DefaultPrintTicket.xml','UserPrintTicket.xml')) {
            if((Get-FileHash -LiteralPath (Join-Path $Respaldo $name)).Hash -ne $metadata.Hashes.$name) { throw "El respaldo no coincide con su hash: $name" }
        }
        # La copia User contiene los valores efectivos previos, incluso si originalmente se heredaban.
        $queue.DefaultPrintTicket=Ticket-FromXml ([IO.File]::ReadAllText((Join-Path $Respaldo 'DefaultPrintTicket.xml')))
        $queue.UserPrintTicket=Ticket-FromXml ([IO.File]::ReadAllText((Join-Path $Respaldo 'UserPrintTicket.xml')))
        $queue.Commit();$queue.Refresh()
        Write-Host 'RESTAURADO: configuracion general y valores efectivos anteriores del usuario.' -ForegroundColor Green
        Ticket-Size $queue.UserPrintTicket | Format-List | Out-Host
        if($metadata.UsuarioHeredaba) { Write-Host 'Nota: se restauraron los mismos valores como preferencia explicita del usuario.' }
    } else {
        $config=Get-PrintConfiguration -PrinterName $NombreImpresora
        $cap=Read-SafeXml $config.PrintCapabilitiesXML
        $roll=Get-RollOptions $cap | Sort-Object Alto_mm -Descending | Select-Object -First 1
        if(!$roll -or $roll.Alto_mm -lt 500) { throw 'El controlador no anuncia un formato largo de rollo de 65-85 mm. No se cambia nada.' }
        $newDefault=$queue.MergeAndValidatePrintTicket($queue.DefaultPrintTicket,(Ticket-FromXml (Set-RollInTicket $defaultXml $cap $roll))).ValidatedPrintTicket
        $newUser=$queue.MergeAndValidatePrintTicket((Ticket-FromXml $userXml),(Ticket-FromXml (Set-RollInTicket $userXml $cap $roll))).ValidatedPrintTicket
        Assert-Roll $newDefault $roll; Assert-Roll $newUser $roll
        if(!$Respaldo) {
            $desktop=[Environment]::GetFolderPath('DesktopDirectory')
            $Respaldo=Join-Path $desktop ('Respaldo-Termica-'+(Get-Date -Format 'yyyyMMdd-HHmmss')+'-'+[guid]::NewGuid().ToString('N').Substring(0,6))
        }
        if(Test-Path -LiteralPath $Respaldo) { throw 'La carpeta de respaldo ya existe. Elija otra para no sobrescribir el respaldo anterior.' }
        New-Item -ItemType Directory -Path $Respaldo -Force | Out-Null
        $Respaldo=(Resolve-Path -LiteralPath $Respaldo).Path
        Save-Utf8 (Join-Path $Respaldo 'DefaultPrintTicket.xml') $defaultXml
        Save-Utf8 (Join-Path $Respaldo 'UserPrintTicket.xml') $userXml
        Save-Utf8 (Join-Path $Respaldo 'PrintCapabilities.xml') $cap.OuterXml
        $hashes=@{}
        foreach($name in @('DefaultPrintTicket.xml','UserPrintTicket.xml')) {$hashes[$name]=(Get-FileHash -LiteralPath (Join-Path $Respaldo $name)).Hash}
        $metadata=@{Version=1;Equipo=$env:COMPUTERNAME;UsuarioSID=$sid;Impresora=$NombreImpresora;Controlador=$printer.DriverName;Puerto=$printer.PortName;UsuarioHeredaba=$userInherited;Fecha=(Get-Date -Format o);Hashes=$hashes}
        Save-Utf8 (Join-Path $Respaldo 'respaldo.json') ($metadata | ConvertTo-Json -Depth 5)
        Write-Host "Respaldo original: $Respaldo" -ForegroundColor Cyan
        try {
            $queue.DefaultPrintTicket=$newDefault;$queue.UserPrintTicket=$newUser
            $queue.Commit();$queue.Refresh()
            Assert-Roll $queue.DefaultPrintTicket $roll;Assert-Roll $queue.UserPrintTicket $roll
            $effective=New-Object Drawing.Printing.PrinterSettings;$effective.PrinterName=$NombreImpresora
            if([math]::Abs($effective.DefaultPageSettings.PaperSize.Height*0.254-$roll.Alto_mm) -gt 2) { throw 'La preferencia efectiva de Windows no coincide con el formato largo.' }
            Write-Host "CONFIGURADO: $($roll.Ancho_mm) x $($roll.Alto_mm) mm, formato largo anunciado por el controlador." -ForegroundColor Green
        } catch {
            $problem=$_.Exception.Message
            try {$queue.DefaultPrintTicket=Ticket-FromXml $defaultXml;$queue.UserPrintTicket=Ticket-FromXml $userXml;$queue.Commit();Write-Warning 'Se restauro automaticamente la configuracion original.'}
            catch {Write-Warning 'La restauracion automatica fallo. Use el comando Restaurar con el respaldo indicado.'}
            throw $problem
        }
        Write-Host 'Para revertir, con el mismo usuario y el sistema cerrado:'
        Write-Host ('powershell.exe -NoProfile -ExecutionPolicy Bypass -File "'+$PSCommandPath+'" -Modo Restaurar -Respaldo "'+$Respaldo+'"')
        Write-Host 'El programa ya calcula solo el alto de cada ticket; este formato es una red de seguridad si el controlador limita el largo.'
        Write-Host 'Reabra el programa y pruebe imprimiendo una comanda o factura larga.'
    }
} finally {
    if($queue){$queue.Dispose()};if($server){$server.Dispose()}
    if(!$NoPause){Read-Host 'Pulse Enter para cerrar' | Out-Null}
}
