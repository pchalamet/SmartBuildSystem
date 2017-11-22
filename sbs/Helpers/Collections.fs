module Helpers.Collections

open FSharp.Collections

type set<'T when 'T : comparison> = Set<'T>

let (?) (q: bool) (yes: 'a, no: 'a) = if q then yes else no

let unwrap<'T> (value : 'T option) =
    match value with
    | Some x -> x
    | _ -> failwith "Expecting value"

let orDefault<'T> (defValue : 'T) (value : 'T option) =
    match value with
    | Some x -> x
    | None -> defValue


let compareTo<'T, 'U> (this : 'T) (other : System.Object) (fieldOf : 'T -> 'U) =
    match other with
    | :? 'T as x -> System.Collections.Generic.Comparer<'U>.Default.Compare(fieldOf this, fieldOf x)
    | _ -> failwith "Can't compare values with different types"

let refEquals (this : System.Object) (other : System.Object) =
    System.Object.ReferenceEquals(this, other)

let memoize (f: 'a -> 'b) : 'a -> 'b =
    let cache = System.Collections.Concurrent.ConcurrentDictionary<'a, 'b>()
    fun (x: 'a) ->
        cache.GetOrAdd(x, f)
