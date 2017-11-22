module Helpers.Env
open System.Reflection
open System.IO




type DummyType () = class end

let getAssembly () =
    let assembly = typeof<DummyType>.GetTypeInfo().Assembly
    assembly


let Version () =
    let fbAssembly = getAssembly ()
    let version = fbAssembly.GetName().Version
    version




type private AppConfig = FSharp.Configuration.AppSettings<"Examples/App.config">

let MasterRepository () =
    AppConfig.MasterRepo



let IsWorkspaceFolder(wsDir : DirectoryInfo) =
    let subDir = wsDir |> Helpers.Fs.GetDirectory ".sbs"
    subDir.Exists

let rec private workspaceFolderSearch (dir : DirectoryInfo) =
    if dir = null || not dir.Exists then failwith "Can't find workspace root directory. Check you are in a workspace."
    if IsWorkspaceFolder dir then dir
    else workspaceFolderSearch dir.Parent

let WorkspaceDir () =
    Fs.CurrentDir() |> workspaceFolderSearch
