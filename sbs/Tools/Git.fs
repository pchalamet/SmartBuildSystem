module Tools.Git
open System.IO
open Helpers.Fs
open Helpers.Exec

let LatestChanges (wsDir: DirectoryInfo) =
    let args = sprintf "diff --name-only HEAD^ HEAD"
    ExecGetOutput "git" args wsDir Map.empty
