using System.Net.Sockets;
using System.Net;
using System.Text;
using Experiments.OpenTelemetry.Communication;
using System.Text.Json;

namespace Experiments.OpenTelemetry.Configurator;

internal sealed class Program
{
    static async Task Main()
    {
        var endpoint = new IPEndPoint(IPAddress.Loopback, 5000);

        using Socket client = new(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        await client.ConnectAsync(endpoint).ConfigureAwait(false);

        var message = new ChangeMaxConcurrentActivitiesCountCommand(5);
        var json = $"<|{message.GetType().AssemblyQualifiedName}|>{JsonSerializer.Serialize(message)}";
        var messageBytes = Encoding.UTF8.GetBytes(json);
        _ = await client.SendAsync(messageBytes, SocketFlags.None).ConfigureAwait(false);
        Console.WriteLine($"Socket client sent message: \"{message}\"");

        // Receive ack.
        //var buffer = new byte[1_024];
        //var received = await client.ReceiveAsync(buffer, SocketFlags.None);
        //var response = Encoding.UTF8.GetString(buffer, 0, received);
        //if (response == "<|ACK|>")
        //{
        //    Console.WriteLine(
        //        $"Socket client received acknowledgment: \"{response}\"");
        //    break;
        //}

        client.Shutdown(SocketShutdown.Both);
    }
}
