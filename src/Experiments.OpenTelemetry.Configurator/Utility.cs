using System.Net;
using System.Net.Sockets;
using Experiments.OpenTelemetry.Communication;
using Experiments.OpenTelemetry.Communication.Responses;

namespace Experiments.OpenTelemetry.Configurator;

internal static class Utility
{
    public static async Task<Response> SendCommandAsync(object command)
    {
        var endpoint = new IPEndPoint(IPAddress.Loopback, 5000);

        using Socket client = new(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        await client.ConnectAsync(endpoint).ConfigureAwait(false);

        var messageBytes = CommunicationUtility.GenerateMessageBytes(command);
        _ = await client.SendAsync(messageBytes, SocketFlags.None).ConfigureAwait(false);

        var buffer = new byte[1_024];
        var receivedBytesCount = await client.ReceiveAsync(buffer).ConfigureAwait(false);

        client.Shutdown(SocketShutdown.Both);
        return CommunicationUtility.ParseResponse(new ReadOnlyMemory<byte>(buffer, 0, receivedBytesCount));
    }
}
