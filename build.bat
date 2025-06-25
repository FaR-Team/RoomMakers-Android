@echo off
setlocal

set UNITY_PATH="D:\Unity\2021.3.45f1\Editor\Unity.exe"
set PROJECT_PATH=%~dp0

echo ================================
echo Unity Build Automation Script
echo ================================
echo.

if "%1"=="" (
    echo Usage: build.bat [webgl-dev^|webgl-release^|android-dev^|android-release]
    echo.
    echo Available commands:
    echo   webgl-dev      - Build WebGL Development
    echo   webgl-release  - Build WebGL Release
    echo   android-dev    - Build Android Development
    echo   android-release- Build Android Release
    echo.
    goto :end
)

set BUILD_METHOD=""
set BUILD_NAME=""

if "%1"=="webgl-dev" (
    set BUILD_METHOD=BuildAutomation.BuildWebGLDevelopment
    set BUILD_NAME=WebGL Development
)
if "%1"=="webgl-release" (
    set BUILD_METHOD=BuildAutomation.BuildWebGLRelease
    set BUILD_NAME=WebGL Release
)
if "%1"=="android-dev" (
    set BUILD_METHOD=BuildAutomation.BuildAndroidDevelopment
    set BUILD_NAME=Android Development
)
if "%1"=="android-release" (
    set BUILD_METHOD=BuildAutomation.BuildAndroidRelease
    set BUILD_NAME=Android Release
)

if %BUILD_METHOD%=="" (
    echo Error: Unknown build type "%1"
    goto :end
)

echo Building %BUILD_NAME%...
echo.

%UNITY_PATH% -batchmode -quit -projectPath "%PROJECT_PATH%" -executeMethod %BUILD_METHOD% -logFile build.log

if %ERRORLEVEL%==0 (
    echo.
    echo ================================
    echo Build completed successfully!
    echo ================================
) else (
    echo.
    echo ================================
    echo Build failed! Check build.log
    echo ================================
)

:end
pause