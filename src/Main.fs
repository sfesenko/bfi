module Language.Brainfuck.Main

open System.IO

open Language.Brainfuck.Parser
open Language.Brainfuck.Optimizer
open Language.Brainfuck.Codegen

let openFile args =
  if Array.isEmpty args
  then Error "No input file specified."
  else
    try Ok <| File.ReadAllText args.[0]
    with e -> Error e.Message

let writeErrors = function
  | Ok _ -> 0
  | Error err ->
      eprintfn "Error: %s" err
      1

[<EntryPoint>]
let main argv =
  openFile argv
  |> Result.bind parse
  |> Result.map (optimize >> compile)
  |> Result.map (fun x -> x () )
  |> writeErrors
