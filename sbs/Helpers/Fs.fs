module Helpers.Fs
open System
open System.IO


let GetDirectory (subDir : string) (dir : DirectoryInfo) : DirectoryInfo =
    let newPath = Path.Combine(dir.FullName, subDir)
    DirectoryInfo (newPath)

let GetFile (fileName : string) (dir : DirectoryInfo) : FileInfo =
    let fullFileName = Path.Combine(dir.FullName, fileName)
    FileInfo (fullFileName)

let EnsureExists (dir : DirectoryInfo) =
    if not dir.Exists then dir.Create()
    dir

let CurrentDir () =
    Environment.CurrentDirectory

let ReadAllText (fileName : FileInfo) =
    fileName.FullName |> File.ReadAllText

