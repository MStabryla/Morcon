namespace MorconConTest

open System.Net.Sockets
open System.IO
open System
open System.Linq
open System.Text
open System.Net
open System.Threading.Tasks
open MorconConLib

module MorconConTest =
    let getDeviceByteArray(device: DeviceStartMess) =
        let bytes = BitConverter.GetBytes device.id
        let deviceBytes = BitConverter.GetBytes (int device.deviceType)
        let nameBytes = Encoding.ASCII.GetBytes(device.name)
        // (nameBytes.Concat (bytes.Concat deviceBytes)).ToArray()
        bytes.Concat (deviceBytes.Concat nameBytes)
        


    let serverPort = 3478
    let serverString = $"127.0.0.1:{serverPort}"
    let serverEndpoint = IPEndPoint.Parse serverString
    let client = new UdpClient()

    let device: DeviceStartMess = { 
        id = 1
        deviceType = DeviceType.MOV_DETECTOR
        name = "Test"
    }

    [<EntryPoint>]
    let main _Argv =

        let deviceBytes = getDeviceByteArray(device)
        let getMessageBytes(message: string) =
            let mesBytes = Encoding.ASCII.GetBytes(message)
            (deviceBytes.Concat mesBytes).ToArray()

        for i = 1 to 10 do
            let messageByteArr = getMessageBytes $"Test{i}"
            let result = client.Send(messageByteArr,messageByteArr.Length,serverEndpoint)
            if result <> messageByteArr.Length then
                printfn $"something{result}"
            else
                printfn $"Message {i} send"
            Task.Delay(TimeSpan.FromSeconds 1.0).Wait()
        0

