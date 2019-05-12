# How to build
make

# Usage
sbs usage
````
Usage:
  usage : display help on command or area
  init <folder> : initialize workspace
  clone [--only] [--shallow] [--branch name] <repository...> : clone repositories using wildcards
  view [--only] <name> <repository...> : create a solution with select repositories
  checkout <branch> : checkout given branch on all repositories
  fetch : fetch all branches on all repositories
  pull [--only] <repository...> : pull (ff-only) on all repositories
  build [--release] [--parallel] <view> : build a view
  rebuild [--release] <view> : rebuild a view
  publish [--release] <view> : publish apps in view
  open <view> : open view with your favorite ide
  exec <cmd> :  execute command for each repository (variables: SBS_NAME, SBS_PATH, SBS_URL, SBS_WKS)`
````

# Configuration
Modify `App.config` to link to your master repo. This must be the URL of a valid git repository.

````
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <appSettings>
    <add key="MasterRepo" value="XXXXXX" />
  </appSettings>
</configuration>
````

The repository must contains the `sbs.yaml` file describing all available repositories:
````
repositories:
    - name: sbs                                                                                  
      uri: git@git@github.com:pchalamet/SmartBuildSystem.git                
    - name: npolybool                                                                                              
      uri: git@github.com:pchalamet/NPolyBool.git                                  
````

# Commands
You can create a workspace using `sbs init <folder>`.
Inside this workspace, you can clone all repositories using `sbs clone *`. Note this a wildcard.
