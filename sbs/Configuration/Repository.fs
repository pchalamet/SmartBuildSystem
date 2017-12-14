module Configuration.Repository
open Helpers.Collections
open Helpers.Fs
open System.IO
open System.Xml.Linq
open Helpers.Xml
open Helpers.Xml
open System

[<RequireQualifiedAccess>]
type Configuration =
    { Dependencies : Master.Repository set }



type RepositoryConfiguration() = class
    member val ``auto-dependencies`` : bool = true with get, set 
    member val dependencies : string array = null with get, set
end





let ext2projType = Map [ (".csproj", "fae04ec0-301f-11d3-bf4b-00c04f79efbc")
                         (".fsproj", "f2a71f9b-5d33-465a-a702-920d77279786")
                         (".vbproj", "f184b08f-c81c-45f6-a57f-5abd9991f28f") 
                         (".pssproj", "f5034706-568f-408a-b7b3-4d38c6db8a32")
                         (".sqlproj", "00D1A9C2-B5F0-4AF3-8072-F6C62B433612")]



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
        let refs = xdoc.Descendants() |> Seq.filter (fun x -> x.Name.LocalName = "ProjectReference")
                                        |> Seq.map (fun x -> !> x.Attribute(NsNone + "Include") : string)
                                        |> Set
                                        |> Seq.map (fun x -> prjFile, prjFile.Directory |> GetFile x)
        refs

    let repositories = ext2projType 
                            |> Seq.map (fun s -> s.Key)
                            |> Seq.fold (fun s t -> s |> Seq.append (repoDir.EnumerateFiles("*" + t, SearchOption.AllDirectories))) Seq.empty
                            |> Seq.fold (fun s t -> s |> Seq.append (extractProjectReferences t)) Seq.empty
                            |> Seq.map extractRepoFolder
    repositories





let getConfig (repo : string) (repoConfig : FileInfo) =
    if repoConfig.Exists |> not then (true, Seq.empty)
    else 
        use file = System.IO.File.OpenText(repoConfig.FullName)
        let serializer = new SharpYaml.Serialization.Serializer()
        let repoConfig = serializer.Deserialize<RepositoryConfiguration>(file)
        if repoConfig.dependencies |> isNull then failwithf "repository.yaml in %A is invalid" repo

        (repoConfig.``auto-dependencies``, repoConfig.dependencies |> seq)


let Load (wsDir : DirectoryInfo) (repoName : string) (masterConfig : Master.Configuration) =
    // validate repo name
    let repo = match masterConfig.Repositories |> Seq.tryFind (fun x -> x.Name = repoName) with
               | Some x -> x
               | None -> failwithf "Repository %A does not exist" repoName

    let repoDir = wsDir |> GetDirectory repo.Name
    let autoDeps, dependencies = repoDir |> GetFile "repository.yaml" |> getConfig repo.Name

    let repoMap = masterConfig.Repositories |> Seq.map (fun x -> x.Name, x) 
                                            |> Map

    let autoDependencies = match autoDeps with
                           | true -> scanDependencies repoDir
                           | _ -> Seq.empty

    let getRepo x =
        match repoMap |> Map.tryFind x with
        | Some repo -> repo
        | _ -> failwithf "Repository %A is unknown" x

    let dependencies = autoDependencies |> Seq.append dependencies
                                        |> Seq.filter (fun x -> x <> repoName)
                                        |> Set
                                        |> Set.map getRepo
    { Configuration.Dependencies = dependencies }
