module Tools.MsBuild
open System.IO
open Helpers.Collections

let Build (clean : bool) (config : string) (wsDir : DirectoryInfo) (slnFile : FileInfo) =
    let target = clean ? ("Clean,Restore,Build", "Restore,Build")
    let argMt = "/m"

    let argConfig = sprintf "/p:Configuration=%s" config
    let args = sprintf "/nologo /t:%s %s %s %A" target argMt argConfig slnFile.FullName

    Helpers.Exec.Exec "msbuild" args wsDir Map.empty |> Helpers.IO.CheckResponseCode
