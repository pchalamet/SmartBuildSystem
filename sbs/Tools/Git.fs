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
    let args = sprintf @"clone --recurse-submodules %s %s %s %A" repo.Uri br depth targetDir.FullName
    ExecGetOutput "git" args wsDir Map.empty

let Checkout (repo : Configuration.Master.Repository) (wsDir : DirectoryInfo) (version : string) =
    let targetDir = wsDir |> GetDirectory repo.Name
    let chkVersionArgs = sprintf "rev-parse --verify --quiet %s" version
    let versionErr = ExecGetOutput "git" chkVersionArgs targetDir Map.empty
    match versionErr |> IsError with
    | false -> let currBranchArgs = sprintf "rev-parse --abbrev-ref HEAD"
               let currBrErr = ExecGetOutput "git" currBranchArgs targetDir Map.empty
               if currBrErr.Out <> [ version ] then
                   let args = sprintf "checkout %A" version
                   ExecGetOutput "git" args targetDir Map.empty
               else
                    sprintf "Your branch is up to date with '%s'." version |> Helpers.IO.ResultOk
    | _ -> sprintf "'%s' not found." version |> Helpers.IO.ResultErr

let Fetch (repo : Configuration.Master.Repository) (wsDir : DirectoryInfo) =
    let args = sprintf "fetch --all"
    let targetDir = wsDir |> GetDirectory repo.Name
    ExecGetOutput "git" args targetDir Map.empty

let Pull (repo : Configuration.Master.Repository) (wsDir : DirectoryInfo) =
    let args = sprintf "pull --ff-only"
    let targetDir = wsDir |> GetDirectory repo.Name
    ExecGetOutput "git" args targetDir Map.empty
