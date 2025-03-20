using System.Net.Sockets;
using Functional;

namespace Communication.Sockets;

public static class SocketExtensions
{
    public static async ValueTask<int> ReceiveAllAsync(this Socket socket, byte[] buffer,
        SocketFlags socketFlags = SocketFlags.None, CancellationToken cancellationToken = default)
    {
        // TODO: add  receive while EOF
        return await socket.ReceiveAsync(buffer, SocketFlags.None, cancellationToken).ConfigureAwait(false);

        //int receivedBytesTotal = default;

        //await (socket.ReceiveAsync(buffer, SocketFlags.None, cancellationToken))
        //    .IterateUntilAsync(
        //        receivedBytesCount =>
        //        {
        //            receivedBytesTotal += receivedBytesCount;
        //            return socket.ReceiveAsync(buffer, SocketFlags.None, cancellationToken);
        //        },
        //        receivedBytesCount => 0 == receivedBytesCount
        //    )
        //    .ConfigureAwait(false);

        //return receivedBytesTotal;
    }
}
