namespace Experiments.OpenTelemetry.Communication.Commands;

public record ChangeActivityErrorRatePercentCommand(Percentage ErrorRate);

public struct Percentage
{
    public Percentage(int value)
    {
        if (0 > value || value > 100) { throw new ArgumentOutOfRangeException(nameof(value), $"Invalid percent value: {value}"); }
        Value = value;
    }

    public int Value { get; private set; }
}
