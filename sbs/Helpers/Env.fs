﻿module Helpers.Env
open System.Reflection
open System.Runtime.InteropServices
open System.IO




type DummyType () = class end

let getAssembly () =
    let assembly = typeof<DummyType>.GetTypeInfo().Assembly
    assembly


let Version () =
    let fbAssembly = getAssembly ()
    let version = fbAssembly.GetName().Version
    version

let IsWindows () =
    RuntimeInformation.IsOSPlatform(OSPlatform.Windows)


let IsWorkspaceFolder(wsDir : DirectoryInfo) =
    let masterConfig = wsDir |> Helpers.Fs.GetFile "sbs.yaml"
    masterConfig.Exists

let rec private workspaceFolderSearch (dir : DirectoryInfo) =
    if (dir |> isNull) || (not dir.Exists) then failwith "Can't find workspace root directory. Check you are in a workspace."
    if IsWorkspaceFolder dir then dir
    else workspaceFolderSearch dir.Parent

let WorkspaceDir () =
    Fs.CurrentDir() |> workspaceFolderSearch
    
let SbsDir () =
    let fbAssembly = getAssembly().Location |> FileInfo
    fbAssembly.Directory
