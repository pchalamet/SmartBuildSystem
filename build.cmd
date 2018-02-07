setlocal
set PATH=%PATH%;C:\Program Files (x86)\Microsoft Visual Studio\2017\BuildTools\MSBuild\15.0\bin\amd64
set HERE=%~dp0

set CONFIG=%1
set VERSION=%2
if "%CONFIG%" == "" set CONFIG=Release
if "%VERSION%" == "" set VERSION=0.0

if exist %HERE%artifacts rmdir /q /s %HERE%artifacts

%HERE%.paket\paket.exe restore
msbuild /t:Build /p:Configuration=%CONFIG% %HERE%SmartBuildSystem.sln
%HERE%packages\7-Zip.CommandLine\tools\7za.exe a -r artifacts\sbs.zip .\sbs\bin\%CONFIG%\*
%HERE%packages\NuGet.CommandLine\tools\NuGet.exe pack sbs.nuspec -OutputDirectory %HERE%artifacts -Version %VERSION% -Properties Configuration=%CONFIG%
move %HERE%artifacts\sbs.%VERSION%.nupkg %HERE%artifacts\sbs.nupkg
