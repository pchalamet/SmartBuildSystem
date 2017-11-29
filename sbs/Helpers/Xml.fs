module Helpers.Xml
open System.Xml.Linq

#nowarn "0077" // op_Explicit


let NsNone = XNamespace.None

let inline (!>) (x : ^a) : ^b = (((^a or ^b) : (static member op_Explicit : ^a -> ^b) x))

