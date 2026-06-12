namespace MorconConLib

open System
open System.Net
open System.Net.Sockets
open System.Linq
open System.Text

type DeviceType = 
| UNKNOWN = 0
| TEMP_SENSOR = 1
| HUM_SENSOR = 2
| WIND_SENSOR = 3
| ALARM = 4
| MOV_DETECTOR = 5

exception SmallSizeException of int * int
type DeviceStartMess = { id: int; deviceType: DeviceType; name: string;  }
type Mess(array : byte[]) = 
    let bytes = array
    let _id = System.DateTime.UtcNow.Ticks
    let deviceData : DeviceStartMess = 
        try
            if array.Length < 9 then
                raise (SmallSizeException(array.Length, 9))
            let _id = BitConverter.ToInt32(array.AsSpan(0,4))
            let _deviceType = BitConverter.ToInt32(array.AsSpan(4,4))
            
            let _name = BitConverter.ToString(array.Skip(8).TakeWhile(fun x -> x <> 255uy).ToArray())
            { id = _id; deviceType = enum _deviceType ; name = _name}
        with
        | SmallSizeException(actSize, desiredSize) ->
            printfn "Desired Size %d exceeds actual size %d" desiredSize actSize
            { id = 0; deviceType = DeviceType.UNKNOWN ; name = ""}


    member this.id : int64 = _id 
    member this.DeviceData = deviceData
    member this.Message = 
        Encoding.ASCII.GetString(bytes.SkipWhile(fun x -> x = 255uy).ToArray())
    


module DeviceUDP =
    open System.Threading

    let valueTaskToTask<'T>(x : Tasks.ValueTask<'T>) = x.AsTask()

    let serverPort = 3478

    type DeviceConnectionServer(serverPort: int) =
        let server = new UdpClient(serverPort)
        let _receivedMessageEvent = new Event<Mess>()

        member private _.ReceiveMessageAsync(can) = 
            server.ReceiveAsync(can)

        member private _.CastToMess(bytes: byte[]) =
            Mess(bytes);

        member this.Port = (server.Client.LocalEndPoint :?> IPEndPoint).Port
        member this.ReceivedMessageEvent = _receivedMessageEvent.Publish
        member this.Listen(onMessage: Mess -> unit, canToken: CancellationToken) =
            let sub = this.ReceivedMessageEvent.Add(onMessage )
            let rec loop () = async {
                try
                    let! result = this.ReceiveMessageAsync(canToken) |> valueTaskToTask |> Async.AwaitTask
                    printfn $"[DEBUG]: result message from {result.RemoteEndPoint.ToString()}"
                    this.CastToMess result.Buffer |> _receivedMessageEvent.Trigger
                    return! loop ()
                with
                | :? OperationCanceledException when canToken.IsCancellationRequested ->
                    ()
                | ex ->
                    printfn "[ERROR]: UDP listener error %A" ex
                    return! loop ()
            }
            try
                loop ()
            finally
                printfn $"[DEBUG]:Listening stopped!"
                // sub.Dispose()
            

        member _.Shutdown() =
            server.Close()

        interface IDisposable with
            member _.Dispose() = server.Dispose()