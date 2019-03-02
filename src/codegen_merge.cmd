set CWD=%~dp0
cd Build\CodeGen\tools
C:\Tools\ILRepack.exe /wildcards /verbose /out:GoliathData_merge.exe GoliathData.exe Goliath.*.dll Microsoft*.dll Mono*.dll Npgsql.dll Razor*.dll System*.dll
MOVE GoliathData_merge.exe  %CWD%\Build\GoliathData.exe
cd %CWD%\Build
pause