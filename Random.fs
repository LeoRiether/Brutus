module Random

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
