set HERE=%~dp0
set CONFIG=%1
%HERE%.paket\paket.exe restore
msbuild /t:Build /p:Configuration=%CONFIG% %HERE%SmartBuildSystem.sln
%HERE%packages\7-Zip.CommandLine\tools\7za.exe a -r artifacts\sbs.zip sbs\bin\%CONFIG%\*
