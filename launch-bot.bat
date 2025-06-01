@echo off
REM Quick launcher for TwitchBot - optimized for voice commands
title TwitchBot
cd /d "%~dp0TwitchBot01"
dotnet run --verbosity quiet
