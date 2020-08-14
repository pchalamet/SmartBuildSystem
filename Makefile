config ?= Debug

build:
	dotnet build -c $(config) sbs
	dotnet test -c $(config) sbs

publish: build
	rm -rf out
	mkdir out
	dotnet publish -c $(config) -r win10-x64 -p:PublishSingleFile=true -p:PublishTrimmed=true -o $(PWD)/out/win10 sbs
	cd out/win10; zip -r ../win10.zip ./*

	dotnet publish -c $(config) -r osx-x64 -p:PublishSingleFile=true -p:PublishTrimmed=true -o $(PWD)/out/osx sbs
	cd out/osx; zip -r ../osx.zip ./*

	dotnet publish -c $(config) -r linux-x64 -p:PublishSingleFile=true -p:PublishTrimmed=true -o $(PWD)/out/linux sbs
	cd out/linux; zip -r ../linux.zip ./*
