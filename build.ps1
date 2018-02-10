param([String]$config = "Release", [String]$fx="netcoreapp2.0")

rmdir -recurse artifacts
mkdir artifacts

dotnet publish SmartBuildSystem.sln -c $config -f $fx
Compress-Archive sbs\bin\$config\$fx\publish\. artifacts\sbs.zip
