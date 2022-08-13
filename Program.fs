open System.Diagnostics
open FSharp.Collections

let rng = System.Random()
let gen lo hi = rng.Next(lo, hi) // apparently this is [lo, hi)??
let shuffle a =
    let mutable a = Array.copy a
    let n = Array.length a
    for i in 1..(n-1) do
        let j = gen 0 (n-1)

        let x = a.[i]
        a.[i] <- a.[j]
        a.[j] <- x
    a

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

let _fibonacciGenerator (_: int) =
    let n = gen 2 10
    let a = Array.init n (fun _ -> gen 1 55)

    String.concat "\n" [
        yield "1"
        yield sprintf "%d" n
        yield String.concat " " (Array.map (sprintf "%d") a)
    ]

// mostly copied from http://fssnip.net/sw/title/RunProcess
let run programName (input : string) =
    let startInfo =
        ProcessStartInfo(
            RedirectStandardOutput = true,
            RedirectStandardInput = true,
            UseShellExecute = false,
            FileName = programName
        )

    use p = new Process(StartInfo = startInfo)
    let started =
        try
            p.Start()
        with | ex ->
            ex.Data.Add("filename", programName)
            reraise()
    if not started then
        failwithf "Failed to start process %s" programName

    p.StandardInput.WriteLine(input)
    p.StandardInput.Flush()

    p.WaitForExit()

    p.StandardOutput.ReadToEnd()

let printn = printfn "%s"
type Color = System.ConsoleColor
let cprint =
    let lockObj = obj()
    fun color s ->
        lock lockObj (fun _ ->
            System.Console.ForegroundColor <- color
            printfn "%s" s
            System.Console.ResetColor())

[<EntryPoint>]
let main argv =
    let (progA, progB) = (argv.[0], argv.[1])
    let (runA, runB) = (run progA, run progB)

    let normalize (s : string) =
        s.Split("\n")
        |> Array.map (fun t -> t.Trim())
        |> Array.filter (not << System.String.IsNullOrEmpty)
        |> String.concat "\n"

    let testCases = 1000

    for test in 1..testCases do
        let input = generator test

        let (outA, outB) = (runA input, runB input)
        let (outA, outB) = (normalize outA, normalize outB)
        if outA = outB then
            // Only print every 10th "ok"
            if test % 10 = 0 then
                cprint Color.Cyan (sprintf "test #%d:" test)
                printn input
                cprint Color.Green "ok"
                printfn ""
        else
            cprint Color.Cyan (sprintf "test #%d:" test)
            printn input
            cprint Color.Red "fail"
            printfn ""
            cprint Color.Yellow "A returned <"
            printn outA
            cprint Color.Yellow ">"
            printfn ""
            cprint Color.Yellow "B returned <"
            printn outB
            cprint Color.Yellow ">"

            exit 0

    0
