open Random

// generator : (testNumber : int) -> string
let generator (_ : int) =
    let n = gen 2 10
    let k = gen 1 n
    let a = Array.init n (fun _ -> gen 1 10)

    String.concat "\n" [
        yield "1"
        yield sprintf "%d %d" n k
        yield String.concat " " (Array.map (sprintf "%d") a)
    ]

[<EntryPoint>]
let main argv =
    let (progA, progB) = (argv.[0], argv.[1])

    Runner.run 1000 generator progA progB

    0
