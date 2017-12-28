module Core.Repository
open System.IO
open Helpers
open Helpers.Xml
open Configuration
open System.Xml.Linq



let private scanDependencies (repoDir : DirectoryInfo) =
    let wsDir = repoDir.Parent

    let extractRepoFolder ((projectFile, file) : FileInfo * FileInfo) =
        if file.FullName.ToLowerInvariant().StartsWith(wsDir.FullName.ToLowerInvariant()) |> not then 
            failwithf "Invalid path %s in project %A" file.FullName projectFile.FullName

        let relativeFile = file.FullName.Substring(wsDir.FullName.Length + 1)
        let idx = relativeFile.IndexOf(System.IO.Path.DirectorySeparatorChar)
        let repo = relativeFile.Substring(0, idx).ToLowerInvariant()
        repo

    let extractProjectReferences (prjFile : FileInfo) =
        let xdoc = XDocument.Load (prjFile.FullName)
        let refs = xdoc.Descendants() 
                        |> Seq.filter (fun x -> x.Name.LocalName = "ProjectReference")
                        |> Seq.map (fun x -> !> x.Attribute(NsNone + "Include") : string)
                        |> Set
                        |> Seq.map (fun x -> prjFile, prjFile.Directory |> Fs.GetFile x)
        refs

    let repositories = Project.SupportedProjectExtensions
                            |> Seq.fold (fun s t -> s |> Seq.append (repoDir.EnumerateFiles("*" + t, SearchOption.AllDirectories))) Seq.empty
                            |> Seq.fold (fun s t -> s |> Seq.append (extractProjectReferences t)) Seq.empty
                            |> Seq.map extractRepoFolder
    repositories


type Repository =
    { RepositoryName : string }
with
    member this.ScanDependencies wsDir (masterConfig : Configuration.Master.Configuration) =
        // validate repo name
        let repo = match masterConfig.Repositories |> Seq.tryFind (fun x -> x.Name = this.RepositoryName) with
                   | Some x -> x
                   | None -> failwithf "Repository %A does not exist" this.RepositoryName

        let repoDir = wsDir |> Fs.GetDirectory repo.Name
        let autoDeps, dependencies = repoDir |> Repository.Load

        let repoMap = masterConfig.Repositories |> Seq.map (fun x -> x.Name, x) 
                                                |> Map

        let autoDependencies = match autoDeps with
                               | true -> scanDependencies repoDir
                               | _ -> Seq.empty

        let getRepo x =
            match repoMap |> Map.tryFind x with
            | Some repo -> repo
            | _ -> failwithf "Repository %A is an unknown dependency of %A" x this.RepositoryName

        let dependencies = autoDependencies 
                                |> Seq.append dependencies
                                |> Seq.filter (fun x -> x <> this.RepositoryName)
                                |> Set
                                |> Set.map getRepo
        dependencies
