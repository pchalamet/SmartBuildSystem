config ?= Debug

all: build

build:
	dotnet build -c $(config) sbs
	dotnet test -c $(config) sbs

publish:
	rm -rf out
	dotnet publish -c $(config) -r win10-x64 -o $(CURDIR)\out\win10 sbs
	dotnet publish -c $(config) -r osx.10.11-x64 -o $(CURDIR)\out\osx sbs
