@echo off
"%ProgramFiles(x86)%\MSBuild\14.0\Bin\msbuild" Goliath.Data.build /p:NuGetPath="C:\Tools\Nuget" /p:BuildTools="C:\Tools\BuildTools" %*
pause