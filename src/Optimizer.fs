module Bfi.Optimizer

open Bfi.Ast

[<Literal>]
let maxPasses = 64

let rec optPass changed acc ops =
  match ops with
  | Add 0y :: rest
  | Mov 0 :: rest -> optPass true acc rest

  | Loop [Add -1y] :: rest
  | Loop [Add 1y] :: rest -> optPass true acc <| set0 :: rest

  | Add _ :: (Read :: _ as rest)
  | Set _ :: (Read :: _ as rest)
  | Add _ :: (Set _ :: _ as rest)
  | Set _ :: (Set _ :: _ as rest) -> optPass true acc rest

  | Set s :: Add a :: rest -> optPass true acc <| Set (s + a) :: rest

  | Add a :: Add b :: rest -> optPass true acc <| Add (a + b) :: rest
  | Mov a :: Mov b :: rest -> optPass true acc <| Mov (a + b) :: rest

  | Set 0y as s :: Loop _ :: rest -> optPass true acc <| s :: rest

  | Loop _ as l :: Loop _ :: rest
  | Loop [Loop _ as l] :: rest -> optPass true acc <| l :: rest

  | Loop ops :: rest ->
      let changed, ops = optPass false [] ops
      optPass changed (Loop ops :: acc) rest

  | op :: rest -> optPass changed (op :: acc) rest
  | [] -> (changed, List.rev acc)

let rec optimize' passesLeft ops =
  match passesLeft with
  | 0 -> ops
  | _ ->
      let changed, ops = optPass false [] ops

      match changed with
      | true -> optimize' (passesLeft - 1) ops
      | _ -> ops
  
let inline optimize ops = optimize' maxPasses (set0 :: ops)