using System.Net;
using System.Net.Sockets;
using System.Text;
using Communication.Sockets;
using Experiments.OpenTelemetry.Common;
using Experiments.OpenTelemetry.Communication;
using Experiments.OpenTelemetry.Communication.Commands;
using Experiments.OpenTelemetry.Communication.Responses;
using Functional;
using Microsoft.Extensions.Logging;
using static Functional.F;
using Unit = System.ValueTuple;

namespace Experiments.OpenTelemetry.Host;

internal sealed class CommandServer(
    int port,
    ILogger logger,
    IHostConfiguration hostConfiguration,
    IActivityConfiguration activityConfiguration,
    IHostConfigurationUpdater hostConfigurationUpdater) : IDisposable
{
    private int _disposed; // 0 == false, anything else == true
    private readonly int _port = port;
    private readonly ILogger _logger = logger;
    private Socket? _listenSocket;
    private Socket? _transmitSocket;
    private readonly byte[] _buffer = new byte[1_024];
    private readonly IHostConfiguration _hostConfiguration = hostConfiguration;
    private readonly IActivityConfiguration _activityConfiguration = activityConfiguration;
    private readonly IHostConfigurationUpdater _hostConfigurationUpdater = hostConfigurationUpdater;

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        _listenSocket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        _listenSocket.Bind(new IPEndPoint(IPAddress.Loopback, _port));
        _listenSocket.Listen();

        while (!cancellationToken.IsCancellationRequested)
        {
            _transmitSocket = await _listenSocket.AcceptAsync(cancellationToken).ConfigureAwait(false);

            (await (await _transmitSocket.ReceiveAsync(_buffer, SocketFlags.None, cancellationToken).ConfigureAwait(false))
                .Pipe(receivedBytesCount => Try(() => CommunicationUtility.ParseMessage(new Memory<byte>(_buffer, 0, receivedBytesCount)))
                        .Run()
                        .Match(
                            ex => Async<Exceptional<Unit>>(ex),
                            cmd => TryAsync(() => HandleCommand(cmd, _transmitSocket)).RunAsync()
                        )
                )
                .ConfigureAwait(false)
            )
            .Pipe(result => result.Match(
                    ex => { _logger.LogError(ex, "Command processing error"); _transmitSocket.Close(); },
                    _ => _transmitSocket.Close()
                )
            );
        }
    }

    public void Dispose()
    {
        if (Interlocked.CompareExchange(ref _disposed, 1, 0) == 1) { return; }

        _transmitSocket?.Dispose();
        _listenSocket?.Dispose();
    }

    private async Task<Unit> HandleCommand(object command, Socket socket, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Handling {CommandType} command: {CommandDescription}",
            command.GetType().Name, command.ToString());

        switch (command)
        {
            case PrintConfigurationParametersCommand:
                var response = new TextResponse(FormatConfigurationParameters());
                await socket.SendAsync(
                    new ReadOnlyMemory<byte>(CommunicationUtility.GenerateMessageBytes(response)), cancellationToken
                ).ConfigureAwait(false);
                break;
            case ChangeMaxConcurrentActivitiesCountCommand cmd:
                _hostConfigurationUpdater.SetMaxConcurrentExecutingActivities(cmd.MaxCount);
                await socket.SendAsync(new ReadOnlyMemory<byte>(CommunicationUtility.GenerateAckBytes()), cancellationToken).ConfigureAwait(false);
                break;
            case ChangeEntrypointActivityQueuePeriodCommand cmd:
                _hostConfigurationUpdater.SetEntrypointActivityQueuePeriod(cmd.Period);
                await socket.SendAsync(new ReadOnlyMemory<byte>(CommunicationUtility.GenerateAckBytes()), cancellationToken).ConfigureAwait(false);
                break;
            case ChangeActivityErrorRatePercentCommand cmd:
                _hostConfigurationUpdater.SetActivityErrorRatePercent(cmd.ErrorRate);
                await socket.SendAsync(new ReadOnlyMemory<byte>(CommunicationUtility.GenerateAckBytes()), cancellationToken).ConfigureAwait(false);
                break;
            case ChangeActivityExecutionTimeThresholdCommand { ThresholdType: ThresholdType.Min } cmd:
                _hostConfigurationUpdater.SetActivityExecutionTimeMinMilliseconds(cmd.Milliseconds);
                await socket.SendAsync(new ReadOnlyMemory<byte>(CommunicationUtility.GenerateAckBytes()), cancellationToken).ConfigureAwait(false);
                break;
            case ChangeActivityExecutionTimeThresholdCommand { ThresholdType: ThresholdType.Max } cmd:
                _hostConfigurationUpdater.SetActivityExecutionTimeMaxMilliseconds(cmd.Milliseconds);
                await socket.SendAsync(new ReadOnlyMemory<byte>(CommunicationUtility.GenerateAckBytes()), cancellationToken).ConfigureAwait(false);
                break;
            case ChangeActivityWorkItemProcessingTimeThresholdCommand { ThresholdType: ThresholdType.Min } cmd:
                _hostConfigurationUpdater.SetActivityWorkItemProcessingTimeMinMilliseconds(cmd.Milliseconds);
                await socket.SendAsync(new ReadOnlyMemory<byte>(CommunicationUtility.GenerateAckBytes()), cancellationToken).ConfigureAwait(false);
                break;
            case ChangeActivityWorkItemProcessingTimeThresholdCommand { ThresholdType: ThresholdType.Max } cmd:
                _hostConfigurationUpdater.SetActivityWorkItemProcessingTimeMaxMilliseconds(cmd.Milliseconds);
                await socket.SendAsync(new ReadOnlyMemory<byte>(CommunicationUtility.GenerateAckBytes()), cancellationToken).ConfigureAwait(false);
                break;
        }

        return new();
    }

    private string FormatConfigurationParameters()
    {
        var sb = new StringBuilder();

        sb.Append("\r\nHost configuration: \r\n");
        sb.Append($"{nameof(_hostConfiguration.MaxConcurrentExecutingActivities)}: {_hostConfiguration.MaxConcurrentExecutingActivities}\r\n");
        sb.Append($"{nameof(_hostConfiguration.EntrypointActivityQueuePeriod)}: {_hostConfiguration.EntrypointActivityQueuePeriod}\r\n");
        sb.Append($"{nameof(_hostConfiguration.ActivityQueueLimit)}: {_hostConfiguration.ActivityQueueLimit}\r\n");

        sb.Append("Activity configuration: \r\n");
        sb.Append($"{nameof(_activityConfiguration.ErrorRatePercent)}: {_activityConfiguration.ErrorRatePercent}\r\n");
        sb.Append($"{nameof(_activityConfiguration.ActivityExecutionTimeMinMilliseconds)}: {_activityConfiguration.ActivityExecutionTimeMinMilliseconds}\r\n");
        sb.Append($"{nameof(_activityConfiguration.ActivityExecutionTimeMaxMilliseconds)}: {_activityConfiguration.ActivityExecutionTimeMaxMilliseconds}\r\n");
        sb.Append($"{nameof(_activityConfiguration.ActivityWorkItemProcessingTimeMinMilliseconds)}: {_activityConfiguration.ActivityWorkItemProcessingTimeMinMilliseconds}\r\n");
        sb.Append($"{nameof(_activityConfiguration.ActivityWorkItemProcessingTimeMaxMilliseconds)}: {_activityConfiguration.ActivityWorkItemProcessingTimeMaxMilliseconds}\r\n");

        return sb.ToString();
    }
}
