setlocal
set PATH=%PATH%;C:\Program Files (x86)\Microsoft Visual Studio\2017\BuildTools\MSBuild\15.0\bin\amd64
set HERE=%~dp0

set CONFIG=%1
if "%CONFIG%" == "" set CONFIG=Release

let FX=%2
if "%FX%" == "" set FX=netcoreapp2.0

rmdir /s /q %HERE%artifacts
mkdir %HERE%artifacts
%HERE%.paket\paket restore
dotnet publish %HERE%SmartBuildSystem.sln -c Release -f netcoreapp2.0
%HERE%packages\7-Zip.CommandLine\tools\7za.exe a -r artifacts\sbs.zip .\sbs\bin\%CONFIG%\%FX%\publish\*
