@echo off
setlocal
set "scriptDir=%~dp0"
if exist "%scriptDir%OffscreenFixer.exe" (
	"%scriptDir%OffscreenFixer.exe"
	exit /b %errorlevel%
)

echo OffscreenFixer.exe was not found.
echo Build the executable from OffscreenFixer.cs or download the release asset.
exit /b 1
