@echo off
"%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe" Goliath.Data.build /p:NuGetPath="C:\Tools\Nuget" /p:BuildTools="C:\Tools\BuildTools" %*
pause