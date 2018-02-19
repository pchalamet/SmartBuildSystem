module Tools.Publish
open System.IO
open System.Linq
open System.Xml.Linq
open Helpers.Xml
open Helpers.Fs
open Helpers.Exec
open Helpers.IO
open System


type ProjectType =
    | MsBuild of FileInfo
    | MsBuildSdk of FileInfo * string
    | Unknown


let private sniffProjectType (folder : DirectoryInfo) =
    let project = folder.EnumerateFiles("*.csproj").Single()
    let content = XDocument.Load(project.FullName)
    let targetFxVer = content.Descendants(NsMsBuild + "TargetFrameworkVersion").SingleOrDefault()
    let targetFx = content.Descendants(NsNone + "TargetFramework").SingleOrDefault()
    if targetFxVer |> isNull |> not then ProjectType.MsBuild project
    else if targetFx |> isNull |> not then ProjectType.MsBuildSdk (project, (!> targetFx : string))
    else ProjectType.Unknown


let private publishArtifact (wsDir : DirectoryInfo) (sourceFolder : DirectoryInfo) (artifactName : string) (appName : string) =
    let targetFile = wsDir |> GetDirectory ".apps"
                           |> GetDirectory appName
                           |> EnsureExists
                           |> GetFile (sprintf "%s.zip" artifactName)

    System.IO.Compression.ZipFile.CreateFromDirectory(sourceFolder.FullName, targetFile.FullName, Compression.CompressionLevel.Fastest, false)


let private publishAppMsBuild wsDir (project : FileInfo) config app =
    wsDir |> GetDirectory ".apps"
          |> GetDirectory app
          |> Delete

    let publishTarget = Helpers.Env.SbsDir() |> GetFile "publish.targets"
    let tmpPublishTarget = project.Directory |> GetFile (sprintf "%s.publish.targets" project.Name)
    publishTarget.CopyTo(tmpPublishTarget.FullName, true) |> ignore
    let output = wsDir |> GetDirectory ".apps"
                       |> GetDirectory app
                       |> GetDirectory "tmp"
    let publishArgs = sprintf "/t:SBSPublish /p:SBSApp=%A /p:SBSProject=%A /p:SBSTargetFolder=%A %A" 
                              app 
                              project.FullName 
                              output.FullName
                              tmpPublishTarget.FullName
    Exec "msbuild" publishArgs project.Directory Map.empty |> CheckResponseCode
    
    let config = project.Directory |> GetDirectory "config"

    if config |> Exists |> not then failwithf "Can't find configurations for application %A" app

    publishArtifact wsDir output "app" app
    publishArtifact wsDir config "config" app
    output |> Delete
    tmpPublishTarget.Delete()

let private publishAppMsBuildSdk wsDir (project : FileInfo) config target app =
    let publishArgs = sprintf "publish --no-restore --configuration %s %A" config project.FullName
    Exec "dotnet" publishArgs project.Directory Map.empty |> CheckResponseCode

    let output = project.Directory |> GetDirectory "bin"
                                   |> GetDirectory config
                                   |> GetDirectory target
                                   |> GetDirectory "publish"
    let config = project.Directory |> GetDirectory "config"

    if output |> Exists |> not then failwithf "Can't find output for application %A" app
    if config |> Exists |> not then failwithf "Can't find configurations for application %A" app

    wsDir |> GetDirectory ".apps"
          |> GetDirectory app
          |> Delete

    publishArtifact wsDir output "app" app
    publishArtifact wsDir config "config" app


let private publishApp wsDir (projectFolder : DirectoryInfo) config name =
    let prjType = projectFolder |> sniffProjectType
    let publishApplicationArtifacts = match prjType with
                                      | MsBuild project -> publishAppMsBuild wsDir project config
                                      | MsBuildSdk (project,target) -> publishAppMsBuildSdk wsDir project config target
                                      | _ -> failwithf "Unknown project type"
    publishApplicationArtifacts name


let private loadView (wsDir : DirectoryInfo) viewName =
    let viewFile = wsDir |> GetFile (sprintf "%s.view" viewName)
    let repos = File.ReadLines(viewFile.FullName) |> Seq.map (fun x -> x.Split([|" <- "|], StringSplitOptions.RemoveEmptyEntries).[0])
    repos


let Publish (wsDir : DirectoryInfo) viewName config =
    let repositories = loadView wsDir viewName
    for repository in repositories do
        let repoDir = wsDir |> GetDirectory repository
        let manifests = repoDir.EnumerateFiles("manifest.yaml", SearchOption.AllDirectories)
        for manifest in manifests do
            let app = Configuration.Manifest.Load manifest
            publishApp wsDir manifest.Directory config app.Name
