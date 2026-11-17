@echo off
setlocal

set askFirst=1
for %%A in (%*) do if "%%A"=="/f" set askFirst=0
if %askFirst%==1 (
  echo This builds and locally publishes Universal Asset Tool with animated FBX support.
  pause
)

set repoRoot=%~dp0%..\..\..\
set patchedAssimpPath=%repoRoot%scratch\assimp-upstream
set converterProject=%repoRoot%Tools\MeltyTool\native\AssimpMorphFbxConverter
set assimpPatch=%converterProject%\assimp-fbx-morph-animation.patch
set converterBuild=%repoRoot%scratch\assimp-morph-fbx-converter-build
set projectPath=%~dp0%..\FinModelUtility\UniversalAssetTool\UniversalAssetTool.Ui
set publishPath=%projectPath%\bin\Release\net10.0-windows\win-x64\publish
set destination=%~dp0%..\cli\tools\universal_asset_tool\

set dotnetCommand=dotnet
if exist "%USERPROFILE%\.dotnet\dotnet.exe" set dotnetCommand=%USERPROFILE%\.dotnet\dotnet.exe

if not exist "%patchedAssimpPath%\CMakeLists.txt" (
  echo Downloading Assimp source...
  git clone https://github.com/assimp/assimp.git "%patchedAssimpPath%"
  if errorlevel 1 exit /b 1
  git -C "%patchedAssimpPath%" checkout f1de01d68eb224cf9f97ac3a3b1cad9aa92ec7ac
  if errorlevel 1 exit /b 1
)

git -C "%patchedAssimpPath%" apply --reverse --check --ignore-space-change "%assimpPatch%" >nul 2>nul
if errorlevel 1 (
  echo Applying MeltyTool FBX morph-animation patch...
  git -C "%patchedAssimpPath%" apply --check --ignore-space-change "%assimpPatch%"
  if errorlevel 1 (
    echo ERROR: Assimp source has incompatible local changes.
    exit /b 1
  )
  git -C "%patchedAssimpPath%" apply --ignore-space-change "%assimpPatch%"
  if errorlevel 1 exit /b 1
)

tasklist /FI "IMAGENAME eq universal_asset_tool.exe" 2>NUL | find /I "universal_asset_tool.exe" >NUL
if not errorlevel 1 (
  echo ERROR: Universal Asset Tool is running. Close it and publish again.
  exit /b 1
)

echo Building bundled animated FBX converter...
cmake -S "%converterProject%" -B "%converterBuild%" -G "Visual Studio 18 2026" -A x64 -DASSIMP_SOURCE_DIR="%patchedAssimpPath%"
if errorlevel 1 exit /b 1
cmake --build "%converterBuild%" --config Release --target assimp_morph_fbx_converter --parallel
if errorlevel 1 exit /b 1

set converterExe=%converterBuild%\Release\assimp_morph_fbx_converter.exe
set converterDll=%converterBuild%\assimp\bin\Release\assimp-vc145-mt.dll
if not exist "%converterExe%" (
  echo ERROR: Animated FBX converter executable was not built.
  exit /b 1
)
if not exist "%converterDll%" (
  echo ERROR: Patched Assimp runtime was not built.
  exit /b 1
)

echo Publishing Universal Asset Tool...
"%dotnetCommand%" publish "%projectPath%\UniversalAssetTool.Ui.csproj" -c Release -p:EnablePatchedAssimpMorphFbx=true
if errorlevel 1 (
  echo ERROR: Universal Asset Tool build failed. Existing files were not replaced.
  exit /b 1
)

if not exist "%publishPath%\glfw3.dll" if exist "%publishPath%\..\glfw3.dll" copy /y "%publishPath%\..\glfw3.dll" "%publishPath%\glfw3.dll" >nul
if not exist "%publishPath%\OpenAL32.dll" if exist "%publishPath%\..\OpenAL32.dll" copy /y "%publishPath%\..\OpenAL32.dll" "%publishPath%\OpenAL32.dll" >nul
copy /y "%converterExe%" "%publishPath%\assimp_morph_fbx_converter.exe" >nul
copy /y "%converterDll%" "%publishPath%\assimp-vc145-mt.dll" >nul

if not exist "%publishPath%\universal_asset_tool.exe" exit /b 1
if not exist "%publishPath%\assimp_morph_fbx_converter.exe" exit /b 1
if not exist "%publishPath%\assimp-vc145-mt.dll" exit /b 1
if not exist "%publishPath%\glfw3.dll" exit /b 1
if not exist "%publishPath%\OpenAL32.dll" exit /b 1

echo Installing the updated tool locally...
if not exist "%destination%" mkdir "%destination%"
del /q "%destination%*"
xcopy /e /i /y "%publishPath%\*" "%destination%\" >nul
if errorlevel 1 exit /b 1

echo Universal Asset Tool with animated FBX export was published successfully.
pause
