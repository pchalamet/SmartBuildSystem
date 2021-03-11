module Tools.MsBuild
open System.IO
open Helpers.Collections

let Build (clean : bool) (mt : bool) (config : string) (wsDir : DirectoryInfo) (slnFile : FileInfo) =
    let cleanOpt = clean ? ("--no-incremental", "")
    let mtOpt = mt ? ("-maxcpucount", "")
    let confOpt = sprintf "-c %s" config

    let args = sprintf "build %s %s %s %s" cleanOpt mtOpt confOpt slnFile.FullName
    Helpers.Exec.Exec "dotnet" args wsDir Map.empty |> Helpers.IO.CheckResponseCode


let Test (mt : bool) (config : string) (wsDir : DirectoryInfo) (slnFile : FileInfo) =
    let mtOpt = mt ? ("-maxcpucount", "")
    let confOpt = sprintf "-c %s" config

    let args = sprintf "test %s %s %s" mtOpt confOpt slnFile.FullName
    Helpers.Exec.Exec "dotnet" args wsDir Map.empty |> Helpers.IO.CheckResponseCode
