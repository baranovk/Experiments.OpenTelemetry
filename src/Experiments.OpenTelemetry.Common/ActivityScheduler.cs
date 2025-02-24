using System.Collections.Concurrent;
using System.Reactive.Subjects;

namespace Experiments.OpenTelemetry.Common;

public sealed record ActivityDescriptor(Type ActivityType);

public interface IActivityScheduler
{
    void QueueActivity(ActivityDescriptor descriptor);
}

public sealed class ActivityScheduler : IObservable<ActivityDescriptor>, IActivityScheduler, IDisposable
{
    private bool _disposed;
    private readonly Subject<ActivityDescriptor> _activitySubject;
    private readonly BlockingCollection<ActivityDescriptor> _activityQueue;

    public ActivityScheduler(int queueLimit = 100, CancellationToken cancellationToken = default)
    {
        _activityQueue = new(queueLimit);
        _activitySubject = new Subject<ActivityDescriptor>();

        Task.Factory.StartNew(
            () =>
            {
                foreach (var descriptor in _activityQueue)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    _activitySubject.OnNext(descriptor);
                }
            },
            cancellationToken,
            TaskCreationOptions.LongRunning,
            TaskScheduler.Default
        )
        .ContinueWith(t =>
        {
            Console.WriteLine(t.Exception!.ToString());
            _activitySubject.OnError(t.Exception!);
        },
        default, TaskContinuationOptions.OnlyOnFaulted, TaskScheduler.Default);
    }

    public void Dispose()
    {
        if (_disposed) { return; }

        _activitySubject.Dispose();
        _activityQueue.Dispose();

        _disposed = true;
    }

    public void QueueActivity(ActivityDescriptor descriptor) => _activityQueue.Add(descriptor);

    public IDisposable Subscribe(IObserver<ActivityDescriptor> observer) => _activitySubject.Subscribe(observer);
}
