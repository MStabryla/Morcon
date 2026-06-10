namespace MorconConLib

type Mess = { a: int64; b: string }
module Say =
    let hello name =
        printfn "Hello %s" name
