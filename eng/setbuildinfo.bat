@echo off

setlocal

set Architecture=%1
set Config=%2

IF /I "%Architecture:~0,3%"=="ARM" (
    ECHO ARM
    ECHO ##vso[task.setvariable variable=TestParameter]
    ECHO ##vso[task.setvariable variable=RunTests]false
) ELSE (
    ECHO NOT ARM
    ECHO ##vso[task.setvariable variable=TestParameter]-test
    ECHO ##vso[task.setvariable variable=RunTests]true
)

if /I "%SYSTEM_TEAMPROJECT%" == "Public" (
    ECHO Public CI
    ECHO ##vso[task.setvariable variable=SignType]test
) ELSE (
    ECHO Not public CI
    ECHO ##vso[task.setvariable variable=SignType]real
)

if "%AdditionalBuildParameters%" == "" (
    REM Make sure the variable is set as empty in Azure pipelines, instead of not being set at all
    REM (which would cause the literal "$(AdditionalBuildParameters)" to be passed to the script)
    ECHO Setting AdditionalBuildParameters to empty
    ECHO ##vso[task.setvariable variable=AdditionalBuildParameters]
) ELSE (
    ECHO AdditionalBuildParameters is already set to: %AdditionalBuildParameters%
)

