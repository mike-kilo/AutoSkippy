@echo off
IF [%1]==[] GOTO :EOF
IF [%2]==[] GOTO :EOF
IF NOT EXIST %1\NUL GOTO :EOF
IF NOT EXIST %1\%2.exe GOTO :EOF

set signtool="C:\Program Files (x86)\Windows Kits\10\bin\10.0.26100.0\x64\signtool.exe"
set curDir=%cd%
set hashesFile=md5hashes.txt

cd %1

@echo on
%signtool% sign /fd SHA256 /n "MiKoSoft" ".\%2.exe"
rem Allow signing to settle down by waiting 5 seconds
rem TIMEOUT /T 5 /NOBREAK > NUL
rem Timeout tries to redirect the input, which is not supported
rem use this silly action to silently simulate 5 seconds wait
ping 127.0.0.1 -n 5 -w 1000 >NUL
%signtool% timestamp /force /tr http://timestamp.digicert.com /td SHA256 ".\%2.exe"
%signtool% verify /pa ".\%2.exe"

cd %curDir%