@echo off

if "%~1" == "" (goto noinput)

if exist "%~1" (goto validinput)

:doesntexist
echo Input CCI does not exist!
goto end

:noinput
echo No input CCI!
goto end

:validinput
ctrtool --romfsdir="%~2" "%~1"

:end
