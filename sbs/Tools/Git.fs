module Tools.Git
open System.IO
open Helpers.Fs
open Helpers.Exec
open Helpers.Collections
open Helpers.IO


[<RequireQualifiedAccess>]
type private Branch =
    | None
    | Default
    | Named of string


let Clone (repo : Configuration.Master.Repository) (wsDir : DirectoryInfo) (shallow : bool) (branch : string option) =
    let gitBranch = match branch with
                    | Some name -> let chkBranchArgs = sprintf "ls-remote --exit-code --heads %s %s" repo.Uri name
                                   match ExecGetOutput "git" chkBranchArgs wsDir Map.empty |> IsError with
                                   | true -> Branch.Default
                                   | false -> Branch.Named name
                    | None -> Branch.None

    let br = match gitBranch with
             | Branch.None -> "--no-single-branch"
             | Branch.Default -> "--single-branch"
             | Branch.Named name -> sprintf "--branch %s --single-branch" name
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
