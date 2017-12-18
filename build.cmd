setlocal
set PATH=%PATH%;C:\Program Files (x86)\Microsoft Visual Studio\2017\BuildTools\MSBuild\15.0\bin\amd64
set HERE=%~dp0

set CONFIG=%1
if "%CONFIG%" == "" set CONFIG=Release

%HERE%.paket\paket.exe restore
msbuild /t:Build /p:Configuration=%CONFIG% %HERE%SmartBuildSystem.sln
%HERE%packages\7-Zip.CommandLine\tools\7za.exe a -r artifacts\sbs.zip .\sbs\bin\%CONFIG%\*
