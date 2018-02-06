module Helpers.Collections
open FSharp.Collections

type set<'T when 'T : comparison> = Set<'T>

let (?) (q: bool) (yes: 'a, no: 'a) = if q then yes else no


type Set<'T when 'T : comparison>
with
    static member substract a b = Set.difference b a
