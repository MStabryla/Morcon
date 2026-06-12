namespace MorconConSerLib

open System
open System.Threading
open MorconConLib

module MorconServerTest =
    let MessageReceived(mess: Mess) =
        printfn $"[{mess.DeviceData.id}:{mess.DeviceData.name}] {mess.Message}"

    [<EntryPoint>]
    let main _argv =
        use ser = new DeviceUDP.DeviceConnectionServer(DeviceUDP.serverPort)
        use cts = new CancellationTokenSource()

        Console.CancelKeyPress.Add(fun args ->
            args.Cancel <- true
            printfn "Cancellation requested, stopping server ..."
            cts.Cancel())

        printfn $"Server started on {ser.Port} ..."
        try
            Async.RunSynchronously(ser.Listen(MessageReceived, cts.Token), cancellationToken = cts.Token)
        with
            | :? OperationCanceledException as ex ->
            printfn "Server stopped ..."
        0
