﻿; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define MyAppName       "PermaTop"
#define MyAppExeName    "PermaTop.exe"
#define MyAppVersion    GetFileVersion('..\PermaTop\bin\Release\net6.0-windows\PermaTop.exe')
#define MyAppPublisher  "Léo Corporation"
#define MyAppURL        "https://leocorporation.dev/"
;#define MyAppStartYear  "2021"
#define CurrentYear     GetDateTimeString('yyyy','','')

[Setup]
; NOTE: The value of AppId uniquely identifies this application. Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{BCDA56A9-7F5C-4FE2-AFE2-3FD50CBC509F}}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}

AppCopyright=© {#CurrentYear} {#MyAppPublisher}
AppPublisher={#MyAppPublisher}

AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}

VersionInfoDescription={#MyAppName} Installer
VersionInfoVersion={#MyAppVersion}
VersionInfoProductName={#MyAppName}

UninstallDisplayName={#MyAppName}
UninstallDisplayIcon={app}\{#MyAppExeName}

WizardStyle=modern
ShowLanguageDialog=yes
UsePreviousLanguage=no

DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName=Léo Corp
DisableProgramGroupPage=yes
LicenseFile=..\SETUP_LICENSE
; Uncomment the following line to run in non administrative install mode (install for current user only.)
;PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog
OutputDir=Output
OutputBaseFilename=PermaTopSetup
SetupIconFile=..\PermaTop\PermaTop.ico
Compression=lzma
SolidCompression=yes

[Languages]
Name: "english";           MessagesFile: "compiler:Default.isl"
Name: "french";            MessagesFile: "compiler:Languages\French.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "..\PermaTop\bin\Release\net6.0-windows\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\Xalyus Updater\bin\Release\net6.0-windows\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "..\PermaTop\bin\Release\net6.0-windows\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[Code]
function IsDotNetInstalled(DotNetName: string): Boolean;
var
  Cmd, Args: string;
  FileName: string;
  Output: AnsiString;
  Command: string;
  ResultCode: Integer;
begin
  FileName := ExpandConstant('{tmp}\dotnet.txt');
  Cmd := ExpandConstant('{cmd}');
  Command := 'dotnet --list-runtimes';
  Args := '/C ' + Command + ' > "' + FileName + '" 2>&1';
  if Exec(Cmd, Args, '', SW_HIDE, ewWaitUntilTerminated, ResultCode) and
     (ResultCode = 0) then
  begin
    if LoadStringFromFile(FileName, Output) then
    begin
      if Pos(DotNetName, Output) > 0 then
      begin
        Log('"' + DotNetName + '" found in output of "' + Command + '"');
        Result := True;
      end
        else
      begin
        Log('"' + DotNetName + '" not found in output of "' + Command + '"');
        Result := False;
      end;
    end
      else
    begin
      Log('Failed to read output of "' + Command + '"');
    end;
  end
    else
  begin
    Log('Failed to execute "' + Command + '"');
    Result := False;
  end;
  DeleteFile(FileName);
end;

function IsDotNetCore60RuntimeInstalled(): Boolean;
begin
    Result := IsDotNetInstalled('Microsoft.WindowsDesktop.App 6.0.');
end;

function InitializeSetup(): Boolean;
begin
    if not IsDotNetCore60RuntimeInstalled() then begin
        MsgBox('The required .NET Core 6.0 Windows Desktop Runtime is not installed on your system. Please install it and try again.', mbError, MB_OK);
        Result := False;
        Exit;
    end;

    Result := True;
end;