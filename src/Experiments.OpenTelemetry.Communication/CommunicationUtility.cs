using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Experiments.OpenTelemetry.Communication.Responses;

namespace Experiments.OpenTelemetry.Communication;

public static class CommunicationUtility
{
    private const string Ack = "<|ACK|>";
    private static readonly Regex MessageRegex = new(@"^\<\|(?<cmdtype>.*?)\|\>(?<cmddata>.*)$");

    public static byte[] GenerateMessageBytes(object command)
        => Encoding.UTF8.GetBytes($"<|{command.GetType().AssemblyQualifiedName}|>{JsonSerializer.Serialize(command)}");

    public static byte[] GenerateAckBytes() => Encoding.UTF8.GetBytes("<|ACK|>");

    public static Response ParseResponse(ReadOnlyMemory<byte> responseBytes)
    {
        var message = Encoding.UTF8.GetString(responseBytes.Span);

        if (Ack == message.Trim()) { return new AckResponse(); }

        var match = MessageRegex.Match(message);
        if (!match.Success) { throw new InvalidDataException(); }

        return ParseMessage(message) switch
        {
            TextResponse txtResponse => txtResponse,
            _ => throw new InvalidDataException()
        };
    }

    public static object ParseMessage(ReadOnlyMemory<byte> responseBytes) => ParseMessage(Encoding.UTF8.GetString(responseBytes.Span));

    public static object ParseMessage(string message)
    {
        var match = MessageRegex.Match(message);
        if (!match.Success) { throw new InvalidDataException(); }

        return JsonSerializer.Deserialize(match.Groups["cmddata"].Value, Type.GetType(match.Groups["cmdtype"].Value)!)
            ?? throw new InvalidDataException();
    }
}
