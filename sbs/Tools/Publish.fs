module Tools.Publish
open System.IO
open System.Linq
open System.Xml.Linq
open Helpers.Xml
open Helpers.Fs


type ProjectType =
    | MsBuild of FileInfo
    | MsBuildSdk of FileInfo
    | Unknown


let private sniffProjectType (folder : DirectoryInfo) =
    let project = folder.EnumerateFiles("*.csproj").Single()
    let content = XDocument.Load(project.FullName)
    let targetFxVer = content.Descendants(NsMsBuild + "TargetFrameworkVersion").SingleOrDefault()
    let targetFx = content.Descendants(NsNone + "TargetFramework").SingleOrDefault()
    if targetFxVer |> isNull |> not then ProjectType.MsBuild project
    else if targetFx |> isNull |> not then ProjectType.MsBuildSdk project
    else ProjectType.Unknown


let private publishArtifact (wsDir : DirectoryInfo) (sourceFolder : DirectoryInfo) (artifactName : string) (appName : string) =
    let targetFile = wsDir |> GetDirectory "apps"
                           |> GetDirectory appName
                           |> EnsureExists
                           |> GetFile (sprintf "%s.zip" artifactName)

    System.IO.Compression.ZipFile.CreateFromDirectory(sourceFolder.FullName, targetFile.FullName, Compression.CompressionLevel.Fastest, false)


let private publishAppMsBuild wsDir (project : FileInfo) config app =
    let output = project.Directory |> GetDirectory "bin"
                                   |> GetDirectory config
    let config = project.Directory |> GetDirectory "config"

    wsDir |> GetDirectory "apps"
          |> GetDirectory app
          |> Delete

    publishArtifact wsDir output "app" app
    publishArtifact wsDir config "config" app



let private publishApp wsDir (projectFolder : DirectoryInfo) config name =
    let prjType = projectFolder |> sniffProjectType
    let publishApplicationArtifacts = match prjType with
                                      | MsBuild project -> publishAppMsBuild wsDir project config
                                      //| MsBuildSdk project -> publishAppMsBuild project config
                                      | _ -> failwithf "Unknown project type"
    publishApplicationArtifacts name


let private loadView (wsDir : DirectoryInfo) viewName =
    let viewFile = wsDir |> GetFile (sprintf "%s.view" viewName)
    let repos = File.ReadLines(viewFile.FullName)
    repos


let Publish (wsDir : DirectoryInfo) viewName config =
    let repositories = loadView wsDir viewName
    for repository in repositories do
        let repoDir = wsDir |> GetDirectory repository
        let manifests = repoDir.EnumerateFiles("manifest.yaml", SearchOption.AllDirectories)
        for manifest in manifests do
            let app = Configuration.Manifest.Load manifest
            publishApp wsDir manifest.Directory config app.Name
