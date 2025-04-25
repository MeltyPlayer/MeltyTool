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

echo Deleting old universal asset tool...
del /q "!universalAssetToolBasePath!*"

echo Building new universal asset tool...
cd ../
cd "FinModelUtility\UniversalAssetTool\UniversalAssetTool.Ui\"

dotnet publish -c Release

echo Copying new universal asset tool...
move "bin\Release\net10.0-windows\win-x64\publish\*" "!universalAssetToolBasePath!"

pause