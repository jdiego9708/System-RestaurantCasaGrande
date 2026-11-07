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
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
MinVersion=10.0
; Admin: la reserva de URL (netsh) y la regla de firewall de la API movil lo exigen.
PrivilegesRequired=admin
ChangesEnvironment=yes
; Si SISMistico esta abierto durante una actualizacion, lo cierra (Restart Manager) y no lo reabre solo.
CloseApplications=yes
RestartApplications=no

[Languages]
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"

[Tasks]
Name: "desktopicon"; Description: "Crear un acceso directo en el escritorio"; GroupDescription: "Accesos directos:"
Name: "api"; Description: "Activar la API movil y abrir el firewall (para los celulares de los meseros)"; GroupDescription: "Meseros con celular:"
Name: "alegra"; Description: "Configurar credenciales de Alegra en variables de entorno del sistema"; GroupDescription: "Facturacion electronica:"; Flags: unchecked

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
Source: "..\scripts\Cambiar-Clave-API.ps1"; DestDir: "{app}\Herramientas"; Flags: ignoreversion
Source: "..\scripts\Cambiar-Codigo-Maestro.ps1"; DestDir: "{app}\Herramientas"; Flags: ignoreversion

[Icons]
Name: "{group}\SISRestaurante"; Filename: "{app}\{#AppExe}"
Name: "{group}\Datos para conectar celulares"; Filename: "{sys}\WindowsPowerShell\v1.0\powershell.exe"; Parameters: "-NoProfile -ExecutionPolicy Bypass -NoExit -File ""{app}\Herramientas\Configurar-Red.ps1"" -SoloInformar -RutaConfig ""{app}\SISMistico.exe.config"""; Comment: "Muestra la IP, el puerto y la clave que se escriben en cada celular"
Name: "{group}\Cambiar clave de la API"; Filename: "{sys}\WindowsPowerShell\v1.0\powershell.exe"; Parameters: "-NoProfile -ExecutionPolicy Bypass -NoExit -File ""{app}\Herramientas\Cambiar-Clave-API.ps1"" -RutaConfig ""{app}\SISMistico.exe.config"""; Comment: "Cambia la clave que escriben los celulares (personalizada o aleatoria) sin reinstalar"
Name: "{group}\Cambiar codigo maestro"; Filename: "{sys}\WindowsPowerShell\v1.0\powershell.exe"; Parameters: "-NoProfile -ExecutionPolicy Bypass -NoExit -File ""{app}\Herramientas\Cambiar-Codigo-Maestro.ps1"" -RutaConfig ""{app}\SISMistico.exe.config"""; Comment: "Cambia el codigo maestro de 8 digitos que se pide en el celular para quitar productos de un pedido"
Name: "{group}\Desinstalar SISRestaurante"; Filename: "{uninstallexe}"
Name: "{autodesktop}\SISRestaurante"; Filename: "{app}\{#AppExe}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#AppExe}"; Description: "Abrir SISRestaurante"; Flags: nowait postinstall skipifsilent unchecked

[UninstallDelete]
Type: filesandordirs; Name: "{app}\logs"

[Code]
const
  ClavePlaceholder = 'cambiar-esta-clave';
  NombreRegla = 'SISRestaurante API';
  AlfabetoClave = 'abcdefghjkmnpqrstuvwxyz23456789';

var
  ApiPage: TInputQueryWizardPage;
  AlegraPage: TInputQueryWizardPage;
  AlegraEmissionPage: TInputQueryWizardPage;
  MasterPage: TInputQueryWizardPage;
  MasterCodeElegido: String;
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

// Clave aleatoria FACIL DE TECLEAR en el celular: 12 caracteres en minuscula, sin 0/o ni 1/l/i (se confunden).
function NuevaClave: String;
var
  I, V: Integer;
  TipoLib: Variant;
  Texto: String;
begin
  Result := '';
  // Fuente buena: un GUID de Windows (CoCreateGuid, 122 bits aleatorios). Cada par de digitos hexadecimales
  // (0..255) elige un simbolo del alfabeto de 31.
  try
    TipoLib := CreateOleObject('Scriptlet.TypeLib');
    Texto := TipoLib.Guid;
    StringChangeEx(Texto, '{', '', True);
    StringChangeEx(Texto, '}', '', True);
    StringChangeEx(Texto, '-', '', True);
    Texto := Lowercase(Texto);
    for I := 0 to 11 do
    begin
      V := StrToInt('$' + Copy(Texto, I * 2 + 1, 2));
      Result := Result + AlfabetoClave[(V mod Length(AlfabetoClave)) + 1];
    end;
  except
    Result := '';
  end;
  if Length(Result) = 12 then Exit;
  // Respaldo (menos fuerte): Random sin semilla repetiria la clave, asi que se avanza segun la hora.
  for I := 1 to StrToIntDef(GetDateTimeString('nnss', #0, #0), 1) + 1 do Random(10);
  Result := '';
  for I := 1 to 12 do
    Result := Result + AlfabetoClave[Random(Length(AlfabetoClave)) + 1];
end;

// Una clave propia puede ser corta y facil de recordar: minimo 8 caracteres, sin espacios, y no la de ejemplo.
function ClaveValida(const Clave: String): Boolean;
begin
  Result := (Length(Clave) >= 8) and (Pos(' ', Clave) = 0) and (Clave <> ClavePlaceholder);
end;

// Codigo maestro: exactamente 8 digitos. Solo sirve para quitar productos de un pedido desde el celular.
function CodigoMaestroValido(const Codigo: String): Boolean;
var
  I: Integer;
begin
  Result := Length(Codigo) = 8;
  if not Result then Exit;
  for I := 1 to 8 do
    if (Codigo[I] < '0') or (Codigo[I] > '9') then
    begin
      Result := False;
      Exit;
    end;
end;

// Sal aleatoria (32 hex) a partir de un GUID de Windows.
function NuevaSal: String;
var
  TipoLib: Variant;
  Texto: String;
begin
  Result := '';
  // Variable local: pasar Result como parametro por referencia provoca "Access violation" en Inno Setup.
  try
    TipoLib := CreateOleObject('Scriptlet.TypeLib');
    Texto := TipoLib.Guid;
    StringChangeEx(Texto, '{', '', True);
    StringChangeEx(Texto, '}', '', True);
    StringChangeEx(Texto, '-', '', True);
    Result := Lowercase(Texto);
  except
    Result := '';
  end;
  if Length(Result) <> 32 then
    Result := Lowercase(GetSHA256OfString(GetDateTimeString('yyyymmddhhnnss', #0, #0) + ExpandConstant('{computername}{username}')));
end;

// En la configuracion solo queda sal + SHA-256(sal + codigo); el codigo en claro no se guarda.
procedure GuardarCodigoMaestro(const Codigo: String);
var
  Doc: Variant;
  Sal: String;
begin
  Sal := NuevaSal;
  if NuevoDocumento(Doc) then
  begin
    EscribirAjuste(Doc, 'MasterCodeSalt', Sal);
    EscribirAjuste(Doc, 'MasterCodeHash', Lowercase(GetSHA256OfString(Sal + Codigo)));
    Doc.save(RutaConfig);
  end
  else
    MsgBox('No se pudo editar ' + RutaConfig + '. Ejecute "Cambiar codigo maestro" del menu Inicio.', mbError, MB_OK);
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
  ClaveParam: String;
begin
  Result := True;
  // Instalacion silenciosa: una /ApiKey=... invalida se rechaza de entrada (en el asistente se valida en su pagina).
  ClaveParam := ExpandConstant('{param:ApiKey|}');
  if (ClaveParam <> '') and not ClaveValida(ClaveParam) then
  begin
    SuppressibleMsgBox('La clave indicada en /ApiKey debe tener al menos 8 caracteres, sin espacios, y no puede ser ' +
                       'la de ejemplo.', mbError, MB_OK, IDOK);
    Result := False;
    Exit;
  end;
  // Igual con /MasterCode=12345678 en una instalacion silenciosa.
  ClaveParam := ExpandConstant('{param:MasterCode|}');
  if (ClaveParam <> '') and not CodigoMaestroValido(ClaveParam) then
  begin
    SuppressibleMsgBox('El codigo indicado en /MasterCode debe tener exactamente 8 digitos.', mbError, MB_OK, IDOK);
    Result := False;
    Exit;
  end;
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

procedure GenerarClaveClick(Sender: TObject);
begin
  ApiPage.Values[0] := NuevaClave;
end;

procedure InitializeWizard;
var
  Boton: TNewButton;
begin
  ApiPage := CreateInputQueryPage(wpSelectTasks, 'API movil',
    'Clave y puerto con los que se conectaran los celulares de los meseros',
    'La clave se escribe una vez en cada celular ("Configurar servidor"). Deje la clave aleatoria que se propone ' +
    '(facil de teclear: sin letras ni numeros que se confundan) o escriba la suya: una clave corta y facil de ' +
    'recordar ahorra trabajo cuando hay varios celulares. Se puede cambiar despues con "Cambiar clave de la API" ' +
    'del menu Inicio.');
  ApiPage.Add('Clave de la API (minimo 8 caracteres, sin espacios):', False);
  ApiPage.Add('Puerto:', False);

  Boton := TNewButton.Create(ApiPage);
  Boton.Parent := ApiPage.Surface;
  Boton.Caption := 'Generar otra clave aleatoria';
  Boton.Width := ScaleX(200);
  Boton.Height := ScaleY(25);
  Boton.Left := 0;
  Boton.Top := ApiPage.Edits[1].Top + ApiPage.Edits[1].Height + ScaleY(14);
  Boton.OnClick := @GenerarClaveClick;

  MasterPage := CreateInputQueryPage(ApiPage.ID, 'Codigo maestro',
    'Codigo de 8 digitos para quitar productos de un pedido',
    'Es unico para todo el sistema y se pide en el celular, junto con una observacion, cada vez que se quita un ' +
    'producto de un pedido. No reemplaza el PIN de 4 digitos de cada empleado. Se guarda cifrado (no en claro). ' +
    'Se puede cambiar despues con "Cambiar codigo maestro" del menu Inicio. Si ya hay uno configurado, deje los ' +
    'dos campos vacios para conservarlo.');
  MasterPage.Add('Codigo maestro (8 digitos):', True);
  MasterPage.Add('Repita el codigo:', True);

  ApiPageLista := False;
  AlegraPage :=CreateInputQueryPage(ApiPage.ID, 'Conexion con Alegra',
    'Credenciales API de la cuenta del restaurante',
    'Se guardan como ALEGRA_EMAIL y ALEGRA_API_KEY en el entorno del sistema. ' +
    'Este paso no emite facturas ni verifica permisos. Si no selecciona esta tarea, conserva las credenciales existentes.');
  AlegraPage.Add('Correo de la cuenta API:', False);
  AlegraPage.Add('Token / API key:', True);
  AlegraPage.Add('Autenticacion: Bearer (token recomendado) o Basic (anterior):', False);
  AlegraPage.Values[2] := 'Bearer';
  AlegraEmissionPage := CreateInputQueryPage(AlegraPage.ID, 'Emision electronica en Alegra',
    'Credencial para emitir y consultar el resultado fiscal',
    'Para Casa Grande: use el token anterior con Basic. El token nuevo con Bearer se conserva para crear facturas y consultar catalogos. No se envian facturas durante la instalacion.');
  AlegraEmissionPage.Add('Correo de la cuenta API de emision:', False);
  AlegraEmissionPage.Add('Token de emision (anterior):', True);
  AlegraEmissionPage.Add('Autenticacion de emision (Basic o Bearer):', False);
  AlegraEmissionPage.Values[2] := 'Basic';
end;

function ShouldSkipPage(PageID: Integer): Boolean;
begin
  Result := (PageID = ApiPage.ID) and not WizardIsTaskSelected('api');
  if (PageID = AlegraPage.ID) or (PageID = AlegraEmissionPage.ID) then Result := not WizardIsTaskSelected('alegra');
end;

function PrepareToInstall(var NeedsRestart: Boolean): String;
begin
  Result := '';
  if WizardIsTaskSelected('alegra') then
  begin
    if (Trim(AlegraPage.Values[0]) = '') or (Trim(AlegraPage.Values[1]) = '') then
      Result := 'Para configurar Alegra, ingrese el correo y el token en el asistente interactivo.';
    if Pos(':', AlegraPage.Values[0]) > 0 then
      Result := 'El correo de Alegra no puede contener dos puntos.';
    if (CompareText(Trim(AlegraPage.Values[2]), 'Bearer') <> 0) and
       (CompareText(Trim(AlegraPage.Values[2]), 'Basic') <> 0) then
      Result := 'La autenticacion de Alegra debe ser Bearer o Basic.';
    if (Trim(AlegraEmissionPage.Values[0]) = '') or (Trim(AlegraEmissionPage.Values[1]) = '') then
      Result := 'Ingrese el correo y token de emision en la segunda pagina de Alegra.';
    if Pos(':', AlegraEmissionPage.Values[0]) > 0 then
      Result := 'El correo de emision no puede contener dos puntos.';
    if (CompareText(Trim(AlegraEmissionPage.Values[2]), 'Bearer') <> 0) and
       (CompareText(Trim(AlegraEmissionPage.Values[2]), 'Basic') <> 0) then
      Result := 'La autenticacion de emision debe ser Bearer o Basic.';
  end;
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
  if (CurPageID = AlegraEmissionPage.ID) and (AlegraEmissionPage.Values[0] = '') then
    AlegraEmissionPage.Values[0] := AlegraPage.Values[0];
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
  if CurPageID = MasterPage.ID then
  begin
    if (MasterPage.Values[0] = '') and (MasterPage.Values[1] = '') and (LeerAjuste('MasterCodeHash') <> '') then
      MasterCodeElegido := ''   // actualizacion: se conserva el codigo que ya tenia el restaurante
    else if not CodigoMaestroValido(MasterPage.Values[0]) then
    begin
      MsgBox('El codigo maestro debe tener exactamente 8 digitos (solo numeros).', mbError, MB_OK);
      Result := False;
    end
    else if MasterPage.Values[0] <> MasterPage.Values[1] then
    begin
      MsgBox('Los dos codigos no coinciden.', mbError, MB_OK);
      Result := False;
    end
    else
      MasterCodeElegido := MasterPage.Values[0];
    Exit;
  end;
  if CurPageID <> ApiPage.ID then Exit;
  Clave := Trim(ApiPage.Values[0]);
  Puerto := StrToIntDef(Trim(ApiPage.Values[1]), 0);
  if not ClaveValida(Clave) then
  begin
    MsgBox('La clave debe tener al menos 8 caracteres, sin espacios, y no puede ser la de ejemplo.', mbError, MB_OK);
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
  if (CurStep = ssPostInstall) and WizardIsTaskSelected('alegra') then
  begin
    // No registrar ni pasar el token por linea de comandos. No se borra al actualizar o desinstalar.
    if not RegWriteStringValue(HKLM, 'SYSTEM\CurrentControlSet\Control\Session Manager\Environment',
      'ALEGRA_EMAIL', Trim(AlegraPage.Values[0])) then
      RaiseException('No se pudo guardar ALEGRA_EMAIL en el entorno del sistema.');
    if not RegWriteStringValue(HKLM, 'SYSTEM\CurrentControlSet\Control\Session Manager\Environment',
      'ALEGRA_API_KEY', Trim(AlegraPage.Values[1])) then
      RaiseException('No se pudo guardar ALEGRA_API_KEY en el entorno del sistema.');
    if not RegWriteStringValue(HKLM, 'SYSTEM\CurrentControlSet\Control\Session Manager\Environment',
      'ALEGRA_AUTH_SCHEME', Trim(AlegraPage.Values[2])) then
      RaiseException('No se pudo guardar ALEGRA_AUTH_SCHEME en el entorno del sistema.');
    if not RegWriteStringValue(HKLM, 'SYSTEM\CurrentControlSet\Control\Session Manager\Environment',
      'ALEGRA_EMISSION_EMAIL', Trim(AlegraEmissionPage.Values[0])) then
      RaiseException('No se pudo guardar ALEGRA_EMISSION_EMAIL.');
    if not RegWriteStringValue(HKLM, 'SYSTEM\CurrentControlSet\Control\Session Manager\Environment',
      'ALEGRA_EMISSION_API_KEY', Trim(AlegraEmissionPage.Values[1])) then
      RaiseException('No se pudo guardar ALEGRA_EMISSION_API_KEY.');
    if not RegWriteStringValue(HKLM, 'SYSTEM\CurrentControlSet\Control\Session Manager\Environment',
      'ALEGRA_EMISSION_AUTH_SCHEME', Trim(AlegraEmissionPage.Values[2])) then
      RaiseException('No se pudo guardar ALEGRA_EMISSION_AUTH_SCHEME.');
  end;
  if CurStep = ssPostInstall then
  begin
    // Instalacion silenciosa: la pagina no se mostro, se usa /MasterCode (sin el, queda lo que hubiera).
    if MasterCodeElegido = '' then MasterCodeElegido := ExpandConstant('{param:MasterCode|}');
    if MasterCodeElegido <> '' then GuardarCodigoMaestro(MasterCodeElegido);
  end;
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
