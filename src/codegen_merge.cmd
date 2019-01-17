set CWD=%~dp0
cd Build\CodeGen\tools
C:\Tools\ILRepack.exe /wildcards /internalize /verbose /out:GoliathData_merge.exe GoliathData.exe *.dll
MOVE GoliathData_merge.exe  %CWD%\Build\GoliathData.exe
cd %CWD%\Build
pause