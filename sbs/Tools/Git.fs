module Tools.Git
open System.IO
open Helpers.Fs
open Helpers.Exec

let Clone (repo : Configuration.Master.Repository) (wsDir : DirectoryInfo) (shallow : bool) =
    let bronly = "--no-single-branch"
    let depth = if shallow then "--depth=1"
                else ""
    let targetDir = wsDir |> GetDirectory repo.Name |> EnsureExists
    let args = sprintf @"clone %s --quiet %s %s %A" repo.Uri bronly depth targetDir.FullName
    ExecGetOutput "git" args wsDir Map.empty

let Checkout (repo : Configuration.Master.Repository) (wsDir : DirectoryInfo) (version : string) =
    let args = sprintf "checkout %A" version
    let targetDir = wsDir |> GetDirectory repo.Name |> EnsureExists
    ExecGetOutput "git" args targetDir Map.empty
