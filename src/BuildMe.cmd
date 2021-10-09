@echo off
set PkgFolder=%~dp0Build
set version=%1
set build=%2
set apiKey=%3
if [%version%]==[] set version="2.1.0"
if [%build%]==[] set build="1"
dotnet publish CodeGenerator/CodeGenerator.csproj -p:PublishProfile=FolderProfile -p:PackageVersion=%version%.%build% -p:Version=%version%.%build% -p:AssemblyVersion=%version%.%build% -p:FileVersion=%version%.%build%
IF NOT EXIST %PkgFolder% mkdir %PkgFolder%

xcopy %~dp0CodeGenerator\bin\Release\publish %PkgFolder%\GoliathData\tools\net5.0\any  /y /f /i /e
REM xcopy %~dp0DotnetToolSettings.xml %PkgFolder%\GoliathData\tools\netcoreapp3.1\any  /y /f /i
pause