namespace Experiments.OpenTelemetry.Common;

public class ActivityContext
{
    public ActivityContext(string correlationId)
    {
        CorrelationId = correlationId;
        Bag = new();
    }

    private ActivityContext(string correlationId, ActivityContextBag bag)
    {
        CorrelationId = correlationId;
        Bag = bag;
    }

    public string CorrelationId { get; private set; }

    public ActivityContextBag Bag { get; init; }

    public ActivityContext Clone() => new(CorrelationId, Bag.Clone());
}

public class ActivityContextBag
{
    private readonly Dictionary<string, object?> _bag = [];

    public ActivityContextBag Set(string key, object? value)
    {
        if (!_bag.TryAdd(key, value)) { _bag[key] = value; }
        return this;
    }

    public object? Get(string key) => _bag.TryGetValue(key, out var value) ? value : null;

    public ActivityContextBag Clone() => _bag.Aggregate(new ActivityContextBag(), (bag, kv) => bag.Set(kv.Key, kv.Value));
}
