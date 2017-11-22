module Core.Git
open System.IO
open Helpers.Fs
open Helpers.Exec

let GitClone (repo : Configuration.Master.Repository) (wsDir : DirectoryInfo) (branch : string) (shallow : bool) =
    let bronly = sprintf "--branch %s --no-single-branch" branch
    let depth = if shallow then "--depth=1"
                else ""

    let targetDir = wsDir |> GetDirectory repo.Name |> EnsureExists
    let args = sprintf @"clone %s --quiet %s %s %A" repo.Uri bronly depth targetDir.FullName

    ExecGetOutput "git" args wsDir Map.empty
