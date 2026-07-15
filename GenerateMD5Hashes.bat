@echo off
set curDir=%cd%
set hashesFile=md5hashes.txt
cd %1
echo Generated on %DATE% at %TIME% > %hashesFile%
for /f %%a in ('dir /b *') do (
  certUtil -hashfile %%a MD5 | findstr /v "CertUtil" >> %hashesFile%)
cd %curDir%