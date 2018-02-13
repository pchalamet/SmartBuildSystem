module Core.Repository
open System.IO
open Helpers
open Helpers.Xml
open Configuration
open System.Xml.Linq



let private scanRepositoryDependencies (repoDir : DirectoryInfo) =
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
                        |> Seq.distinct
                        |> Seq.map (fun x -> prjFile, prjFile.Directory |> Fs.GetFile x)
        refs

    let repositories = Project.SupportedProjectExtensions
                            |> Seq.fold (fun s t -> s |> Seq.append (repoDir.EnumerateFiles("*" + t, SearchOption.AllDirectories))) Seq.empty
                            |> Seq.fold (fun s t -> s |> Seq.append (extractProjectReferences t)) Seq.empty
                            |> Seq.map extractRepoFolder
    repositories


let FindDependencies wsDir (masterConfig : Configuration.Master.Configuration) name =
    // validate repo name
    let repo = match masterConfig.GetRepository name with
                | Some x -> x
                | None -> failwithf "Repository %A does not exist" name

    let repoDir = wsDir |> Fs.GetDirectory repo.Name
    let autoDeps, dependencies = repoDir |> Repository.Load

    let autoDependencies = if autoDeps then scanRepositoryDependencies repoDir
                            else Seq.empty

    let getRepo x =
        match masterConfig.GetRepository x with
        | Some repo -> repo
        | _ -> failwithf "Repository %A is an unknown dependency of %A" x name

    let dependencies = autoDependencies 
                            |> Seq.append dependencies
                            |> Seq.filter (fun x -> x <> name)
                            |> Set
                            |> Set.map getRepo
    dependencies
