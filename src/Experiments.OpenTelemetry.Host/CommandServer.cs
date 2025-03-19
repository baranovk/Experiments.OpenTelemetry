using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Experiments.OpenTelemetry.Common;
using Experiments.OpenTelemetry.Communication;
using Functional;
using Microsoft.Extensions.Logging;
using static Functional.F;

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
    private static readonly Regex CommandRegex = new(@"^\<\|(?<cmdtype>.*?)\|\>(?<cmddata>.*)$");
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

            int receivedBytesTotal = default;

            (await (_transmitSocket.ReceiveAsync(_buffer, SocketFlags.None, cancellationToken))
                .AsTask()
                .IterateUntilAsync(
                    receivedBytesCount =>
                    {
                        receivedBytesTotal += receivedBytesCount;
                        return _transmitSocket.ReceiveAsync(_buffer, SocketFlags.None, cancellationToken).AsTask();
                    },
                    receivedBytesTotal => 0 == receivedBytesTotal
                )
                .ConfigureAwait(false)
            )
            .Pipe(_ => ParseCommand(new ReadOnlySpan<byte>(_buffer, 0, receivedBytesTotal))
                        .Match(
                            ex => _logger.LogError(ex, "Parse command exception"),
                            opt => opt.Match(
                                () => _logger.LogInformation("Invalid command format"),
                                cmd => HandleCommand(cmd)
                            )
                        )
                        .Pipe(_ => _transmitSocket.Close())
            );
        }
    }

    public void Dispose()
    {
        if (Interlocked.CompareExchange(ref _disposed, 1, 0) == 1) { return; }

        _transmitSocket?.Dispose();
        _listenSocket?.Dispose();
    }

    private static Exceptional<Option<object>> ParseCommand(ReadOnlySpan<byte> buffer)
        => Some(CommandRegex.Match(Encoding.UTF8.GetString(buffer)))
            .Match(
                () => throw new InvalidOperationException(),
                match => match.Success
                    ? Try(() => Some(
                        JsonSerializer.Deserialize(
                            match.Groups["cmddata"].Value,
                            Type.GetType(match.Groups["cmdtype"].Value)!
                        ))
                      )
                      .Run()
                    : Functional.Exceptional.Of<Option<object>>(None)
            );

    private void HandleCommand(object command)
    {
        _logger.LogInformation("Handling {CommandType} command: {CommandDescription}",
            command.GetType().Name, command.ToString());

        switch (command)
        {
            case PrintConfigurationParametersCommand:
                PrintConfigurationParameters();
                break;
            case ChangeMaxConcurrentActivitiesCountCommand cmd:
                _hostConfigurationUpdater.SetMaxConcurrentExecutingActivities(cmd.MaxCount);
                break;
            case ChangeEntrypointActivityQueuePeriodCommand cmd:
                _hostConfigurationUpdater.SetEntrypointActivityQueuePeriod(cmd.Period);
                break;
            case ChangeActivityErrorRatePercentCommand cmd:
                _hostConfigurationUpdater.SetActivityErrorRatePercent(cmd.ErrorRate);
                break;
            case ChangeActivityExecutionTimeThresholdCommand { ThresholdType: ThresholdType.Min } cmd:
                _hostConfigurationUpdater.SetActivityExecutionTimeMinMilliseconds(cmd.Milliseconds);
                break;
            case ChangeActivityExecutionTimeThresholdCommand { ThresholdType: ThresholdType.Max } cmd:
                _hostConfigurationUpdater.SetActivityExecutionTimeMaxMilliseconds(cmd.Milliseconds);
                break;
            case ChangeActivityWorkItemProcessingTimeThresholdCommand { ThresholdType: ThresholdType.Min } cmd:
                _hostConfigurationUpdater.SetActivityWorkItemProcessingTimeMinMilliseconds(cmd.Milliseconds);
                break;
            case ChangeActivityWorkItemProcessingTimeThresholdCommand { ThresholdType: ThresholdType.Max } cmd:
                _hostConfigurationUpdater.SetActivityWorkItemProcessingTimeMaxMilliseconds(cmd.Milliseconds);
                break;
        }
    }

    private void PrintConfigurationParameters()
    {
        var sb = new StringBuilder();

        sb.Append("\r\nHost configuration: \r\n");
        sb.Append($"{nameof(_hostConfiguration.MaxConcurrentExecutingActivities)}: {_hostConfiguration.MaxConcurrentExecutingActivities}\r\n");
        sb.Append($"{nameof(_hostConfiguration.EntrypointActivityQueuePeriod)}: {_hostConfiguration.EntrypointActivityQueuePeriod}\r\n");
        sb.Append($"{nameof(_hostConfiguration.ActivityQueueLimit)}: {_hostConfiguration.ActivityQueueLimit}\r\n");

        sb.Append("Activity configuration: \r\n");
        sb.Append($"{nameof(_activityConfiguration.ErrorRatePercent)}: {_activityConfiguration.ErrorRatePercent}");
        sb.Append($"{nameof(_activityConfiguration.ActivityExecutionTimeMinMilliseconds)}: {_activityConfiguration.ActivityExecutionTimeMinMilliseconds}\r\n");
        sb.Append($"{nameof(_activityConfiguration.ActivityExecutionTimeMaxMilliseconds)}: {_activityConfiguration.ActivityExecutionTimeMaxMilliseconds}\r\n");
        sb.Append($"{nameof(_activityConfiguration.ActivityWorkItemProcessingTimeMinMilliseconds)}: {_activityConfiguration.ActivityWorkItemProcessingTimeMinMilliseconds}\r\n");
        sb.Append($"{nameof(_activityConfiguration.ActivityWorkItemProcessingTimeMaxMilliseconds)}: {_activityConfiguration.ActivityWorkItemProcessingTimeMaxMilliseconds}\r\n");

        _logger.LogInformation("{ConfigurationPerameters}", sb.ToString());
    }
}
