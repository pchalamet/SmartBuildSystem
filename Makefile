config ?= Debug

all:
	dotnet build -c $(config) sbs
	dotnet publish -c $(config) -o $(CURDIR)\out\sbs sbs
