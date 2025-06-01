@echo off
title TwitchBot Launcher
echo ================================
echo     TwitchBot Voice Launcher
echo ================================
echo.
echo Starting TwitchBot...
echo Project location: %~dp0TwitchBot01
echo.

REM Change to the TwitchBot01 directory
cd /d "%~dp0TwitchBot01"

REM Check if the project exists
if not exist "TwitchBot01.csproj" (
    echo ERROR: TwitchBot01.csproj not found!
    echo Make sure this batch file is in the correct directory.
    pause
    exit /b 1
)

REM Check if appsettings.json exists
if not exist "appsettings.json" (
    echo ERROR: appsettings.json not found!
    echo Please copy appsettings.template.json to appsettings.json and configure your credentials.
    pause
    exit /b 1
)

echo Configuration found. Launching TwitchBot...
echo.
echo Press Ctrl+C to stop the bot, or close this window.
echo ================================
echo.

REM Run the TwitchBot
dotnet run

echo.
echo ================================
echo TwitchBot has stopped.
echo ================================
pause
