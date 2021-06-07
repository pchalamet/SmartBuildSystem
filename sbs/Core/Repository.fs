module Core.Repository
open System.IO
open Helpers
open Configuration
open System.Xml.Linq



let findProjects wsDir (repo : Configuration.Master.Repository) =
    let isValidProject (file : FileInfo) =
        try
            let xdoc = XDocument.Load(file.FullName)
            xdoc.Root.Name.LocalName = "Project"
        with
            _ -> false

    let repoDir = wsDir |> Fs.GetDirectory repo.Name
    repoDir.EnumerateFiles("*.*proj", SearchOption.AllDirectories) |> Seq.filter isValidProject
   

let FindDependencies wsDir (masterConfig : Configuration.Master.Configuration) name =
    // validate repo name
    let repo = match masterConfig.GetRepository name with
                | Some x -> x
                | None -> failwithf "Repository %A does not exist" name

    let repoDir = wsDir |> Fs.GetDirectory repo.Name
    let repoConfig = repoDir |> Repository.Load

    let getRepo x =
        match masterConfig.GetRepository x with
        | Some repo -> repo
        | _ -> failwithf "Repository %A is an unknown dependency of %A" x name

    let dependencies = Seq.empty 
                            |> Seq.append repoConfig.Dependencies
                            |> Seq.filter (fun x -> x <> name)
                            |> Set
                            |> Set.map getRepo
    dependencies
