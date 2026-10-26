; Instalador de SISRestaurante (Restaurante Casa Grande) - Inno Setup 6.4 o superior.
; No se compila a mano: lo hace installer\Publicar-Instalador.ps1, que antes compila la app en Release y deja
; los archivos listos en installer\build\app.

#ifndef AppVersion
  #define AppVersion "1.0.0"
#endif
#ifndef SourceDir
  #define SourceDir "build\app"
#endif
#define AppExe "SISMistico.exe"
#define ApiRutaBase "/api/v1/"

[Setup]
; AppId identifica la instalacion para actualizar y desinstalar: NO cambiarlo nunca.
AppId={{B7C0E4F2-6A1D-4C53-9E55-0C4A5D7A2F10}
AppName=SISRestaurante - Casa Grande
AppVersion={#AppVersion}
AppPublisher=Juan Diego Duque
DefaultDirName={autopf}\SISRestaurante
DefaultGroupName=SISRestaurante
DisableProgramGroupPage=yes
UninstallDisplayName=SISRestaurante - Casa Grande
UninstallDisplayIcon={app}\{#AppExe}
SetupIconFile=..\SISMistico\SISMistico\mycomputer.ico
OutputDir=Output
OutputBaseFilename=SISRestaurante-Setup-{#AppVersion}
Compression=lzma2/max
SolidCompression=yes
WizardStyle=modern
ArchitecturesInstallIn64BitMode=x64compatible
MinVersion=10.0
; Admin: la reserva de URL (netsh) y la regla de firewall de la API movil lo exigen.
PrivilegesRequired=admin
; Si SISMistico esta abierto durante una actualizacion, lo cierra (Restart Manager) y no lo reabre solo.
CloseApplications=yes
RestartApplications=no

[Languages]
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"

[Tasks]
Name: "desktopicon"; Description: "Crear un acceso directo en el escritorio"; GroupDescription: "Accesos directos:"
Name: "api"; Description: "Activar la API movil y abrir el firewall (para los celulares de los meseros)"; GroupDescription: "Meseros con celular:"

[Dirs]
; La app reescribe su propio SISMistico.exe.config (ruta de imagenes, cadena de conexion, correo, licencias) y
; Configuration.Save crea un archivo temporal en la misma carpeta; tambien escribe logs\ aqui.
; Con ClickOnce vivia en el perfil del usuario; en Archivos de programa el usuario normal necesita permiso.
Name: "{app}"; Permissions: users-modify
Name: "{app}\logs"; Permissions: users-modify

[Files]
Source: "{#SourceDir}\*"; DestDir: "{app}"; Excludes: "SISMistico.exe.config"; Flags: recursesubdirs createallsubdirs ignoreversion
; El config solo se instala la primera vez: una actualizacion no pisa los cambios del restaurante.
Source: "{#SourceDir}\SISMistico.exe.config"; DestDir: "{app}"; Flags: onlyifdoesntexist
Source: "..\scripts\Configurar-Red.ps1"; DestDir: "{app}\Herramientas"; Flags: ignoreversion

[Icons]
Name: "{group}\SISRestaurante"; Filename: "{app}\{#AppExe}"
Name: "{group}\Datos para conectar celulares"; Filename: "{sys}\WindowsPowerShell\v1.0\powershell.exe"; Parameters: "-NoProfile -ExecutionPolicy Bypass -NoExit -File ""{app}\Herramientas\Configurar-Red.ps1"" -SoloInformar -RutaConfig ""{app}\SISMistico.exe.config"""; Comment: "Muestra la IP, el puerto y la clave que se escriben en cada celular"
Name: "{group}\Desinstalar SISRestaurante"; Filename: "{uninstallexe}"
Name: "{autodesktop}\SISRestaurante"; Filename: "{app}\{#AppExe}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#AppExe}"; Description: "Abrir SISRestaurante"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
Type: filesandordirs; Name: "{app}\logs"

[Code]
const
  ClavePlaceholder = 'cambiar-esta-clave';
  NombreRegla = 'SISRestaurante API';
  Alfabeto = 'ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789';

var
  ApiPage: TInputQueryWizardPage;
  ApiPageLista: Boolean;
  ClaveElegida: String;
  PuertoElegido: String;

function RutaConfig: String;
begin
  Result := ExpandConstant('{app}\SISMistico.exe.config');
end;

function NuevoDocumento(var Doc: Variant): Boolean;
begin
  Result := False;
  if not FileExists(RutaConfig) then Exit;
  try
    Doc := CreateOleObject('MSXML2.DOMDocument.6.0');
    Doc.async := False;
    Result := Doc.load(RutaConfig);
  except
    Result := False;
  end;
end;

function LeerAjuste(const Clave: String): String;
var
  Doc, Nodo: Variant;
begin
  Result := '';
  if not NuevoDocumento(Doc) then Exit;
  Nodo := Doc.selectSingleNode('/configuration/appSettings/add[@key="' + Clave + '"]');
  if not VarIsClear(Nodo) then
    Result := Nodo.getAttribute('value');
end;

procedure EscribirAjuste(Doc: Variant; const Clave, Valor: String);
var
  Nodo, Padre: Variant;
begin
  Nodo := Doc.selectSingleNode('/configuration/appSettings/add[@key="' + Clave + '"]');
  if VarIsClear(Nodo) then
  begin
    Padre := Doc.selectSingleNode('/configuration/appSettings');
    Nodo := Doc.createElement('add');
    Nodo.setAttribute('key', Clave);
    Padre.appendChild(Nodo);
  end;
  Nodo.setAttribute('value', Valor);
end;

function NuevaClave: String;
var
  I: Integer;
  TipoLib: Variant;
  Texto: String;
begin
  Result := '';
  // Fuente buena: un GUID de Windows (CoCreateGuid, 122 bits aleatorios) -> 24 caracteres hexadecimales.
  try
    TipoLib := CreateOleObject('Scriptlet.TypeLib');
    Texto := TipoLib.Guid;
    StringChangeEx(Texto, '{', '', True);
    StringChangeEx(Texto, '}', '', True);
    StringChangeEx(Texto, '-', '', True);
    Result := Lowercase(Copy(Texto, 1, 24));
  except
    Result := '';
  end;
  if Length(Result) = 24 then Exit;
  // Respaldo (menos fuerte): Random sin semilla repetiria la clave, asi que se avanza segun la hora.
  for I := 1 to StrToIntDef(GetDateTimeString('nnss', #0, #0), 1) + 1 do Random(10);
  Result := '';
  for I := 1 to 24 do
    Result := Result + Alfabeto[Random(Length(Alfabeto)) + 1];
end;

function PuertoDe(const Prefijo: String): String;
var
  P, Q: Integer;
  Resto: String;
begin
  Result := '8099';
  P := Pos('://', Prefijo);
  if P = 0 then Exit;
  Resto := Copy(Prefijo, P + 3, Length(Prefijo));
  P := Pos(':', Resto);
  if P = 0 then Exit;
  Resto := Copy(Resto, P + 1, Length(Resto));
  Q := Pos('/', Resto);
  if Q > 1 then Result := Copy(Resto, 1, Q - 1);
end;

function CorrerNetsh(const Parametros: String): Integer;
var
  Codigo: Integer;
begin
  if not Exec(ExpandConstant('{sys}\netsh.exe'), Parametros, '', SW_HIDE, ewWaitUntilTerminated, Codigo) then
    Codigo := -1;
  Result := Codigo;
end;

procedure QuitarRedApi(const Prefijo: String);
begin
  CorrerNetsh('http delete urlacl url=' + Prefijo);
  CorrerNetsh('advfirewall firewall delete rule name="' + NombreRegla + '"');
end;

function InitializeSetup: Boolean;
var
  Release: Cardinal;
begin
  Result := True;
  // .NET Framework 4.8 = Release 528040 o mayor (Windows 10 1903+ y Windows 11 ya lo traen).
  if not RegQueryDWordValue(HKLM, 'SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full', 'Release', Release)
     or (Release < 528040) then
  begin
    MsgBox('SISRestaurante necesita .NET Framework 4.8.' + #13#10 + #13#10 +
           'Instalelo desde https://dotnet.microsoft.com/download/dotnet-framework/net48 ' +
           'o con Windows Update, y vuelva a ejecutar este instalador.', mbError, MB_OK);
    Result := False;
    Exit;
  end;
  if not RegKeyExists(HKLM, 'SOFTWARE\Microsoft\Microsoft SQL Server Local DB\Installed Versions') then
    MsgBox('No se encontro SQL Server Express LocalDB en este equipo.' + #13#10 + #13#10 +
           'La aplicacion lo usa por defecto: instalelo antes de abrirla, o configure otra cadena de conexion ' +
           '(Administracion avanzada). El instalador continuara.', mbInformation, MB_OK);
end;

procedure InitializeWizard;
begin
  ApiPage := CreateInputQueryPage(wpSelectTasks, 'API movil',
    'Datos con los que se conectaran los celulares de los meseros',
    'La clave es la que cada celular escribe en "Configurar servidor". Se puede cambiar despues en el ' +
    'archivo SISMistico.exe.config (claves ApiKey y ApiPrefijo).');
  ApiPage.Add('Clave de la API (minimo 12 caracteres, sin espacios):', False);
  ApiPage.Add('Puerto:', False);
  ApiPageLista := False;
end;

function ShouldSkipPage(PageID: Integer): Boolean;
begin
  Result := (PageID = ApiPage.ID) and not WizardIsTaskSelected('api');
end;

// Clave por defecto: /ApiKey=... de la linea de comandos, la que ya tenia el restaurante o una nueva.
function ClavePorDefecto: String;
var
  Existente: String;
begin
  Result := ExpandConstant('{param:ApiKey|}');
  if Result <> '' then Exit;
  Existente := LeerAjuste('ApiKey');
  if (Existente <> '') and (Existente <> ClavePlaceholder) then
    Result := Existente
  else
    Result := NuevaClave;
end;

function PuertoPorDefecto: String;
begin
  Result := ExpandConstant('{param:ApiPort|}');
  if Result = '' then
    Result := PuertoDe(LeerAjuste('ApiPrefijo'));
end;

procedure CurPageChanged(CurPageID: Integer);
begin
  if (CurPageID = ApiPage.ID) and not ApiPageLista then
  begin
    ApiPageLista := True;
    // En una actualizacion se conserva la clave y el puerto que ya tenia el restaurante.
    ApiPage.Values[0] := ClavePorDefecto;
    ApiPage.Values[1] := PuertoPorDefecto;
  end;
  if (CurPageID = wpFinished) and (ClaveElegida <> '') then
    WizardForm.FinishedLabel.Caption := WizardForm.FinishedLabel.Caption + #13#10 + #13#10 +
      'Datos para cada celular:' + #13#10 +
      '  Puerto: ' + PuertoElegido + #13#10 +
      '  Clave de API: ' + ClaveElegida + #13#10 +
      '  IP del PC: en el menu Inicio, "Datos para conectar celulares".';
end;

function NextButtonClick(CurPageID: Integer): Boolean;
var
  Puerto: Integer;
  Clave: String;
begin
  Result := True;
  if CurPageID <> ApiPage.ID then Exit;
  Clave := Trim(ApiPage.Values[0]);
  Puerto := StrToIntDef(Trim(ApiPage.Values[1]), 0);
  if (Length(Clave) < 12) or (Pos(' ', Clave) > 0) or (Clave = ClavePlaceholder) then
  begin
    MsgBox('La clave debe tener al menos 12 caracteres, sin espacios, y no puede ser la de ejemplo.', mbError, MB_OK);
    Result := False;
  end
  else if (Puerto < 1024) or (Puerto > 65535) then
  begin
    MsgBox('El puerto debe ser un numero entre 1024 y 65535.', mbError, MB_OK);
    Result := False;
  end
  else
  begin
    ClaveElegida := Clave;
    PuertoElegido := IntToStr(Puerto);
  end;
end;

procedure ConfigurarApi;
var
  Doc: Variant;
  Prefijo: String;
begin
  Prefijo := 'http://+:' + PuertoElegido + '{#ApiRutaBase}';
  if NuevoDocumento(Doc) then
  begin
    EscribirAjuste(Doc, 'ApiHabilitada', 'true');
    EscribirAjuste(Doc, 'ApiPrefijo', Prefijo);
    EscribirAjuste(Doc, 'ApiKey', ClaveElegida);
    Doc.save(RutaConfig);
  end
  else
    MsgBox('No se pudo editar ' + RutaConfig + '. Active la API a mano (ApiHabilitada, ApiPrefijo, ApiKey).', mbError, MB_OK);

  // Reserva de URL por SID ("Todos"): no depende del idioma de Windows y evita ejecutar la app como admin.
  CorrerNetsh('http delete urlacl url=' + Prefijo);
  if CorrerNetsh('http add urlacl url=' + Prefijo + ' sddl=D:(A;;GX;;;WD)') <> 0 then
    MsgBox('No se pudo reservar la URL ' + Prefijo + '. Ejecute "Datos para conectar celulares" o ' +
           'Herramientas\Configurar-Red.ps1 como administrador.', mbError, MB_OK);
  // Solo redes Privadas/Dominio y solo la subred local: nada se expone a internet.
  CorrerNetsh('advfirewall firewall delete rule name="' + NombreRegla + '"');
  if CorrerNetsh('advfirewall firewall add rule name="' + NombreRegla + '" dir=in action=allow protocol=TCP ' +
                 'localport=' + PuertoElegido + ' profile=private,domain remoteip=localsubnet') <> 0 then
    MsgBox('No se pudo crear la regla de firewall para el puerto ' + PuertoElegido + '.', mbError, MB_OK);
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
  if (CurStep = ssPostInstall) and WizardIsTaskSelected('api') then
  begin
    // Instalacion silenciosa (/VERYSILENT /TASKS=api): la pagina no se mostro, se usan los valores por defecto.
    if ClaveElegida = '' then ClaveElegida := ClavePorDefecto;
    if PuertoElegido = '' then PuertoElegido := PuertoPorDefecto;
    ConfigurarApi;
  end;
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
var
  Prefijo: String;
begin
  if CurUninstallStep = usUninstall then
  begin
    Prefijo := LeerAjuste('ApiPrefijo');
    if Prefijo = '' then Prefijo := 'http://+:8099{#ApiRutaBase}';
    QuitarRedApi(Prefijo);
  end;
end;
