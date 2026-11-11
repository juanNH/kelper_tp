#define AppName "Kepler-7"
#define AppExe "Kepler7.Game.exe"
#define AppVersion GetVersionNumbersString("publish\" + AppExe)
#define SqlConfig "Kepler7.Persistencia.Sql.dll.config"

[Setup]
AppId={{5256512A-CFCE-4352-93A4-5B94777B4828}
AppName={#AppName}
AppVersion={#AppVersion}
DefaultDirName={localappdata}\Programs\{#AppName}
DefaultGroupName={#AppName}
DisableProgramGroupPage=yes
UninstallDisplayIcon={app}\{#AppExe}
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
MinVersion=10.0
OutputDir=.
OutputBaseFilename=Kepler7-Setup-x64
WizardStyle=modern
Compression=lzma2
SolidCompression=yes

[Languages]
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"

[Files]
Source: "Preparar-Base.ps1"; Flags: dontcopy
Source: "publish\database\*.sql"; DestDir: "{app}\database"; Flags: ignoreversion
Source: "publish\*"; DestDir: "{app}"; Excludes: "\database\*,{#SqlConfig}"; Flags: ignoreversion recursesubdirs
Source: "{tmp}\{#SqlConfig}"; DestDir: "{app}"; ExternalSize: 1024; Flags: external ignoreversion

[Icons]
Name: "{autoprograms}\{#AppName}"; Filename: "{app}\{#AppExe}"; WorkingDir: "{app}"

[Run]
Filename: "{app}\{#AppExe}"; WorkingDir: "{app}"; Description: "Abrir {#AppName}"; Flags: nowait postinstall skipifsilent

[Code]
var
  PaginaSql: TWizardPage;
  InstanciaSql: TNewComboBox;
  PrepararBase: TNewCheckBox;

procedure AgregarInstanciasLocales(Raiz: Integer);
var
  Instancias: TArrayOfString;
  Instancia: String;
  I: Integer;
begin
  if not RegGetValueNames(Raiz,
    'SOFTWARE\Microsoft\Microsoft SQL Server\Instance Names\SQL', Instancias) then
    Exit;

  for I := 0 to GetArrayLength(Instancias) - 1 do
  begin
    if CompareText(Instancias[I], 'MSSQLSERVER') = 0 then
      Instancia := '.'
    else
      Instancia := '.\' + Instancias[I];

    if InstanciaSql.Items.IndexOf(Instancia) = -1 then
    begin
      if Instancia = '.' then
        InstanciaSql.Items.Insert(0, Instancia)
      else
        InstanciaSql.Items.Add(Instancia);
    end;
  end;
end;

procedure InitializeWizard;
var
  Etiqueta: TNewStaticText;
  InstanciaInicial: String;
begin
  PaginaSql := CreateCustomPage(wpSelectDir, 'Base de datos',
    'Elegí la instancia de SQL Server que usará Kepler-7.');

  Etiqueta := TNewStaticText.Create(PaginaSql);
  Etiqueta.Parent := PaginaSql.Surface;
  Etiqueta.Caption := '&Servidor o instancia de SQL Server';

  InstanciaSql := TNewComboBox.Create(PaginaSql);
  InstanciaSql.Parent := PaginaSql.Surface;
  InstanciaSql.Top := Etiqueta.Top + Etiqueta.Height + ScaleY(6);
  InstanciaSql.Width := PaginaSql.SurfaceWidth;
  InstanciaSql.Style := csDropDown;
  Etiqueta.FocusControl := InstanciaSql;

  AgregarInstanciasLocales(HKLM64);
  AgregarInstanciasLocales(HKLM32);
  if InstanciaSql.Items.Count = 0 then
    InstanciaSql.Items.Add('.');

  InstanciaInicial := ExpandConstant('{param:SQLSERVER|}');
  if InstanciaInicial = '' then
    InstanciaInicial := GetPreviousData('InstanciaSql', InstanciaSql.Items[0]);
  InstanciaSql.Text := InstanciaInicial;

  Etiqueta := TNewStaticText.Create(PaginaSql);
  Etiqueta.Parent := PaginaSql.Surface;
  Etiqueta.Top := InstanciaSql.Top + InstanciaSql.Height + ScaleY(8);
  Etiqueta.AutoSize := False;
  Etiqueta.Width := PaginaSql.SurfaceWidth;
  Etiqueta.WordWrap := True;
  Etiqueta.Caption :=
    'Seleccioná una instancia local o escribí una dirección como SERVIDOR\INSTANCIA ' +
    'o tcp:servidor,1433. La conexión usa tu cuenta de Windows.';
  Etiqueta.AdjustHeight;

  PrepararBase := TNewCheckBox.Create(PaginaSql);
  PrepararBase.Parent := PaginaSql.Surface;
  PrepararBase.Top := Etiqueta.Top + Etiqueta.Height + ScaleY(20);
  PrepararBase.Width := PaginaSql.SurfaceWidth;
  PrepararBase.Caption := '&Crear o actualizar la base Kepler7 en esta instancia';
  PrepararBase.Checked := ExpandConstant('{param:PREPARARBASE|0}') = '1';

  Etiqueta := TNewStaticText.Create(PaginaSql);
  Etiqueta.Parent := PaginaSql.Surface;
  Etiqueta.Top := PrepararBase.Top + PrepararBase.Height + ScaleY(6);
  Etiqueta.AutoSize := False;
  Etiqueta.Width := PaginaSql.SurfaceWidth;
  Etiqueta.WordWrap := True;
  Etiqueta.Caption :=
    'Marcá esta opción si la base necesita preparación y tu cuenta tiene permisos ' +
    'para crear la base o sus tablas.';
  Etiqueta.AdjustHeight;
end;

function NextButtonClick(CurPageID: Integer): Boolean;
begin
  Result := True;
  if CurPageID = PaginaSql.ID then
  begin
    InstanciaSql.Text := Trim(InstanciaSql.Text);
    Result := InstanciaSql.Text <> '';
    if not Result then
      SuppressibleMsgBox('Ingresá el nombre o la dirección de una instancia de SQL Server.',
        mbError, MB_OK, IDOK);
  end;
end;

procedure RegisterPreviousData(PreviousDataKey: Integer);
begin
  SetPreviousData(PreviousDataKey, 'InstanciaSql', InstanciaSql.Text);
end;

function GuardarConfiguracionSql: Boolean;
var
  Cadena: String;
  Lineas: TArrayOfString;
begin
  Cadena := InstanciaSql.Text;
  StringChangeEx(Cadena, '"', '""', True);
  Cadena := 'Server="' + Cadena + '";Database=Kepler7;Integrated Security=True;';
  { LocalDB usa canales locales y no admite TLS. }
  if CompareText(Copy(InstanciaSql.Text, 1, 10), '(localdb)\') = 0 then
    Cadena := Cadena + 'Encrypt=False'
  else
    Cadena := Cadena + 'Encrypt=True;TrustServerCertificate=True';

  StringChangeEx(Cadena, '&', '&amp;', True);
  StringChangeEx(Cadena, '<', '&lt;', True);
  StringChangeEx(Cadena, '>', '&gt;', True);
  StringChangeEx(Cadena, '"', '&quot;', True);

  SetArrayLength(Lineas, 6);
  Lineas[0] := '<?xml version="1.0" encoding="utf-8"?>';
  Lineas[1] := '<configuration>';
  Lineas[2] := '  <connectionStrings>';
  Lineas[3] := '    <add name="Kepler7" providerName="Microsoft.Data.SqlClient" connectionString="' +
    Cadena + '" />';
  Lineas[4] := '  </connectionStrings>';
  Lineas[5] := '</configuration>';
  Result := SaveStringsToUTF8File(ExpandConstant('{tmp}\{#SqlConfig}'), Lineas, False);
end;

function PrepararBaseSql: String;
var
  Codigo: Integer;
  Salida: TExecOutput;
  PaginaProgreso: TOutputMarqueeProgressWizardPage;
begin
  Result := '';
  PaginaProgreso := CreateOutputMarqueeProgressPage('Base de datos',
    'Preparando Kepler7 en ' + InstanciaSql.Text + '.');
  PaginaProgreso.Show;
  try
    PaginaProgreso.Animate;
    ExtractTemporaryFile('Preparar-Base.ps1');
    ExtractTemporaryFile('001_CrearBase.sql');
    ExtractTemporaryFile('002_Partidas.sql');
    ExtractTemporaryFile('003_Progreso.sql');

    if not ExecAndCaptureOutput(
      ExpandConstant('{sys}\WindowsPowerShell\v1.0\powershell.exe'),
      '-NoProfile -NonInteractive -ExecutionPolicy Bypass -File "' +
        ExpandConstant('{tmp}\Preparar-Base.ps1') + '"',
      ExpandConstant('{tmp}'), SW_HIDE, ewWaitUntilTerminated, Codigo, Salida) then
      Result := SysErrorMessage(Codigo)
    else if Codigo <> 0 then
    begin
      Result := 'Código de salida: ' + IntToStr(Codigo);
      if GetArrayLength(Salida.StdErr) > 0 then
        Result := Salida.StdErr[0];
    end;
  finally
    PaginaProgreso.Hide;
  end;
end;

function PrepareToInstall(var NeedsRestart: Boolean): String;
begin
  Result := '';
  if not GuardarConfiguracionSql then
  begin
    Result := 'No se pudo guardar la configuración de SQL Server.';
    Exit;
  end;

  if PrepararBase.Checked then
  begin
    Result := PrepararBaseSql;
    if Result <> '' then
      Result := 'No se pudo preparar la base Kepler7 en ' + InstanciaSql.Text + '.' + #13#10 +
        Result;
  end;
end;
