set HERE=%~dp0
set CONFIG=%1
%HERE%.paket\paket.exe restore
msbuild /t:Build /p:Configuration=%CONFIG% %HERE%SmartBuildSystem.sln
%HERE%packages\NuGet.CommandLine\tools\NuGet.exe pack %HERE%sbs\sbs.fsproj -OutputDirectory %HERE%artifacts -Tool -Properties Configuration=Release
