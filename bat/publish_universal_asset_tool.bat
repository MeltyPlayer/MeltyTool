@echo off 
setlocal EnableDelayedExpansion

SET /A askFirst = 1
FOR %%A IN (%*) DO (
  IF "%%A"=="/f" (
  	set /A askFirst = 0
  )
)

IF %askFirst%==1 (
  echo This will rebuild Universal Asset Tool via Visual Studio. Are you sure you wish to proceed?
  pause
)

set universalAssetToolBasePath=%~dp0%..\cli\tools\universal_asset_tool\
set projectPath=%~dp0%..\FinModelUtility\UniversalAssetTool\UniversalAssetTool.Ui
set publishPath=%projectPath%\bin\Release\net10.0-windows\win-x64\publish

rem Prefer the per-user SDK installed by dotnet-install. A runtime-only
rem installation may appear earlier on PATH and cannot run `dotnet publish`.
set dotnetCommand=dotnet
if exist "%USERPROFILE%\.dotnet\dotnet.exe" (
  set dotnetCommand=%USERPROFILE%\.dotnet\dotnet.exe
)

echo Building new universal asset tool...
pushd "%projectPath%"

"%dotnetCommand%" publish -c Release
if errorlevel 1 (
  echo ERROR: Universal Asset Tool build failed. Existing local files were not replaced.
  popd
  exit /b 1
)

popd

rem Some SDK versions leave native Content files beside the RID build output
rem instead of copying them into the single-file publish directory.
if not exist "%publishPath%\glfw3.dll" if exist "%publishPath%\..\glfw3.dll" copy /y "%publishPath%\..\glfw3.dll" "%publishPath%\glfw3.dll" >nul
if not exist "%publishPath%\OpenAL32.dll" if exist "%publishPath%\..\OpenAL32.dll" copy /y "%publishPath%\..\OpenAL32.dll" "%publishPath%\OpenAL32.dll" >nul

if not exist "%publishPath%\universal_asset_tool.exe" (
  echo ERROR: Publish succeeded but universal_asset_tool.exe was not found in:
  echo %publishPath%
  exit /b 1
)
if not exist "%publishPath%\glfw3.dll" (
  echo ERROR: Publish output is missing the required glfw3.dll native library.
  exit /b 1
)
if not exist "%publishPath%\OpenAL32.dll" (
  echo ERROR: Publish output is missing the required OpenAL32.dll native library.
  exit /b 1
)

rem Replacing a running single-file app can leave a mixture of old and new
rem files. Abort before touching the destination when the UI is open.
tasklist /FI "IMAGENAME eq universal_asset_tool.exe" 2>NUL | find /I "universal_asset_tool.exe" >NUL
if not errorlevel 1 (
  echo ERROR: Universal Asset Tool is still running. Close it and publish again.
  echo Existing local files were not replaced.
  exit /b 1
)

echo Copying new universal asset tool...
if not exist "%universalAssetToolBasePath%" mkdir "%universalAssetToolBasePath%"
del /q "%universalAssetToolBasePath%*"
xcopy /e /i /y "%publishPath%\*" "%universalAssetToolBasePath%\" >nul
if errorlevel 1 (
  echo ERROR: Failed to copy the published Universal Asset Tool.
  exit /b 1
)

echo Universal Asset Tool was published successfully.

pause
