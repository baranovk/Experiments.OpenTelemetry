using System.Collections.Concurrent;
using System.Reactive.Subjects;
using Experiments.OpenTelemetry.Common;
using Microsoft.Extensions.Logging;

namespace Experiments.OpenTelemetry.Host;

internal delegate void OnEnqueueActivity(string activityUid, int activityQueueLength);

internal sealed class ActivityScheduler : IObservable<ActivityDescriptor>, IActivityScheduler, IDisposable
{
    private bool _disposed;
    private readonly ILogger _logger;
    private readonly OnEnqueueActivity? _onAfterQueueActivity;
    private readonly Subject<ActivityDescriptor> _activitySubject;
    private readonly BlockingCollection<ActivityDescriptor> _activityQueue;

    public ActivityScheduler(
        ILogger logger,
        int activityQueueLimit = 100,
        OnEnqueueActivity? onAfterQueueActivity = null,
        CancellationToken cancellationToken = default)
    {
        _logger = logger;
        _activityQueue = new(activityQueueLimit);
        _onAfterQueueActivity = onAfterQueueActivity;
        _activitySubject = new Subject<ActivityDescriptor>();

        Task.Factory.StartNew(
            () =>
            {
                foreach (var descriptor in _activityQueue.GetConsumingEnumerable(cancellationToken))
                {
                    _activitySubject.OnNext(descriptor);
                }
            },
            cancellationToken,
            TaskCreationOptions.LongRunning,
            TaskScheduler.Default
        )
        .ContinueWith(t =>
        {
            _logger.LogError(t.Exception, "Error on schedule activity");
            _activitySubject.OnError(t.Exception!);
        },
        default, TaskContinuationOptions.OnlyOnFaulted, TaskScheduler.Default);
    }

    public void Dispose()
    {
        if (_disposed) { return; }

        _activitySubject.Dispose();
        _activityQueue.CompleteAdding();
        _activityQueue.Dispose();

        _disposed = true;
    }

    public void QueueActivity(ActivityDescriptor descriptor)
    {
        _activityQueue.Add(descriptor);
        _onAfterQueueActivity?.Invoke(descriptor.ActivityUid, _activityQueue.Count);
    }

    public IDisposable Subscribe(IObserver<ActivityDescriptor> observer) => _activitySubject.Subscribe(observer);
}
