@echo off

REM Copyright (c) .NET Foundation and contributors. All rights reserved.
REM Licensed under the MIT license. See LICENSE file in the project root for full license information.

powershell -ExecutionPolicy Bypass -NoProfile -NoLogo -Command "& \"%~dp0common\build.ps1\" -restore -build /p:Architecture=x64 -pack %*; exit $LastExitCode;"
if %errorlevel% neq 0 exit /b %errorlevel%
powershell -ExecutionPolicy Bypass -NoProfile -NoLogo -Command "& \"%~dp0common\build.ps1\" -restore -build /p:Architecture=x86 -pack %*; exit $LastExitCode;"
if %errorlevel% neq 0 exit /b %errorlevel%
powershell -ExecutionPolicy Bypass -NoProfile -NoLogo -Command "& \"%~dp0common\build.ps1\" -restore -build /p:UseLocalNetCoreInstallerMSIs=true /p:Projects=\"%~dp0..\src\NetCoreInstaller\NetCoreInstaller.csproj\" %*; exit $LastExitCode;"
if %errorlevel% neq 0 exit /b %errorlevel%
