@echo off
title Soul Knight

:: Kill Unity Hub so it doesn't intercept
taskkill /f /im "Unity Hub.exe" >nul 2>&1
timeout /t 2 /nobreak >nul

:: Launch Unity Editor directly with project AND scene
start "" "C:\Program Files\Unity\Hub\Editor\6000.0.77f1\Editor\Unity.exe" -projectPath "C:\Projects\soul-knight" -executeMethod PlayModeLauncher.EnterPlayMode