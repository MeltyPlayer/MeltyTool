@echo off 
echo This will delete all file hierarchy caches. Are you sure you wish to proceed?

pause

cd ../roms/

del /s /q hierarchy.cache

pause