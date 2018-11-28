@echo off

setlocal

set Architecture=%1

IF /I "%Architecture:~0,3%"=="ARM" (
    ECHO ARM
    ECHO ##vso[task.setvariable variable=TestParameter]
    ECHO ##vso[task.setvariable variable=RunTests]false
) ELSE (
    ECHO NOT ARM
    ECHO ##vso[task.setvariable variable=TestParameter]-test
    ECHO ##vso[task.setvariable variable=RunTests]true
)