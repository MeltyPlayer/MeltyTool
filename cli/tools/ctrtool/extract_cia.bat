@echo off

if "%~1" == "" (goto noinput)

if exist "%~1" (goto validinput)

:doesntexist
echo Input CIA does not exist!
goto end

:noinput
echo No input CIA!
goto end

:validinput
ctrtool --contents=contents "%~1"
if not exist contents.0000.00000000 (goto ciaextractfailed)
rename contents.0000.00000000 nsmb2.cxi
ctrtool --romfsdir="%~2" nsmb2.cxi
for "%%f" in (contents.000*) do (del "%%f")
goto end

:ciaextractfailed
echo Extracting CIA failed!
goto end

:end
