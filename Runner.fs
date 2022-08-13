module Runner

open System.Diagnostics
open FSharp.Collections

// executes a shell command
// mostly copied from http://fssnip.net/sw/title/RunProcess
let exec programName (input : string) =
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


let run testCases generator progA progB =
    let (execA, execB) = (exec progA, exec progB)

    let normalize (s : string) =
        s.Split("\n")
        |> Array.map (fun t -> t.Trim())
        |> Array.filter (not << System.String.IsNullOrEmpty)
        |> String.concat "\n"

    let runSingleTestcase test =
        let input = generator test

        let (outA, outB) = (execA input, execB input)
        let (outA, outB) = (normalize outA, normalize outB)
        if outA = outB then
            // Only print every 10th "ok"
            if test % 10 = 0 then
                cprint Color.Cyan (sprintf "test #%d:" test)
                printn input
                cprint Color.Green "ok"
                printfn ""
            false // continue to next testcase
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
            true // break

    seq { 1..testCases }
        |> Seq.tryFind runSingleTestcase // runs until failure
        |> ignore

