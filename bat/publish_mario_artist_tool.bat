@echo off 
setlocal EnableDelayedExpansion

echo This will rebuild Mario Artist Tool via Visual Studio. Are you sure you wish to proceed?

pause

set marioArtistToolBasePath=%~dp0%..\cli\tools\mario_artist_tool\

echo Deleting old Mario Artist Tool...
del /q "!marioArtistToolBasePath!*"

echo Building new Mario Artist Tool...
cd ../
cd "FinModelUtility\MarioArtistTool\MarioArtistTool.Desktop"

dotnet publish -c Release

echo Copying new universal asset tool...
move "bin\Release\net10.0\win-x64\publish\*" "!marioArtistToolBasePath!"

pause