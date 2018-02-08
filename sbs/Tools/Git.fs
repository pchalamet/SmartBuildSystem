module Tools.Git
open System.IO
open Helpers.Fs
open Helpers.Exec
open Helpers.Collections
open Helpers.IO

let Clone (repo : Configuration.Master.Repository) (wsDir : DirectoryInfo) (shallow : bool) (branch : string option) =
    let gitBranch = match branch with
                    | Some name -> let chkBranchArgs = sprintf "ls-remote --heads %s %s" repo.Uri name
                                   match Exec "git" chkBranchArgs wsDir Map.empty |> IsError with
                                   | true -> Some "master"
                                   | false -> Some name
                    | None -> None

    let br = match gitBranch with
             | Some name -> sprintf "--branch %s --single-branch" name
             | None -> "--no-single-branch"
    let depth = shallow ? ("--depth=1", "")
    let targetDir = wsDir |> GetDirectory repo.Name
    let args = sprintf @"clone %s %s %s %A" repo.Uri br depth targetDir.FullName
    Exec "git" args wsDir Map.empty

let Checkout (repo : Configuration.Master.Repository) (wsDir : DirectoryInfo) (version : string) =
    let args = sprintf "checkout %A" version
    let targetDir = wsDir |> GetDirectory repo.Name
    Exec "git" args targetDir Map.empty

let Fetch (repo : Configuration.Master.Repository) (wsDir : DirectoryInfo) =
    let args = sprintf "fetch --all"
    let targetDir = wsDir |> GetDirectory repo.Name
    Exec "git" args targetDir Map.empty

let Pull (repo : Configuration.Master.Repository) (wsDir : DirectoryInfo) =
    let args = sprintf "pull --ff-only"
    let targetDir = wsDir |> GetDirectory repo.Name
    Exec "git" args targetDir Map.empty
