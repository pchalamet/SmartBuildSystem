module Helpers.Env
open System.Reflection





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
