@echo off
rem Double-clique sur ce fichier pour compiler TOUS les mods de ce dossier et les copier dans le jeu.
rem (change le chemin GamePath dans Directory.Build.props si ton jeu est ailleurs)
set "RACINE=%~dp0"
set ERREUR=0
for /d %%D in ("%RACINE%*") do if exist "%%D\%%~nxD.csproj" (
  echo.
  echo ===== Compilation de %%~nxD =====
  dotnet build "%%D\%%~nxD.csproj" -c Release
  if errorlevel 1 set ERREUR=1
)
echo.
if "%ERREUR%"=="1" (echo ECHEC : lis les messages d erreur ci-dessus.) else (echo OK : les mods ont ete copies dans ULTRAKILL\BepInEx\plugins)
pause
