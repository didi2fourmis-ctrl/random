@echo off
rem Double-clique pour compiler le mod et le copier dans ULTRAKILL\BepInEx\plugins
rem (change le chemin GamePath dans ChaosSwap.csproj si ton jeu est ailleurs)
dotnet build "%~dp0ChaosSwap.csproj" -c Release
if errorlevel 1 (echo ECHEC : lis les erreurs ci-dessus.) else (echo OK : mod copie dans BepInEx\plugins\ChaosSwap)
pause
