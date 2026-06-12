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
type Mess(byteArray : byte[]) = 
    let _id = System.DateTime.UtcNow.Ticks
    let deviceData : DeviceStartMess = 
        try
            if byteArray.Length < 9 then
                raise (SmallSizeException(byteArray.Length, 9))
            let _id = BitConverter.ToInt32(byteArray.AsSpan(0,4))
            let _deviceType = BitConverter.ToInt32(byteArray.AsSpan(4,4))
            
            let _name = Encoding.ASCII.GetString(byteArray.Skip(8).TakeWhile(fun x -> x <> 255uy).ToArray())
            { id = _id; deviceType = enum _deviceType ; name = _name}
        with
        | SmallSizeException(actSize, desiredSize) ->
            printfn "Desired Size %d exceeds actual size %d" desiredSize actSize
            { id = 0; deviceType = DeviceType.UNKNOWN ; name = ""}
    let messageBytes = 
        byteArray.Skip(8).SkipWhile(fun x -> x <> 255uy).Skip(1).ToArray()
    
    member _.Id
        with get() = _id 
    member _.DeviceData = deviceData
    member _.Message 
        with get() = Encoding.ASCII.GetString(messageBytes)
    member _.DeviceBytes 
        with get() = messageBytes
    override _.ToString() : string =
        let sb = new StringBuilder()
        sb.AppendFormat $"[{deviceData.id}:{deviceData.name}] {Encoding.ASCII.GetString messageBytes}" |> fun s -> s.ToString()

    static member Serialize(mess: Mess) =

        let idBytes = BitConverter.GetBytes mess.DeviceData.id
        let deviceBytes = BitConverter.GetBytes (int mess.DeviceData.deviceType)
        let nameBytes = Encoding.ASCII.GetBytes(mess.DeviceData.name)
        // (nameBytes.Concat (bytes.Concat deviceBytes)).ToArray()
        idBytes.Concat deviceBytes |> fun b -> b.Concat nameBytes |> fun b -> b.Append 255uy  |> fun b -> b.Concat mess.DeviceBytes
    static member Serialize(message: string, device: DeviceStartMess) =
        let idBytes = BitConverter.GetBytes device.id
        let deviceBytes = BitConverter.GetBytes (int device.deviceType)
        let nameBytes = Encoding.ASCII.GetBytes device.name
        // (nameBytes.Concat (bytes.Concat deviceBytes)).ToArray()
        idBytes.Concat deviceBytes |> fun b -> b.Concat nameBytes |> fun b -> b.Append 255uy  |> fun b -> b.Concat(Encoding.ASCII.GetBytes message)
    static member Deserialize(bytes: byte[]) =
        new Mess(bytes)
    


module DeviceUDP =
    open System.Threading

    let valueTaskToTask<'T>(x : Tasks.ValueTask<'T>) = x.AsTask()

    let serverPort = 3478

    type DeviceConnectionServer(serverPort: int) =
        let server = new UdpClient(serverPort)
        let _receivedMessageEvent = new Event<Mess>()

        member private _.ReceiveMessageAsync(can) = 
            server.ReceiveAsync(can)

        member this.Port = (server.Client.LocalEndPoint :?> IPEndPoint).Port
        member this.ReceivedMessageEvent = _receivedMessageEvent.Publish
        member this.Listen(onMessage: Mess -> unit, canToken: CancellationToken) =
            let sub = this.ReceivedMessageEvent.Add(onMessage )
            let rec loop () = async {
                try
                    let! result = this.ReceiveMessageAsync(canToken) |> valueTaskToTask |> Async.AwaitTask
                    // printfn $"[DEBUG]: result message from {result.RemoteEndPoint.ToString()}"
                    Mess.Deserialize result.Buffer |> _receivedMessageEvent.Trigger
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