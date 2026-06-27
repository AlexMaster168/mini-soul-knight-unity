@echo off
reg add "HKCR\Directory\shell\OpenInUnity" /ve /d "Открыть в Unity" /f >nul 2>&1
reg add "HKCR\Directory\shell\OpenInUnity\command" /ve /d "\"C:\Program Files\Unity\Hub\Editor\6000.0.77f1\Editor\Unity.exe\" -projectPath \"%%1\"" /f >nul 2>&1
echo Готово! Теперь правый клик по папке проекта = "Открыть в Unity"
pause