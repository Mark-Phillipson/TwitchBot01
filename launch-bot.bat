@echo off
REM Quick launcher for TwitchBot - optimized for voice commands
title TwitchBot
start "" /D "%~dp0TwitchBot01" dotnet run --project "%~dp0TwitchBot01\TwitchBot01.csproj"
