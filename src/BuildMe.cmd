@echo off
"%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\MSBuild.exe" Goliath.Data.build /p:NuGetPath="C:\Tools\Nuget" /p:BuildTools="C:\Tools\BuildTools" %*
pause