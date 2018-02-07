setlocal
set PATH=%PATH%;C:\Program Files (x86)\Microsoft Visual Studio\2017\BuildTools\MSBuild\15.0\bin\amd64
set HERE=%~dp0

%HERE%packages\NuGet.CommandLine\tools\NuGet.exe push artifacts\sbs.nupkg 2cc12764-6c61-4af8-9661-74a9e05ddaa8 -source http://factory.smartadserver.com/ngsvr/nuget
