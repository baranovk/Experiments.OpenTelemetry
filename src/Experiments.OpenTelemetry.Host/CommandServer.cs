using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Experiments.OpenTelemetry.Communication;
using Functional;
using Microsoft.Extensions.Logging;
using static Functional.F;

namespace Experiments.OpenTelemetry.Host;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S1144:Unused private types or members should be removed", Justification = "<Pending>")]
internal sealed class CommandServer(int port, ILogger logger, IHostConfigurationUpdater configuration) : IDisposable
{
    private int _disposed; // 0 == false, anything else == true
    private readonly int _port = port;
    private readonly ILogger _logger = logger;
    private Socket? _listenSocket;
    private Socket? _transmitSocket;
    private readonly byte[] _buffer = new byte[1_024];
    private static readonly Regex CommandRegex = new(@"^\<\|(?<cmdtype>.*?)\|\>(?<cmddata>.*)$");
    private readonly IHostConfigurationUpdater _configuration = configuration;

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        _listenSocket = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        _listenSocket.Bind(new IPEndPoint(IPAddress.Loopback, _port));
        _listenSocket.Listen();

        while (!cancellationToken.IsCancellationRequested)
        {
            _transmitSocket = await _listenSocket.AcceptAsync(cancellationToken).ConfigureAwait(false);

            int receivedBytesCount, receivedBytesTotal = default;

            while (0 < (receivedBytesCount = await _transmitSocket.ReceiveAsync(_buffer, SocketFlags.None, cancellationToken).ConfigureAwait(false)))
            {
                receivedBytesTotal += receivedBytesCount;
            }

            ParseCommand(new ReadOnlySpan<byte>(_buffer, 0, receivedBytesTotal))
                .Match(
                    ex => _logger.LogError(ex, "Parse command exception"),
                    opt => opt.Match(
                        () => _logger.LogInformation("Invalid command format"),
                        cmd => HandleCommand(cmd)
                    )
                )
                .Pipe(_ => _transmitSocket.Close());
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
            case ChangeMaxConcurrentActivitiesCountCommand cmd:
                _configuration.SetMaxConcurrentExecutingActivities(cmd.MaxCount);
                break;
            case ChangeEntrypointActivityQueuePeriodCommand cmd:
                _configuration.SetEntrypointActivityQueuePeriod(cmd.Period);
                break;
            case ChangeActivityErrorRatePercentCommand cmd:
                _configuration.SetActivityErrorRatePercent(cmd.ErrorRate);
                break;
            case ChangeActivityExecutionTimeThresholdCommand { ThresholdType: ThresholdType.Min } cmd:
                _configuration.SetActivityExecutionTimeMinMilliseconds(cmd.Milliseconds);
                break;
            case ChangeActivityExecutionTimeThresholdCommand { ThresholdType: ThresholdType.Max } cmd:
                _configuration.SetActivityExecutionTimeMaxMilliseconds(cmd.Milliseconds);
                break;
            case ChangeActivityWorkItemProcessingTimeThresholdCommand { ThresholdType: ThresholdType.Min } cmd:
                _configuration.SetActivityWorkItemProcessingTimeMinMilliseconds(cmd.Milliseconds);
                break;
            case ChangeActivityWorkItemProcessingTimeThresholdCommand { ThresholdType: ThresholdType.Max } cmd:
                _configuration.SetActivityWorkItemProcessingTimeMaxMilliseconds(cmd.Milliseconds);
                break;
        }
    }
}
