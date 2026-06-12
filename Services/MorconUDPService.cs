namespace Morcon.Services;

using System.Threading;
using Microsoft.Extensions.Hosting;
using Microsoft.FSharp.Control;
using Microsoft.FSharp.Core;
using MorconConLib;

public class MorconUDPService : BackgroundService
{
    public const int DEFAULT_PORT = 3478;
    private readonly DeviceUDP.DeviceConnectionServer _server;

    private static void WriteData(string data)
    {
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.BackgroundColor = ConsoleColor.Black;
        Console.Write("data");
        Console.ResetColor();
        Console.Write(": ");
        Console.WriteLine(data);
        
    }
    private readonly ILogger _logger;

    public MorconUDPService(IConfiguration config, ILogger<MorconUDPService> logger)
    {
        var port = config.GetValue<int?>("udpPort") ?? DEFAULT_PORT;
        _logger = logger;
        _server = new DeviceUDP.DeviceConnectionServer(port);
    }

    private Unit? Handler(Mess message)
    {
        WriteData(message.ToString());
        return null;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // WriteInfo($"UDP Server started on port {_server.Port}");
        _logger.LogInformation($"UDP Server started on port {_server.Port}");
        return FSharpAsync.StartAsTask(
            _server.Listen(FuncConvert.FromFunc<Mess, Unit?>(Handler), stoppingToken),
            null,
            cancellationToken: stoppingToken
        );
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("UDP Server is stopping down");
        _server.Shutdown();
        return base.StopAsync(cancellationToken);
    }
}
