module Tools.Git
open System.IO
open Helpers.Fs
open Helpers.Exec
open Helpers.Collections

let Clone (repo : Configuration.Master.Repository) (wsDir : DirectoryInfo) (shallow : bool) =
    let bronly = "--no-single-branch"
    let depth = shallow ? ("--depth=1", "")
    let targetDir = wsDir |> GetDirectory repo.Name
    let args = sprintf @"clone %s %s %s %A" repo.Uri bronly depth targetDir.FullName
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
